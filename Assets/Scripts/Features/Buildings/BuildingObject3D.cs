using UnityEngine;
using UnityEngine.EventSystems;

public class BuildingObject3D : MonoBehaviour, IPointerClickHandler
{
    private TrainingBuilding trainingBuilding;

    private void Awake()
    {
        trainingBuilding = GetComponent<TrainingBuilding>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (WarManager.instance.isPanning)
            return;

        if (trainingBuilding.locked)
        {
            Debug.Log("Building is locked");
        }
        else
        {
            BuildingManager3D.Instance.ShowBuildingPanel(trainingBuilding);
        }
    }
}