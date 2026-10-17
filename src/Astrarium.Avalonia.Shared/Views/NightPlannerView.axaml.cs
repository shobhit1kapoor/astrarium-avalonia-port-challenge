using Avalonia.Controls;
using Avalonia.Interactivity;
using Astrarium.Avalonia.ViewModels;
using Astrarium.Port.Core;

namespace Astrarium.Avalonia.Views;

public partial class NightPlannerView : UserControl
{
    private readonly NightPlannerViewModel _viewModel;
    private bool _hasStarted;

    public NightPlannerView()
        : this(new ObservationPlanner(new SkySceneCalculator(StarCatalog.LoadBundled())),
            ObservingLocation.Greenwich, DateTimeOffset.UtcNow)
    {
    }

    public NightPlannerView(ObservationPlanner planner, ObservingLocation location, DateTimeOffset referenceInstant)
    {
        InitializeComponent();
        _viewModel = new NightPlannerViewModel(planner, location, referenceInstant);
        DataContext = _viewModel;
        AttachedToVisualTree += (_, _) =>
        {
            if (_hasStarted)
                return;

            _hasStarted = true;
            _viewModel.BuildPlanCommand.Execute(null);
        };
        DetachedFromVisualTree += (_, _) => _viewModel.Cancel();
    }

    public event Func<PlannedObservation, Task>? ShowTargetRequested;
    public event Action? CloseRequested;

    public NightPlannerViewModel ViewModel => _viewModel;

    public void Cancel() => _viewModel.Cancel();

    private async void OnShowTargetClicked(object? sender, RoutedEventArgs e)
    {
        if ((sender as Button)?.DataContext is PlannedObservationItem item && ShowTargetRequested is { } handler)
            await handler(item.Observation);
    }

    private void OnCloseClicked(object? sender, RoutedEventArgs e)
    {
        _viewModel.Cancel();
        CloseRequested?.Invoke();
    }
}
