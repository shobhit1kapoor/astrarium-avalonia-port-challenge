using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Astrarium.Avalonia.Views;
using Astrarium.Avalonia.ViewModels;
using Astrarium.Port.Core;

namespace Astrarium.Avalonia.Tests;

[TestClass]
public sealed class DisplayScalingTests
{
    private static readonly double[] TestScales = [1.0, 1.25, 1.5, 2.0];

    [TestMethod]
    public void MainWindowLayoutsRenderAtCommonDisplayScales()
    {
        using var session = HeadlessUnitTestSession.StartNew(typeof(HeadlessTestApplication));
        var preferencesPath = Path.Combine(Path.GetTempPath(), "Astrarium.Avalonia.Tests", Guid.NewGuid().ToString("N"), "preferences.json");

        try
        {
            session.Dispatch(() =>
            {
                var window = new MainWindow(new UserPreferencesStore(preferencesPath));
                var viewModel = (MainViewModel)window.DataContext!;
                window.Width = 1080;
                window.Height = 720;
                window.Show();
                WaitUntil(() => !viewModel.IsBusy && viewModel.Scene is not null,
                    "main window finished its initial sky calculation");
                VerifyWindowAtScales(window, 1080, 720, "main");
            }, CancellationToken.None);
        }
        finally
        {
            var testDirectory = Path.GetDirectoryName(preferencesPath);
            if (testDirectory is not null && Directory.Exists(testDirectory))
                Directory.Delete(testDirectory, recursive: true);
        }
    }

    [TestMethod]
    public void NightPlannerLayoutsRenderAtCommonDisplayScales()
    {
        using var session = HeadlessUnitTestSession.StartNew(typeof(HeadlessTestApplication));

        session.Dispatch(() =>
        {
            var catalog = StarCatalog.LoadBundled();
            var calculator = new SkySceneCalculator(catalog);
            var planner = new NightPlannerWindow(new ObservationPlanner(calculator), ObservingLocation.Greenwich,
                new DateTimeOffset(2026, 10, 17, 21, 0, 0, TimeSpan.Zero));
            var viewModel = (NightPlannerViewModel)planner.DataContext!;
            planner.Width = 780;
            planner.Height = 600;
            planner.Show();
            WaitUntil(() => !viewModel.IsBusy && viewModel.Plan is not null,
                "planner finished building its initial observing plan");
            VerifyWindowAtScales(planner, 780, 600, "planner");
        }, CancellationToken.None);
    }

    private static void VerifyWindowAtScales(Window window, double minimumWidth, double minimumHeight, string snapshotPrefix)
    {
        foreach (var scale in TestScales)
        {
            window.SetRenderScaling(scale);
            Dispatcher.UIThread.RunJobs();

            Assert.AreEqual(scale, window.RenderScaling, 0.001, $"Avalonia did not apply {scale:P0} render scaling.");
            Assert.AreEqual(minimumWidth, window.ClientSize.Width, 1, $"Client width changed at {scale:P0} scaling.");
            Assert.AreEqual(minimumHeight, window.ClientSize.Height, 1, $"Client height changed at {scale:P0} scaling.");

            foreach (var control in window.GetVisualDescendants().OfType<Control>())
            {
                Assert.IsTrue(double.IsFinite(control.Bounds.X) && double.IsFinite(control.Bounds.Y) &&
                              double.IsFinite(control.Bounds.Width) && double.IsFinite(control.Bounds.Height),
                    $"{control.GetType().Name} has invalid layout bounds at {scale:P0} scaling: {control.Bounds}.");
                Assert.IsTrue(control.Bounds.Width >= 0 && control.Bounds.Height >= 0,
                    $"{control.GetType().Name} has a negative layout size at {scale:P0} scaling: {control.Bounds}.");
            }

            using var frame = window.CaptureRenderedFrame()
                ?? throw new AssertFailedException("Avalonia did not capture a rendered frame.");
            Assert.AreEqual((int)(minimumWidth * scale), frame.PixelSize.Width,
                $"Unexpected raster width at {scale:P0} scaling.");
            Assert.AreEqual((int)(minimumHeight * scale), frame.PixelSize.Height,
                $"Unexpected raster height at {scale:P0} scaling.");

            var snapshotDirectory = Environment.GetEnvironmentVariable("ASTRARIUM_DPI_SNAPSHOT_DIR");
            if (!string.IsNullOrWhiteSpace(snapshotDirectory))
            {
                Directory.CreateDirectory(snapshotDirectory);
                frame.Save(Path.Combine(snapshotDirectory, $"{snapshotPrefix}-{scale:0.##}.png"), PngBitmapEncoderOptions.Default);
            }
        }

        window.Close();
        Dispatcher.UIThread.RunJobs();
    }

    private static void WaitUntil(Func<bool> condition, string description)
    {
        var elapsed = System.Diagnostics.Stopwatch.StartNew();
        while (!condition() && elapsed.Elapsed < TimeSpan.FromSeconds(30))
        {
            Dispatcher.UIThread.RunJobs();
            Thread.Sleep(TimeSpan.FromMilliseconds(10));
        }

        Assert.IsTrue(condition(), $"Timed out while waiting for {description} after {elapsed.Elapsed}.");
    }
}
