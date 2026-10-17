using Astrarium.Port.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Astrarium.Port.Core.Tests;

[TestClass]
public sealed class SkySceneCalculatorTests
{
    private static readonly DateTimeOffset SampleInstant = new(2026, 10, 17, 21, 0, 0, TimeSpan.Zero);

    [TestMethod]
    public void BundledCatalogContainsThePinnedNakedEyeSubset()
    {
        var catalog = StarCatalog.LoadBundled();

        Assert.HasCount(5070, catalog.Stars);
        Assert.IsTrue(catalog.Stars.Any(star => star.Proper == "Sirius"));
        Assert.IsTrue(catalog.Stars.All(star => star.Magnitude <= 6.0));
    }

    [TestMethod]
    public void BundledDeepSkyCatalogContainsBrightNamedTargetsAndMessierAliases()
    {
        var catalog = DeepSkyCatalog.LoadBundled();

        Assert.IsGreaterThanOrEqualTo(500, catalog.Objects.Count);
        var andromeda = catalog.Objects.Single(item => item.Id == "openngc:NGC0224");
        Assert.AreEqual("Andromeda Galaxy", andromeda.Name);
        Assert.Contains("M 31", andromeda.Designation);
        Assert.Contains("M31", andromeda.Designation);
        Assert.Contains("NGC224", andromeda.Designation);
        Assert.AreEqual(10.6847917, andromeda.RightAscensionDegreesJ2000, 1e-6);
        Assert.IsTrue(catalog.Objects.Any(item => item.Designation.Contains("M 45", StringComparison.Ordinal)));
    }

    [TestMethod]
    public void CalculationProducesStarsSunMoonAndAllSevenPlanets()
    {
        var scene = CreateCalculator().Calculate(SampleInstant, ObservingLocation.Greenwich);

        Assert.AreEqual(5070, scene.Objects.Count(item => item.Kind == SkyObjectKind.Star));
        Assert.AreEqual(1, scene.Objects.Count(item => item.Kind == SkyObjectKind.Sun));
        Assert.AreEqual(1, scene.Objects.Count(item => item.Kind == SkyObjectKind.Moon));
        Assert.AreEqual(7, scene.Objects.Count(item => item.Kind == SkyObjectKind.Planet));
        Assert.AreEqual(DeepSkyCatalog.LoadBundled().Objects.Count, scene.Objects.Count(item => item.Kind == SkyObjectKind.DeepSky));
        Assert.IsTrue(scene.Objects.Any(item => item.Id == "openngc:NGC0224" && item.Kind == SkyObjectKind.DeepSky));
        Assert.IsTrue(scene.Objects.All(IsFiniteAndInRange));
    }

    [TestMethod]
    public void ObserverLocationChangesLocalHorizontalCoordinates()
    {
        var calculator = CreateCalculator();
        var greenwich = calculator.Calculate(SampleInstant, ObservingLocation.Greenwich);
        var sydney = calculator.Calculate(SampleInstant, new ObservingLocation("Sydney", -33.8688, 151.2093, 58));
        var greenwichSirius = greenwich.Objects.Single(item => item.Id == "hip:32349");
        var sydneySirius = sydney.Objects.Single(item => item.Id == "hip:32349");

        Assert.AreEqual(greenwichSirius.RightAscensionDegrees, sydneySirius.RightAscensionDegrees, 1e-9);
        Assert.AreEqual(greenwichSirius.DeclinationDegrees, sydneySirius.DeclinationDegrees, 1e-9);
        Assert.IsGreaterThan(5, Math.Abs(greenwichSirius.AltitudeDegrees - sydneySirius.AltitudeDegrees));
        Assert.IsGreaterThan(5, Math.Abs(greenwichSirius.AzimuthDegrees - sydneySirius.AzimuthDegrees));
    }

    [TestMethod]
    public void ChangingTimeChangesPlanetPositions()
    {
        var calculator = CreateCalculator();
        var first = calculator.Calculate(SampleInstant, ObservingLocation.Greenwich);
        var nextNight = calculator.Calculate(SampleInstant.AddDays(1), ObservingLocation.Greenwich);
        var firstMars = first.Objects.Single(item => item.Id == "planet:4");
        var nextMars = nextNight.Objects.Single(item => item.Id == "planet:4");

        Assert.IsGreaterThan(0.001, Math.Abs(firstMars.RightAscensionDegrees - nextMars.RightAscensionDegrees));
    }

    [TestMethod]
    public void InvalidObserverCoordinatesAreRejected()
    {
        var location = new ObservingLocation("Outside Earth", 90.1, 0);

        Assert.ThrowsExactly<ArgumentOutOfRangeException>(location.Validate);
    }

    private static SkySceneCalculator CreateCalculator() => new(StarCatalog.LoadBundled());

    private static bool IsFiniteAndInRange(SkyChartObject item) =>
        double.IsFinite(item.RightAscensionDegrees) && item.RightAscensionDegrees is >= 0 and < 360 &&
        double.IsFinite(item.DeclinationDegrees) && item.DeclinationDegrees is >= -90 and <= 90 &&
        double.IsFinite(item.AzimuthDegrees) && item.AzimuthDegrees is >= 0 and < 360 &&
        double.IsFinite(item.AltitudeDegrees) && item.AltitudeDegrees is >= -90 and <= 90 &&
        double.IsFinite(item.Magnitude);
}
