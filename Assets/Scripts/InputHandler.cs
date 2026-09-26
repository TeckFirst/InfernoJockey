using UnityEngine;
using System;

public class InputHandler : MonoBehaviour
{
    // Event triggered when a valid lane is tapped, passing the target lane index (1-9)
    public event Action<int> OnLaneSelected;

    private void Update()
    {
        // Support both Unity Editor (Mouse) and Mobile Touch
#if UNITY_EDITOR || UNITY_STANDALONE
        if (Input.GetMouseButtonDown(0))
        {
            ProcessInput(Input.mousePosition);
        }
#else
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                ProcessInput(touch.position);
            }
        }
#endif
    }

    private void ProcessInput(Vector2 screenPosition)
    {
        float screenWidth = Screen.width;
        if (screenWidth <= 0) return;

        // Normalize screen coordinate between 0.0 and 1.0
        float normalizedX = Mathf.Clamp01(screenPosition.x / screenWidth);

        // Map normalized width to 9 distinct lanes (1 to 9)
        int laneIndex = Mathf.FloorToInt(normalizedX * 9) + 1;
        laneIndex = Mathf.Clamp(laneIndex, 1, 9);

        // Broadcast the selection event
        OnLaneSelected?.Invoke(laneIndex);
    }
}