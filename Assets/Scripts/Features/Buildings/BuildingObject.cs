using UnityEngine;
using UnityEngine.EventSystems;

public class BuildingObject : MonoBehaviour, IPointerClickHandler
{
    [Header("Building Configuration")]
    public string BuildingID;

    [Header("Visual Feedback")]
    public GameObject highlightEffect;
    public Animator buildingAnimator;

    private Building _buildingData;
    private bool _isSelected = false;

    void Start()
    {
        // Register with building manager
        BuildingManager.Instance.RegisterBuildingObject(this);
        Debug.Log($"🏯 BuildingObject registered: {BuildingID}");

        // Get building data
        _buildingData = BuildingManager.Instance.GetBuildingInstance(BuildingID);

        // Set up visual appearance
        UpdateVisuals();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log($"🖱️ Building clicked: {BuildingID}");

        // Notify BuildingManager to show panel for this building
        BuildingManager.Instance.ShowBuildingPanel(BuildingID);

        // Visual feedback
        SetSelected(true);
    }

    public void SetSelected(bool selected)
    {
        _isSelected = selected;

        if (highlightEffect != null)
            highlightEffect.SetActive(selected);

        if (buildingAnimator != null)
            buildingAnimator.SetBool("Selected", selected);

        Debug.Log($"🎯 Building {BuildingID} {(selected ? "selected" : "deselected")}");
    }

    public void UpdateVisuals()
    {
        if (_buildingData == null) return;

        // Update building appearance based on level
        int level = _buildingData.Data.Level;

        // You can add level-based visual changes here
        if (level > 0)
        {
            // Building is active - maybe change color, enable particles, etc.
            Debug.Log($"🎨 Updating visuals for {BuildingID} - Level {level}");
        }
    }

    void OnDestroy()
    {
        BuildingManager.Instance?.UnregisterBuildingObject(this);
    }
}