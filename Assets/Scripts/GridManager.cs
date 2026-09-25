using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    [Header("Grid Settings")]
    [Tooltip("Distance between adjacent lanes horizontally and vertically.")]
    [SerializeField] private float laneSpacing = 2.0f;

    // Dictionary to hold world positions for lanes 1 through 9
    private Dictionary<int, Vector3> lanePositions = new Dictionary<int, Vector3>();

    private void Awake()
    {
        InitializeGrid();
    }

    /// <summary>
    /// Mathematically defines a 3x3 grid of lanes (1-9) with Lane 5 as the center anchor (0,0).
    /// Layout convention (Phone / Numeric keypad style or standard top-down matrix):
    /// 7  8  9  (Top row: Y = +spacing)
    /// 4  5  6  (Middle row: Y = 0)
    /// 1  2  3  (Bottom row: Y = -spacing)
    /// </summary>
    public void InitializeGrid()
    {
        lanePositions.Clear();

        // Map 3x3 coordinates relative to Lane 5 at (0,0)
        // Col: -1 (Left), 0 (Center), 1 (Right)
        // Row: -1 (Bottom), 0 (Center), 1 (Top)

        int laneId = 1;
        for (int row = -1; row <= 1; row++)
        {
            for (int col = -1; col <= 1; col++)
            {
                float xPos = col * laneSpacing;
                float yPos = row * laneSpacing;

                lanePositions[laneId] = new Vector3(xPos, yPos, 0f);
                laneId++;
            }
        }
    }

    /// <summary>
    /// Checks if a given lane exists within the 1-9 grid range.
    /// </summary>
    public bool HasLane(int lane)
    {
        return lanePositions.ContainsKey(lane);
    }

    /// <summary>
    /// Returns the 3D world position for a specified lane.
    /// </summary>
    public Vector3 GetLanePosition(int lane)
    {
        if (lanePositions.ContainsKey(lane))
        {
            // Return world position relative to this manager's transform
            return transform.TransformPoint(lanePositions[lane]);
        }

        Debug.LogWarning($"Lane {lane} does not exist on the grid. Returning Vector3.zero.");
        return transform.position;
    }

    /// <summary>
    /// Evaluates visibility-based movement rules. 
    /// For a standard 3x3 layout:
    /// - Center Lane (5) has unobstructed line-of-sight to all other lanes (1-9).
    /// - Non-center lanes can typically move directly to immediate spatial neighbors (Chebyshev distance <= 1).
    /// </summary>
    public bool IsDirectMove(int currentLane, int targetLane)
    {
        if (!HasLane(currentLane) || !HasLane(targetLane)) return false;
        if (currentLane == targetLane) return true;

        // Center lane (Lane 5) has absolute line-of-sight to everything in this layout design
        if (currentLane == 5) return true;

        // Get grid matrix indices (0 to 2) for current and target
        GetGridCoordinates(currentLane, out int currX, out int currY);
        GetGridCoordinates(targetLane, out int targetX, out int targetY);

        int deltaX = Mathf.Abs(currX - targetX);
        int deltaY = Mathf.Abs(currY - targetY);

        // A direct move from an outer/corner lane is allowed if it's strictly adjacent (Chebyshev distance <= 1)
        // e.g., moving from Lane 1 to Lane 2 or Lane 4 or diagonal Lane 5.
        // Moving from Lane 9 to Lane 1 has deltaX = 2, deltaY = 2, so it returns false (multi-step required).
        return deltaX <= 1 && deltaY <= 1;
    }

    /// <summary>
    /// Alias method to match IsAdjacent test criteria seamlessly.
    /// </summary>
    public bool IsAdjacent(int laneA, int laneB)
    {
        return IsDirectMove(laneA, laneB);
    }

    /// <summary>
    /// Helper to convert 1-9 lane index into 2D grid matrix coordinates (0 to 2).
    /// </summary>
    private void GetGridCoordinates(int lane, out int x, out int y)
    {
        // Lane 1-9 mapped back to 0-indexed row/col
        int index = lane - 1;
        x = index % 3; // 0: Left, 1: Center, 2: Right
        y = index / 3; // 0: Bottom, 1: Center, 2: Top
    }
}