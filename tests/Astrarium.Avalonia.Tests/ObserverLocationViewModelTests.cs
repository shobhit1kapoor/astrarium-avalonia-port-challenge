using Astrarium.Avalonia.ViewModels;
using Astrarium.Port.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Astrarium.Avalonia.Tests;

[TestClass]
public sealed class ObserverLocationViewModelTests
{
    [TestMethod]
    public async Task ApplyingCustomSiteKeepsElevationInSceneAndPreferences()
    {
        var preferences = new MemoryPreferencesStore();
        var viewModel = new MainViewModel(new SkySceneCalculator(StarCatalog.LoadBundled()), preferences);
        viewModel.LocationName = "Mauna Kea";
        viewModel.LatitudeText = "19.8207";
        viewModel.LongitudeText = "-155.4681";
        viewModel.ElevationText = "4205";

        await viewModel.ApplyLocationCommand.ExecuteAsync(null);

        Assert.AreEqual(4205, viewModel.CurrentLocation.ElevationMeters);
        Assert.AreEqual(4205, viewModel.Scene!.Location.ElevationMeters);

        viewModel.FlushPreferences();

        Assert.AreEqual(4205, preferences.Saved!.Location.ElevationMeters);
    }

    [TestMethod]
    public async Task InvalidElevationDoesNotReplaceCurrentSite()
    {
        var viewModel = new MainViewModel(new SkySceneCalculator(StarCatalog.LoadBundled()), new MemoryPreferencesStore());
        viewModel.LocationName = "Too high";
        viewModel.LatitudeText = "19.8207";
        viewModel.LongitudeText = "-155.4681";
        viewModel.ElevationText = "10001";

        await WaitUntilIdleAsync(viewModel);
        await viewModel.ApplyLocationCommand.ExecuteAsync(null);

        Assert.AreEqual(ObservingLocation.Greenwich, viewModel.CurrentLocation);
        Assert.Contains("Elevation must be between -500 m and 10,000 m", viewModel.StatusMessage);
    }

    private static async Task WaitUntilIdleAsync(MainViewModel viewModel)
    {
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        while (viewModel.IsBusy)
            await Task.Delay(10, timeout.Token);
    }

    private sealed class MemoryPreferencesStore : IUserPreferencesStore
    {
        public UserPreferences? Saved { get; private set; }

        public UserPreferences? Load() => Saved;

        public void Save(UserPreferences preferences) => Saved = preferences;
    }
}
