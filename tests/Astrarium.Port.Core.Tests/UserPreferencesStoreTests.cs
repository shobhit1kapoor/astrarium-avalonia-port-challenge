using System.Text.Json;
using Astrarium.Port.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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
            ShowBodies: false)
        {
            ShowDeepSky = false
        };

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
            Assert.AreEqual(expected.ShowDeepSky, actual.ShowDeepSky);
        }
        finally
        {
            DeletePreferencesDirectory(path);
        }
    }

    [TestMethod]
    public void LoadDefaultsDeepSkyLayerToVisibleForExistingPreferences()
    {
        var path = NewPreferencesPath();

        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllText(path, """
                {
                  "SchemaVersion": 1,
                  "Location": { "Name": "Greenwich", "LatitudeDegrees": 51.4779, "LongitudeEastDegrees": 0.0 },
                  "InstantUtc": "2026-10-17T21:00:00Z",
                  "ShowStars": true,
                  "ShowConstellations": true,
                  "ShowBodies": true
                }
                """);

            var preferences = new UserPreferencesStore(path).Load();

            Assert.IsNotNull(preferences);
            Assert.IsTrue(preferences.ShowDeepSky);
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

    [TestMethod]
    public void LoadIgnoresOversizedPreferences()
    {
        var path = NewPreferencesPath();

        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllText(path, new string(' ', 64 * 1024 + 1));

            Assert.IsNull(new UserPreferencesStore(path).Load());
        }
        finally
        {
            DeletePreferencesDirectory(path);
        }
    }

    [TestMethod]
    public void SaveRejectsOversizedObserverNameWithoutReplacingExistingPreferences()
    {
        var path = NewPreferencesPath();

        try
        {
            var store = new UserPreferencesStore(path);
            var original = new UserPreferences(
                UserPreferences.CurrentSchemaVersion,
                ObservingLocation.Greenwich,
                DateTimeOffset.UtcNow,
                true,
                true,
                true);
            store.Save(original);
            var originalBytes = File.ReadAllBytes(path);
            var oversized = original with
            {
                Location = new ObservingLocation(new string('x', 64 * 1024 + 1), 0, 0)
            };

            Assert.ThrowsExactly<InvalidDataException>(() => store.Save(oversized));
            CollectionAssert.AreEqual(originalBytes, File.ReadAllBytes(path));
        }
        finally
        {
            DeletePreferencesDirectory(path);
        }
    }

    [TestMethod]
    public void LoadIgnoresNullLocation()
    {
        var path = NewPreferencesPath();

        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllText(path, """
                {
                  "SchemaVersion": 1,
                  "Location": null,
                  "InstantUtc": "2026-10-17T21:00:00Z",
                  "ShowStars": true,
                  "ShowConstellations": true,
                  "ShowBodies": true
                }
                """);

            Assert.IsNull(new UserPreferencesStore(path).Load());
        }
        finally
        {
            DeletePreferencesDirectory(path);
        }
    }

    [TestMethod]
    public void LoadIgnoresPreferencesWithMissingRequiredFields()
    {
        var path = NewPreferencesPath();

        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllText(path, """
                {
                  "SchemaVersion": 1,
                  "Location": { "Name": "Greenwich", "LatitudeDegrees": 51.4779, "LongitudeEastDegrees": 0.0 },
                  "InstantUtc": "2026-10-17T21:00:00Z",
                  "ShowStars": true,
                  "ShowConstellations": true
                }
                """);
            Assert.IsNull(new UserPreferencesStore(path).Load());

            File.WriteAllText(path, """
                {
                  "SchemaVersion": 1,
                  "Location": { "Name": "Greenwich", "LongitudeEastDegrees": 0.0 },
                  "InstantUtc": "2026-10-17T21:00:00Z",
                  "ShowStars": true,
                  "ShowConstellations": true,
                  "ShowBodies": true
                }
                """);
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
