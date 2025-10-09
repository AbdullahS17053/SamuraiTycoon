using System.Collections.Generic;
using UnityEngine;

public class BuildingManager3D : MonoBehaviour
{
    public static BuildingManager3D Instance { get; private set; }

    [Header("Building Configs - DRAG BUILDINGS HERE!")]
    public TrainingBuilding[] buildings;
    private TrainingBuilding currentBuilding;

    [Header("UI References")]
    public BuildingPanelUI3D buildingPanel;

    private void Awake()
    {
        Instance = this;
    }

    public TrainingBuilding GetBuilding(int index)
    {
        return buildings[index];
    }

    public void ShowBuildingPanel(TrainingBuilding building)
    {
        buildingPanel.gameObject.SetActive(true);
        buildingPanel.OnBuildingUpgraded(building);
    }

    public void HideBuildingPanel()
    {
        buildingPanel.gameObject.SetActive(false);
    }

    public void UpdateBuildingPanel()
    {
        buildingPanel.OnBuildingUpgraded(null);
    }
}