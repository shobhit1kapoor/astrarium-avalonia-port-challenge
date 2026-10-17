using Avalonia.Controls;
using Avalonia.Interactivity;
using Astrarium.Avalonia.ViewModels;
using Astrarium.Port.Core;

namespace Astrarium.Avalonia.Views;

public partial class NightPlannerWindow : Window
{
    private readonly NightPlannerViewModel _viewModel;

    public NightPlannerWindow()
        : this(new ObservationPlanner(new SkySceneCalculator(StarCatalog.LoadBundled())),
            ObservingLocation.Greenwich, DateTimeOffset.UtcNow)
    {
    }

    public NightPlannerWindow(ObservationPlanner planner, ObservingLocation location, DateTimeOffset referenceInstant)
    {
        InitializeComponent();
        _viewModel = new NightPlannerViewModel(planner, location, referenceInstant);
        DataContext = _viewModel;
        Opened += (_, _) => _viewModel.BuildPlanCommand.Execute(null);
        Closing += (_, _) => _viewModel.Cancel();
    }

    public event Func<PlannedObservation, Task>? ShowTargetRequested;

    private async void OnShowTargetClicked(object? sender, RoutedEventArgs e)
    {
        if ((sender as Button)?.DataContext is PlannedObservationItem item && ShowTargetRequested is { } handler)
            await handler(item.Observation);
    }

    private void OnCloseClicked(object? sender, RoutedEventArgs e) => Close();
}
