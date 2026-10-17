using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Astrarium.Avalonia.Views;

namespace Astrarium.Avalonia.Browser;

public partial class BrowserApp : Application
{
    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is ISingleViewApplicationLifetime singleView)
            singleView.MainView = new MainView(new BrowserPreferencesStore(), PlannerPresentation.Inline);

        base.OnFrameworkInitializationCompleted();
    }
}
