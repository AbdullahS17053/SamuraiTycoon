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
        Debug.Log($"👥 CapacityModule initialized - Level: {runtimeData.level}/{maxLevel}");
    }

    public override void OnBuildingTick(Building building, double deltaTime)
    {
        // Capacity affects income calculation in other modules
    }

    public override void OnUpgrade(Building building, int oldLevel, int newLevel)
    {
        Debug.Log($"🏗️ Capacity increased to {GetMaxCapacity(building)}");
    }

    public override void OnButtonClick(Building building)
    {
        if (IsMaxLevel())
        {
            Debug.Log($"🎯 {moduleName} is already at maximum level!");
            return;
        }

        // Hire a worker
        double cost = GetHireCost();
        if (GameManager.Instance != null && GameManager.Instance.Economy != null &&
            GameManager.Instance.Economy.SpendGold(cost))
        {
            if (currentWorkers < GetMaxCapacity(building))
            {
                currentWorkers++;
                runtimeData.timesActivated++;

                // Check if we reached max level (all worker slots filled)
                if (currentWorkers >= GetMaxCapacity(building))
                {
                    runtimeData.level = maxLevel;
                    Debug.Log($"🎉 {moduleName} reached maximum capacity!");
                    TriggerCompleted(building.Data.ID);
                }
                else
                {
                    runtimeData.level = currentWorkers;
                }

                Debug.Log($"👨‍🌾 Hired worker for {building.Config.DisplayName}. Total: {currentWorkers}/{GetMaxCapacity(building)}");
                TriggerProgress(building.Data.ID);
            }
            else
            {
                Debug.Log($"❌ Maximum capacity reached for {building.Config.DisplayName}");
                GameManager.Instance.Economy.AddGold(cost); // Refund
            }
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
        return Mathf.Min(baseCapacity + (building.Data.Level * capacityPerLevel), maxCapacity);
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
}