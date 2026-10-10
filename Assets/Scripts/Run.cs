using System;

namespace InfernoJockey
{
    /// <summary>
    /// One run of the simplest playable loop. The drone occupies one lane of the current
    /// obstacle set. A tap moves to an adjacent lane or fires down the occupied lane.
    /// Advance consumes the set: a blocked lane crashes, melted ore tags, pickups collect,
    /// then fuel drains by one. Empty fuel or a crash ends the run. The drone is gone.
    /// Speed rises only when a fire melts ore. Does not draw, scroll, or buy.
    /// </summary>
    public sealed class Run
    {
        public const int StartingLane = 5;
        public const int FuelDrain = 1;

        Run(
            int occupiedLane,
            int fuel,
            int ammo,
            SpeedMeter speed,
            ObstacleSet current,
            int taggedCount,
            bool isOver,
            bool crashed)
        {
            OccupiedLane = occupiedLane;
            Fuel = fuel;
            Ammo = ammo;
            Speed = speed;
            Current = current;
            TaggedCount = taggedCount;
            IsOver = isOver;
            Crashed = crashed;
        }

        public int OccupiedLane { get; }
        public int Fuel { get; }
        public int Ammo { get; }
        public SpeedMeter Speed { get; }
        public ObstacleSet Current { get; }
        public int TaggedCount { get; }
        public bool IsOver { get; }
        public bool Crashed { get; }

        public int SetIndex
        {
            get { return Current.SetIndex; }
        }

        public bool OutOfFuel
        {
            get { return IsOver && !Crashed; }
        }

        /// <summary>
        /// Starts a run on set 0, center lane, base speed, with the given fuel and ammo.
        /// Both must be 0 or greater. A zero-fuel start is already over.
        /// </summary>
        public static Run Start(int fuel, int ammo)
        {
            if (fuel < 0)
                throw new ArgumentOutOfRangeException(nameof(fuel), fuel, "Fuel must be 0 or greater.");
            if (ammo < 0)
                throw new ArgumentOutOfRangeException(nameof(ammo), ammo, "Ammo must be 0 or greater.");

            bool alreadyOver = fuel <= 0;
            return new Run(
                StartingLane,
                fuel,
                ammo,
                new SpeedMeter(),
                ObstacleSet.For(0),
                0,
                alreadyOver,
                false);
        }

        /// <summary>
        /// Resolve one tap. Adjacent moves. Occupied with ammo fires, spends one, and
        /// melts ore (exposing speed) or clears an obstacle. Other taps do nothing.
        /// An ended run ignores the tap. Does not advance the set.
        /// </summary>
        public Run Tap(int tappedLane)
        {
            if (IsOver)
                return this;
            if (!LaneGrid.IsValid(tappedLane))
                throw new ArgumentOutOfRangeException(nameof(tappedLane), tappedLane, "Lane must be 1–9.");

            TapResult result = TapResolver.Resolve(OccupiedLane, tappedLane, Ammo);
            if (result.Action == TapAction.Ignore)
                return this;

            if (result.Action == TapAction.Move)
                return Copy(occupiedLane: result.Lane);

            // Fire
            ObstacleSet after = Current.ApplyShot(result.Lane);
            SpeedMeter afterSpeed = Speed;
            if (Current.Content(result.Lane) == LaneContent.Ore)
                afterSpeed = Speed.Expose();

            return Copy(ammo: Ammo - 1, speed: afterSpeed, current: after);
        }

        /// <summary>
        /// Consume the current set and load the next. A blocked occupied lane crashes.
        /// Melted ore on the occupied lane tags. Fuel and laser cores on the occupied
        /// safe lane collect. Fuel then drains by one. Zero or less ends the run without
        /// a crash. An ended run stays ended.
        /// </summary>
        public Run Advance()
        {
            if (IsOver)
                return this;

            if (Current.Blocks(OccupiedLane))
                return Copy(isOver: true, crashed: true);

            int nextFuel = Fuel;
            int nextAmmo = Ammo;
            int nextTagged = TaggedCount;

            if (Current.CanTag(OccupiedLane))
                nextTagged++;

            if (Current.HasFuelOn(OccupiedLane))
                nextFuel++;
            if (Current.HasLaserCoreOn(OccupiedLane))
                nextAmmo++;

            nextFuel -= FuelDrain;
            if (nextFuel < 0)
                nextFuel = 0;

            bool ended = nextFuel <= 0;
            ObstacleSet next = ended ? Current : ObstacleSet.For(Current.SetIndex + 1);

            return Copy(
                fuel: nextFuel,
                ammo: nextAmmo,
                current: next,
                taggedCount: nextTagged,
                isOver: ended,
                crashed: false);
        }

        Run Copy(
            int? occupiedLane = null,
            int? fuel = null,
            int? ammo = null,
            SpeedMeter speed = null,
            ObstacleSet current = null,
            int? taggedCount = null,
            bool? isOver = null,
            bool? crashed = null)
        {
            return new Run(
                occupiedLane ?? OccupiedLane,
                fuel ?? Fuel,
                ammo ?? Ammo,
                speed ?? Speed,
                current ?? Current,
                taggedCount ?? TaggedCount,
                isOver ?? IsOver,
                crashed ?? Crashed);
        }
    }
}
