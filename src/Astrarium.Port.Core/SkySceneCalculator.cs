using Astrarium.Algorithms;

namespace Astrarium.Port.Core;

/// <summary>
/// Produces a topocentric sky scene using Astrarium's VSOP87, ELP2000, coordinate,
/// precession, and visibility calculations.
/// </summary>
public sealed class SkySceneCalculator
{
    private readonly StarCatalog _catalog;
    private readonly DeepSkyCatalog _deepSkyCatalog;

    private const int Earth = 3;
    private static readonly (int Number, string Name)[] Planets =
    [
        (1, "Mercury"), (2, "Venus"), (4, "Mars"), (5, "Jupiter"),
        (6, "Saturn"), (7, "Uranus"), (8, "Neptune")
    ];

    public SkySceneCalculator(StarCatalog catalog)
        : this(catalog, DeepSkyCatalog.LoadBundled())
    {
    }

    public SkySceneCalculator(StarCatalog catalog, DeepSkyCatalog deepSkyCatalog)
    {
        _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
        _deepSkyCatalog = deepSkyCatalog ?? throw new ArgumentNullException(nameof(deepSkyCatalog));
    }

    public SkyScene Calculate(DateTimeOffset instant, ObservingLocation location)
    {
        ArgumentNullException.ThrowIfNull(location);
        location.Validate();

        var utc = instant.ToUniversalTime();
        var jd = new Date(utc.UtcDateTime).ToJulianDay();
        var jde = jd + Date.DeltaT(jd) / 86400.0;
        var nutation = Nutation.NutationElements(jd);
        var obliquity = Date.TrueObliquity(jd, nutation.deltaEpsilon);
        var apparentSiderealTime = Date.ApparentSiderealTime(jd, nutation.deltaPsi, obliquity);
        var observer = new CrdsGeographical(
            -location.LongitudeEastDegrees,
            location.LatitudeDegrees,
            elevation: location.ElevationMeters,
            name: location.Name);
        var precession = Precession.ElementsFK5(Date.EPOCH_J2000, jd);

        var objects = new List<SkyChartObject>(_catalog.Stars.Count + _deepSkyCatalog.Objects.Count + 9);
        AddStars(objects, jd, apparentSiderealTime, observer, _catalog, precession);
        AddSun(objects, jde, obliquity, nutation, apparentSiderealTime, observer);
        AddMoon(objects, jd, obliquity, nutation, apparentSiderealTime, observer);
        AddPlanets(objects, jde, obliquity, nutation, apparentSiderealTime, observer);
        AddDeepSky(objects, apparentSiderealTime, observer, _deepSkyCatalog, precession);

        return new SkyScene(utc, location, objects, apparentSiderealTime);
    }

    private static void AddStars(
        List<SkyChartObject> objects,
        double jd,
        double siderealTime,
        CrdsGeographical observer,
        StarCatalog catalog,
        PrecessionalElements precession)
    {
        var yearsSinceJ2000 = (jd - Date.EPOCH_J2000) / 365.25;

        foreach (var star in catalog.Stars)
        {
            var position = star.PositionParsecsJ2000;
            var velocity = star.VelocityParsecsPerYear;
            var x = position[0] + velocity[0] * yearsSinceJ2000;
            var y = position[1] + velocity[1] * yearsSinceJ2000;
            var z = position[2] + velocity[2] * yearsSinceJ2000;
            var radius = Math.Sqrt(x * x + y * y + z * z);

            CrdsEquatorial equatorial;
            if (radius > 0 && double.IsFinite(radius))
            {
                equatorial = new CrdsEquatorial(
                    Angle.To360(Angle.ToDegrees(Math.Atan2(y, x))),
                    Angle.ToDegrees(Math.Asin(Math.Clamp(z / radius, -1.0, 1.0))));
            }
            else
            {
                equatorial = new CrdsEquatorial(star.RaDegreesJ2000, star.DecDegreesJ2000);
            }

            equatorial = Precession.GetEquatorialCoordinates(equatorial, precession);
            var horizontal = equatorial.ToHorizontal(observer, siderealTime);
            var id = !string.IsNullOrWhiteSpace(star.Hip) ? $"hip:{star.Hip}" : $"hyg:{star.Id}";

            objects.Add(new SkyChartObject(
                id,
                star.DisplayName,
                SkyObjectKind.Star,
                equatorial.Alpha,
                equatorial.Delta,
                horizontal.Azimuth,
                horizontal.Altitude,
                star.Magnitude,
                star.ColorIndex,
                star.Designation,
                star.Constellation));
        }
    }

    private static void AddDeepSky(
        List<SkyChartObject> objects,
        double siderealTime,
        CrdsGeographical observer,
        DeepSkyCatalog catalog,
        PrecessionalElements precession)
    {
        foreach (var item in catalog.Objects)
        {
            var j2000 = new CrdsEquatorial(item.RightAscensionDegreesJ2000, item.DeclinationDegreesJ2000);
            var equatorial = Precession.GetEquatorialCoordinates(j2000, precession);
            var horizontal = equatorial.ToHorizontal(observer, siderealTime);
            objects.Add(new SkyChartObject(
                item.Id,
                item.Name,
                SkyObjectKind.DeepSky,
                equatorial.Alpha,
                equatorial.Delta,
                horizontal.Azimuth,
                horizontal.Altitude,
                item.Magnitude,
                Designation: item.Designation,
                Constellation: item.Constellation,
                ObjectType: item.ObjectType,
                MajorAxisArcMinutes: item.MajorAxisArcMinutes,
                MinorAxisArcMinutes: item.MinorAxisArcMinutes));
        }
    }

    private static void AddSun(
        List<SkyChartObject> objects,
        double jde,
        double obliquity,
        NutationElements nutation,
        double siderealTime,
        CrdsGeographical observer)
    {
        var earth = PlanetPositions.GetPlanetCoordinates(Earth, jde, highPrecision: true);
        var ecliptical = new CrdsEcliptical(Angle.To360(earth.L + 180), -earth.B, earth.R);
        ecliptical += PlanetPositions.CorrectionForFK5(jde, ecliptical);
        ecliptical += Nutation.NutationEffect(nutation.deltaPsi);
        ecliptical += Aberration.AberrationEffect(ecliptical.Distance);

        var equatorial = ecliptical.ToEquatorial(obliquity);
        var topocentric = equatorial.ToTopocentric(observer, siderealTime, SolarEphem.Parallax(ecliptical.Distance));
        var horizontal = topocentric.ToHorizontal(observer, siderealTime);
        objects.Add(new SkyChartObject("sun", "Sun", SkyObjectKind.Sun, topocentric.Alpha, topocentric.Delta,
            horizontal.Azimuth, horizontal.Altitude, -26.74));
    }

    private static void AddMoon(
        List<SkyChartObject> objects,
        double jd,
        double obliquity,
        NutationElements nutation,
        double siderealTime,
        CrdsGeographical observer)
    {
        var ecliptical = LunarMotion.GetCoordinates(jd);
        ecliptical += Nutation.NutationEffect(nutation.deltaPsi);
        var equatorial = ecliptical.ToEquatorial(obliquity);
        var topocentric = equatorial.ToTopocentric(observer, siderealTime, LunarEphem.Parallax(ecliptical.Distance));
        var horizontal = topocentric.ToHorizontal(observer, siderealTime);
        var sun = SolarEphem.Ecliptical(jd);
        var elongation = BasicEphem.Elongation(sun, ecliptical);
        var phaseAngle = BasicEphem.PhaseAngle(elongation, sun.Distance * 149597871.0, ecliptical.Distance);
        var phase = BasicEphem.Phase(phaseAngle);
        var magnitude = -12.7 - 2.5 * Math.Log10(Math.Max(phase, 0.001));

        objects.Add(new SkyChartObject("moon", "Moon", SkyObjectKind.Moon, topocentric.Alpha, topocentric.Delta,
            horizontal.Azimuth, horizontal.Altitude, magnitude));
    }

    private static void AddPlanets(
        List<SkyChartObject> objects,
        double jde,
        double obliquity,
        NutationElements nutation,
        double siderealTime,
        CrdsGeographical observer)
    {
        var earth = PlanetPositions.GetPlanetCoordinates(Earth, jde, highPrecision: true);
        var sun = new CrdsEcliptical(Angle.To360(earth.L + 180), -earth.B, earth.R);
        sun += PlanetPositions.CorrectionForFK5(jde, sun);
        sun += Nutation.NutationEffect(nutation.deltaPsi);
        sun += Aberration.AberrationEffect(sun.Distance);

        foreach (var (planetNumber, name) in Planets)
        {
            var lightTime = 0.0;
            var lightTimeDifference = double.PositiveInfinity;
            CrdsHeliocentrical heliocentric;
            CrdsEcliptical ecliptical;

            do
            {
                heliocentric = PlanetPositions.GetPlanetCoordinates(planetNumber, jde - lightTime, highPrecision: true);
                ecliptical = heliocentric.ToRectangular(earth).ToEcliptical();
                var updatedLightTime = PlanetPositions.LightTimeEffect(ecliptical.Distance);
                lightTimeDifference = Math.Abs(lightTime - updatedLightTime);
                lightTime = updatedLightTime;
            } while (lightTimeDifference > TimeSpan.FromSeconds(1).TotalDays);

            ecliptical += PlanetPositions.CorrectionForFK5(jde, ecliptical);
            ecliptical += Nutation.NutationEffect(nutation.deltaPsi);
            var geocentric = ecliptical.ToEquatorial(obliquity);
            var topocentric = geocentric.ToTopocentric(observer, siderealTime, PlanetEphem.Parallax(ecliptical.Distance));
            var horizontal = topocentric.ToHorizontal(observer, siderealTime);
            var elongation = BasicEphem.Elongation(sun, ecliptical);
            var phaseAngle = BasicEphem.PhaseAngle(elongation, sun.Distance, ecliptical.Distance);
            var magnitude = PlanetEphem.Magnitude(planetNumber, ecliptical.Distance, heliocentric.R, phaseAngle);

            objects.Add(new SkyChartObject($"planet:{planetNumber}", name, SkyObjectKind.Planet,
                topocentric.Alpha, topocentric.Delta, horizontal.Azimuth, horizontal.Altitude, magnitude));
        }
    }
}
