using UnityEngine;

public class LaneSelector : MonoBehaviour
{
    [Tooltip("How far the grid shifts per lane (in local units). Tune these so the selected lane sits where you want it.")]
    public float laneOffsetX = 1.05f;   // roughly cellWidth * scale
    public float laneOffsetY = 2.1f;    // roughly cellHeight * scale

    // Optional: the current safe lane (1–9). You can drive this later from pi digits.
    public int safeLane = 3;

    public void SelectLane(int laneNumber)   // expects 1–9
    {
        if (laneNumber < 1 || laneNumber > 9)
        {
            Debug.LogWarning("Lane must be 1–9");
            return;
        }

        int index = laneNumber - 1;          // convert to 0–8 for math
        int col = index % 3;                 // 0 left, 1 center, 2 right
        int row = index / 3;                 // 0 top, 1 middle, 2 bottom

        // Center the chosen lane (lane 5 → col 1, row 1 → offset 0,0)
        float x = (1 - col) * laneOffsetX;
        float y = (1 - row) * laneOffsetY;

        transform.localPosition = new Vector3(x, y, transform.localPosition.z);
    }

    // Convenience method if you later want a “go to safe lane” button or automatic call
    public void GoToSafeLane()
    {
        SelectLane(safeLane);
    }
}