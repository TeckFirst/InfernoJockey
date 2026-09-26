using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private InputHandler inputHandler;
    [SerializeField] private GridManager gridManager;

    [Header("Movement Settings")]
    [Tooltip("Constant forward speed of the player.")]
    [SerializeField] private float forwardSpeed = 10f;
    [Tooltip("Speed at which the player slides laterally/vertically between lanes.")]
    [SerializeField] private float laneTransitionSpeed = 15f;

    private int currentLane = 5; // Default starting lane (center anchor)
    private int targetLane = 5;

    private void OnEnable()
    {
        if (inputHandler != null)
        {
            inputHandler.OnLaneSelected += HandleLaneSelected;
        }
    }

    private void OnDisable()
    {
        if (inputHandler != null)
        {
            inputHandler.OnLaneSelected -= HandleLaneSelected;
        }
    }

    private void Update()
    {
        ApplyMovement();
    }

    private void HandleLaneSelected(int requestedLane)
    {
        if (gridManager == null) return;

        // Uses your actual GridManager's IsDirectMove logic to validate reachability
        if (gridManager.IsDirectMove(currentLane, requestedLane))
        {
            targetLane = requestedLane;
            currentLane = targetLane; // Update reference point
        }
        else
        {
            Debug.Log($"Rejected move from Lane {currentLane} to Lane {requestedLane}");
        }
    }

    private void ApplyMovement()
    {
        // 1. Maintain constant forward momentum along the Z-axis
        float forwardMove = forwardSpeed * Time.deltaTime;

        if (gridManager != null)
        {
            // 2. Fetch the exact world position from your GridManager dictionary
            Vector3 targetLaneWorldPos = gridManager.GetLanePosition(targetLane);

            // 3. Smoothly interpolate X and Y toward the target lane while driving Z forward
            float newX = Mathf.Lerp(transform.position.x, targetLaneWorldPos.x, laneTransitionSpeed * Time.deltaTime);
            float newY = Mathf.Lerp(transform.position.y, targetLaneWorldPos.y, laneTransitionSpeed * Time.deltaTime);
            float newZ = transform.position.z + forwardMove;

            transform.position = new Vector3(newX, newY, newZ);
        }
        else
        {
            transform.position += new Vector3(0f, 0f, forwardMove);
        }
    }
}