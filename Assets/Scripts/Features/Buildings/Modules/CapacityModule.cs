using UnityEngine;
using UnityEngine.UIElements;

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

    public override void Initialize(BuildingModuleData data)
    {
        runtimeData = data;
        Debug.Log($"👥 CapacityModule initialized");
    }

    public override void OnBuildingTick(Building building, double deltaTime)
    {
        // Capacity affects income calculation in other modules
        // This module mainly manages state
    }

    public override void OnUpgrade(Building building, int oldLevel, int newLevel)
    {
        Debug.Log($"🏗️ Capacity increased to {GetMaxCapacity(building)}");
    }

    public override void OnButtonClick(Building building)
    {
        // Hire a worker
        double cost = GetHireCost();
        if (GameManager.Instance.Economy.SpendGold(cost))
        {
            if (currentWorkers < GetMaxCapacity(building))
            {
                currentWorkers++;
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
        return $"Workers: {currentWorkers}/{GetMaxCapacity(building)}\nHire: {GetHireCost()} gold";
    }

    public int GetMaxCapacity(Building building)
    {
        return baseCapacity + (building.Data.Level * capacityPerLevel);
    }

    private double GetHireCost()
    {
        return hireCost * Mathf.Pow(hireCostMultiplier, currentWorkers);
    }

    public bool HasAvailableCapacity(Building building)
    {
        return currentWorkers < GetMaxCapacity(building);
    }
}