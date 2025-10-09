using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.UI;
using static AcademyBuilding;

public class AcademyBuilding : MonoBehaviour
{
    [Header("Academy Configuration")]
    public string academyName = "Military Academy";
    public BuildingUnlock[] Buildings;

    [Header("UI Reference")]
    public GameObject academyPanel;

    private bool isPanelOpen = false;

    [System.Serializable]
    public class BuildingUnlock
    {
        public TrainingBuilding building;
        public int unlockCost;
        public GameObject purchased;
        public bool locked = false;
    }

    void Start()
    {
        for (int i = 0; Buildings.Length > i; i++)
        {
            Buildings[i].building.locked = Buildings[i].locked;

            if (!Buildings[i].building.locked)
            {
                Buildings[i].purchased.SetActive(true);
            }
        }
    }

    void OnMouseDown()
    {
        ToggleAcademyPanel();
    }

    public void ToggleAcademyPanel()
    {
        isPanelOpen = !isPanelOpen;

        if (academyPanel != null)
        {
            academyPanel.SetActive(isPanelOpen);
        }
    }

    void RefreshAcademyUI()
    {
        for (int i = 0; Buildings.Length >= i; i++)
        {
            if (!Buildings[i].building.locked)
            {
                Buildings[i].purchased.SetActive(true);
            }
        }
    }

    public void UnlockBuilding(int buildingIndex)
    {
        if (EconomyManager.Instance.SpendGold(Buildings[buildingIndex].unlockCost))
        {

            Buildings[buildingIndex].building.UnlockBuilding();
            RefreshAcademyUI();
        }
        else
        {
            Debug.Log("❌ Not enough gold to unlock building");
        }
    }
}