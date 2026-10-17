using Avalonia.Controls;
using Avalonia.Interactivity;
using Astrarium.Port.Core;
using Astrarium.Avalonia.ViewModels;

namespace Astrarium.Avalonia.Views;

public partial class MainWindow : Window
{
    private readonly MainView _mainView;

    public MainWindow()
        : this(null)
    {
    }

    public MainWindow(IUserPreferencesStore? preferencesStore)
    {
        InitializeComponent();
        _mainView = new MainView(preferencesStore, PlannerPresentation.Window);
        _mainView.PlannerRequested += OnPlannerRequested;
        MainContent.Content = _mainView;
        DataContext = _mainView.DataContext;
        Closing += (_, _) => _mainView.FlushPreferences();
        Opened += (_, _) =>
        {
            var smokeMarkerPath = Environment.GetEnvironmentVariable("ASTRARIUM_WINDOW_SMOKE_FILE");
            if (!string.IsNullOrWhiteSpace(smokeMarkerPath))
                File.WriteAllText(smokeMarkerPath, "opened");
        };
    }

    private async void OnPlannerRequested(object? sender, PlannerRequestedEventArgs request)
    {
        var plannerWindow = new NightPlannerWindow(request.Planner, request.Location, request.InstantUtc);
        plannerWindow.ShowTargetRequested += async target =>
        {
            plannerWindow.Close();
            await _mainView.ViewModel.GoToPlannedObservationAsync(target);
        };

        await plannerWindow.ShowDialog(this);
    }
}
