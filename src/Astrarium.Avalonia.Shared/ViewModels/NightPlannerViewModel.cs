using System.Collections.ObjectModel;
using System.Globalization;
using Astrarium.Port.Core;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Astrarium.Avalonia.ViewModels;

public partial class NightPlannerViewModel : ObservableObject
{
    private const string DateFormat = "yyyy-MM-dd HH:mm";
    private readonly ObservationPlanner _planner;
    private readonly ObservingLocation _location;
    private CancellationTokenSource? _planningCancellation;

    public NightPlannerViewModel(ObservationPlanner planner, ObservingLocation location, DateTimeOffset referenceInstant)
    {
        _planner = planner;
        _location = location;
        location.Validate();

        var night = referenceInstant.ToUniversalTime().UtcDateTime.Date.AddHours(18);
        if (referenceInstant.ToUniversalTime().Hour < 6)
            night = night.AddDays(-1);
        StartUtcText = FormatUtc(new DateTimeOffset(night, TimeSpan.Zero));
        EndUtcText = FormatUtc(new DateTimeOffset(night.AddDays(1).AddHours(-12), TimeSpan.Zero));
    }

    [ObservableProperty]
    private string startUtcText = string.Empty;

    [ObservableProperty]
    private string endUtcText = string.Empty;

    [ObservableProperty]
    private string minimumAltitudeText = "20";

    [ObservableProperty]
    private string maximumSunAltitudeText = "-12";

    [ObservableProperty]
    private string minimumWindowMinutesText = "30";

    [ObservableProperty]
    private string maximumMagnitudeText = "5";

    [ObservableProperty]
    private bool isBusy;

    [ObservableProperty]
    private double progressPercent;

    [ObservableProperty]
    private string statusMessage = "Preparing an observing plan…";

    [ObservableProperty]
    private ObservationPlan? plan;

    public string LocationSummary => $"{_location.Name} · {_location.LatitudeDegrees:0.####}°, {_location.LongitudeEastDegrees:0.####}°";
    public ObservableCollection<PlannedObservationItem> Targets { get; } = [];
    public bool HasPlan => Plan is not null;
    public bool HasTargets => Targets.Count > 0;
    public bool HasNoTargets => HasPlan && !HasTargets;

    public string DarknessSummary
    {
        get
        {
            if (Plan is null)
                return "Build a plan to find astronomical darkness and good observing windows.";
            if (Plan.DarkWindows.Count == 0)
                return "No astronomical-dark interval in this time range.";

            return string.Join("  ·  ", Plan.DarkWindows.Select(window =>
                $"{window.StartUtc:ddd HH:mm}–{window.EndUtc:ddd HH:mm} UTC ({FormatDuration(window.Duration)})"));
        }
    }

    public string PlanSummary => Plan is null
        ? ""
        : $"{Targets.Count} best matches shown · Sun below {Plan.MaximumSunAltitudeDegrees:0.#}° · {Plan.SamplingInterval.TotalMinutes:0}-minute samples; window edges are interpolated.";

    partial void OnPlanChanged(ObservationPlan? value)
    {
        OnPropertyChanged(nameof(HasPlan));
        OnPropertyChanged(nameof(HasNoTargets));
        OnPropertyChanged(nameof(DarknessSummary));
        OnPropertyChanged(nameof(PlanSummary));
    }

    [RelayCommand]
    private async Task BuildPlanAsync()
    {
        Plan = null;
        Targets.Clear();
        OnPropertyChanged(nameof(HasTargets));

        if (!TryReadInputs(out var start, out var end, out var options, out var error))
        {
            StatusMessage = error;
            return;
        }

        _planningCancellation?.Cancel();
        var cancellation = new CancellationTokenSource();
        _planningCancellation = cancellation;
        IsBusy = true;
        ProgressPercent = 0;
        StatusMessage = "Sampling the calculated sky across this time range…";
        var progress = new Progress<double>(value => ProgressPercent = value);
        try
        {
            var result = await Task.Run(
                () => _planner.Build(start, end, _location, options, progress, cancellation.Token),
                cancellation.Token);

            Plan = result;
            foreach (var target in result.Targets)
                Targets.Add(new PlannedObservationItem(target));
            OnPropertyChanged(nameof(HasTargets));
            OnPropertyChanged(nameof(HasNoTargets));
            OnPropertyChanged(nameof(PlanSummary));
            StatusMessage = result.Targets.Count == 0
                ? "No targets meet these conditions. Lower the altitude or Sun limit, raise the magnitude limit, or widen the time range."
                : $"Plan ready · showing up to {result.Targets.Count} best matches.";
        }
        catch (OperationCanceledException)
        {
            StatusMessage = "Plan cancelled.";
        }
        catch (Exception exception) when (exception is ArgumentException or InvalidOperationException)
        {
            StatusMessage = exception.Message;
        }
        catch (Exception exception)
        {
            StatusMessage = $"Could not build the plan: {exception.Message}";
        }
        finally
        {
            if (ReferenceEquals(_planningCancellation, cancellation))
                _planningCancellation = null;
            IsBusy = false;
            cancellation.Dispose();
        }
    }

    [RelayCommand]
    private void CancelPlanning() => _planningCancellation?.Cancel();

    public void Cancel() => _planningCancellation?.Cancel();

    private bool TryReadInputs(
        out DateTimeOffset start,
        out DateTimeOffset end,
        out ObservationPlanOptions options,
        out string error)
    {
        start = default;
        end = default;
        options = new ObservationPlanOptions();

        if (!DateTimeOffset.TryParseExact(StartUtcText, DateFormat, CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out start) ||
            !DateTimeOffset.TryParseExact(EndUtcText, DateFormat, CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out end))
        {
            error = "Enter start and end as YYYY-MM-DD HH:mm UTC.";
            return false;
        }

        if (!TryParseNumber(MinimumAltitudeText, out var minimumAltitude) ||
            !TryParseNumber(MaximumSunAltitudeText, out var maximumSunAltitude) ||
            !TryParseNumber(MinimumWindowMinutesText, out var minimumWindow) ||
            !TryParseNumber(MaximumMagnitudeText, out var maximumMagnitude))
        {
            error = "Enter numeric values for all four observing conditions.";
            return false;
        }

        options = new ObservationPlanOptions
        {
            MinimumAltitudeDegrees = minimumAltitude,
            MaximumSunAltitudeDegrees = maximumSunAltitude,
            MinimumWindowMinutes = minimumWindow,
            MaximumMagnitude = maximumMagnitude
        };

        try
        {
            options.Validate();
            if (end <= start)
                throw new ArgumentException("The end of the observing window must be later than its start.");
            if (end - start > TimeSpan.FromHours(48))
                throw new ArgumentOutOfRangeException(nameof(end), "An observing plan cannot span more than 48 hours.");
        }
        catch (ArgumentException exception)
        {
            error = exception.Message;
            return false;
        }

        error = string.Empty;
        return true;
    }

    private static bool TryParseNumber(string value, out double number) =>
        double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out number) && double.IsFinite(number);

    private static string FormatUtc(DateTimeOffset instant) => instant.ToUniversalTime().ToString(DateFormat, CultureInfo.InvariantCulture);

    private static string FormatDuration(TimeSpan duration) =>
        duration.TotalHours >= 1 ? $"{(int)duration.TotalHours}h {duration.Minutes:00}m" : $"{duration.TotalMinutes:0}m";
}

public sealed class PlannedObservationItem
{
    public PlannedObservationItem(PlannedObservation observation)
    {
        Observation = observation;
        Name = observation.Target.Name;
        Kind = observation.Target.Kind.ToString();
        Conditions = $"mag {observation.Target.Magnitude:0.0} · best altitude {observation.BestAltitudeDegrees:0}° · az {observation.BestAzimuthDegrees:000}°";
        Window = $"{observation.WindowStartUtc:HH:mm}–{observation.WindowEndUtc:HH:mm} UTC";
        BestTime = $"Best {observation.BestTimeUtc:HH:mm} UTC · {observation.Duration.TotalMinutes:0} min";
        ShowTargetLabel = $"Show {Name} on the sky chart";
    }

    public PlannedObservation Observation { get; }
    public string Name { get; }
    public string Kind { get; }
    public string Conditions { get; }
    public string Window { get; }
    public string BestTime { get; }
    public string ShowTargetLabel { get; }
}
