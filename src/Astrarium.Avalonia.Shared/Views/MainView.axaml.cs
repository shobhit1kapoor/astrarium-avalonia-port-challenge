using Avalonia.Controls;
using Avalonia.Interactivity;
using Astrarium.Avalonia.ViewModels;
using Astrarium.Port.Core;

namespace Astrarium.Avalonia.Views;

public enum PlannerPresentation
{
    Window,
    Inline
}

public sealed class PlannerRequestedEventArgs(
    ObservationPlanner planner,
    ObservingLocation location,
    DateTimeOffset instantUtc) : EventArgs
{
    public ObservationPlanner Planner { get; } = planner;
    public ObservingLocation Location { get; } = location;
    public DateTimeOffset InstantUtc { get; } = instantUtc;
}

public partial class MainView : UserControl
{
    private readonly SkySceneCalculator _calculator;
    private readonly PlannerPresentation _plannerPresentation;

    public MainView()
        : this(null, PlannerPresentation.Inline)
    {
    }

    public MainView(IUserPreferencesStore? preferencesStore, PlannerPresentation plannerPresentation)
    {
        InitializeComponent();
        _plannerPresentation = plannerPresentation;
        _calculator = new SkySceneCalculator(StarCatalog.LoadBundled());
        DataContext = new MainViewModel(_calculator, preferencesStore);
    }

    public MainViewModel ViewModel => (MainViewModel)DataContext!;

    public event EventHandler<PlannerRequestedEventArgs>? PlannerRequested;

    public void FlushPreferences() => ViewModel.FlushPreferences();

    private void OnOpenPlannerClicked(object? sender, RoutedEventArgs e)
    {
        var viewModel = ViewModel;
        var planner = new ObservationPlanner(_calculator);
        if (_plannerPresentation == PlannerPresentation.Window)
        {
            PlannerRequested?.Invoke(this,
                new PlannerRequestedEventArgs(planner, viewModel.CurrentLocation, viewModel.CurrentInstant));
            return;
        }

        var plannerView = new NightPlannerView(planner, viewModel.CurrentLocation, viewModel.CurrentInstant);
        plannerView.CloseRequested += CloseEmbeddedPlanner;
        plannerView.ShowTargetRequested += async target =>
        {
            CloseEmbeddedPlanner();
            await viewModel.GoToPlannedObservationAsync(target);
        };
        PlannerHost.Content = plannerView;
        PlannerOverlay.IsVisible = true;
    }

    private void CloseEmbeddedPlanner()
    {
        PlannerOverlay.IsVisible = false;
        PlannerHost.Content = null;
    }

    private void OnObjectSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (DataContext is not MainViewModel viewModel)
            return;

        if (e.AddedItems.OfType<SkyChartObject>().LastOrDefault() is { } selectedObject)
            viewModel.SelectedObject = selectedObject;
    }
}
