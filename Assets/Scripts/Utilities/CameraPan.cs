using UnityEngine;

public class CameraPan : MonoBehaviour
{
    [Header("Pan Settings")]
    public float panSpeed = 0.1f;
    public float panSmoothing = 5f;
    public bool invertX = false;
    public bool invertY = false;

    [Header("Bounds Settings")]
    public bool useBounds = true;
    public Vector2 xBounds = new Vector2(-10f, 10f);
    public Vector2 zBounds = new Vector2(-10f, 10f);

    [Header("Input Settings")]
    public bool useTouchInput = true;
    public bool useMouseInput = true;
    public float mousePanThreshold = 0.1f; // Dead zone for mouse movement

    private Vector3 targetPosition;
    private Vector3 lastPanPosition;
    private int panFingerId = -1; // Touch finger ID
    private bool isPanning = false;
    private Camera cam;

    void Awake()
    {
        cam = GetComponent<Camera>();
        if (cam == null)
        {
            cam = Camera.main;
        }

        targetPosition = transform.position;
    }

    void Update()
    {
        HandleInput();
        SmoothMoveToTarget();
    }

    void HandleInput()
    {
        // Touch input (mobile)
        if (useTouchInput && Input.touchCount > 0)
        {
            HandleTouchInput();
        }
        // Mouse input (desktop)
        else if (useMouseInput)
        {
            HandleMouseInput();
        }
    }

    void HandleTouchInput()
    {
        Touch touch = Input.GetTouch(0);

        switch (touch.phase)
        {
            case TouchPhase.Began:
                if (panFingerId == -1)
                {
                    panFingerId = touch.fingerId;
                    lastPanPosition = GetWorldPosition(touch.position);
                    isPanning = true;
                }
                break;

            case TouchPhase.Moved:
                if (touch.fingerId == panFingerId)
                {
                    Vector3 currentPanPosition = GetWorldPosition(touch.position);
                    Vector3 offset = lastPanPosition - currentPanPosition;

                    PanCamera(offset);

                    lastPanPosition = currentPanPosition;
                }
                break;

            case TouchPhase.Ended:
            case TouchPhase.Canceled:
                if (touch.fingerId == panFingerId)
                {
                    panFingerId = -1;
                    isPanning = false;
                }
                break;
        }
    }

    void HandleMouseInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            lastPanPosition = GetWorldPosition(Input.mousePosition);
            isPanning = true;
        }
        else if (Input.GetMouseButton(0) && isPanning)
        {
            Vector3 currentPanPosition = GetWorldPosition(Input.mousePosition);
            Vector3 offset = lastPanPosition - currentPanPosition;

            // Apply threshold to prevent jittery movement
            if (offset.magnitude > mousePanThreshold)
            {
                PanCamera(offset);
                lastPanPosition = currentPanPosition;
            }
        }
        else if (Input.GetMouseButtonUp(0))
        {
            isPanning = false;
        }

        // Mouse wheel pan (alternative method)
        if (Input.GetMouseButton(2)) // Middle mouse button
        {
            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");

            Vector3 offset = new Vector3(mouseX, 0, mouseY) * panSpeed * 2f;
            PanCamera(offset);
        }
    }

    void PanCamera(Vector3 offset)
    {
        // Apply inversion settings
        float xMultiplier = invertX ? -1f : 1f;
        float yMultiplier = invertY ? -1f : 1f;

        // Calculate new position
        Vector3 newPosition = targetPosition + new Vector3(
            offset.x * panSpeed * xMultiplier,
            0,
            offset.z * panSpeed * yMultiplier
        );

        // Apply bounds
        if (useBounds)
        {
            newPosition.x = Mathf.Clamp(newPosition.x, xBounds.x, xBounds.y);
            newPosition.z = Mathf.Clamp(newPosition.z, zBounds.x, zBounds.y);
        }

        targetPosition = newPosition;
    }

    void SmoothMoveToTarget()
    {
        if (Vector3.Distance(transform.position, targetPosition) > 0.01f)
        {
            transform.position = Vector3.Lerp(
                transform.position,
                targetPosition,
                Time.deltaTime * panSmoothing
            );
        }
    }

    Vector3 GetWorldPosition(Vector3 screenPosition)
    {
        // Create a plane at y=0 (or whatever your ground level is)
        Plane plane = new Plane(Vector3.up, Vector3.zero);

        Ray ray = cam.ScreenPointToRay(screenPosition);

        if (plane.Raycast(ray, out float distance))
        {
            return ray.GetPoint(distance);
        }

        // Fallback: return current world position if raycast fails
        return transform.position;
    }

    // Public methods to control camera programmatically
    public void SetTargetPosition(Vector3 newPosition)
    {
        if (useBounds)
        {
            newPosition.x = Mathf.Clamp(newPosition.x, xBounds.x, xBounds.y);
            newPosition.z = Mathf.Clamp(newPosition.z, zBounds.x, zBounds.y);
        }
        targetPosition = newPosition;
    }

    public void MoveToPosition(Vector3 newPosition)
    {
        SetTargetPosition(newPosition);
        transform.position = newPosition; // Instant move
    }

    public void SetBounds(Vector2 newXBounds, Vector2 newZBounds)
    {
        xBounds = newXBounds;
        zBounds = newZBounds;

        // Clamp current position to new bounds
        Vector3 clampedPosition = transform.position;
        clampedPosition.x = Mathf.Clamp(clampedPosition.x, xBounds.x, xBounds.y);
        clampedPosition.z = Mathf.Clamp(clampedPosition.z, zBounds.x, zBounds.y);
        SetTargetPosition(clampedPosition);
    }

    // Debug visualization for bounds
    void OnDrawGizmosSelected()
    {
        if (useBounds)
        {
            Gizmos.color = Color.red;
            Vector3 center = new Vector3((xBounds.x + xBounds.y) * 0.5f, 0, (zBounds.x + zBounds.y) * 0.5f);
            Vector3 size = new Vector3(xBounds.y - xBounds.x, 0.1f, zBounds.y - zBounds.x);
            Gizmos.DrawWireCube(center, size);
        }
    }

    // Context menu methods for easy setup
    [ContextMenu("Set Bounds From Current Scene")]
    void SetBoundsFromScene()
    {
        // This is a helper method - you might want to customize this based on your scene
        xBounds = new Vector2(-50f, 50f);
        zBounds = new Vector2(-50f, 50f);
        Debug.Log($"Camera bounds set to: X({xBounds.x}, {xBounds.y}), Z({zBounds.x}, {zBounds.y})");
    }

    [ContextMenu("Reset Camera Position")]
    void ResetCameraPosition()
    {
        targetPosition = Vector3.zero;
        if (useBounds)
        {
            targetPosition.x = Mathf.Clamp(targetPosition.x, xBounds.x, xBounds.y);
            targetPosition.z = Mathf.Clamp(targetPosition.z, zBounds.x, zBounds.y);
        }
        transform.position = targetPosition;
    }
}