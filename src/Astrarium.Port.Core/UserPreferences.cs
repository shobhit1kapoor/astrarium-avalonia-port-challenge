using System.Text.Json.Serialization;

namespace Astrarium.Port.Core;

/// <summary>
/// Observer and display choices restored when Astrarium opens again.
/// </summary>
public sealed record UserPreferences(
    [property: JsonRequired] int SchemaVersion,
    [property: JsonRequired] ObservingLocation Location,
    [property: JsonRequired] DateTimeOffset InstantUtc,
    [property: JsonRequired] bool ShowStars,
    [property: JsonRequired] bool ShowConstellations,
    [property: JsonRequired] bool ShowBodies)
{
    public const int CurrentSchemaVersion = 1;

    /// <summary>Whether deep-sky objects are drawn on the chart. Older preference files default to visible.</summary>
    public bool ShowDeepSky { get; init; } = true;

    public void Validate()
    {
        if (SchemaVersion != CurrentSchemaVersion)
            throw new ArgumentOutOfRangeException(nameof(SchemaVersion), "Unsupported preferences version.");

        ArgumentNullException.ThrowIfNull(Location);
        Location.Validate();
    }
}
