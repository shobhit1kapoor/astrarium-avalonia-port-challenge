namespace Astrarium.Port.Core;

public sealed record SkyScene(
    DateTimeOffset Instant,
    ObservingLocation Location,
    IReadOnlyList<SkyChartObject> Objects,
    double LocalSiderealTimeDegrees)
{
    public IEnumerable<SkyChartObject> AboveHorizon => Objects.Where(item => item.AltitudeDegrees >= 0);
}
