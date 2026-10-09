using System;
using NUnit.Framework;

namespace InfernoJockey
{
    public class SpeedMeterTests
    {
        [Test]
        public void New_StartsAtBaseBelowTheCap()
        {
            Assert.AreEqual(4, SpeedMeter.BaseSpeed);
            Assert.AreEqual(1, SpeedMeter.ExposureStep);
            Assert.AreEqual(12, SpeedMeter.Cap);
            Assert.Greater(SpeedMeter.Cap, SpeedMeter.BaseSpeed);
            Assert.Greater(SpeedMeter.ExposureStep, 0);

            SpeedMeter meter = new SpeedMeter();

            Assert.AreEqual(SpeedMeter.BaseSpeed, meter.Speed);
            Assert.IsFalse(meter.AtCap);
        }

        [Test]
        public void Expose_RisesOneStepAndLeavesThePreviousMeterAlone()
        {
            SpeedMeter start = new SpeedMeter();

            SpeedMeter exposed = start.Expose();

            Assert.AreEqual(SpeedMeter.BaseSpeed, start.Speed);
            Assert.AreEqual(SpeedMeter.BaseSpeed + SpeedMeter.ExposureStep, exposed.Speed);
            Assert.IsFalse(exposed.AtCap);
        }

        [Test]
        public void Expose_ClimbsUntilTheCapAndThenStops()
        {
            SpeedMeter meter = new SpeedMeter();
            int stepsToCap = (SpeedMeter.Cap - SpeedMeter.BaseSpeed) / SpeedMeter.ExposureStep;

            for (int i = 0; i < stepsToCap; i++)
            {
                int before = meter.Speed;
                meter = meter.Expose();
                Assert.Greater(meter.Speed, before);
                Assert.LessOrEqual(meter.Speed, SpeedMeter.Cap);
            }

            Assert.AreEqual(SpeedMeter.Cap, meter.Speed);
            Assert.IsTrue(meter.AtCap);

            SpeedMeter stillCapped = meter.Expose().Expose();

            Assert.AreEqual(SpeedMeter.Cap, stillCapped.Speed);
            Assert.IsTrue(stillCapped.AtCap);
            Assert.AreEqual(SpeedMeter.Cap, meter.Speed);
        }

        [Test]
        public void Expose_NeverGoesDown()
        {
            SpeedMeter meter = new SpeedMeter();
            int previous = meter.Speed;

            for (int i = 0; i < 20; i++)
            {
                meter = meter.Expose();
                Assert.GreaterOrEqual(meter.Speed, previous);
                Assert.LessOrEqual(meter.Speed, SpeedMeter.Cap);
                previous = meter.Speed;
            }

            Assert.AreEqual(SpeedMeter.Cap, meter.Speed);
        }

        [Test]
        public void ConstructedAtCap_StaysAtCap()
        {
            SpeedMeter capped = new SpeedMeter(SpeedMeter.Cap);

            Assert.IsTrue(capped.AtCap);
            Assert.AreEqual(SpeedMeter.Cap, capped.Expose().Speed);
        }

        [Test]
        public void Constructor_RejectsSpeedBelowBaseOrAboveCap()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new SpeedMeter(SpeedMeter.BaseSpeed - 1));
            Assert.Throws<ArgumentOutOfRangeException>(() => new SpeedMeter(0));
            Assert.Throws<ArgumentOutOfRangeException>(() => new SpeedMeter(SpeedMeter.Cap + 1));
            Assert.Throws<ArgumentOutOfRangeException>(() => new SpeedMeter(-1));
        }
    }
}
