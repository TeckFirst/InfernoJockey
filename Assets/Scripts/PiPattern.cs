using System;
using System.Collections.Generic;

namespace InfernoJockey
{
    /// <summary>
    /// Deterministic safe-lane and ore-lane sequence for each obstacle set.
    /// Index i, starting at 0, uses digit i mod 30 of pi, including the leading 3.
    /// Ore for that set is the safe lane plus 7, wrapping 9 back to 1.
    /// Does not place obstacles. The safe lane stays open even when the ore lane is occupied.
    /// </summary>
    public static class PiPattern
    {
        public const int Period = 30;
        public const int OreOffset = 7;

        static readonly int[] FirstThirty =
        {
            3, 1, 4, 1, 5, 9, 2, 6, 5, 3,
            5, 8, 9, 7, 9, 3, 2, 3, 8, 4,
            6, 2, 6, 4, 3, 3, 8, 3, 2, 7
        };

        public static IReadOnlyList<int> Digits
        {
            get { return FirstThirty; }
        }

        public static int SafeLane(int setIndex)
        {
            RequireIndex(setIndex);
            return FirstThirty[setIndex % Period];
        }

        public static int OreLane(int setIndex)
        {
            return OreLaneFor(SafeLane(setIndex));
        }

        public static int OreLaneFor(int safeLane)
        {
            return LaneGrid.Offset(safeLane, OreOffset);
        }

        static void RequireIndex(int setIndex)
        {
            if (setIndex < 0)
                throw new ArgumentOutOfRangeException(nameof(setIndex), setIndex, "Set index must be 0 or greater.");
        }
    }
}
