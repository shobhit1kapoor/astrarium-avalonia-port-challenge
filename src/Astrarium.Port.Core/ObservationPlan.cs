namespace Astrarium.Port.Core;

/// <summary>Conditions used to find useful target windows during an observing session.</summary>
public sealed record ObservationPlanOptions
{
    public double MinimumAltitudeDegrees { get; init; } = 20;
    public double MaximumSunAltitudeDegrees { get; init; } = -12;
    public double MinimumWindowMinutes { get; init; } = 30;
    public double MaximumMagnitude { get; init; } = 5;
    public TimeSpan SamplingInterval { get; init; } = TimeSpan.FromMinutes(10);
    public int MaximumTargets { get; init; } = 250;

    public void Validate()
    {
        if (!double.IsFinite(MinimumAltitudeDegrees) || MinimumAltitudeDegrees is < 0 or > 90)
            throw new ArgumentOutOfRangeException(nameof(MinimumAltitudeDegrees), "Minimum altitude must be between 0° and 90°.");
        if (!double.IsFinite(MaximumSunAltitudeDegrees) || MaximumSunAltitudeDegrees is < -30 or > 20)
            throw new ArgumentOutOfRangeException(nameof(MaximumSunAltitudeDegrees), "Maximum Sun altitude must be between -30° and +20°.");
        if (!double.IsFinite(MinimumWindowMinutes) || MinimumWindowMinutes is < 0 or > 2880)
            throw new ArgumentOutOfRangeException(nameof(MinimumWindowMinutes), "Minimum visibility must be between 0 and 2,880 minutes.");
        if (!double.IsFinite(MaximumMagnitude) || MaximumMagnitude is < -30 or > 20)
            throw new ArgumentOutOfRangeException(nameof(MaximumMagnitude), "Magnitude limit must be between -30 and +20.");
        if (SamplingInterval < TimeSpan.FromMinutes(5) || SamplingInterval > TimeSpan.FromHours(1))
            throw new ArgumentOutOfRangeException(nameof(SamplingInterval), "Sampling interval must be between 5 minutes and 1 hour.");
        if (MaximumTargets is < 1 or > 2000)
            throw new ArgumentOutOfRangeException(nameof(MaximumTargets), "Maximum targets must be between 1 and 2,000.");
    }
}

public sealed record ObservationTimeWindow(DateTimeOffset StartUtc, DateTimeOffset EndUtc)
{
    public TimeSpan Duration => EndUtc - StartUtc;
}

/// <summary>A target's best qualifying interval within the requested session range.</summary>
public sealed record PlannedObservation(
    SkyChartObject Target,
    DateTimeOffset WindowStartUtc,
    DateTimeOffset WindowEndUtc,
    DateTimeOffset BestTimeUtc,
    double BestAltitudeDegrees,
    double BestAzimuthDegrees)
{
    public TimeSpan Duration => WindowEndUtc - WindowStartUtc;
}

public sealed record ObservationPlan(
    DateTimeOffset StartUtc,
    DateTimeOffset EndUtc,
    ObservingLocation Location,
    double MaximumSunAltitudeDegrees,
    TimeSpan SamplingInterval,
    IReadOnlyList<ObservationTimeWindow> DarkWindows,
    IReadOnlyList<PlannedObservation> Targets);
