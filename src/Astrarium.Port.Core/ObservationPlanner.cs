namespace Astrarium.Port.Core;

/// <summary>
/// Builds observing windows from Astrarium's calculated topocentric positions.
/// The window edges are linearly interpolated between samples; their precision is
/// therefore bounded by the selected sampling interval.
/// </summary>
public sealed class ObservationPlanner
{
    private readonly SkySceneCalculator _calculator;

    public ObservationPlanner(SkySceneCalculator calculator)
    {
        _calculator = calculator ?? throw new ArgumentNullException(nameof(calculator));
    }

    public ObservationPlan Build(
        DateTimeOffset start,
        DateTimeOffset end,
        ObservingLocation location,
        ObservationPlanOptions? options = null,
        IProgress<double>? progress = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(location);
        location.Validate();
        options ??= new ObservationPlanOptions();
        options.Validate();

        start = start.ToUniversalTime();
        end = end.ToUniversalTime();
        if (end <= start)
            throw new ArgumentException("The end of the observing window must be later than its start.", nameof(end));
        if (end - start > TimeSpan.FromHours(48))
            throw new ArgumentOutOfRangeException(nameof(end), "An observing plan cannot span more than 48 hours.");

        var sampleTimes = CreateSampleTimes(start, end, options.SamplingInterval);
        var tracks = new List<TargetTrack>();
        var sunAltitudes = new double[sampleTimes.Length];

        for (var sampleIndex = 0; sampleIndex < sampleTimes.Length; sampleIndex++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var scene = _calculator.Calculate(sampleTimes[sampleIndex], location);
            var sun = scene.Objects.First(item => item.Kind == SkyObjectKind.Sun);
            sunAltitudes[sampleIndex] = sun.AltitudeDegrees;

            if (sampleIndex == 0)
            {
                for (var objectIndex = 0; objectIndex < scene.Objects.Count; objectIndex++)
                {
                    var item = scene.Objects[objectIndex];
                    if (item.Kind == SkyObjectKind.Sun || item.Kind == SkyObjectKind.Star && item.Magnitude > options.MaximumMagnitude)
                        continue;

                    tracks.Add(new TargetTrack(objectIndex, item.Id, sampleTimes.Length));
                }
            }

            foreach (var track in tracks)
            {
                var item = scene.Objects[track.ObjectIndex];
                if (!StringComparer.Ordinal.Equals(item.Id, track.Id))
                    throw new InvalidOperationException("The sky scene changed object ordering while planning.");
                track.Samples.Add(new TargetSample(item, sampleTimes[sampleIndex], sun.AltitudeDegrees));
            }

            progress?.Report((sampleIndex + 1d) / sampleTimes.Length * 100d);
        }

        var darkWindows = FindWindows(sampleTimes, sunAltitudes.Select(altitude => options.MaximumSunAltitudeDegrees - altitude).ToArray())
            .Select(window => new ObservationTimeWindow(window.Start, window.End))
            .ToArray();

        var minimumDuration = TimeSpan.FromMinutes(options.MinimumWindowMinutes);
        var plannedTargets = new List<PlannedObservation>();
        foreach (var track in tracks)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var margins = track.Samples
                .Select(sample => Math.Min(
                    sample.Target.AltitudeDegrees - options.MinimumAltitudeDegrees,
                    options.MaximumSunAltitudeDegrees - sample.SunAltitudeDegrees))
                .ToArray();

            var bestWindow = FindWindows(sampleTimes, margins)
                .Where(window => window.End - window.Start >= minimumDuration)
                .Select(window => new
                {
                    Window = window,
                    Peak = track.Samples
                        .Where(sample => sample.Instant >= window.Start && sample.Instant <= window.End &&
                            sample.Target.AltitudeDegrees >= options.MinimumAltitudeDegrees &&
                            sample.SunAltitudeDegrees <= options.MaximumSunAltitudeDegrees)
                        .MaxBy(sample => sample.Target.AltitudeDegrees)
                })
                .Where(candidate => candidate.Peak is not null && candidate.Peak.Target.Magnitude <= options.MaximumMagnitude)
                .MaxBy(candidate => candidate.Peak!.Target.AltitudeDegrees);

            if (bestWindow?.Peak is not { } peak)
                continue;

            plannedTargets.Add(new PlannedObservation(
                peak.Target,
                bestWindow.Window.Start,
                bestWindow.Window.End,
                peak.Instant,
                peak.Target.AltitudeDegrees,
                peak.Target.AzimuthDegrees));
        }

        var orderedTargets = plannedTargets
            .OrderBy(target => target.Target.Magnitude)
            .ThenByDescending(target => target.BestAltitudeDegrees)
            .Take(options.MaximumTargets)
            .OrderBy(target => target.WindowStartUtc)
            .ThenByDescending(target => target.BestAltitudeDegrees)
            .ToArray();

        return new ObservationPlan(start, end, location, options.MaximumSunAltitudeDegrees,
            options.SamplingInterval, darkWindows, orderedTargets);
    }

    private static DateTimeOffset[] CreateSampleTimes(DateTimeOffset start, DateTimeOffset end, TimeSpan interval)
    {
        var times = new List<DateTimeOffset>();
        for (var instant = start; instant < end; instant += interval)
            times.Add(instant);
        if (times.Count == 0 || times[^1] != end)
            times.Add(end);
        return times.ToArray();
    }

    private static IReadOnlyList<Window> FindWindows(IReadOnlyList<DateTimeOffset> times, IReadOnlyList<double> margins)
    {
        var windows = new List<Window>();
        DateTimeOffset? start = margins[0] >= 0 ? times[0] : null;

        for (var index = 1; index < times.Count; index++)
        {
            var previousMargin = margins[index - 1];
            var currentMargin = margins[index];
            var wasVisible = previousMargin >= 0;
            var isVisible = currentMargin >= 0;
            if (wasVisible == isVisible)
                continue;

            var crossing = InterpolateCrossing(times[index - 1], times[index], previousMargin, currentMargin);
            if (isVisible)
            {
                start = crossing;
            }
            else if (start is { } windowStart && crossing > windowStart)
            {
                windows.Add(new Window(windowStart, crossing));
                start = null;
            }
        }

        if (start is { } finalStart && times[^1] > finalStart)
            windows.Add(new Window(finalStart, times[^1]));
        return windows;
    }

    private static DateTimeOffset InterpolateCrossing(DateTimeOffset start, DateTimeOffset end, double startMargin, double endMargin)
    {
        var denominator = startMargin - endMargin;
        var fraction = Math.Abs(denominator) < 1e-12 ? 0.5 : Math.Clamp(startMargin / denominator, 0, 1);
        return start + TimeSpan.FromTicks((long)((end - start).Ticks * fraction));
    }

    private sealed class TargetTrack(int objectIndex, string id, int sampleCapacity)
    {
        public int ObjectIndex { get; } = objectIndex;
        public string Id { get; } = id;
        public List<TargetSample> Samples { get; } = new(sampleCapacity);
    }

    private sealed record TargetSample(SkyChartObject Target, DateTimeOffset Instant, double SunAltitudeDegrees);
    private sealed record Window(DateTimeOffset Start, DateTimeOffset End);
}
