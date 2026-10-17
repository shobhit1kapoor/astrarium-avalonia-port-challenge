using Avalonia;
using Avalonia.Browser;
using System.Runtime.InteropServices.JavaScript;

namespace Astrarium.Avalonia.Browser;

internal sealed partial class Program
{
    private static async Task Main(string[] args)
    {
        await JSHost.ImportAsync("AstrariumStorage", "../js/storage.js");
        await BuildAvaloniaApp()
            .WithInterFont()
            .StartBrowserAppAsync("out", new BrowserPlatformOptions
            {
                RenderingMode = [BrowserRenderingMode.WebGL2, BrowserRenderingMode.Software2D]
            });
    }

    public static AppBuilder BuildAvaloniaApp() => AppBuilder.Configure<BrowserApp>();
}
