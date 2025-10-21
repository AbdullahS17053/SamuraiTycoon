using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.UIElements;
using static AcademyBuilding;

public class AcademyBuilding : MonoBehaviour, IPointerClickHandler
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
        public GameObject visuals;
        public int unlockCost;
        public GameObject purchased;
        public bool locked = false;
        public float timeToUnlock = 60f;
    }

    void Start()
    {
        for (int i = 0; Buildings.Length > i; i++)
        {
            Buildings[i].building.locked = Buildings[i].locked;

            if (!Buildings[i].building.locked)
            {
                Buildings[i].purchased.SetActive(true);
                Buildings[i].visuals.SetActive(true);
            }
        }
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        if (WarManager.instance.isPanning)
            return;

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

    public void UnlockBuilding(int buildingIndex)
    {
        if (EconomyManager.Instance.SpendGold(Buildings[buildingIndex].unlockCost))
        {

            Buildings[buildingIndex].building.UnlockBuilding(Buildings[buildingIndex].timeToUnlock);
            Buildings[buildingIndex].purchased.SetActive(true);
            Buildings[buildingIndex].visuals.SetActive(true);
        }
        else
        {
            Debug.Log("❌ Not enough gold to unlock building");
        }
        AdManager.instance.OpenAd();
    }
}