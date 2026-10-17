using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Astrarium.Algorithms.Tests
{
    [TestClass]
    public class RefractionTests
    {
        [TestMethod]
        public void FlatteningOfSun()
        {
            // true solar diameter
            double sunDiameter = 32.0 / 60; 

            // true altitude of upper limb of Sun
            double h_upper = 0.5541;

            // true altitude of Sun center
            double h_center = h_upper - sunDiameter / 2;

            // Flattening derives the apparent diameter from the atmospheric
            // pressure and temperature. The second positional parameter is P,
            // not the Sun's diameter.
            double mu0 = Refraction.Flattening(h_center);

            Assert.AreEqual(0.8693, mu0, 1e-4);
        }
    }
}
