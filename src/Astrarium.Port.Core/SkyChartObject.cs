namespace Astrarium.Port.Core;

public enum SkyObjectKind
{
    Star,
    Sun,
    Moon,
    Planet
}

public sealed record SkyChartObject(
    string Id,
    string Name,
    SkyObjectKind Kind,
    double RightAscensionDegrees,
    double DeclinationDegrees,
    double AzimuthDegrees,
    double AltitudeDegrees,
    double Magnitude,
    double? ColorIndex = null,
    string? Designation = null,
    string? Constellation = null);
