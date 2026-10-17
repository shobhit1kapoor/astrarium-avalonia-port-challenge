using System.Runtime.InteropServices.JavaScript;
using System.Runtime.Versioning;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Astrarium.Port.Core;

namespace Astrarium.Avalonia.Browser;

[JsonSerializable(typeof(UserPreferences))]
internal partial class BrowserJsonContext : JsonSerializerContext
{
}

[SupportedOSPlatform("browser")]
public sealed class BrowserPreferencesStore : IUserPreferencesStore
{
    private const string StorageKey = "astrarium.preferences.v1";
    private const int MaximumPayloadBytes = 64 * 1024;
    private UserPreferences? _memoryFallback;

    public UserPreferences? Load()
    {
        try
        {
            var payload = BrowserStorageInterop.Read(StorageKey);
            if (string.IsNullOrEmpty(payload) || Encoding.UTF8.GetByteCount(payload) > MaximumPayloadBytes)
                return _memoryFallback;

            var preferences = JsonSerializer.Deserialize(payload, BrowserJsonContext.Default.UserPreferences);
            if (preferences is null)
                return _memoryFallback;

            preferences.Validate();
            return preferences with { InstantUtc = preferences.InstantUtc.ToUniversalTime() };
        }
        catch (Exception exception) when (exception is JSException or JsonException or ArgumentException or
                                         NotSupportedException)
        {
            return _memoryFallback;
        }
    }

    public void Save(UserPreferences preferences)
    {
        ArgumentNullException.ThrowIfNull(preferences);
        preferences.Validate();

        var snapshot = preferences with { InstantUtc = preferences.InstantUtc.ToUniversalTime() };
        var payload = JsonSerializer.Serialize(snapshot, BrowserJsonContext.Default.UserPreferences);
        if (Encoding.UTF8.GetByteCount(payload) > MaximumPayloadBytes)
            throw new InvalidDataException("The observer preferences exceed the supported browser storage size.");

        _memoryFallback = snapshot;
        _ = BrowserStorageInterop.Write(StorageKey, payload);
    }
}

[SupportedOSPlatform("browser")]
internal static partial class BrowserStorageInterop
{
    [JSImport("read", "AstrariumStorage")]
    internal static partial string? Read(string key);

    [JSImport("write", "AstrariumStorage")]
    internal static partial bool Write(string key, string value);
}
