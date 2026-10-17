namespace Astrarium.Port.Core;

/// <summary>
/// Observer and display choices restored when Astrarium opens again.
/// </summary>
public sealed record UserPreferences(
    int SchemaVersion,
    ObservingLocation Location,
    DateTimeOffset InstantUtc,
    bool ShowStars,
    bool ShowConstellations,
    bool ShowBodies)
{
    public const int CurrentSchemaVersion = 1;

    public void Validate()
    {
        if (SchemaVersion != CurrentSchemaVersion)
            throw new ArgumentOutOfRangeException(nameof(SchemaVersion), "Unsupported preferences version.");

        ArgumentNullException.ThrowIfNull(Location);
        Location.Validate();
    }
}
