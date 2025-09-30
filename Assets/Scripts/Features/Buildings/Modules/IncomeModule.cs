using UnityEngine;

[CreateAssetMenu(fileName = "IncomeModule", menuName = "Samurai Tycoon/Modules/Income")]
public class IncomeModule : BuildingModule
{
    [Header("Income Settings")]
    public double baseIncomeBonus = 1.0;
    public float incomeMultiplier = 1.1f;
    public float incomeInterval = 1.0f;

    [Header("Effect Description")]
    public string effectDescription = "Increases building income";

    private double timer;

    public override void Initialize(BuildingModuleData data)
    {
        SetRuntimeData(data);
        if (runtimeData.level == 0) runtimeData.level = 1;
        Debug.Log($"💰 IncomeModule initialized - Level: {runtimeData.level}/{maxLevel}");
    }

    public override void OnBuildingTick(Building building, double deltaTime)
    {
        if (!isActive || IsMaxLevel()) return;

        timer += deltaTime;
        if (timer >= incomeInterval)
        {
            double income = CalculateIncome(building);
            if (GameManager.Instance != null && GameManager.Instance.Economy != null)
            {
                GameManager.Instance.Economy.AddGold(income);
            }
            timer = 0;
        }
    }

    public override void OnUpgrade(Building building, int oldLevel, int newLevel)
    {
        Debug.Log($"⬆️ IncomeModule upgraded: {oldLevel} → {newLevel}");
    }

    public override void OnButtonClick(Building building)
    {
        if (IsMaxLevel())
        {
            Debug.Log($"🎯 {moduleName} is already at maximum level!");
            return;
        }

        double cost = GetCurrentCost(runtimeData.timesActivated);

        if (GameManager.Instance != null && GameManager.Instance.Economy != null &&
            GameManager.Instance.Economy.SpendGold(cost))
        {
            runtimeData.timesActivated++;
            runtimeData.level++;

            Debug.Log($"💰 IncomeModule upgraded to level {runtimeData.level}/{maxLevel} for {building.Config.DisplayName}");
            TriggerProgress(building.Data.ID);

            if (IsMaxLevel())
            {
                Debug.Log($"🎉 {moduleName} reached maximum level!");
                TriggerCompleted(building.Data.ID);
            }
        }
    }

    public override string GetStatusText(Building building)
    {
        if (IsMaxLevel())
        {
            double income = CalculateIncome(building);
            return $"Income: +{income}/s\nLevel: {runtimeData.level}/{maxLevel}\nMAXED OUT!";
        }
        else
        {
            double income = CalculateIncome(building);
            double nextCost = GetCurrentCost(runtimeData.timesActivated);
            return $"Income: +{income}/s\nLevel: {runtimeData.level}/{maxLevel}\nNext: {nextCost:F0} Gold";
        }
    }

    public override string GetEffectDescription()
    {
        return effectDescription;
    }

    private double CalculateIncome(Building building)
    {
        return baseIncomeBonus * Mathf.Pow(incomeMultiplier, runtimeData.level - 1);
    }
}