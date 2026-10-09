using System;
using System.Collections.Generic;

namespace InfernoJockey
{
    public enum LaneContent
    {
        Open = 0,
        Obstacle = 1,
        Ore = 2,
        Melted = 3
    }

    /// <summary>
    /// One obstacle set. The pi safe lane is open. Ore sits on safe lane + 7.
    /// Every other lane is an obstacle. Fuel and laser cores spawn only on the safe lane.
    /// A shot clears an obstacle or melts ore. Melted ore can be tagged on the next beat.
    /// If a proposal blocks every lane, refinery equipment opens the safe lane. That clear is a safety net.
    /// </summary>
    public sealed class ObstacleSet
    {
        public const int FuelCadence = 3;
        public const int LaserCoreCadence = 4;
        public const int LaserCorePhase = 2;

        readonly LaneContent[] contents;

        ObstacleSet(int setIndex, int safeLane, int oreLane, LaneContent[] contents, bool wasAutoCleared)
        {
            SetIndex = setIndex;
            SafeLane = safeLane;
            OreLane = oreLane;
            this.contents = contents;
            WasAutoCleared = wasAutoCleared;
        }

        public int SetIndex { get; }
        public int SafeLane { get; }
        public int OreLane { get; }
        public bool WasAutoCleared { get; }

        public bool HasFuel
        {
            get { return SetIndex % FuelCadence == 0; }
        }

        public bool HasLaserCore
        {
            get { return SetIndex % LaserCoreCadence == LaserCorePhase; }
        }

        public bool HasOpenLane
        {
            get { return OpenCount > 0; }
        }

        public int OpenCount
        {
            get { return OpenLanes.Count; }
        }

        public IReadOnlyList<int> OpenLanes
        {
            get
            {
                var open = new List<int>(LaneGrid.LaneCount);
                for (int lane = LaneGrid.MinLane; lane <= LaneGrid.MaxLane; lane++)
                {
                    if (IsOpen(lane))
                        open.Add(lane);
                }
                return open;
            }
        }

        public static ObstacleSet For(int setIndex)
        {
            int safeLane = PiPattern.SafeLane(setIndex);
            int oreLane = PiPattern.OreLane(setIndex);
            var placed = new LaneContent[LaneGrid.LaneCount];
            for (int lane = LaneGrid.MinLane; lane <= LaneGrid.MaxLane; lane++)
            {
                if (lane == safeLane)
                    placed[lane - 1] = LaneContent.Open;
                else if (lane == oreLane)
                    placed[lane - 1] = LaneContent.Ore;
                else
                    placed[lane - 1] = LaneContent.Obstacle;
            }
            return new ObstacleSet(setIndex, safeLane, oreLane, placed, false);
        }

        /// <summary>
        /// Contents are lane 1 at index 0 through lane 9 at index 8.
        /// A set with no open lane is auto-cleared on the safe lane.
        /// </summary>
        public static ObstacleSet FromContents(int setIndex, LaneContent[] contents)
        {
            if (contents == null)
                throw new ArgumentException("Contents are required.", nameof(contents));
            if (contents.Length != LaneGrid.LaneCount)
                throw new ArgumentException("Contents must cover lanes 1–9.", nameof(contents));

            int safeLane = PiPattern.SafeLane(setIndex);
            int oreLane = PiPattern.OreLane(setIndex);
            var placed = (LaneContent[])contents.Clone();
            bool wasAutoCleared = false;
            if (!HasAnyOpen(placed))
            {
                placed[safeLane - 1] = LaneContent.Open;
                wasAutoCleared = true;
            }
            return new ObstacleSet(setIndex, safeLane, oreLane, placed, wasAutoCleared);
        }

        public LaneContent Content(int lane)
        {
            RequireLane(lane);
            return contents[lane - 1];
        }

        public bool IsOpen(int lane)
        {
            LaneContent content = Content(lane);
            return content == LaneContent.Open || content == LaneContent.Melted;
        }

        public bool Blocks(int lane)
        {
            LaneContent content = Content(lane);
            return content == LaneContent.Obstacle || content == LaneContent.Ore;
        }

        public bool CanTag(int lane)
        {
            return Content(lane) == LaneContent.Melted;
        }

        public bool HasFuelOn(int lane)
        {
            RequireLane(lane);
            return HasFuel && lane == SafeLane;
        }

        public bool HasLaserCoreOn(int lane)
        {
            RequireLane(lane);
            return HasLaserCore && lane == SafeLane;
        }

        /// <summary>
        /// A fire down this lane. An obstacle opens. Ore melts and stays for the tag beat.
        /// Does not spend ammo and does not change any other lane.
        /// </summary>
        public ObstacleSet ApplyShot(int lane)
        {
            RequireLane(lane);
            var placed = (LaneContent[])contents.Clone();
            if (placed[lane - 1] == LaneContent.Obstacle)
                placed[lane - 1] = LaneContent.Open;
            else if (placed[lane - 1] == LaneContent.Ore)
                placed[lane - 1] = LaneContent.Melted;
            return new ObstacleSet(SetIndex, SafeLane, OreLane, placed, WasAutoCleared);
        }

        static bool HasAnyOpen(LaneContent[] placed)
        {
            for (int i = 0; i < placed.Length; i++)
            {
                if (placed[i] == LaneContent.Open || placed[i] == LaneContent.Melted)
                    return true;
            }
            return false;
        }

        static void RequireLane(int lane)
        {
            if (!LaneGrid.IsValid(lane))
                throw new ArgumentOutOfRangeException(nameof(lane), lane, "Lane must be 1–9.");
        }
    }
}
