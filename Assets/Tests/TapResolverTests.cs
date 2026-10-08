using System;
using NUnit.Framework;

namespace InfernoJockey
{
    public class TapResolverTests
    {
        [Test]
        public void AdjacentTap_MovesThere()
        {
            TapResult result = TapResolver.Resolve(1, 5, 1);

            Assert.AreEqual(TapAction.Move, result.Action);
            Assert.AreEqual(5, result.Lane);
            Assert.IsFalse(result.SpendsAmmo);
        }

        [Test]
        public void AdjacentTap_MovesWithNoAmmo()
        {
            TapResult result = TapResolver.Resolve(5, 9, 0);

            Assert.AreEqual(TapAction.Move, result.Action);
            Assert.AreEqual(9, result.Lane);
            Assert.IsFalse(result.SpendsAmmo);
        }

        [Test]
        public void OccupiedTap_FiresDownThatLaneWhenAmmoRemains()
        {
            TapResult one = TapResolver.Resolve(4, 4, 1);
            TapResult many = TapResolver.Resolve(4, 4, 6);

            Assert.AreEqual(TapAction.Fire, one.Action);
            Assert.AreEqual(4, one.Lane);
            Assert.IsTrue(one.SpendsAmmo);
            Assert.AreEqual(TapAction.Fire, many.Action);
            Assert.AreEqual(4, many.Lane);
            Assert.IsTrue(many.SpendsAmmo);
        }

        [Test]
        public void OccupiedTap_DoesNotFireWhenAmmoIsEmpty()
        {
            TapResult result = TapResolver.Resolve(6, 6, 0);

            Assert.AreEqual(TapAction.Ignore, result.Action);
            Assert.AreEqual(6, result.Lane);
            Assert.IsFalse(result.SpendsAmmo);
        }

        [Test]
        public void EmptyAmmo_StillAllowsAMove()
        {
            TapResult blockedFire = TapResolver.Resolve(8, 8, 0);
            TapResult dodge = TapResolver.Resolve(8, 5, 0);

            Assert.AreEqual(TapAction.Ignore, blockedFire.Action);
            Assert.AreEqual(TapAction.Move, dodge.Action);
            Assert.AreEqual(8, blockedFire.Lane);
            Assert.AreEqual(5, dodge.Lane);
        }

        [Test]
        public void NonAdjacentTap_DoesNothing()
        {
            TapResult opposite = TapResolver.Resolve(1, 9, 3);
            TapResult sameRow = TapResolver.Resolve(4, 6, 3);
            TapResult sameColumn = TapResolver.Resolve(2, 8, 0);

            Assert.AreEqual(TapAction.Ignore, opposite.Action);
            Assert.AreEqual(9, opposite.Lane);
            Assert.AreEqual(TapAction.Ignore, sameRow.Action);
            Assert.AreEqual(6, sameRow.Lane);
            Assert.AreEqual(TapAction.Ignore, sameColumn.Action);
            Assert.AreEqual(8, sameColumn.Lane);
            Assert.IsFalse(opposite.SpendsAmmo);
            Assert.IsFalse(sameRow.SpendsAmmo);
            Assert.IsFalse(sameColumn.SpendsAmmo);
        }

        [Test]
        public void HiddenLane_IsStillResolvedAndDoesNotThrow()
        {
            TapResult hidden = TapResolver.Resolve(1, 9, 2);

            Assert.AreEqual(TapAction.Ignore, hidden.Action);
            Assert.AreEqual(9, hidden.Lane);
            Assert.IsFalse(hidden.SpendsAmmo);
        }

        [Test]
        public void Resolve_DoesNotKeepAmmoOfItsOwn()
        {
            TapResult first = TapResolver.Resolve(5, 5, 1);
            TapResult second = TapResolver.Resolve(5, 5, 1);

            Assert.AreEqual(TapAction.Fire, first.Action);
            Assert.AreEqual(TapAction.Fire, second.Action);
            Assert.IsTrue(first.SpendsAmmo);
            Assert.IsTrue(second.SpendsAmmo);
        }

        [Test]
        public void InvalidLaneOrAmmo_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => TapResolver.Resolve(0, 1, 1));
            Assert.Throws<ArgumentOutOfRangeException>(() => TapResolver.Resolve(10, 1, 1));
            Assert.Throws<ArgumentOutOfRangeException>(() => TapResolver.Resolve(1, 0, 1));
            Assert.Throws<ArgumentOutOfRangeException>(() => TapResolver.Resolve(1, 10, 1));
            Assert.Throws<ArgumentOutOfRangeException>(() => TapResolver.Resolve(1, 2, -1));
        }

        [Test]
        public void EveryLane_MatchesMoveFireOrIgnore()
        {
            for (int occupied = 1; occupied <= 9; occupied++)
            {
                for (int tapped = 1; tapped <= 9; tapped++)
                {
                    TapResult withAmmo = TapResolver.Resolve(occupied, tapped, 1);
                    TapResult dry = TapResolver.Resolve(occupied, tapped, 0);

                    if (tapped == occupied)
                    {
                        Assert.AreEqual(TapAction.Fire, withAmmo.Action);
                        Assert.AreEqual(occupied, withAmmo.Lane);
                        Assert.IsTrue(withAmmo.SpendsAmmo);
                        Assert.AreEqual(TapAction.Ignore, dry.Action);
                        Assert.AreEqual(occupied, dry.Lane);
                        Assert.IsFalse(dry.SpendsAmmo);
                    }
                    else if (LaneGrid.IsAdjacent(occupied, tapped))
                    {
                        Assert.AreEqual(TapAction.Move, withAmmo.Action);
                        Assert.AreEqual(tapped, withAmmo.Lane);
                        Assert.IsFalse(withAmmo.SpendsAmmo);
                        Assert.AreEqual(TapAction.Move, dry.Action);
                        Assert.AreEqual(tapped, dry.Lane);
                        Assert.IsFalse(dry.SpendsAmmo);
                    }
                    else
                    {
                        Assert.AreEqual(TapAction.Ignore, withAmmo.Action);
                        Assert.AreEqual(tapped, withAmmo.Lane);
                        Assert.IsFalse(withAmmo.SpendsAmmo);
                        Assert.AreEqual(TapAction.Ignore, dry.Action);
                        Assert.AreEqual(tapped, dry.Lane);
                        Assert.IsFalse(dry.SpendsAmmo);
                    }
                }
            }
        }
    }
}
