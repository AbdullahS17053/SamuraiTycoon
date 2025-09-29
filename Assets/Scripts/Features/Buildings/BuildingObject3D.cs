using UnityEngine;
using UnityEngine.EventSystems;

public class BuildingObject3D : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Building Configuration")]
    public string BuildingID;

    [Header("Visual Feedback")]
    public GameObject highlightEffect;
    public Material highlightMaterial;
    public Material normalMaterial;
    public float highlightScale = 1.1f;

    [Header("Animation")]
    public Animator animator;
    public float clickAnimationDuration = 0.3f;

    private Renderer _renderer;
    private Vector3 _originalScale;
    private bool _isSelected = false;
    private bool _isHovered = false;
    private Building _buildingData;

    void Start()
    {
        _renderer = GetComponent<Renderer>();
        _originalScale = transform.localScale;

        // Register with building manager
        BuildingManager3D.Instance.RegisterBuildingObject(this);
        Debug.Log($"🏯 3D BuildingObject registered: {BuildingID}");

        // Get building data
        _buildingData = BuildingManager3D.Instance.GetBuildingInstance(BuildingID);

        // Set up visual appearance
        UpdateVisuals();

        // Ensure we have a collider for raycasting
        if (GetComponent<Collider>() == null)
        {
            gameObject.AddComponent<BoxCollider>();
            Debug.Log($"📦 Added BoxCollider to {BuildingID}");
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log($"🖱️ 3D Building clicked: {BuildingID}");

        // Visual feedback
        StartCoroutine(ClickAnimation());

        // Notify BuildingManager to show panel for this building
        BuildingManager3D.Instance.ShowBuildingPanel(BuildingID);

        // Set selected state
        SetSelected(true);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        _isHovered = true;
        UpdateVisualState();
        Debug.Log($"🎯 Hovering over: {BuildingID}");
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _isHovered = false;
        UpdateVisualState();
    }

    private System.Collections.IEnumerator ClickAnimation()
    {
        if (animator != null)
        {
            animator.SetTrigger("Click");
        }
        else
        {
            // Simple scale animation
            transform.localScale = _originalScale * 0.9f;
            yield return new WaitForSeconds(clickAnimationDuration / 2);
            transform.localScale = _originalScale;
        }
    }

    public void SetSelected(bool selected)
    {
        _isSelected = selected;
        UpdateVisualState();
        Debug.Log($"🎯 Building {BuildingID} {(selected ? "selected" : "deselected")}");
    }

    private void UpdateVisualState()
    {
        // Update material based on state
        if (_renderer != null)
        {
            if (_isSelected)
            {
                _renderer.material = highlightMaterial;
                transform.localScale = _originalScale * highlightScale;
            }
            else if (_isHovered)
            {
                _renderer.material = highlightMaterial;
                transform.localScale = _originalScale * (highlightScale * 0.5f);
            }
            else
            {
                _renderer.material = normalMaterial;
                transform.localScale = _originalScale;
            }
        }

        // Update highlight effect
        if (highlightEffect != null)
        {
            highlightEffect.SetActive(_isSelected || _isHovered);
        }
    }

    public void UpdateVisuals()
    {
        if (_buildingData == null) return;

        // Update building appearance based on level
        int level = _buildingData.Data.Level;

        // Visual changes based on level
        if (level > 0)
        {
            // Building is active - you can add level-based visual changes here
            Debug.Log($"🎨 Updating 3D visuals for {BuildingID} - Level {level}");

            // Example: Change color based on level
            if (_renderer != null && level > 1)
            {
                Color levelColor = Color.Lerp(Color.white, Color.yellow, level / 10f);
                _renderer.material.color = levelColor;
            }
        }
    }

    void OnDestroy()
    {
        BuildingManager3D.Instance?.UnregisterBuildingObject(this);
    }
}