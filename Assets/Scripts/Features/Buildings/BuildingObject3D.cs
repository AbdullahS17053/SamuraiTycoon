using UnityEngine;
using UnityEngine.EventSystems;

public class BuildingObject3D : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler
{
    [Header("Building Configuration")]
    public string BuildingID;

    private bool _isSelected = false;
    private Building _buildingData;

    void Start()
    {

        // Register with building manager
        BuildingManager3D.Instance.RegisterBuildingObject(this);
        Debug.Log($"🏯 3D BuildingObject registered: {BuildingID}");

        // Get building data
        _buildingData = BuildingManager3D.Instance.GetBuildingInstance(BuildingID);

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


        // Notify BuildingManager to show panel for this building
        BuildingManager3D.Instance.ShowBuildingPanel(BuildingID);

        // Set selected state
        SetSelected(true);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log($"🎯 Hovering over: {BuildingID}");
    }

    public void SetSelected(bool selected)
    {
        _isSelected = selected;
        Debug.Log($"🎯 Building {BuildingID} {(selected ? "selected" : "deselected")}");
    }

    void OnDestroy()
    {
        BuildingManager3D.Instance?.UnregisterBuildingObject(this);
    }
}