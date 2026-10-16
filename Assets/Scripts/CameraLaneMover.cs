using UnityEngine;

public class CameraLaneMover : MonoBehaviour
{
    [SerializeField]
    private Transform[] laneMarkers = new Transform[9];

    [SerializeField]
    private int startingLane = 5;

    public int CurrentLane { get; private set; }

    private void Awake()
    {
        CurrentLane = startingLane;
    }

    public bool MoveToLane(int targetLane)
    {
        if (!IsValidLane(targetLane))
        {
            return false;
        }

        if (!IsAdjacent(CurrentLane, targetLane))
        {
            return false;
        }

        transform.position = laneMarkers[targetLane - 1].position;
        CurrentLane = targetLane;

        return true;
    }

    public static bool IsAdjacent(int fromLane, int toLane)
    {
        if (!IsValidLane(fromLane) || !IsValidLane(toLane))
        {
            return false;
        }

        if (fromLane == toLane)
        {
            return false;
        }

        Vector2Int from = LaneToGridPosition(fromLane);
        Vector2Int to = LaneToGridPosition(toLane);

        int deltaX = Mathf.Abs(from.x - to.x);
        int deltaY = Mathf.Abs(from.y - to.y);

        return deltaX <= 1 && deltaY <= 1;
    }

    private static bool IsValidLane(int lane)
    {
        return lane >= 1 && lane <= 9;
    }

    private static Vector2Int LaneToGridPosition(int lane)
    {
        int zeroBased = lane - 1;

        int x = zeroBased % 3;
        int y = zeroBased / 3;

        return new Vector2Int(x, y);
    }
}