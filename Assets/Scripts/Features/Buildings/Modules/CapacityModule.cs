using UnityEngine;

[CreateAssetMenu(fileName = "CapacityModule", menuName = "Samurai Tycoon/Modules/Capacity")]
public class CapacityModule : BuildingModule
{
    [Header("Capacity Settings")]
    public int baseCapacity = 1;
    public int capacityPerLevel = 1;
    public int maxCapacity = 10;

    [Header("Workers")]
    public int currentWorkers = 0;
    public double hireCost = 100;
    public float hireCostMultiplier = 1.2f;

    [Header("Effect Description")]
    public string effectDescription = "Increases workforce capacity";

    public override void Initialize(BuildingModuleData data)
    {
        SetRuntimeData(data);
        if (runtimeData.level == 0) runtimeData.level = 1;

        // Ensure current workers don't exceed capacity
        if (currentWorkers > GetMaxCapacity(null))
        {
            currentWorkers = GetMaxCapacity(null);
        }

        Debug.Log($"🔧 CapacityModule initialized - Level: {runtimeData.level}/{maxLevel}, Workers: {currentWorkers}");
    }

    public override void OnBuildingTick(Building building, double deltaTime)
    {
        // Capacity affects income calculation in other modules
        // No active ticking needed for capacity
    }

    public override void OnUpgrade(Building building, int oldLevel, int newLevel)
    {
        Debug.Log($"🔧 Capacity increased to {GetMaxCapacity(building)}");
        TriggerProgress(building.Data.ID);
    }

    public override void OnButtonClick(Building building)
    {
        if (IsMaxLevel())
        {
            Debug.Log($"🔧 {moduleName} is already at maximum level!");
            return;
        }

        double cost = GetHireCost();

        if (SpendGoldForActivation(cost, $"hiring worker for {building.Config.DisplayName}"))
        {
            if (currentWorkers < GetMaxCapacity(building))
            {
                currentWorkers++;
                runtimeData.timesActivated++;
                runtimeData.level = currentWorkers;

                // Update UI through events
                if (BuildingPanelUI3D.Instance != null)
                {
                    BuildingPanelUI3D.Instance.UpdateLevelSlider();
                    BuildingPanelUI3D.Instance.UpdateAllUI();
                }

                Debug.Log($"🔧 Hired worker for {building.Config.DisplayName}. Total: {currentWorkers}/{GetMaxCapacity(building)}");
                TriggerProgress(building.Data.ID);

                // Check if we reached max capacity
                if (currentWorkers >= GetMaxCapacity(building))
                {
                    runtimeData.level = maxLevel;
                    Debug.Log($"🔧 {moduleName} reached maximum capacity!");
                    TriggerCompleted(building.Data.ID);
                }
            }
            else
            {
                Debug.Log($"🔧 Maximum capacity reached for {building.Config.DisplayName}");
                // Refund since we can't hire
                GameManager.Instance.Economy.AddGold(cost);
            }
        }
        else
        {
            Debug.Log($"❌ Not enough gold to hire worker. Need: {cost}");
        }
    }

    public override string GetStatusText(Building building)
    {
        if (IsMaxLevel())
        {
            return $"Workers: {currentWorkers}/{GetMaxCapacity(building)}\nLevel: {runtimeData.level}/{maxLevel}\nMAXED OUT!";
        }
        else
        {
            double nextCost = GetHireCost();
            return $"Workers: {currentWorkers}/{GetMaxCapacity(building)}\nLevel: {runtimeData.level}/{maxLevel}\nHire: {nextCost:F0} Gold";
        }
    }

    public override string GetEffectDescription()
    {
        return effectDescription;
    }

    public int GetMaxCapacity(Building building)
    {
        int calculatedCapacity = baseCapacity + (building?.Data.Level ?? 1) * capacityPerLevel;
        return Mathf.Min(calculatedCapacity, maxCapacity);
    }

    private double GetHireCost()
    {
        return hireCost * Mathf.Pow(hireCostMultiplier, currentWorkers);
    }

    public bool HasAvailableCapacity(Building building)
    {
        return currentWorkers < GetMaxCapacity(building);
    }

    // Override cost methods for capacity module
    public override double GetCurrentCost(int timesActivated)
    {
        return GetHireCost();
    }

    public override bool CanActivate(Building building, double currentGold)
    {
        return !IsMaxLevel() && currentGold >= GetHireCost() && currentWorkers < GetMaxCapacity(building);
    }

    // Capacity module reaches max level when all worker slots are filled
    public override bool IsMaxLevel()
    {
        return currentWorkers >= maxCapacity || base.IsMaxLevel();
    }

    public float GetEfficiencyPercentage(Building building)
    {
        if (GetMaxCapacity(building) == 0) return 0f;
        return (float)currentWorkers / GetMaxCapacity(building);
    }
}