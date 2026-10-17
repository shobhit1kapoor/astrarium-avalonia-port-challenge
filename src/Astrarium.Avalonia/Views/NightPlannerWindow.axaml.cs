using Avalonia.Controls;
using Astrarium.Port.Core;

namespace Astrarium.Avalonia.Views;

public partial class NightPlannerWindow : Window
{
    private readonly NightPlannerView _plannerView;

    public NightPlannerWindow()
        : this(new ObservationPlanner(new SkySceneCalculator(StarCatalog.LoadBundled())),
            ObservingLocation.Greenwich, DateTimeOffset.UtcNow)
    {
    }

    public NightPlannerWindow(ObservationPlanner planner, ObservingLocation location, DateTimeOffset referenceInstant)
    {
        InitializeComponent();
        _plannerView = new NightPlannerView(planner, location, referenceInstant);
        _plannerView.ShowTargetRequested += ForwardShowTargetRequestedAsync;
        _plannerView.CloseRequested += Close;
        PlannerContent.Content = _plannerView;
        DataContext = _plannerView.DataContext;
        Closing += (_, _) => _plannerView.Cancel();
    }

    public event Func<PlannedObservation, Task>? ShowTargetRequested;

    private async Task ForwardShowTargetRequestedAsync(PlannedObservation observation)
    {
        if (ShowTargetRequested is { } handler)
            await handler(observation);
    }
}
