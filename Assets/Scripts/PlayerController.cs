using System;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Lane & Camera Settings")]
    [Tooltip("The current lane the player/gun is aimed at (0 to 8).")]
    [SerializeField] private int currentLane = 0;

    [Tooltip("Reference to the Camera that will move between locations.")]
    [SerializeField] private Transform targetCameraTransform;

    [Tooltip("Speed at which the camera moves to a new lane position.")]
    [SerializeField] private float cameraMoveSpeed = 10f;

    [System.Serializable]
    public struct LaneConfig
    {
        [Tooltip("Descriptive name for this lane/button (e.g., 'Lane 1', 'Top-Left').")]
        public string laneName;
        [Tooltip("The in-game GameObject/Transform representing the camera position for this lane.")]
        public Transform cameraPosition;
    }

    [Header("9-Button & Location Mapping")]
    [Tooltip("Map your 9 buttons/lanes here. Index 0 corresponds to button 0, etc.")]
    [SerializeField] private LaneConfig[] lanes = new LaneConfig[9];

    // Event for future classes (like an Inventory or AmmoTracker) to subscribe to
    public event Action OnGunFired;

    private Coroutine cameraMoveCoroutine;

    /// <summary>
    /// Call this method when a button is pressed, passing its corresponding index (0 through 8).
    /// </summary>
    /// <param name="buttonIndex">The index of the pressed button (0-8).</param>
    public void OnButtonPressed(int buttonIndex)
    {
        // Validate index range
        if (buttonIndex < 0 || buttonIndex >= lanes.Length)
        {
            Debug.LogWarning($"Button index {buttonIndex} is out of range (0-8).");
            return;
        }

        if (currentLane == buttonIndex)
        {
            // If already looking at the lane, fire the gun
            FireGun();
        }
        else
        {
            // If not looking at the lane, move the camera there and update the current lane
            MoveCameraToLane(buttonIndex);
            currentLane = buttonIndex;
        }
    }

    private void FireGun()
    {
        Debug.Log($"Gun fired at Lane {currentLane}!");

        // Trigger the event so future ammo-tracking classes can listen and update
        OnGunFired?.Invoke();

        // TODO: Add your visual/audio muzzle flash, projectile spawning, etc., here.
    }

    private void MoveCameraToLane(int laneIndex)
    {
        if (lanes[laneIndex].cameraPosition == null)
        {
            Debug.LogError($"Camera position Transform is missing for lane {laneIndex} ({lanes[laneIndex].laneName})!");
            return;
        }

        Transform targetPos = lanes[laneIndex].cameraPosition;

        // If you want instant snapping, use:
        // if (targetCameraTransform != null) targetCameraTransform.SetPositionAndRotation(targetPos.position, targetPos.rotation);

        // Smooth transition implementation using a Coroutine:
        if (cameraMoveCoroutine != null)
        {
            StopCoroutine(cameraMoveCoroutine);
        }
        cameraMoveCoroutine = StartCoroutine(SmoothMoveCamera(targetPos));
    }

    private System.Collections.IEnumerator SmoothMoveCamera(Transform target)
    {
        if (targetCameraTransform == null) yield break;

        Vector3 startPos = targetCameraTransform.position;
        Quaternion startRot = targetCameraTransform.rotation;
        float elapsedTime = 0f;
        float duration = 1f / cameraMoveSpeed; // Adjust feel via cameraMoveSpeed

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / duration);

            targetCameraTransform.position = Vector3.Lerp(startPos, target.position, t);
            targetCameraTransform.rotation = Quaternion.Slerp(startRot, target.rotation, t);

            yield return null;
        }

        targetCameraTransform.SetPositionAndRotation(target.position, target.rotation);
    }
}