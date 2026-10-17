using Avalonia.Controls;
using Avalonia.Interactivity;
using Astrarium.Avalonia.ViewModels;
using Astrarium.Port.Core;

namespace Astrarium.Avalonia.Views;

public partial class MainWindow : Window
{
    private readonly SkySceneCalculator _calculator;

    public MainWindow()
    {
        InitializeComponent();
        _calculator = new SkySceneCalculator(StarCatalog.LoadBundled());
        DataContext = new MainViewModel(_calculator);
        Closing += (_, _) => (DataContext as MainViewModel)?.FlushPreferences();
        Opened += (_, _) =>
        {
            var smokeMarkerPath = Environment.GetEnvironmentVariable("ASTRARIUM_WINDOW_SMOKE_FILE");
            if (!string.IsNullOrWhiteSpace(smokeMarkerPath))
                File.WriteAllText(smokeMarkerPath, "opened");
        };
    }

    private async void OnOpenPlannerClicked(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not MainViewModel viewModel)
            return;

        var plannerWindow = new NightPlannerWindow(
            new ObservationPlanner(_calculator),
            viewModel.CurrentLocation,
            viewModel.CurrentInstant);
        plannerWindow.ShowTargetRequested += async target =>
        {
            plannerWindow.Close();
            await viewModel.GoToPlannedObservationAsync(target);
        };

        await plannerWindow.ShowDialog(this);
    }

    private void OnObjectSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (DataContext is not MainViewModel viewModel)
            return;

        if (e.AddedItems.OfType<SkyChartObject>().LastOrDefault() is { } selectedObject)
            viewModel.SelectedObject = selectedObject;
    }
}
