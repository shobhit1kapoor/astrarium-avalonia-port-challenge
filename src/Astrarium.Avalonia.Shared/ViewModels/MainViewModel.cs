using System.Collections.ObjectModel;
using System.Globalization;
using Astrarium.Port.Core;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Astrarium.Avalonia.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    private readonly SkySceneCalculator _calculator;
    private readonly IUserPreferencesStore _preferencesStore;
    private readonly SemaphoreSlim _calculationGate = new(1, 1);
    private CancellationTokenSource? _pendingCalculation;
    private CancellationTokenSource? _pendingPreferencesSave;
    private DateTimeOffset _referenceInstant;
    private ObservingLocation _location = ObservingLocation.Greenwich;
    private bool _isRestoringPreferences = true;

    public MainViewModel(SkySceneCalculator calculator, IUserPreferencesStore? preferencesStore = null)
    {
        _calculator = calculator;
        _preferencesStore = preferencesStore ?? new UserPreferencesStore();
        var preferences = _preferencesStore.Load();
        _location = preferences?.Location ?? ObservingLocation.Greenwich;
        _referenceInstant = preferences?.InstantUtc ?? new DateTimeOffset(DateTime.UtcNow.Date.AddHours(21), TimeSpan.Zero);
        DateTimeText = FormatDateTime(_referenceInstant);
        LocationName = _location.Name;
        LatitudeText = _location.LatitudeDegrees.ToString("F4", CultureInfo.InvariantCulture);
        LongitudeText = _location.LongitudeEastDegrees.ToString("F4", CultureInfo.InvariantCulture);
        LocationPresets = new ObservableCollection<LocationPreset>
        {
            new("Greenwich, UK", new ObservingLocation("Greenwich", 51.4779, 0.0, 46)),
            new("New York, USA", new ObservingLocation("New York", 40.7128, -74.0060, 10)),
            new("Mauna Kea, Hawaiʻi", new ObservingLocation("Mauna Kea", 19.8207, -155.4681, 4205)),
            new("Sydney, Australia", new ObservingLocation("Sydney", -33.8688, 151.2093, 58)),
            new("Reykjavík, Iceland", new ObservingLocation("Reykjavík", 64.1466, -21.9426, 61))
        };

        ShowStars = preferences?.ShowStars ?? true;
        ShowConstellations = preferences?.ShowConstellations ?? true;
        ShowBodies = preferences?.ShowBodies ?? true;
        _isRestoringPreferences = false;

        _ = RecalculateAsync(_referenceInstant);
    }

    [ObservableProperty]
    private SkyScene? scene;

    [ObservableProperty]
    private SkyChartObject? selectedObject;

    [ObservableProperty]
    private string locationName = string.Empty;

    [ObservableProperty]
    private string latitudeText = string.Empty;

    [ObservableProperty]
    private string longitudeText = string.Empty;

    [ObservableProperty]
    private string dateTimeText = string.Empty;

    [ObservableProperty]
    private double timelineOffsetHours;

    [ObservableProperty]
    private string searchText = string.Empty;

    [ObservableProperty]
    private string statusMessage = "Calculating the sky…";

    [ObservableProperty]
    private bool isBusy = true;

    [ObservableProperty]
    private bool showStars = true;

    [ObservableProperty]
    private bool showConstellations = true;

    [ObservableProperty]
    private bool showBodies = true;

    [ObservableProperty]
    private LocationPreset? selectedLocationPreset;

    public ObservableCollection<LocationPreset> LocationPresets { get; }

    public ObservingLocation CurrentLocation => _location;

    public DateTimeOffset CurrentInstant => _referenceInstant.AddHours(TimelineOffsetHours).ToUniversalTime();

    public IReadOnlyList<SkyChartObject> VisibleTargets { get; private set; } = [];

    public IReadOnlyList<SkyChartObject> SearchResults { get; private set; } = [];

    public bool HasVisibleTargets => VisibleTargets.Count > 0;

    public bool IsSearching => !string.IsNullOrWhiteSpace(SearchText);

    public bool IsNotSearching => !IsSearching;

    public bool HasSearchResults => SearchResults.Count > 0;

    public int StarCount => Scene?.Objects.Count(item => item.Kind == SkyObjectKind.Star) ?? 0;

    public string SceneSummary => Scene is null
        ? "Loading catalog and calculating positions"
        : $"{StarCount:N0} stars  ·  VSOP87 planets  ·  ELP2000 Moon";

    public string TimeSummary => Scene is null
        ? "—"
        : Scene.Instant.ToString("ddd, dd MMM  ·  HH:mm 'UTC'", CultureInfo.InvariantCulture);

    public string SkySummary => Scene is null
        ? "Preparing the sky"
        : $"{VisibleTargets.Count} bright targets above the horizon";

    partial void OnSceneChanged(SkyScene? value)
    {
        if (value is null)
            return;

        VisibleTargets = value.Objects
            .Where(item => item.AltitudeDegrees >= 0 &&
                (item.Kind != SkyObjectKind.Star || item.Magnitude <= 2.5))
            .OrderByDescending(item => item.AltitudeDegrees)
            .Take(24)
            .ToArray();

        OnPropertyChanged(nameof(VisibleTargets));
        OnPropertyChanged(nameof(HasVisibleTargets));

        var updatedSelection = SelectedObject is null
            ? null
            : value.Objects.FirstOrDefault(item => item.Id == SelectedObject.Id);
        if (updatedSelection is not null)
        {
            SelectedObject = updatedSelection;
        }
        else
        {
            SelectedObject = value.Objects
                .Where(item => item.Kind == SkyObjectKind.Star && item.AltitudeDegrees > 15)
                .OrderBy(item => item.Magnitude)
                .FirstOrDefault()
                ?? value.Objects.FirstOrDefault(item => item.Kind is SkyObjectKind.Planet or SkyObjectKind.Moon);
        }

        OnPropertyChanged(nameof(StarCount));
        OnPropertyChanged(nameof(SceneSummary));
        OnPropertyChanged(nameof(TimeSummary));
        OnPropertyChanged(nameof(SkySummary));
        UpdateSearchResults();
    }

    partial void OnSelectedObjectChanged(SkyChartObject? value)
    {
        OnPropertyChanged(nameof(SelectedRightAscension));
        OnPropertyChanged(nameof(SelectedDeclination));
        OnPropertyChanged(nameof(SelectedAltitude));
        OnPropertyChanged(nameof(SelectedAzimuth));
        OnPropertyChanged(nameof(SelectedMagnitude));
        OnPropertyChanged(nameof(SelectedAltAz));
    }

    partial void OnSearchTextChanged(string value)
    {
        OnPropertyChanged(nameof(IsSearching));
        OnPropertyChanged(nameof(IsNotSearching));
        UpdateSearchResults();
    }

    partial void OnTimelineOffsetHoursChanged(double value)
    {
        _referenceInstant = _referenceInstant.ToUniversalTime();
        var next = _referenceInstant.AddHours(value);
        DateTimeText = FormatDateTime(next);
        _ = RecalculateAsync(next);
        SchedulePreferenceSave();
    }

    partial void OnSelectedLocationPresetChanged(LocationPreset? value)
    {
        if (value is null)
            return;

        _location = value.Location;
        LocationName = _location.Name;
        LatitudeText = _location.LatitudeDegrees.ToString("F4", CultureInfo.InvariantCulture);
        LongitudeText = _location.LongitudeEastDegrees.ToString("F4", CultureInfo.InvariantCulture);
        _ = RecalculateAsync();
        SchedulePreferenceSave();
    }

    partial void OnShowStarsChanged(bool value) => SchedulePreferenceSave();

    partial void OnShowConstellationsChanged(bool value) => SchedulePreferenceSave();

    partial void OnShowBodiesChanged(bool value) => SchedulePreferenceSave();

    public string SelectedRightAscension => SelectedObject is null ? "—" : FormatRightAscension(SelectedObject.RightAscensionDegrees);
    public string SelectedDeclination => SelectedObject is null ? "—" : FormatSignedDegrees(SelectedObject.DeclinationDegrees);
    public string SelectedAltitude => SelectedObject is null ? "—" : FormatSignedDegrees(SelectedObject.AltitudeDegrees);
    public string SelectedAzimuth => SelectedObject is null ? "—" : $"{SelectedObject.AzimuthDegrees:000.0}°";
    public string SelectedMagnitude => SelectedObject is null ? "—" : SelectedObject.Magnitude.ToString("0.00", CultureInfo.InvariantCulture);
    public string SelectedAltAz => SelectedObject is null ? "—" : $"{SelectedAltitude}  /  {SelectedAzimuth}";

    public async Task GoToPlannedObservationAsync(PlannedObservation plannedObservation)
    {
        ArgumentNullException.ThrowIfNull(plannedObservation);
        _referenceInstant = plannedObservation.BestTimeUtc.ToUniversalTime();
        SelectedObject = plannedObservation.Target;
        SchedulePreferenceSave();

        if (TimelineOffsetHours != 0)
        {
            TimelineOffsetHours = 0;
            return;
        }

        DateTimeText = FormatDateTime(_referenceInstant);
        await RecalculateAsync(_referenceInstant);
    }

    [RelayCommand]
    private async Task ApplyLocationAsync()
    {
        if (!double.TryParse(LatitudeText, NumberStyles.Float, CultureInfo.InvariantCulture, out var latitude) ||
            !double.TryParse(LongitudeText, NumberStyles.Float, CultureInfo.InvariantCulture, out var longitude))
        {
            StatusMessage = "Enter latitude and longitude as decimal degrees.";
            return;
        }

        var candidate = new ObservingLocation(LocationName.Trim(), latitude, longitude);
        try
        {
            candidate.Validate();
        }
        catch (ArgumentException error)
        {
            StatusMessage = error.Message;
            return;
        }

        _location = candidate;
        SelectedLocationPreset = null;
        SchedulePreferenceSave();
        await RecalculateAsync();
    }

    [RelayCommand]
    private async Task ApplyDateTimeAsync()
    {
        if (!DateTimeOffset.TryParse(DateTimeText, CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var selected))
        {
            StatusMessage = "Use YYYY-MM-DD HH:mm in UTC.";
            return;
        }

        _referenceInstant = selected;
        TimelineOffsetHours = 0;
        DateTimeText = FormatDateTime(selected);
        SchedulePreferenceSave();
        await RecalculateAsync(selected);
    }

    [RelayCommand]
    private async Task SetNowAsync()
    {
        _referenceInstant = DateTimeOffset.UtcNow;
        TimelineOffsetHours = 0;
        DateTimeText = FormatDateTime(_referenceInstant);
        SchedulePreferenceSave();
        await RecalculateAsync(_referenceInstant);
    }

    [RelayCommand]
    private async Task StepBackAsync() => await StepByHoursAsync(-1);

    [RelayCommand]
    private async Task StepForwardAsync() => await StepByHoursAsync(1);

    private async Task StepByHoursAsync(double hours)
    {
        var next = Scene?.Instant.AddHours(hours) ?? _referenceInstant.AddHours(hours);
        _referenceInstant = next;
        TimelineOffsetHours = 0;
        DateTimeText = FormatDateTime(next);
        SchedulePreferenceSave();
        await RecalculateAsync(next);
    }

    public void FlushPreferences()
    {
        var pending = _pendingPreferencesSave;
        _pendingPreferencesSave = null;
        pending?.Cancel();

        try
        {
            _preferencesStore.Save(CreatePreferencesSnapshot());
        }
        catch (Exception error)
        {
            StatusMessage = $"Could not save preferences: {error.Message}";
        }
    }

    private void SchedulePreferenceSave()
    {
        if (_isRestoringPreferences)
            return;

        _pendingPreferencesSave?.Cancel();
        var cancellation = new CancellationTokenSource();
        _pendingPreferencesSave = cancellation;
        _ = SavePreferencesAfterDelayAsync(CreatePreferencesSnapshot(), cancellation);
    }

    private async Task SavePreferencesAfterDelayAsync(UserPreferences preferences, CancellationTokenSource cancellation)
    {
        try
        {
            await Task.Delay(300, cancellation.Token);
            cancellation.Token.ThrowIfCancellationRequested();
            _preferencesStore.Save(preferences);
        }
        catch (OperationCanceledException)
        {
            // A newer setting change or application close superseded this save.
        }
        catch (Exception error)
        {
            if (!cancellation.IsCancellationRequested)
                StatusMessage = $"Could not save preferences: {error.Message}";
        }
        finally
        {
            if (ReferenceEquals(_pendingPreferencesSave, cancellation))
                _pendingPreferencesSave = null;
            cancellation.Dispose();
        }
    }

    private UserPreferences CreatePreferencesSnapshot() => new(
        UserPreferences.CurrentSchemaVersion,
        _location,
        _referenceInstant.AddHours(TimelineOffsetHours).ToUniversalTime(),
        ShowStars,
        ShowConstellations,
        ShowBodies);

    private async Task RecalculateAsync(DateTimeOffset? instant = null)
    {
        _pendingCalculation?.Cancel();
        var cancellation = new CancellationTokenSource();
        _pendingCalculation = cancellation;
        var token = cancellation.Token;
        var requestedInstant = (instant ?? _referenceInstant.AddHours(TimelineOffsetHours)).ToUniversalTime();
        var requestedLocation = _location;

        IsBusy = true;
        StatusMessage = "Calculating apparent positions for this observer…";

        try
        {
            await Task.Delay(90, token);
            await _calculationGate.WaitAsync(token);
            SkyScene calculated;
            try
            {
                calculated = await Task.Run(() => _calculator.Calculate(requestedInstant, requestedLocation));
            }
            finally
            {
                _calculationGate.Release();
            }

            token.ThrowIfCancellationRequested();
            Scene = calculated;
            IsBusy = false;
            StatusMessage = $"Sky for {_location.Name} · topocentric";
        }
        catch (OperationCanceledException)
        {
            // A newer time or location request superseded this calculation.
        }
        catch (Exception error)
        {
            if (!token.IsCancellationRequested)
            {
                IsBusy = false;
                StatusMessage = $"Sky calculation failed: {error.Message}";
            }
        }
        finally
        {
            if (ReferenceEquals(_pendingCalculation, cancellation))
                _pendingCalculation = null;
            cancellation.Dispose();
        }
    }

    private void UpdateSearchResults()
    {
        if (Scene is null || string.IsNullOrWhiteSpace(SearchText))
        {
            SearchResults = [];
        }
        else
        {
            var query = SearchText.Trim();
            SearchResults = Scene.Objects
                .Where(item => item.Name.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                    (item.Designation?.Contains(query, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    item.Id.Contains(query, StringComparison.OrdinalIgnoreCase))
                .OrderBy(item => item.Magnitude)
                .Take(12)
                .ToArray();
        }

        OnPropertyChanged(nameof(SearchResults));
        OnPropertyChanged(nameof(HasSearchResults));
    }

    private static string FormatDateTime(DateTimeOffset instant) =>
        instant.ToUniversalTime().ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);

    private static string FormatRightAscension(double degrees)
    {
        var hours = Angle.NormalizeDegrees(degrees) / 15.0;
        var h = (int)hours;
        var minutes = (hours - h) * 60;
        var m = (int)minutes;
        var seconds = (minutes - m) * 60;
        return $"{h:00}h {m:00}m {seconds:00.0}s";
    }

    private static string FormatSignedDegrees(double degrees) =>
        $"{degrees:+00.0;-00.0;00.0}°";
}

public sealed record LocationPreset(string Label, ObservingLocation Location);

internal static class Angle
{
    public static double NormalizeDegrees(double degrees)
    {
        var normalized = degrees % 360.0;
        return normalized < 0 ? normalized + 360.0 : normalized;
    }
}
