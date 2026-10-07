using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace InfernoJockey
{
    public class LaneGridTests
    {
        [Test]
        public void Lanes_AreNumberedOneThroughNine()
        {
            Assert.AreEqual(9, LaneGrid.LaneCount);
            CollectionAssert.AreEqual(new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 }, LaneGrid.Lanes);
        }

        [Test]
        public void IsValid_AcceptsOnlyOneThroughNine()
        {
            Assert.IsFalse(LaneGrid.IsValid(0));
            Assert.IsFalse(LaneGrid.IsValid(10));
            Assert.IsFalse(LaneGrid.IsValid(-1));
            for (int lane = 1; lane <= 9; lane++)
                Assert.IsTrue(LaneGrid.IsValid(lane));
        }

        [Test]
        public void RowAndColumn_AreRowMajor()
        {
            Assert.AreEqual(0, LaneGrid.Row(1));
            Assert.AreEqual(0, LaneGrid.Column(1));
            Assert.AreEqual(0, LaneGrid.Row(3));
            Assert.AreEqual(2, LaneGrid.Column(3));
            Assert.AreEqual(1, LaneGrid.Row(5));
            Assert.AreEqual(1, LaneGrid.Column(5));
            Assert.AreEqual(2, LaneGrid.Row(7));
            Assert.AreEqual(0, LaneGrid.Column(7));
            Assert.AreEqual(2, LaneGrid.Row(9));
            Assert.AreEqual(2, LaneGrid.Column(9));
        }

        [Test]
        public void LaneAt_RoundTripsEveryLane()
        {
            for (int lane = 1; lane <= 9; lane++)
                Assert.AreEqual(lane, LaneGrid.LaneAt(LaneGrid.Row(lane), LaneGrid.Column(lane)));
        }

        [Test]
        public void InvalidLane_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => LaneGrid.Row(0));
            Assert.Throws<ArgumentOutOfRangeException>(() => LaneGrid.Column(10));
            Assert.Throws<ArgumentOutOfRangeException>(() => LaneGrid.Neighbors(-1));
            Assert.Throws<ArgumentOutOfRangeException>(() => LaneGrid.Distance(1, 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => LaneGrid.Offset(0, 1));
            Assert.Throws<ArgumentOutOfRangeException>(() => LaneGrid.Center(0));
            Assert.Throws<ArgumentOutOfRangeException>(() => LaneGrid.LaneAt(3, 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => LaneGrid.LaneAt(0, -1));
        }

        [Test]
        public void Distance_IsChebyshev()
        {
            Assert.AreEqual(0, LaneGrid.Distance(5, 5));
            Assert.AreEqual(1, LaneGrid.Distance(1, 2));
            Assert.AreEqual(1, LaneGrid.Distance(1, 5));
            Assert.AreEqual(2, LaneGrid.Distance(1, 3));
            Assert.AreEqual(2, LaneGrid.Distance(1, 6));
            Assert.AreEqual(2, LaneGrid.Distance(1, 9));
            Assert.AreEqual(2, LaneGrid.Distance(4, 6));
            Assert.AreEqual(2, LaneGrid.Distance(2, 8));
        }

        [Test]
        public void IsAdjacent_IsSymmetricAndExcludesSelf()
        {
            for (int lane = 1; lane <= 9; lane++)
                Assert.IsFalse(LaneGrid.IsAdjacent(lane, lane));

            Assert.IsTrue(LaneGrid.IsAdjacent(1, 5));
            Assert.IsTrue(LaneGrid.IsAdjacent(5, 1));
            Assert.IsFalse(LaneGrid.IsAdjacent(1, 9));
            Assert.IsFalse(LaneGrid.IsAdjacent(1, 3));
        }

        [Test]
        public void Corners_HaveThreeNeighbors()
        {
            AssertNeighbors(1, 2, 4, 5);
            AssertNeighbors(3, 2, 5, 6);
            AssertNeighbors(7, 4, 5, 8);
            AssertNeighbors(9, 5, 6, 8);
        }

        [Test]
        public void Edges_HaveFiveNeighbors()
        {
            AssertNeighbors(2, 1, 3, 4, 5, 6);
            AssertNeighbors(4, 1, 2, 5, 7, 8);
            AssertNeighbors(6, 2, 3, 5, 8, 9);
            AssertNeighbors(8, 4, 5, 6, 7, 9);
        }

        [Test]
        public void Center_HasEightNeighbors()
        {
            AssertNeighbors(5, 1, 2, 3, 4, 6, 7, 8, 9);
        }

        [Test]
        public void Neighbors_AreValidAdjacentAndExcludeSelf()
        {
            for (int lane = 1; lane <= 9; lane++)
            {
                IReadOnlyList<int> neighbors = LaneGrid.Neighbors(lane);
                CollectionAssert.AllItemsAreUnique(neighbors);
                Assert.IsFalse(Contains(neighbors, lane));
                for (int i = 0; i < neighbors.Count; i++)
                {
                    Assert.IsTrue(LaneGrid.IsValid(neighbors[i]));
                    Assert.IsTrue(LaneGrid.IsAdjacent(lane, neighbors[i]));
                }
            }
        }

        [Test]
        public void Offset_WrapsOreLaneFromSafeLanePlusSeven()
        {
            Assert.AreEqual(8, LaneGrid.Offset(1, 7));
            Assert.AreEqual(9, LaneGrid.Offset(2, 7));
            Assert.AreEqual(1, LaneGrid.Offset(3, 7));
            Assert.AreEqual(2, LaneGrid.Offset(4, 7));
            Assert.AreEqual(3, LaneGrid.Offset(5, 7));
            Assert.AreEqual(4, LaneGrid.Offset(6, 7));
            Assert.AreEqual(5, LaneGrid.Offset(7, 7));
            Assert.AreEqual(6, LaneGrid.Offset(8, 7));
            Assert.AreEqual(7, LaneGrid.Offset(9, 7));
        }

        [Test]
        public void Offset_WrapsBothDirectionsAndStaysOnGrid()
        {
            Assert.AreEqual(9, LaneGrid.Offset(1, -1));
            Assert.AreEqual(1, LaneGrid.Offset(9, 1));
            Assert.AreEqual(5, LaneGrid.Offset(5, 9));
            Assert.AreEqual(5, LaneGrid.Offset(5, -9));
            for (int lane = 1; lane <= 9; lane++)
                Assert.IsTrue(LaneGrid.IsValid(LaneGrid.Offset(lane, lane * 3 - 20)));
        }

        [Test]
        public void Centers_MatchTheVisibleGrid()
        {
            AssertPoint(LaneGrid.Center(5), 0f, 0f, 0f);
            AssertPoint(LaneGrid.Center(1), -1f, 1f, 0f);
            AssertPoint(LaneGrid.Center(3), 1f, 1f, 0f);
            AssertPoint(LaneGrid.Center(7), -1f, -1f, 0f);
            AssertPoint(LaneGrid.Center(9), 1f, -1f, 0f);
            AssertPoint(LaneGrid.Center(4), -1f, 0f, 0f);
        }

        [Test]
        public void OrthogonalNeighbors_AreOneCellApart()
        {
            Assert.AreEqual(LaneGrid.CellSize, Gap(4, 5), 0.0001f);
            Assert.AreEqual(LaneGrid.CellSize, Gap(5, 6), 0.0001f);
            Assert.AreEqual(LaneGrid.CellSize, Gap(2, 5), 0.0001f);
            Assert.AreEqual(LaneGrid.CellSize, Gap(5, 8), 0.0001f);
            Assert.AreEqual(LaneGrid.CellSize, Math.Abs(LaneGrid.Center(1).X - LaneGrid.Center(5).X), 0.0001f);
            Assert.AreEqual(LaneGrid.CellSize, Math.Abs(LaneGrid.Center(1).Y - LaneGrid.Center(5).Y), 0.0001f);
        }

        [Test]
        public void Frame_IsTheSharedThreeByThreeWire()
        {
            IReadOnlyList<LaneSegment> frame = LaneGrid.Frame();
            Assert.AreEqual(8, frame.Count);

            AssertSegment(frame[0], -1.5f, 1.5f, -1.5f, -1.5f);
            AssertSegment(frame[1], -0.5f, 1.5f, -0.5f, -1.5f);
            AssertSegment(frame[2], 0.5f, 1.5f, 0.5f, -1.5f);
            AssertSegment(frame[3], 1.5f, 1.5f, 1.5f, -1.5f);
            AssertSegment(frame[4], -1.5f, 1.5f, 1.5f, 1.5f);
            AssertSegment(frame[5], -1.5f, 0.5f, 1.5f, 0.5f);
            AssertSegment(frame[6], -1.5f, -0.5f, 1.5f, -0.5f);
            AssertSegment(frame[7], -1.5f, -1.5f, 1.5f, -1.5f);
        }

        [Test]
        public void EveryCenter_SitsInsideItsCell()
        {
            IReadOnlyList<LaneSegment> frame = LaneGrid.Frame();
            for (int lane = 1; lane <= 9; lane++)
            {
                LanePoint center = LaneGrid.Center(lane);
                Assert.AreEqual(0f, center.Z, 0.0001f);
                Assert.Greater(center.X, frame[0].A.X);
                Assert.Less(center.X, frame[3].A.X);
                Assert.Less(center.Y, frame[4].A.Y);
                Assert.Greater(center.Y, frame[7].A.Y);
            }
        }

        static void AssertNeighbors(int lane, params int[] expected)
        {
            CollectionAssert.AreEqual(expected, LaneGrid.Neighbors(lane));
        }

        static bool Contains(IReadOnlyList<int> lanes, int lane)
        {
            for (int i = 0; i < lanes.Count; i++)
            {
                if (lanes[i] == lane)
                    return true;
            }
            return false;
        }

        static void AssertPoint(LanePoint point, float x, float y, float z)
        {
            Assert.AreEqual(x, point.X, 0.0001f);
            Assert.AreEqual(y, point.Y, 0.0001f);
            Assert.AreEqual(z, point.Z, 0.0001f);
        }

        static void AssertSegment(LaneSegment segment, float x0, float y0, float x1, float y1)
        {
            AssertPoint(segment.A, x0, y0, 0f);
            AssertPoint(segment.B, x1, y1, 0f);
        }

        static float Gap(int fromLane, int toLane)
        {
            LanePoint from = LaneGrid.Center(fromLane);
            LanePoint to = LaneGrid.Center(toLane);
            float dx = from.X - to.X;
            float dy = from.Y - to.Y;
            return (float)Math.Sqrt(dx * dx + dy * dy);
        }
    }
}