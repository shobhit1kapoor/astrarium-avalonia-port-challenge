using Avalonia.Controls;
using Astrarium.Avalonia.ViewModels;
using Astrarium.Port.Core;

namespace Astrarium.Avalonia.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainViewModel(new SkySceneCalculator(StarCatalog.LoadBundled()));
    }

    private void OnObjectSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (DataContext is not MainViewModel viewModel)
            return;

        if (e.AddedItems.OfType<SkyChartObject>().LastOrDefault() is { } selectedObject)
            viewModel.SelectedObject = selectedObject;
    }
}
