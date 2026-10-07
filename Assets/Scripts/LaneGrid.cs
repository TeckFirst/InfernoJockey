using System;
using System.Collections.Generic;

namespace InfernoJockey
{
    /// <summary>
    /// Fixed 3×3 lane topology and the visible wireframe the player sees.
    /// Lanes are numbered row-major, 1–9. Adjacent means Chebyshev distance 1.
    /// The frame sits in the XY plane at Z = 0: column 0 left, row 0 top, lane 5 at the origin.
    /// No occupancy and no scene types.
    /// </summary>
    public static class LaneGrid
    {
        public const int Size = 3;
        public const int LaneCount = 9;
        public const int MinLane = 1;
        public const int MaxLane = 9;

        /// <summary>World size of one cell. Shared edges, no gaps.</summary>
        public const float CellSize = 1f;

        static readonly int[] All = { 1, 2, 3, 4, 5, 6, 7, 8, 9 };

        public static IReadOnlyList<int> Lanes
        {
            get { return All; }
        }

        public static bool IsValid(int lane)
        {
            return lane >= MinLane && lane <= MaxLane;
        }

        public static int Row(int lane)
        {
            RequireValid(lane);
            return (lane - 1) / Size;
        }

        public static int Column(int lane)
        {
            RequireValid(lane);
            return (lane - 1) % Size;
        }

        public static int LaneAt(int row, int column)
        {
            if (row < 0 || row >= Size)
                throw new ArgumentOutOfRangeException(nameof(row), row, "Row must be 0–2.");
            if (column < 0 || column >= Size)
                throw new ArgumentOutOfRangeException(nameof(column), column, "Column must be 0–2.");
            return row * Size + column + 1;
        }

        public static int Distance(int fromLane, int toLane)
        {
            RequireValid(fromLane);
            RequireValid(toLane);
            int rowDelta = Math.Abs(Row(fromLane) - Row(toLane));
            int columnDelta = Math.Abs(Column(fromLane) - Column(toLane));
            return Math.Max(rowDelta, columnDelta);
        }

        public static bool IsAdjacent(int fromLane, int toLane)
        {
            return Distance(fromLane, toLane) == 1;
        }

        /// <summary>
        /// Neighbors in ascending lane order. Does not include the lane itself.
        /// </summary>
        public static IReadOnlyList<int> Neighbors(int lane)
        {
            RequireValid(lane);
            var neighbors = new List<int>(8);
            for (int candidate = MinLane; candidate <= MaxLane; candidate++)
            {
                if (Distance(lane, candidate) == 1)
                    neighbors.Add(candidate);
            }
            return neighbors;
        }

        /// <summary>
        /// Move a lane id by delta, wrapping 9 back to 1 and 1 back to 9.
        /// Used by ore placement: safe lane + 7.
        /// </summary>
        public static int Offset(int lane, int delta)
        {
            RequireValid(lane);
            int index = (lane - 1 + delta) % LaneCount;
            if (index < 0)
                index += LaneCount;
            return index + 1;
        }

        /// <summary>Center of a lane cell on the visible frame. Lane 5 is the origin.</summary>
        public static LanePoint Center(int lane)
        {
            RequireValid(lane);
            float x = (Column(lane) - 1) * CellSize;
            float y = (1 - Row(lane)) * CellSize;
            return new LanePoint(x, y, 0f);
        }

        /// <summary>
        /// The cyan wireframe: 4 vertical and 4 horizontal segments, shared by neighboring cells.
        /// Outer extent is 3 cells. Stroke these in the scene; this type does not draw.
        /// </summary>
        public static IReadOnlyList<LaneSegment> Frame()
        {
            float half = CellSize * Size * 0.5f;
            float min = -half;
            float max = half;
            var segments = new List<LaneSegment>(8);
            for (int i = 0; i <= Size; i++)
            {
                float x = min + i * CellSize;
                segments.Add(new LaneSegment(new LanePoint(x, max, 0f), new LanePoint(x, min, 0f)));
            }
            for (int i = 0; i <= Size; i++)
            {
                float y = max - i * CellSize;
                segments.Add(new LaneSegment(new LanePoint(min, y, 0f), new LanePoint(max, y, 0f)));
            }
            return segments;
        }

        static void RequireValid(int lane)
        {
            if (!IsValid(lane))
                throw new ArgumentOutOfRangeException(nameof(lane), lane, "Lane must be 1–9.");
        }
    }

    /// <summary>World point on the visible grid. Y is up, X is right, Z is travel.</summary>
    public readonly struct LanePoint
    {
        public readonly float X;
        public readonly float Y;
        public readonly float Z;

        public LanePoint(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }
    }

    /// <summary>One edge of the visible wireframe.</summary>
    public readonly struct LaneSegment
    {
        public readonly LanePoint A;
        public readonly LanePoint B;

        public LaneSegment(LanePoint a, LanePoint b)
        {
            A = a;
            B = b;
        }
    }
}
