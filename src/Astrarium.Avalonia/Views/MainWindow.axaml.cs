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
}
