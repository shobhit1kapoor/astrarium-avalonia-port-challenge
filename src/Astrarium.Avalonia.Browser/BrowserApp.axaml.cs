using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using System.Runtime.InteropServices.JavaScript;
using Astrarium.Avalonia.Views;

namespace Astrarium.Avalonia.Browser;

public partial class BrowserApp : Application
{
    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is ISingleViewApplicationLifetime singleView)
        {
            var mainView = new MainView(new BrowserPreferencesStore(), PlannerPresentation.Inline);
            mainView.AttachedToVisualTree += (_, _) => BrowserHostInterop.HideSplash();
            singleView.MainView = mainView;
        }

        base.OnFrameworkInitializationCompleted();
    }
}

internal static partial class BrowserHostInterop
{
    [JSImport("hideSplash", "AstrariumStorage")]
    internal static partial void HideSplash();
}
