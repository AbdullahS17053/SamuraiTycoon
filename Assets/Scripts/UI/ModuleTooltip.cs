using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class ModuleTooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Tooltip References")]
    public GameObject tooltipPanel;
    public TextMeshProUGUI tooltipTitle;
    public TextMeshProUGUI tooltipDescription;

    [Header("Tooltip Settings")]
    public Vector2 offset = new Vector2(10, 10);
    public float showDelay = 0.5f;

    private string _title;
    private string _description;
    private bool _isHovering = false;
    private float _hoverTimer = 0f;
    private Canvas _parentCanvas;

    void Start()
    {
        // Find the parent canvas
        _parentCanvas = GetComponentInParent<Canvas>();

        // If tooltip panel is not assigned, try to find it in the scene
        if (tooltipPanel == null)
        {
            // Look for a tooltip panel in the canvas
            var tooltip = FindObjectOfType<ModuleTooltipPanel>();
            if (tooltip != null)
            {
                tooltipPanel = tooltip.gameObject;
                tooltipTitle = tooltip.titleText;
                tooltipDescription = tooltip.descriptionText;
            }
        }

        // Hide tooltip initially
        if (tooltipPanel != null)
        {
            tooltipPanel.SetActive(false);
        }
    }

    void Update()
    {
        if (_isHovering && tooltipPanel != null)
        {
            _hoverTimer += Time.deltaTime;

            if (_hoverTimer >= showDelay && !tooltipPanel.activeInHierarchy)
            {
                ShowTooltip();
            }

            // Update tooltip position to follow mouse
            if (tooltipPanel.activeInHierarchy)
            {
                UpdateTooltipPosition();
            }
        }
    }

    public void SetTooltip(string title, string description)
    {
        _title = title;
        _description = description;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        _isHovering = true;
        _hoverTimer = 0f;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _isHovering = false;
        HideTooltip();
    }

    private void ShowTooltip()
    {
        if (tooltipPanel != null)
        {
            // Set tooltip content
            if (tooltipTitle != null)
                tooltipTitle.text = _title;

            if (tooltipDescription != null)
                tooltipDescription.text = _description;

            tooltipPanel.SetActive(true);
            UpdateTooltipPosition();
        }
        else
        {
            // Fallback: log to console
            Debug.Log($"🔍 {_title}: {_description}");
        }
    }

    private void HideTooltip()
    {
        if (tooltipPanel != null)
        {
            tooltipPanel.SetActive(false);
        }
    }

    private void UpdateTooltipPosition()
    {
        if (tooltipPanel == null || _parentCanvas == null) return;

        Vector2 mousePosition = Input.mousePosition;
        Vector2 newPosition = mousePosition + offset;

        // Convert screen position to canvas position
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _parentCanvas.transform as RectTransform,
            newPosition,
            _parentCanvas.worldCamera,
            out Vector2 canvasPos
        );

        tooltipPanel.transform.localPosition = canvasPos;

        // Keep tooltip within screen bounds
        RectTransform tooltipRect = tooltipPanel.GetComponent<RectTransform>();
        RectTransform canvasRect = _parentCanvas.GetComponent<RectTransform>();

        if (tooltipRect != null && canvasRect != null)
        {
            Vector3[] canvasCorners = new Vector3[4];
            Vector3[] tooltipCorners = new Vector3[4];

            canvasRect.GetWorldCorners(canvasCorners);
            tooltipRect.GetWorldCorners(tooltipCorners);

            // Adjust position if tooltip goes off-screen
            float rightEdge = tooltipCorners[2].x;
            float leftEdge = tooltipCorners[0].x;
            float topEdge = tooltipCorners[1].y;
            float bottomEdge = tooltipCorners[0].y;

            float canvasRight = canvasCorners[2].x;
            float canvasLeft = canvasCorners[0].x;
            float canvasTop = canvasCorners[1].y;
            float canvasBottom = canvasCorners[0].y;

            Vector3 adjustedPosition = tooltipPanel.transform.position;

            if (rightEdge > canvasRight)
            {
                adjustedPosition.x -= (rightEdge - canvasRight);
            }
            if (leftEdge < canvasLeft)
            {
                adjustedPosition.x += (canvasLeft - leftEdge);
            }
            if (topEdge > canvasTop)
            {
                adjustedPosition.y -= (topEdge - canvasTop);
            }
            if (bottomEdge < canvasBottom)
            {
                adjustedPosition.y += (canvasBottom - bottomEdge);
            }

            tooltipPanel.transform.position = adjustedPosition;
        }
    }

    void OnDisable()
    {
        HideTooltip();
        _isHovering = false;
        _hoverTimer = 0f;
    }

    void OnDestroy()
    {
        HideTooltip();
    }
}

// Helper class for global tooltip panel
public class ModuleTooltipPanel : MonoBehaviour
{
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI descriptionText;
}