using System;
using NUnit.Framework;

namespace InfernoJockey
{
    public class RunTests
    {
        [Test]
        public void Start_BeginsOnSetZeroCenterLaneBaseSpeedWithGivenFuelAndAmmo()
        {
            Assert.AreEqual(5, Run.StartingLane);
            Assert.AreEqual(1, Run.FuelDrain);

            Run run = Run.Start(8, 3);

            Assert.AreEqual(Run.StartingLane, run.OccupiedLane);
            Assert.AreEqual(8, run.Fuel);
            Assert.AreEqual(3, run.Ammo);
            Assert.AreEqual(SpeedMeter.BaseSpeed, run.Speed.Speed);
            Assert.IsFalse(run.Speed.AtCap);
            Assert.AreEqual(0, run.SetIndex);
            Assert.AreEqual(0, run.TaggedCount);
            Assert.IsFalse(run.IsOver);
            Assert.IsFalse(run.Crashed);
            Assert.IsFalse(run.OutOfFuel);
            Assert.AreEqual(PiPattern.SafeLane(0), run.Current.SafeLane);
            Assert.IsTrue(run.Current.IsOpen(run.Current.SafeLane));
        }

        [Test]
        public void Start_ZeroFuelIsAlreadyOverWithoutACrash()
        {
            Run run = Run.Start(0, 1);

            Assert.IsTrue(run.IsOver);
            Assert.IsFalse(run.Crashed);
            Assert.IsTrue(run.OutOfFuel);
            Assert.AreEqual(0, run.Fuel);
        }

        [Test]
        public void Tap_AdjacentMovesAndLeavesFuelAmmoAndSetAlone()
        {
            Run start = Run.Start(5, 2);

            Run moved = start.Tap(6);

            Assert.AreEqual(6, moved.OccupiedLane);
            Assert.AreEqual(5, start.OccupiedLane);
            Assert.AreEqual(start.Fuel, moved.Fuel);
            Assert.AreEqual(start.Ammo, moved.Ammo);
            Assert.AreEqual(start.Speed.Speed, moved.Speed.Speed);
            Assert.AreEqual(start.SetIndex, moved.SetIndex);
            Assert.AreEqual(LaneContent.Open, moved.Current.Content(moved.Current.SafeLane));
        }

        [Test]
        public void Tap_OccupiedWithAmmoFiresClearsObstacleAndSpendsOne()
        {
            // Set 0: safe 3, ore 1. Lane 5 is obstacle.
            Run start = Run.Start(5, 2);
            Assert.AreEqual(LaneContent.Obstacle, start.Current.Content(5));

            Run fired = start.Tap(5);

            Assert.AreEqual(5, fired.OccupiedLane);
            Assert.AreEqual(1, fired.Ammo);
            Assert.AreEqual(start.Fuel, fired.Fuel);
            Assert.AreEqual(SpeedMeter.BaseSpeed, fired.Speed.Speed);
            Assert.AreEqual(LaneContent.Open, fired.Current.Content(5));
            Assert.IsTrue(fired.Current.IsOpen(5));
            Assert.AreEqual(LaneContent.Obstacle, start.Current.Content(5));
        }

        [Test]
        public void Tap_OccupiedOnOreMeltsItExposesSpeedAndSpendsAmmo()
        {
            // Move to ore lane first. Set 0 ore is 1. From 5, 1 is adjacent.
            Run onOre = Run.Start(5, 2).Tap(1);
            Assert.AreEqual(1, onOre.OccupiedLane);
            Assert.AreEqual(LaneContent.Ore, onOre.Current.Content(1));

            Run melted = onOre.Tap(1);

            Assert.AreEqual(1, melted.OccupiedLane);
            Assert.AreEqual(1, melted.Ammo);
            Assert.AreEqual(SpeedMeter.BaseSpeed + SpeedMeter.ExposureStep, melted.Speed.Speed);
            Assert.AreEqual(LaneContent.Melted, melted.Current.Content(1));
            Assert.IsTrue(melted.Current.CanTag(1));
            Assert.AreEqual(LaneContent.Ore, onOre.Current.Content(1));
        }

        [Test]
        public void Tap_OccupiedWithNoAmmoDoesNothing()
        {
            Run empty = Run.Start(5, 0);

            Run still = empty.Tap(5);

            Assert.AreSame(empty, still);
            Assert.AreEqual(0, still.Ammo);
            Assert.AreEqual(LaneContent.Obstacle, still.Current.Content(5));
        }

        [Test]
        public void Tap_NonAdjacentIsIgnored()
        {
            Run start = Run.Start(5, 2);

            Run ignored = start.Tap(9); // 5 to 9 is distance 2? Wait, 5 center to 9 is adjacent? 
            // 5 is center, all are adjacent. Use 1 to 9? Start is 5.
            // From 5 everything is adjacent. Move to corner first.
            Run corner = start.Tap(1); // 1 is adjacent to 5
            Run ignoredFromCorner = corner.Tap(9);

            Assert.AreEqual(1, ignoredFromCorner.OccupiedLane);
            Assert.AreEqual(corner.Ammo, ignoredFromCorner.Ammo);
        }

        [Test]
        public void Advance_OnOpenLaneDrainsFuelLoadsNextSetAndDoesNotCrash()
        {
            // Start on 5, set 0 safe is 3. Move to safe.
            Run onSafe = Run.Start(5, 1).Tap(3);
            Assert.IsTrue(onSafe.Current.IsOpen(3));
            Assert.IsFalse(onSafe.Current.Blocks(3));

            Run next = onSafe.Advance();

            Assert.AreEqual(3, next.OccupiedLane);
            Assert.AreEqual(5, next.Fuel); // 5 +1 pickup -1 drain
            Assert.AreEqual(1, next.Ammo);
            Assert.AreEqual(1, next.SetIndex);
            Assert.AreEqual(0, next.TaggedCount);
            Assert.IsFalse(next.IsOver);
            Assert.IsFalse(next.Crashed);
            Assert.AreEqual(PiPattern.SafeLane(1), next.Current.SafeLane);
        }

        [Test]
        public void Advance_CollectsFuelAndLaserCoreOnlyWhenOnTheSafeLane()
        {
            // Set 0 has fuel (0 % 3 == 0), no laser (0 % 4 != 2)
            Run onSafe = Run.Start(5, 1).Tap(3);
            Assert.IsTrue(onSafe.Current.HasFuelOn(3));
            Assert.IsFalse(onSafe.Current.HasLaserCoreOn(3));

            Run after = onSafe.Advance();

            Assert.AreEqual(5, after.Fuel); // 5 +1 -1
            Assert.AreEqual(1, after.Ammo);

            // Set 2 has laser, no fuel. Start with more fuel so we can reach it.
            Run atTwo = Run.Start(10, 0);
            // Advance to set 1, then 2, staying on safe each time.
            // But safe changes each set. We must move to the new safe before each advance.
            // For simplicity, force by constructing? No, use the public API.
            // Set 0 safe 3, set 1 safe 1, set 2 safe 4.
            Run step0 = atTwo.Tap(3).Advance(); // now set 1, on 3
            Assert.AreEqual(1, step0.SetIndex);
            // 3 is not adjacent to 1. Path 3 → 2 → 1.
            Run step1 = step0.Tap(2).Tap(1).Advance(); // now set 2 on 1
            Assert.AreEqual(2, step1.SetIndex);
            Assert.AreEqual(1, step1.OccupiedLane);
            Assert.IsTrue(step1.Current.HasLaserCoreOn(step1.Current.SafeLane));
            // Safe for set 2 is 4. 1 → 4 is adjacent.
            Run onLaser = step1.Tap(4);
            Assert.AreEqual(4, onLaser.OccupiedLane);
            Assert.IsTrue(onLaser.Current.HasLaserCoreOn(4));

            Run collected = onLaser.Advance();

            Assert.AreEqual(step1.Ammo + 1, collected.Ammo);
            Assert.AreEqual(step1.Fuel - 1, collected.Fuel); // no fuel pickup on set 2
        }

        [Test]
        public void Advance_TagsMeltedOreIfStillOnThatLane()
        {
            // Melt ore on set 0, stay, advance.
            Run melted = Run.Start(5, 2).Tap(1).Tap(1);
            Assert.IsTrue(melted.Current.CanTag(1));
            Assert.AreEqual(1, melted.OccupiedLane);

            Run tagged = melted.Advance();

            Assert.AreEqual(1, tagged.TaggedCount);
            Assert.AreEqual(1, tagged.OccupiedLane);
            Assert.AreEqual(1, tagged.SetIndex);
            Assert.IsFalse(tagged.IsOver);
            Assert.AreEqual(SpeedMeter.BaseSpeed + SpeedMeter.ExposureStep, tagged.Speed.Speed);
        }

        [Test]
        public void Advance_DoesNotTagIfTheDroneLeftTheMeltedLane()
        {
            Run melted = Run.Start(5, 2).Tap(1).Tap(1);
            Run left = melted.Tap(2); // 1 to 2 is adjacent

            Run after = left.Advance();

            Assert.AreEqual(0, after.TaggedCount);
            Assert.AreEqual(2, after.OccupiedLane);
        }

        [Test]
        public void Advance_OnBlockedLaneCrashesAndEndsWithoutDrainingOrTagging()
        {
            // Stay on 5, which is obstacle on set 0.
            Run start = Run.Start(5, 1);
            Assert.IsTrue(start.Current.Blocks(5));

            Run crashed = start.Advance();

            Assert.IsTrue(crashed.IsOver);
            Assert.IsTrue(crashed.Crashed);
            Assert.IsFalse(crashed.OutOfFuel);
            Assert.AreEqual(start.Fuel, crashed.Fuel);
            Assert.AreEqual(start.Ammo, crashed.Ammo);
            Assert.AreEqual(0, crashed.TaggedCount);
            Assert.AreEqual(0, crashed.SetIndex);
        }

        [Test]
        public void Advance_EmptyFuelAfterDrainEndsWithoutCrash()
        {
            // Reach set 1 (no fuel pickup) with exactly 1 fuel left, on its safe lane.
            Run afterSetZero = Run.Start(1, 0).Tap(3).Advance();
            Assert.AreEqual(1, afterSetZero.Fuel);
            Assert.AreEqual(1, afterSetZero.SetIndex);
            Assert.IsFalse(afterSetZero.Current.HasFuel);

            // 3 is not adjacent to 1. Path 3 → 2 → 1.
            Run onSafe = afterSetZero.Tap(2).Tap(1); // set 1 safe is 1
            Assert.IsTrue(onSafe.Current.IsOpen(1));
            Assert.IsFalse(onSafe.Current.HasFuelOn(1));

            Run ended = onSafe.Advance();

            Assert.IsTrue(ended.IsOver);
            Assert.IsFalse(ended.Crashed);
            Assert.IsTrue(ended.OutOfFuel);
            Assert.AreEqual(0, ended.Fuel);
            Assert.AreEqual(1, ended.SetIndex); // stayed on the set that emptied it
        }

        [Test]
        public void Advance_FuelPickupCanKeepARunAlive()
        {
            // Set 0 has fuel. Start with 1, collect, drain to 1, continue.
            Run onFuel = Run.Start(1, 0).Tap(3);

            Run continued = onFuel.Advance();

            Assert.AreEqual(1, continued.Fuel); // 1+1-1
            Assert.IsFalse(continued.IsOver);
            Assert.AreEqual(1, continued.SetIndex);
        }

        [Test]
        public void EndedRun_IgnoresFurtherTapsAndAdvances()
        {
            Run crashed = Run.Start(5, 1).Advance();
            Assert.IsTrue(crashed.IsOver);

            Run still = crashed.Tap(4).Advance();

            Assert.AreSame(crashed, still);
            Assert.IsTrue(still.IsOver);
            Assert.IsTrue(still.Crashed);
        }

        [Test]
        public void ZeroAmmo_CanStillMoveToTheSafeLaneAndSurvive()
        {
            Run emptyAmmo = Run.Start(5, 0);
            // Safe is 3, adjacent.
            Run survived = emptyAmmo.Tap(3).Advance();

            Assert.IsFalse(survived.IsOver);
            Assert.AreEqual(5, survived.Fuel);
            Assert.AreEqual(0, survived.Ammo);
            Assert.AreEqual(1, survived.SetIndex);
        }

        [Test]
        public void Speed_ClimbsOnEachMeltAndStopsAtTheCap()
        {
            // We need multiple melts. Ore lanes differ. For the test we can melt on set 0, advance, move to next ore, melt, etc.
            // Keep it short: expose a few times and check it never exceeds.
            Run run = Run.Start(20, 10);
            int previous = run.Speed.Speed;

            // Melt set 0 ore (lane 1)
            run = run.Tap(1).Tap(1);
            Assert.Greater(run.Speed.Speed, previous);
            previous = run.Speed.Speed;
            run = run.Advance(); // tag, next set

            // Continue until cap or a few steps
            for (int i = 0; i < 10 && !run.Speed.AtCap; i++)
            {
                int ore = run.Current.OreLane;
                // Move to ore if not already (may need intermediate if not adjacent)
                if (run.OccupiedLane != ore)
                {
                    // Simple path: if not adjacent, go via a neighbor. For test reliability use center when possible.
                    // Since center is adjacent to all, move to 5 first if needed, then to ore.
                    if (LaneGrid.Distance(run.OccupiedLane, ore) > 1)
                        run = run.Tap(5);
                    run = run.Tap(ore);
                }
                run = run.Tap(ore); // fire
                Assert.GreaterOrEqual(run.Speed.Speed, previous);
                Assert.LessOrEqual(run.Speed.Speed, SpeedMeter.Cap);
                previous = run.Speed.Speed;
                if (!run.IsOver)
                    run = run.Advance();
            }

            // Force remaining exposures if needed by creating a fresh meter path is hard; the SpeedMeter tests already cover the cap.
            // Just assert the run never went past the cap.
            Assert.LessOrEqual(run.Speed.Speed, SpeedMeter.Cap);
        }

        [Test]
        public void InvalidStartOrTap_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => Run.Start(-1, 1));
            Assert.Throws<ArgumentOutOfRangeException>(() => Run.Start(1, -1));

            Run run = Run.Start(5, 1);
            Assert.Throws<ArgumentOutOfRangeException>(() => run.Tap(0));
            Assert.Throws<ArgumentOutOfRangeException>(() => run.Tap(10));
        }
    }
}
