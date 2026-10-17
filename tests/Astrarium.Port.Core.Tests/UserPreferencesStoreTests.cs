using Astrarium.Port.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text.Json;

namespace Astrarium.Port.Core.Tests;

[TestClass]
public sealed class UserPreferencesStoreTests
{
    [TestMethod]
    public void SaveAndLoadPreservesObserverTimeAndLayers()
    {
        var path = NewPreferencesPath();
        var expected = new UserPreferences(
            UserPreferences.CurrentSchemaVersion,
            new ObservingLocation("Mauna Kea", 19.8207, -155.4681, 4205),
            new DateTimeOffset(2026, 10, 17, 21, 30, 0, TimeSpan.FromHours(-7)),
            ShowStars: false,
            ShowConstellations: true,
            ShowBodies: false);

        try
        {
            var store = new UserPreferencesStore(path);
            store.Save(expected);

            var actual = store.Load();

            Assert.IsNotNull(actual);
            Assert.AreEqual(expected.Location, actual.Location);
            Assert.AreEqual(expected.InstantUtc.ToUniversalTime(), actual.InstantUtc);
            Assert.AreEqual(expected.ShowStars, actual.ShowStars);
            Assert.AreEqual(expected.ShowConstellations, actual.ShowConstellations);
            Assert.AreEqual(expected.ShowBodies, actual.ShowBodies);
        }
        finally
        {
            DeletePreferencesDirectory(path);
        }
    }

    [TestMethod]
    public void LoadIgnoresMalformedOrUnsupportedPreferences()
    {
        var path = NewPreferencesPath();

        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllText(path, "{ definitely not valid JSON }");
            Assert.IsNull(new UserPreferencesStore(path).Load());

            var unsupported = new UserPreferences(99, ObservingLocation.Greenwich, DateTimeOffset.UtcNow, true, true, true);
            File.WriteAllText(path, JsonSerializer.Serialize(unsupported));
            Assert.IsNull(new UserPreferencesStore(path).Load());
        }
        finally
        {
            DeletePreferencesDirectory(path);
        }
    }

    private static string NewPreferencesPath() =>
        Path.Combine(Path.GetTempPath(), $"astrarium-preferences-tests-{Guid.NewGuid():N}", "preferences.json");

    private static void DeletePreferencesDirectory(string path)
    {
        var directory = Path.GetDirectoryName(path);
        if (directory is not null && Directory.Exists(directory))
            Directory.Delete(directory, recursive: true);
    }
}
