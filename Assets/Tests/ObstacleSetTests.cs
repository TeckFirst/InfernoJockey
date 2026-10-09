using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace InfernoJockey
{
    public class ObstacleSetTests
    {
        [Test]
        public void For_UsesThePiSafeLaneAndOreLane()
        {
            Assert.AreEqual(3, ObstacleSet.FuelCadence);
            Assert.AreEqual(4, ObstacleSet.LaserCoreCadence);

            for (int setIndex = 0; setIndex < PiPattern.Period * 2; setIndex++)
            {
                ObstacleSet set = ObstacleSet.For(setIndex);

                Assert.AreEqual(setIndex, set.SetIndex);
                Assert.AreEqual(PiPattern.SafeLane(setIndex), set.SafeLane);
                Assert.AreEqual(PiPattern.OreLane(setIndex), set.OreLane);
                Assert.AreNotEqual(set.SafeLane, set.OreLane);
                Assert.IsFalse(set.WasAutoCleared);
            }
        }

        [Test]
        public void For_LeavesTheSafeLaneOpenAndPutsOreOnTheOreLane()
        {
            ObstacleSet first = ObstacleSet.For(0);

            Assert.AreEqual(3, first.SafeLane);
            Assert.AreEqual(1, first.OreLane);
            Assert.AreEqual(LaneContent.Open, first.Content(3));
            Assert.AreEqual(LaneContent.Ore, first.Content(1));
            Assert.IsTrue(first.IsOpen(3));
            Assert.IsFalse(first.IsOpen(1));
            Assert.IsTrue(first.Blocks(1));

            ObstacleSet later = ObstacleSet.For(29);
            Assert.AreEqual(LaneContent.Open, later.Content(later.SafeLane));
            Assert.AreEqual(LaneContent.Ore, later.Content(later.OreLane));
        }

        [Test]
        public void For_BlocksEveryLaneThatIsNotSafeOrOre()
        {
            for (int setIndex = 0; setIndex < PiPattern.Period; setIndex++)
            {
                ObstacleSet set = ObstacleSet.For(setIndex);
                for (int lane = 1; lane <= 9; lane++)
                {
                    if (lane == set.SafeLane)
                        Assert.AreEqual(LaneContent.Open, set.Content(lane));
                    else if (lane == set.OreLane)
                        Assert.AreEqual(LaneContent.Ore, set.Content(lane));
                    else
                        Assert.AreEqual(LaneContent.Obstacle, set.Content(lane));
                }
            }
        }

        [Test]
        public void For_AlwaysLeavesTheSafeLaneOpenAtZeroAmmo()
        {
            for (int setIndex = 0; setIndex < 90; setIndex++)
            {
                ObstacleSet set = ObstacleSet.For(setIndex);

                Assert.IsTrue(set.HasOpenLane);
                Assert.AreEqual(1, set.OpenCount);
                Assert.IsTrue(set.IsOpen(set.SafeLane));
                Assert.IsFalse(set.Blocks(set.SafeLane));
                CollectionAssert.AreEqual(new[] { set.SafeLane }, set.OpenLanes);
            }
        }

        [Test]
        public void For_SafeLaneStaysOpenWhenTheOreLaneIsOccupied()
        {
            for (int safe = 1; safe <= 9; safe++)
            {
                int setIndex = IndexWithSafeLane(safe);
                ObstacleSet set = ObstacleSet.For(setIndex);

                Assert.AreEqual(LaneContent.Ore, set.Content(set.OreLane));
                Assert.AreEqual(LaneContent.Open, set.Content(set.SafeLane));
                Assert.IsTrue(set.IsOpen(set.SafeLane));
            }
        }

        [Test]
        public void Pickup_SpawnsOnlyOnTheSafeLaneAtAFixedCadence()
        {
            ObstacleSet fuel = ObstacleSet.For(0);
            ObstacleSet neither = ObstacleSet.For(1);
            ObstacleSet laser = ObstacleSet.For(2);
            ObstacleSet both = ObstacleSet.For(6);

            Assert.IsTrue(fuel.HasFuel);
            Assert.IsFalse(fuel.HasLaserCore);
            Assert.IsFalse(neither.HasFuel);
            Assert.IsFalse(neither.HasLaserCore);
            Assert.IsFalse(laser.HasFuel);
            Assert.IsTrue(laser.HasLaserCore);
            Assert.IsTrue(both.HasFuel);
            Assert.IsTrue(both.HasLaserCore);

            for (int setIndex = 0; setIndex < 36; setIndex++)
            {
                ObstacleSet set = ObstacleSet.For(setIndex);
                bool expectFuel = setIndex % ObstacleSet.FuelCadence == 0;
                bool expectLaser = setIndex % ObstacleSet.LaserCoreCadence == ObstacleSet.LaserCorePhase;

                Assert.AreEqual(expectFuel, set.HasFuel);
                Assert.AreEqual(expectLaser, set.HasLaserCore);
                Assert.AreEqual(expectFuel, set.HasFuelOn(set.SafeLane));
                Assert.AreEqual(expectLaser, set.HasLaserCoreOn(set.SafeLane));

                for (int lane = 1; lane <= 9; lane++)
                {
                    if (lane == set.SafeLane)
                        continue;
                    Assert.IsFalse(set.HasFuelOn(lane));
                    Assert.IsFalse(set.HasLaserCoreOn(lane));
                }
            }
        }

        [Test]
        public void Pickup_NeverLandsOnABlockedLane()
        {
            for (int setIndex = 0; setIndex < 36; setIndex++)
            {
                ObstacleSet set = ObstacleSet.For(setIndex);
                if (!set.HasFuel && !set.HasLaserCore)
                    continue;

                Assert.IsTrue(set.IsOpen(set.SafeLane));
                Assert.IsFalse(set.Blocks(set.SafeLane));
                Assert.AreEqual(LaneContent.Open, set.Content(set.SafeLane));
            }
        }

        [Test]
        public void FromContents_AutoClearsTheSafeLaneWhenNothingIsOpen()
        {
            var blocked = new LaneContent[LaneGrid.LaneCount];
            for (int i = 0; i < blocked.Length; i++)
                blocked[i] = LaneContent.Obstacle;

            ObstacleSet set = ObstacleSet.FromContents(0, blocked);

            Assert.IsTrue(set.WasAutoCleared);
            Assert.AreEqual(3, set.SafeLane);
            Assert.AreEqual(LaneContent.Open, set.Content(3));
            Assert.IsTrue(set.HasOpenLane);
            Assert.AreEqual(1, set.OpenCount);
            Assert.IsTrue(set.HasFuel);
            Assert.IsTrue(set.HasFuelOn(3));
            Assert.IsFalse(set.HasFuelOn(1));
        }

        [Test]
        public void FromContents_AutoClearsOreThatSealsTheLastLane()
        {
            var sealedByOre = new LaneContent[LaneGrid.LaneCount];
            for (int i = 0; i < sealedByOre.Length; i++)
                sealedByOre[i] = LaneContent.Obstacle;
            sealedByOre[0] = LaneContent.Ore;

            ObstacleSet set = ObstacleSet.FromContents(1, sealedByOre);

            Assert.IsTrue(set.WasAutoCleared);
            Assert.AreEqual(1, set.SafeLane);
            Assert.AreEqual(LaneContent.Open, set.Content(1));
            Assert.AreEqual(LaneContent.Obstacle, set.Content(2));
            Assert.IsTrue(set.HasOpenLane);
        }

        [Test]
        public void FromContents_DoesNotAutoClearWhenAnOpenLaneAlreadyExists()
        {
            var contents = new LaneContent[LaneGrid.LaneCount];
            for (int i = 0; i < contents.Length; i++)
                contents[i] = LaneContent.Obstacle;
            contents[4] = LaneContent.Open;

            ObstacleSet set = ObstacleSet.FromContents(0, contents);

            Assert.IsFalse(set.WasAutoCleared);
            Assert.AreEqual(LaneContent.Open, set.Content(5));
            Assert.AreEqual(LaneContent.Obstacle, set.Content(3));
            Assert.AreEqual(1, set.OpenCount);
        }

        [Test]
        public void ApplyShot_ClearsAnObstacleInThatLane()
        {
            ObstacleSet set = ObstacleSet.For(0);

            ObstacleSet cleared = set.ApplyShot(2);

            Assert.AreEqual(LaneContent.Obstacle, set.Content(2));
            Assert.AreEqual(LaneContent.Open, cleared.Content(2));
            Assert.IsTrue(cleared.IsOpen(2));
            Assert.AreEqual(LaneContent.Ore, cleared.Content(1));
            Assert.AreEqual(LaneContent.Open, cleared.Content(3));
            Assert.AreEqual(set.SetIndex, cleared.SetIndex);
            Assert.IsFalse(cleared.WasAutoCleared);
        }

        [Test]
        public void ApplyShot_MeltsOreAndLeavesItTaggable()
        {
            ObstacleSet set = ObstacleSet.For(0);

            ObstacleSet melted = set.ApplyShot(1);

            Assert.AreEqual(LaneContent.Ore, set.Content(1));
            Assert.AreEqual(LaneContent.Melted, melted.Content(1));
            Assert.IsTrue(melted.IsOpen(1));
            Assert.IsTrue(melted.CanTag(1));
            Assert.IsFalse(set.CanTag(1));
            Assert.IsFalse(melted.CanTag(3));
            Assert.AreEqual(2, melted.OpenCount);
        }

        [Test]
        public void ApplyShot_DoesNotChangeAnOpenOrMeltedLane()
        {
            ObstacleSet set = ObstacleSet.For(0);
            ObstacleSet melted = set.ApplyShot(set.OreLane);

            ObstacleSet shotOpen = set.ApplyShot(set.SafeLane);
            ObstacleSet shotMelted = melted.ApplyShot(set.OreLane);

            Assert.AreEqual(LaneContent.Open, shotOpen.Content(set.SafeLane));
            Assert.AreEqual(LaneContent.Melted, shotMelted.Content(set.OreLane));
            Assert.IsTrue(shotOpen.HasFuel);
            Assert.IsTrue(shotMelted.CanTag(set.OreLane));
        }

        [Test]
        public void InvalidIndexLaneOrContents_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => ObstacleSet.For(-1));
            Assert.Throws<ArgumentOutOfRangeException>(() => ObstacleSet.FromContents(-1, new LaneContent[9]));
            Assert.Throws<ArgumentException>(() => ObstacleSet.FromContents(0, null));
            Assert.Throws<ArgumentException>(() => ObstacleSet.FromContents(0, new LaneContent[8]));

            ObstacleSet set = ObstacleSet.For(0);
            Assert.Throws<ArgumentOutOfRangeException>(() => set.Content(0));
            Assert.Throws<ArgumentOutOfRangeException>(() => set.Content(10));
            Assert.Throws<ArgumentOutOfRangeException>(() => set.ApplyShot(0));
            Assert.Throws<ArgumentOutOfRangeException>(() => set.HasFuelOn(-1));
            Assert.Throws<ArgumentOutOfRangeException>(() => set.CanTag(10));
        }

        static int IndexWithSafeLane(int safeLane)
        {
            for (int setIndex = 0; setIndex < PiPattern.Period; setIndex++)
            {
                if (PiPattern.SafeLane(setIndex) == safeLane)
                    return setIndex;
            }
            Assert.Fail("Pi has no set with safe lane " + safeLane);
            return -1;
        }
    }
}
