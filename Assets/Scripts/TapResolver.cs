using System;

namespace InfernoJockey
{
    public enum TapAction
    {
        Ignore = 0,
        Move = 1,
        Fire = 2
    }

    /// <summary>
    /// What a lane tap does. Move names the destination. Fire names the occupied lane.
    /// Ignore names the tapped lane and changes nothing. Fire is the only action that spends ammo.
    /// </summary>
    public readonly struct TapResult
    {
        public readonly TapAction Action;
        public readonly int Lane;

        public TapResult(TapAction action, int lane)
        {
            Action = action;
            Lane = lane;
        }

        public bool SpendsAmmo
        {
            get { return Action == TapAction.Fire; }
        }
    }

    /// <summary>
    /// Resolves the only play input. An adjacent tap moves. An occupied tap fires if ammo remains.
    /// Any other tap does nothing. A hidden lane is still a legal tap; visibility is not an input.
    /// Does not spend ammo, move the drone, or look at obstacles.
    /// </summary>
    public static class TapResolver
    {
        public static TapResult Resolve(int occupiedLane, int tappedLane, int ammo)
        {
            if (!LaneGrid.IsValid(occupiedLane))
                throw new ArgumentOutOfRangeException(nameof(occupiedLane), occupiedLane, "Lane must be 1–9.");
            if (!LaneGrid.IsValid(tappedLane))
                throw new ArgumentOutOfRangeException(nameof(tappedLane), tappedLane, "Lane must be 1–9.");
            if (ammo < 0)
                throw new ArgumentOutOfRangeException(nameof(ammo), ammo, "Ammo must be 0 or greater.");

            if (tappedLane == occupiedLane)
            {
                if (ammo > 0)
                    return new TapResult(TapAction.Fire, occupiedLane);
                return new TapResult(TapAction.Ignore, tappedLane);
            }

            if (LaneGrid.IsAdjacent(occupiedLane, tappedLane))
                return new TapResult(TapAction.Move, tappedLane);

            return new TapResult(TapAction.Ignore, tappedLane);
        }
    }
}
