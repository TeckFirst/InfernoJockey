using System;

namespace InfernoJockey
{
    /// <summary>
    /// Permanent overdrive from raw Kytherium. Speed starts at the base and only rises.
    /// One Expose call is one melt. It does not read obstacles, spend ammo, or tag.
    /// The machinery cap stops the climb. There is no mid-run reset.
    /// Speed is scroll units per second.
    /// </summary>
    public sealed class SpeedMeter
    {
        public const int BaseSpeed = 4;
        public const int ExposureStep = 1;
        public const int Cap = 12;

        public SpeedMeter()
            : this(BaseSpeed)
        {
        }

        public SpeedMeter(int speed)
        {
            if (speed < BaseSpeed || speed > Cap)
                throw new ArgumentOutOfRangeException(nameof(speed), speed, "Speed must stay between the base and the machinery cap.");
            Speed = speed;
        }

        public int Speed { get; }

        public bool AtCap
        {
            get { return Speed == Cap; }
        }

        /// <summary>
        /// One Kytherium exposure. Rises by one step and stops at the cap.
        /// Does not mutate this meter and does not go down.
        /// </summary>
        public SpeedMeter Expose()
        {
            int next = Speed + ExposureStep;
            if (next > Cap)
                next = Cap;
            if (next == Speed)
                return this;
            return new SpeedMeter(next);
        }
    }
}
