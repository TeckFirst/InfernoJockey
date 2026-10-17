using UnityEngine;

public class CameraLaneMover : MonoBehaviour
{
    [SerializeField]
    private Transform[] laneCenters = new Transform[9];

    [SerializeField]
    private int startingLane = 5;

    public int CurrentLane { get; private set; }

    private void Start()
    {
        CurrentLane = startingLane;

        if (IsValidLane(CurrentLane))
        {
            transform.position =
                laneCenters[CurrentLane - 1].position;
        }
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

        transform.position =
            laneCenters[targetLane - 1].position;

        CurrentLane = targetLane;

        return true;
    }

    public bool IsCurrentLane(int lane)
    {
        return lane == CurrentLane;
    }

    public static bool IsAdjacent(int fromLane, int toLane)
    {
        if (!IsValidLane(fromLane) ||
            !IsValidLane(toLane))
        {
            return false;
        }

        if (fromLane == toLane)
        {
            return false;
        }

        Vector2Int from = LaneToGrid(fromLane);
        Vector2Int to = LaneToGrid(toLane);

        int deltaX = Mathf.Abs(from.x - to.x);
        int deltaY = Mathf.Abs(from.y - to.y);

        return deltaX <= 1 && deltaY <= 1;
    }

    private static bool IsValidLane(int lane)
    {
        return lane >= 1 && lane <= 9;
    }

    private static Vector2Int LaneToGrid(int lane)
    {
        int index = lane - 1;

        return new Vector2Int(
            index % 3,
            index / 3);
    }
}