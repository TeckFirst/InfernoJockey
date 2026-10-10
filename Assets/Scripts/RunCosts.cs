using System;

namespace InfernoJockey
{
    /// <summary>
    /// Costs the player manages for each run. One drone is consumed per run.
    /// Fuel and laser cores have unit costs. These are data only. No economy UI.
    /// </summary>
    public sealed class RunCosts
    {
        public const int DroneCost = 100;
        public const int FuelUnitCost = 10;
        public const int LaserCoreUnitCost = 5;

        /// <summary>
        /// Cost of one drone plus the requested fuel and laser cores.
        /// Units must be 0 or greater. Does not buy, spend, or persist.
        /// </summary>
        public static int Total(int fuelUnits, int laserCores)
        {
            if (fuelUnits < 0)
                throw new ArgumentOutOfRangeException(nameof(fuelUnits), fuelUnits, "Fuel units must be 0 or greater.");
            if (laserCores < 0)
                throw new ArgumentOutOfRangeException(nameof(laserCores), laserCores, "Laser cores must be 0 or greater.");

            return DroneCost + fuelUnits * FuelUnitCost + laserCores * LaserCoreUnitCost;
        }
    }
}
