using Astrarium.Port.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Astrarium.Port.Core.Tests;

[TestClass]
public sealed class ObservationPlannerTests
{
    [TestMethod]
    public void BuildFindsAstronomicalDarknessAndQualifyingTargets()
    {
        var planner = CreatePlanner();
        var start = new DateTimeOffset(2026, 10, 17, 16, 0, 0, TimeSpan.Zero);
        var progress = new RecordingProgress();
        var plan = planner.Build(start, start.AddHours(16), ObservingLocation.Greenwich,
            new ObservationPlanOptions { SamplingInterval = TimeSpan.FromMinutes(15) }, progress);

        Assert.HasCount(1, plan.DarkWindows);
        Assert.IsTrue(plan.DarkWindows[0].Duration > TimeSpan.FromHours(8));
        Assert.IsTrue(plan.Targets.Any(target => target.Target.Id == "hip:32349"));
        Assert.IsTrue(plan.Targets.Any(target => target.Target.Kind == SkyObjectKind.DeepSky),
            "Expected the bright deep-sky catalog to contribute qualifying observing targets.");
        Assert.HasCount(250, plan.Targets);
        Assert.AreEqual(100d, progress.Values[^1]);
        Assert.IsTrue(progress.Values.Zip(progress.Values.Skip(1), (first, second) => second >= first).All(isOrdered => isOrdered));
        Assert.IsTrue(plan.Targets.All(target =>
            target.Duration >= TimeSpan.FromMinutes(30) &&
            target.BestAltitudeDegrees >= 20 &&
            target.Target.Magnitude <= 5));
        Assert.IsTrue(plan.Targets.All(target => target.BestTimeUtc >= target.WindowStartUtc && target.BestTimeUtc <= target.WindowEndUtc));
    }

    [TestMethod]
    public void BuildReportsNoAstronomicalDarknessDuringArcticMidsummer()
    {
        var planner = CreatePlanner();
        var start = new DateTimeOffset(2026, 6, 20, 0, 0, 0, TimeSpan.Zero);
        var location = new ObservingLocation("Longyearbyen", 78.2232, 15.6469, 10);
        var plan = planner.Build(start, start.AddHours(24), location,
            new ObservationPlanOptions { SamplingInterval = TimeSpan.FromMinutes(30) });

        Assert.IsEmpty(plan.DarkWindows);
        Assert.IsEmpty(plan.Targets);
    }

    [TestMethod]
    public void BuildFindsSouthernWinterTargetsAndNormalizesLocalInputToUtc()
    {
        var planner = CreatePlanner();
        var location = new ObservingLocation("Sydney", -33.8688, 151.2093, 58);
        var localStart = new DateTimeOffset(2026, 6, 20, 15, 0, 0, TimeSpan.FromHours(10));
        var localEnd = new DateTimeOffset(2026, 6, 21, 10, 0, 0, TimeSpan.FromHours(10));
        var options = new ObservationPlanOptions { SamplingInterval = TimeSpan.FromMinutes(15) };

        var plan = planner.Build(localStart, localEnd, location, options);

        Assert.AreEqual(TimeSpan.Zero, plan.StartUtc.Offset);
        Assert.AreEqual(TimeSpan.Zero, plan.EndUtc.Offset);
        Assert.AreEqual(new DateTimeOffset(2026, 6, 20, 5, 0, 0, TimeSpan.Zero), plan.StartUtc);
        Assert.AreEqual(new DateTimeOffset(2026, 6, 21, 0, 0, 0, TimeSpan.Zero), plan.EndUtc);
        Assert.HasCount(1, plan.DarkWindows);
        Assert.IsTrue(plan.Targets.Any(target => target.Target.Name == "Antares"),
            "Expected a prominent southern winter target in Sydney's winter plan.");
        Assert.IsTrue(plan.Targets.All(target =>
            target.Duration >= TimeSpan.FromMinutes(30) &&
            target.BestAltitudeDegrees >= options.MinimumAltitudeDegrees &&
            target.Target.Magnitude <= options.MaximumMagnitude));

        var calculator = new SkySceneCalculator(StarCatalog.LoadBundled());
        foreach (var window in plan.DarkWindows)
        {
            foreach (var edge in new[] { window.StartUtc, window.EndUtc })
            {
                var sunAltitude = calculator.Calculate(edge, location).Objects
                    .Single(item => item.Kind == SkyObjectKind.Sun).AltitudeDegrees;
                Assert.AreEqual(options.MaximumSunAltitudeDegrees, sunAltitude, 0.25);
            }
        }
    }

    [TestMethod]
    public void BuildRejectsInvalidRangesAndPlannerSettings()
    {
        var planner = CreatePlanner();
        var start = new DateTimeOffset(2026, 10, 17, 18, 0, 0, TimeSpan.Zero);

        Assert.ThrowsExactly<ArgumentException>(() => planner.Build(start, start, ObservingLocation.Greenwich));
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => planner.Build(start, start.AddHours(49), ObservingLocation.Greenwich));
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => planner.Build(start, start.AddHours(12), ObservingLocation.Greenwich,
            new ObservationPlanOptions { MinimumAltitudeDegrees = 91 }));

        using var cancelled = new CancellationTokenSource();
        cancelled.Cancel();
        Assert.ThrowsExactly<OperationCanceledException>(() => planner.Build(start, start.AddHours(12), ObservingLocation.Greenwich,
            cancellationToken: cancelled.Token));
    }

    private static ObservationPlanner CreatePlanner() => new(new SkySceneCalculator(StarCatalog.LoadBundled()));

    private sealed class RecordingProgress : IProgress<double>
    {
        public List<double> Values { get; } = [];

        public void Report(double value) => Values.Add(value);
    }
}
