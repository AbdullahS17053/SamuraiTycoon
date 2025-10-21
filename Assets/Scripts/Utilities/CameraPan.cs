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

    [Header("Tap Settings")]
    public float tapThresholdTime = 0.2f;     // Seconds
    public float tapThresholdDistance = 10f;  // Pixels

    private Vector3 targetPosition;
    private Vector3 lastPanPosition;
    private int panFingerId = -1;
    private bool isPanning = false;

    private float touchStartTime;
    private Vector2 touchStartPos;

    private Camera cam;

    void Awake()
    {
        cam = GetComponent<Camera>();
        if (cam == null)
            cam = Camera.main;

        targetPosition = transform.position;
    }

    void Update()
    {
        HandleInput();
        SmoothMoveToTarget();
    }

    void HandleInput()
    {
        if (useTouchInput && Input.touchCount > 0)
        {
            HandleTouchInput();
        }
        else if (useMouseInput)
        {
            HandleMouseInput();
        }
    }

    // ------------------------
    // TOUCH INPUT
    // ------------------------
    void HandleTouchInput()
    {
        foreach (Touch touch in Input.touches)
        {
            switch (touch.phase)
            {
                case TouchPhase.Began:
                    if (panFingerId == -1)
                    {
                        panFingerId = touch.fingerId;
                        lastPanPosition = GetWorldPosition(touch.position);
                        touchStartPos = touch.position;
                        touchStartTime = Time.time;
                        WarManager.instance.isPanning = false;
                    }
                    break;

                case TouchPhase.Moved:
                    if (touch.fingerId == panFingerId)
                    {
                        float moveDistance = (touch.position - touchStartPos).magnitude;
                        if (moveDistance > tapThresholdDistance)
                        {
                            WarManager.instance.isPanning = true; // Movement is large enough to count as a pan
                            Vector3 currentPanPosition = GetWorldPosition(touch.position);
                            Vector3 offset = lastPanPosition - currentPanPosition;
                            PanCamera(offset);
                            lastPanPosition = currentPanPosition;
                        }
                    }
                    break;

                case TouchPhase.Ended:
                case TouchPhase.Canceled:
                    if (touch.fingerId == panFingerId)
                    {
                        float touchDuration = Time.time - touchStartTime;
                        float totalDistance = (touch.position - touchStartPos).magnitude;

                        // If it's not a pan and quick enough, treat it as a tap
                        if (!isPanning && touchDuration <= tapThresholdTime && totalDistance < tapThresholdDistance)
                        {
                            OnTap(touch.position);
                        }

                        panFingerId = -1;
                        WarManager.instance.isPanning = false;
                    }
                    break;
            }
        }
    }

    // ------------------------
    // MOUSE INPUT
    // ------------------------
    void HandleMouseInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            lastPanPosition = GetWorldPosition(Input.mousePosition);
            touchStartPos = Input.mousePosition;
            touchStartTime = Time.time;
            WarManager.instance.isPanning = false;
        }
        else if (Input.GetMouseButton(0))
        {
            Vector3 currentPanPosition = GetWorldPosition(Input.mousePosition);
            Vector3 offset = lastPanPosition - currentPanPosition;

            if (offset.magnitude > mousePanThreshold)
            {
                WarManager.instance.isPanning = true;
                PanCamera(offset);
                lastPanPosition = currentPanPosition;
            }
        }
        else if (Input.GetMouseButtonUp(0))
        {
            float duration = Time.time - touchStartTime;
            Vector2 distance = (Vector2)Input.mousePosition - touchStartPos;

            if (!isPanning && duration <= tapThresholdTime && distance.magnitude < tapThresholdDistance)
            {
                OnTap(Input.mousePosition);
            }

            WarManager.instance.isPanning = false;
        }


        // Optional middle mouse pan
        if (Input.GetMouseButton(2))
        {
            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");

            Vector3 offset = new Vector3(mouseX, 0, mouseY) * panSpeed * 2f;
            PanCamera(offset);
        }
    }

    // ------------------------
    // CAMERA MOVEMENT
    // ------------------------
    void PanCamera(Vector3 offset)
    {
        float xMultiplier = invertX ? -1f : 1f;
        float yMultiplier = invertY ? -1f : 1f;

        Vector3 newPosition = targetPosition + new Vector3(
            offset.x * panSpeed * xMultiplier,
            0,
            offset.z * panSpeed * yMultiplier
        );

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
        Plane plane = new Plane(Vector3.up, Vector3.zero);
        Ray ray = cam.ScreenPointToRay(screenPosition);

        if (plane.Raycast(ray, out float distance))
        {
            return ray.GetPoint(distance);
        }

        return transform.position;
    }

    // ------------------------
    // TAP HANDLER
    // ------------------------
    protected virtual void OnTap(Vector3 screenPosition)
    {
        Debug.Log($"Tap detected at screen position: {screenPosition}");
        // Example:
        // Ray ray = cam.ScreenPointToRay(screenPosition);
        // if (Physics.Raycast(ray, out RaycastHit hit))
        // {
        //     Debug.Log($"Tapped object: {hit.collider.name}");
        // }
    }

    // ------------------------
    // PUBLIC UTILITY METHODS
    // ------------------------
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
        transform.position = newPosition;
    }

    public void SetBounds(Vector2 newXBounds, Vector2 newZBounds)
    {
        xBounds = newXBounds;
        zBounds = newZBounds;

        Vector3 clampedPosition = transform.position;
        clampedPosition.x = Mathf.Clamp(clampedPosition.x, xBounds.x, xBounds.y);
        clampedPosition.z = Mathf.Clamp(clampedPosition.z, zBounds.x, zBounds.y);
        SetTargetPosition(clampedPosition);
    }

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

    [ContextMenu("Set Bounds From Current Scene")]
    void SetBoundsFromScene()
    {
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
