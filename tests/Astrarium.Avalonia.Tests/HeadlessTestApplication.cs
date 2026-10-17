using Avalonia;
using Avalonia.Headless;
using Avalonia.Themes.Fluent;

namespace Astrarium.Avalonia.Tests;

public sealed class HeadlessTestApplication : Application
{
    public static AppBuilder BuildAvaloniaApp() => AppBuilder.Configure<HeadlessTestApplication>()
        .WithInterFont()
        .UseSkia()
        .UseHeadless(new AvaloniaHeadlessPlatformOptions { UseHeadlessDrawing = false });

    public override void Initialize()
    {
        RequestedThemeVariant = global::Avalonia.Styling.ThemeVariant.Dark;
        Styles.Add(new FluentTheme());
    }
}
