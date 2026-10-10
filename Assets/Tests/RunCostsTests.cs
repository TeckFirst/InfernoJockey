using System;
using NUnit.Framework;

namespace InfernoJockey
{
    public class RunCostsTests
    {
        [Test]
        public void Constants_ArePositiveDesignerData()
        {
            Assert.AreEqual(100, RunCosts.DroneCost);
            Assert.AreEqual(10, RunCosts.FuelUnitCost);
            Assert.AreEqual(5, RunCosts.LaserCoreUnitCost);

            Assert.Greater(RunCosts.DroneCost, 0);
            Assert.Greater(RunCosts.FuelUnitCost, 0);
            Assert.Greater(RunCosts.LaserCoreUnitCost, 0);
            Assert.Greater(RunCosts.DroneCost, RunCosts.FuelUnitCost);
            Assert.Greater(RunCosts.FuelUnitCost, RunCosts.LaserCoreUnitCost);
        }

        [Test]
        public void Total_IsDronePlusFuelAndLaserCores()
        {
            Assert.AreEqual(RunCosts.DroneCost, RunCosts.Total(0, 0));
            Assert.AreEqual(RunCosts.DroneCost + RunCosts.FuelUnitCost, RunCosts.Total(1, 0));
            Assert.AreEqual(RunCosts.DroneCost + RunCosts.LaserCoreUnitCost, RunCosts.Total(0, 1));
            Assert.AreEqual(RunCosts.DroneCost + 3 * RunCosts.FuelUnitCost + 2 * RunCosts.LaserCoreUnitCost, RunCosts.Total(3, 2));
        }

        [Test]
        public void Total_ScalesWithUnitsAndNeverGoesNegative()
        {
            int previous = RunCosts.Total(0, 0);

            for (int fuel = 0; fuel <= 10; fuel++)
            {
                for (int cores = 0; cores <= 10; cores++)
                {
                    int total = RunCosts.Total(fuel, cores);
                    Assert.GreaterOrEqual(total, RunCosts.DroneCost);
                    Assert.GreaterOrEqual(total, previous);
                    Assert.AreEqual(
                        RunCosts.DroneCost + fuel * RunCosts.FuelUnitCost + cores * RunCosts.LaserCoreUnitCost,
                        total);
                }
            }
        }

        [Test]
        public void Total_RejectsNegativeUnits()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => RunCosts.Total(-1, 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => RunCosts.Total(0, -1));
            Assert.Throws<ArgumentOutOfRangeException>(() => RunCosts.Total(-1, -1));
            Assert.Throws<ArgumentOutOfRangeException>(() => RunCosts.Total(5, -3));
        }
    }
}
