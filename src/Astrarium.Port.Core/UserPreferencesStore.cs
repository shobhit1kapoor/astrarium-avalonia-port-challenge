using System.Text;
using System.Text.Json;

namespace Astrarium.Port.Core;

/// <summary>Loads and stores the observer settings used by the application.</summary>
public interface IUserPreferencesStore
{
    UserPreferences? Load();
    void Save(UserPreferences preferences);
}

/// <summary>
/// Reads and atomically writes small per-user settings in the platform's local application data folder.
/// </summary>
public sealed class UserPreferencesStore : IUserPreferencesStore
{
    private const long MaximumFileSizeBytes = 64 * 1024;
    private readonly string _path;

    public UserPreferencesStore(string? path = null)
    {
        if (!string.IsNullOrWhiteSpace(path))
        {
            _path = Path.GetFullPath(path);
            return;
        }

        var localData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        if (string.IsNullOrWhiteSpace(localData))
            throw new InvalidOperationException("The local application data folder is unavailable.");

        _path = Path.Combine(localData, "Astrarium", "preferences.json");
    }

    public UserPreferences? Load()
    {
        if (!File.Exists(_path))
            return null;

        try
        {
            using var stream = File.OpenRead(_path);
            if (stream.Length > MaximumFileSizeBytes)
                return null;

            var preferences = JsonSerializer.Deserialize(stream, AstrariumJsonContext.Default.UserPreferences);
            if (preferences is null)
                return null;

            preferences.Validate();
            return preferences with { InstantUtc = preferences.InstantUtc.ToUniversalTime() };
        }
        catch (Exception error) when (error is IOException or UnauthorizedAccessException or JsonException or
                                     ArgumentException or NotSupportedException or System.Security.SecurityException)
        {
            return null;
        }
    }

    public void Save(UserPreferences preferences)
    {
        ArgumentNullException.ThrowIfNull(preferences);
        preferences.Validate();
        if (Encoding.UTF8.GetByteCount(preferences.Location.Name) > MaximumFileSizeBytes)
            throw new InvalidDataException("The observer name exceeds the supported preferences file size.");

        var directory = Path.GetDirectoryName(_path)
            ?? throw new InvalidOperationException("The preferences path has no parent folder.");
        Directory.CreateDirectory(directory);

        var temporaryPath = $"{_path}.{Guid.NewGuid():N}.tmp";
        try
        {
            using (var stream = new FileStream(temporaryPath, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            {
                JsonSerializer.Serialize(stream,
                    preferences with { InstantUtc = preferences.InstantUtc.ToUniversalTime() },
                    AstrariumJsonContext.Default.UserPreferences);
                if (stream.Length > MaximumFileSizeBytes)
                    throw new InvalidDataException("The preferences exceed the supported file size.");

                stream.Flush(flushToDisk: true);
            }

            File.Move(temporaryPath, _path, overwrite: true);
        }
        finally
        {
            if (File.Exists(temporaryPath))
                File.Delete(temporaryPath);
        }
    }
}
