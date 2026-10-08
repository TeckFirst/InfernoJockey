using System;
using NUnit.Framework;

namespace InfernoJockey
{
    public class PiPatternTests
    {
        static readonly int[] FirstThirty =
        {
            3, 1, 4, 1, 5, 9, 2, 6, 5, 3,
            5, 8, 9, 7, 9, 3, 2, 3, 8, 4,
            6, 2, 6, 4, 3, 3, 8, 3, 2, 7
        };

        [Test]
        public void Digits_AreTheFirstThirtyOfPiIncludingTheLeadingThree()
        {
            Assert.AreEqual(30, PiPattern.Period);
            Assert.AreEqual(7, PiPattern.OreOffset);
            CollectionAssert.AreEqual(FirstThirty, PiPattern.Digits);
        }

        [Test]
        public void Digits_AreValidLanesAndNeverZero()
        {
            Assert.AreEqual(PiPattern.Period, PiPattern.Digits.Count);
            for (int i = 0; i < PiPattern.Digits.Count; i++)
            {
                Assert.IsTrue(LaneGrid.IsValid(PiPattern.Digits[i]));
                Assert.AreNotEqual(0, PiPattern.Digits[i]);
            }
        }

        [Test]
        public void SafeLane_UsesDigitAtIndexModPeriod()
        {
            for (int i = 0; i < FirstThirty.Length; i++)
                Assert.AreEqual(FirstThirty[i], PiPattern.SafeLane(i));

            Assert.AreEqual(3, PiPattern.SafeLane(0));
            Assert.AreEqual(1, PiPattern.SafeLane(1));
            Assert.AreEqual(7, PiPattern.SafeLane(29));
            Assert.AreEqual(3, PiPattern.SafeLane(30));
            Assert.AreEqual(1, PiPattern.SafeLane(31));
            Assert.AreEqual(7, PiPattern.SafeLane(59));
            Assert.AreEqual(4, PiPattern.SafeLane(60 + 2));
        }

        [Test]
        public void SafeLane_RejectsNegativeIndex()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => PiPattern.SafeLane(-1));
            Assert.Throws<ArgumentOutOfRangeException>(() => PiPattern.OreLane(-1));
        }

        [Test]
        public void OreLaneFor_IsSafeLanePlusSevenWrappingNineToOne()
        {
            Assert.AreEqual(8, PiPattern.OreLaneFor(1));
            Assert.AreEqual(9, PiPattern.OreLaneFor(2));
            Assert.AreEqual(1, PiPattern.OreLaneFor(3));
            Assert.AreEqual(2, PiPattern.OreLaneFor(4));
            Assert.AreEqual(3, PiPattern.OreLaneFor(5));
            Assert.AreEqual(4, PiPattern.OreLaneFor(6));
            Assert.AreEqual(5, PiPattern.OreLaneFor(7));
            Assert.AreEqual(6, PiPattern.OreLaneFor(8));
            Assert.AreEqual(7, PiPattern.OreLaneFor(9));
        }

        [Test]
        public void OreLane_FollowsTheSafeLaneForThatSet()
        {
            Assert.AreEqual(1, PiPattern.OreLane(0));
            Assert.AreEqual(8, PiPattern.OreLane(1));
            Assert.AreEqual(2, PiPattern.OreLane(2));
            Assert.AreEqual(5, PiPattern.OreLane(29));
            Assert.AreEqual(PiPattern.OreLane(0), PiPattern.OreLane(30));

            for (int setIndex = 0; setIndex < PiPattern.Period * 2; setIndex++)
            {
                int safeLane = PiPattern.SafeLane(setIndex);
                int oreLane = PiPattern.OreLane(setIndex);
                Assert.AreEqual(PiPattern.OreLaneFor(safeLane), oreLane);
                Assert.AreEqual(LaneGrid.Offset(safeLane, PiPattern.OreOffset), oreLane);
                Assert.IsTrue(LaneGrid.IsValid(oreLane));
                Assert.AreNotEqual(safeLane, oreLane);
            }
        }

        [Test]
        public void OreLaneFor_RejectsAnInvalidSafeLane()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => PiPattern.OreLaneFor(0));
            Assert.Throws<ArgumentOutOfRangeException>(() => PiPattern.OreLaneFor(10));
        }

        [Test]
        public void SafeLane_RepeatsAndStaysOnTheGrid()
        {
            for (int setIndex = 0; setIndex < 90; setIndex++)
            {
                int safeLane = PiPattern.SafeLane(setIndex);
                Assert.IsTrue(LaneGrid.IsValid(safeLane));
                Assert.AreEqual(PiPattern.Digits[setIndex % PiPattern.Period], safeLane);
            }
        }
    }
}
