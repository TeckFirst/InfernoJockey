using UnityEngine;

public class PlayerInput : MonoBehaviour
{
    [Tooltip("Optional: the current safe lane (1–9). Drive this later from your pi sequence.")]
    public int safeLane = 3;

    // Tracks the lane the player is currently in (1–9). Starts at 7 to match the scene.
    public int currentLane { get; private set; } = 7;

    /// <summary>
    /// Call this from every lane button’s On Click event.
    /// If the pressed lane is the one we are already in, fire.
    /// Otherwise move to that lane.
    /// </summary>
    public void OnLaneButtonPressed(int selectedLane)
    {
        if (selectedLane < 1 || selectedLane > 9) return;

        if (selectedLane == currentLane)
        {
            Fire();
        }
        else
        {
            MoveToLane(selectedLane);
        }
    }

    /// <summary>
    /// Moves the LaneGrid so the chosen lane is in front of the camera
    /// and updates currentLane.
    /// </summary>
    private void MoveToLane(int laneNumber)
    {
        int index = laneNumber - 1;
        int col = index % 3;   // 0, 1, 2
        int row = index / 3;   // 0, 1, 2

        float cellWorldWidth = 50f * transform.lossyScale.x;
        float cellWorldHeight = 100f * transform.lossyScale.y;

        float x = (1 - col) * cellWorldWidth;
        float y = (1 - row) * cellWorldHeight;

        // Negative Y matches the Grid Layout Group’s upper-left origin
        transform.localPosition = new Vector3(x, -y, transform.localPosition.z);

        currentLane = laneNumber;

#if UNITY_EDITOR
        Debug.Log($"Moved to lane {currentLane}");
#endif
    }

    /// <summary>
    /// Filler fire method. Replace the body later with a call to your consumables / ammo system.
    /// </summary>
    private void Fire()
    {
#if UNITY_EDITOR
        Debug.Log($"Firing from lane {currentLane}");
#endif
        // TODO: call consumables system to spend ammo and spawn a projectile
    }

    /// <summary>
    /// Convenience method if you later want a “go to safe lane” action.
    /// </summary>
    public void GoToSafeLane()
    {
        OnLaneButtonPressed(safeLane);
    }
}