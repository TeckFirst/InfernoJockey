using UnityEngine;

public class LaneSelector : MonoBehaviour
{
    // Optional: the current safe lane (1–9). You can drive this later from pi digits.
    public int safeLane = 3;

    // Tracks the lane the player is currently in (1–9)
    public int currentLane { get; private set; } = 7;   // game currently starts here

    public void SelectLane(int laneNumber)   // 1–9
    {
        if (laneNumber < 1 || laneNumber > 9) return;

        int index = laneNumber - 1;
        int col = index % 3;   // 0, 1, 2
        int row = index / 3;   // 0, 1, 2

        float cellWorldWidth = 50f * transform.lossyScale.x;
        float cellWorldHeight = 100f * transform.lossyScale.y;

        float x = (1 - col) * cellWorldWidth;
        float y = (1 - row) * cellWorldHeight;

        transform.localPosition = new Vector3(x, -y, transform.localPosition.z);

        currentLane = laneNumber;

#if UNITY_EDITOR
        Debug.Log($"Current lane: {currentLane}");
#endif
    }

    public void GoToSafeLane()
    {
        SelectLane(safeLane);
    }
}