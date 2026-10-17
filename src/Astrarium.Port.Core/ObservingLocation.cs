using System.Text.Json.Serialization;

namespace Astrarium.Port.Core;

/// <summary>
/// A geographic observing site. Longitude is stored positive east, following ISO 6709.
/// </summary>
public sealed record ObservingLocation(
    [property: JsonRequired] string Name,
    [property: JsonRequired] double LatitudeDegrees,
    [property: JsonRequired] double LongitudeEastDegrees,
    double ElevationMeters = 0)
{
    public static ObservingLocation Greenwich { get; } = new("Greenwich", 51.4779, 0.0, 46);

    public void Validate()
    {
        if (!double.IsFinite(LatitudeDegrees) || LatitudeDegrees is < -90 or > 90)
            throw new ArgumentOutOfRangeException(nameof(LatitudeDegrees), "Latitude must be between -90° and +90°.");
        if (!double.IsFinite(LongitudeEastDegrees) || LongitudeEastDegrees is < -180 or > 180)
            throw new ArgumentOutOfRangeException(nameof(LongitudeEastDegrees), "Longitude must be between -180° and +180°.");
        if (!double.IsFinite(ElevationMeters) || ElevationMeters is < -500 or > 10000)
            throw new ArgumentOutOfRangeException(nameof(ElevationMeters), "Elevation must be between -500 m and 10,000 m.");
        if (string.IsNullOrWhiteSpace(Name))
            throw new ArgumentException("A location name is required.", nameof(Name));
    }
}
