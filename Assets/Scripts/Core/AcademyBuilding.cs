using UnityEngine;
using System.Collections.Generic;

public class AcademyBuilding : MonoBehaviour
{
    [Header("Academy Configuration")]
    public string academyName = "Military Academy";
    public List<BuildingUnlock> availableBuildings = new List<BuildingUnlock>();

    [Header("UI Reference")]
    public GameObject academyPanel; // Drag your UI panel here

    private bool isPanelOpen = false;

    [System.Serializable]
    public class BuildingUnlock
    {
        public TrainingBuilding building;
        public string displayName;
        public string description;
        public double unlockCost;
        public bool isUnlocked = false;
        public int requiredLevel = 1;
    }

    void Start()
    {
        // First building is unlocked by default
        if (availableBuildings.Count > 0)
        {
            availableBuildings[0].isUnlocked = true;
            if (availableBuildings[0].building != null)
            {
                availableBuildings[0].building.UnlockBuilding();
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

        if (isPanelOpen)
        {
            RefreshAcademyUI();
            Debug.Log("🏛️ Academy panel opened");
        }
        else
        {
            Debug.Log("🏛️ Academy panel closed");
        }
    }

    void RefreshAcademyUI()
    {
        // This would update your UI with current building states
        // You'll need to implement this based on your UI system
        Debug.Log("🔄 Refreshing academy UI");
    }

    public void UnlockBuilding(int buildingIndex)
    {
        if (buildingIndex < 0 || buildingIndex >= availableBuildings.Count)
        {
            Debug.LogError("❌ Invalid building index");
            return;
        }

        BuildingUnlock buildingUnlock = availableBuildings[buildingIndex];

        if (buildingUnlock.isUnlocked)
        {
            Debug.Log("ℹ️ Building already unlocked");
            return;
        }

        // Check if player has enough gold
        if (GameManager.Instance != null && GameManager.Instance.Economy != null)
        {
            EconomyManager economy = GameManager.Instance.Economy;

            if (economy.Gold >= buildingUnlock.unlockCost)
            {
                economy.SpendGold(buildingUnlock.unlockCost);
                buildingUnlock.isUnlocked = true;

                if (buildingUnlock.building != null)
                {
                    buildingUnlock.building.UnlockBuilding();
                }

                // Refresh troop manager building list
                if (TroopManager.Instance != null)
                {
                    TroopManager.Instance.RefreshTrainingBuildings();
                }

                Debug.Log($"🔓 Unlocked {buildingUnlock.displayName} for {buildingUnlock.unlockCost} gold");
                RefreshAcademyUI();
            }
            else
            {
                Debug.Log("❌ Not enough gold to unlock building");
            }
        }
    }

    public List<BuildingUnlock> GetAvailableBuildings()
    {
        return new List<BuildingUnlock>(availableBuildings);
    }

    public BuildingUnlock GetNextBuildableBuilding()
    {
        foreach (var building in availableBuildings)
        {
            if (!building.isUnlocked)
            {
                return building;
            }
        }
        return null;
    }

    [ContextMenu("Debug Academy Info")]
    public void DebugAcademyInfo()
    {
        Debug.Log($"🏛️ {academyName} - Building Unlocks:");
        foreach (var building in availableBuildings)
        {
            string status = building.isUnlocked ? "UNLOCKED" : $"LOCKED - Cost: {building.unlockCost}";
            Debug.Log($"- {building.displayName}: {status}");
        }
    }
}