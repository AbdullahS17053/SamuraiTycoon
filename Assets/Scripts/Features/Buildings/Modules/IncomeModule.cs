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

    private double _timer;
    private double _lastIncomeGenerated = 0;

    public override void Initialize(BuildingModuleData data)
    {
        SetRuntimeData(data);
        if (runtimeData.level == 0) runtimeData.level = 1;
        Debug.Log($"🔧 IncomeModule initialized - Level: {runtimeData.level}/{maxLevel}");
    }

    public override void OnBuildingTick(Building building, double deltaTime)
    {
        if (!isActive || IsMaxLevel() || GameManager.Instance == null || !GameManager.Instance.passiveIncomeActive) return;

        _timer += deltaTime;
        if (_timer >= incomeInterval)
        {
            double income = CalculateIncome(building);
            if (GameManager.Instance.Economy != null)
            {
                GameManager.Instance.Economy.AddGold(income);
                _lastIncomeGenerated = income;
            }
            _timer = 0;
        }
    }

    public override void OnUpgrade(Building building, int oldLevel, int newLevel)
    {
        Debug.Log($"🔧 IncomeModule upgraded: {oldLevel} → {newLevel}");
        TriggerProgress(building.Data.ID);
    }

    public override void OnButtonClick(Building building)
    {
        if (GameManager.Instance == null || GameManager.Instance.Economy == null)
        {
            Debug.LogError("❌ Economy system not available!");
            return;
        }

        if (IsMaxLevel())
        {
            Debug.Log($"🔧 {moduleName} is already at maximum level!");
            return;
        }

        double cost = GetCurrentCost(runtimeData.timesActivated);

        if (SpendGoldForActivation(cost, $"upgrading income module for {building.Config.DisplayName}"))
        {
            runtimeData.timesActivated++;
            runtimeData.level++;

            // Update UI
            if (BuildingPanelUI3D.Instance != null)
            {
                BuildingPanelUI3D.Instance.UpdateLevelSlider();
                BuildingPanelUI3D.Instance.UpdateAllUI();
            }

            Debug.Log($"🔧 IncomeModule upgraded to level {runtimeData.level}/{maxLevel} for {building.Config.DisplayName}");
            TriggerProgress(building.Data.ID);

            if (IsMaxLevel())
            {
                Debug.Log($"🔧 {moduleName} reached maximum level!");
                TriggerCompleted(building.Data.ID);
            }
        }
        else
        {
            Debug.Log($"❌ Not enough gold to upgrade income module. Need: {cost}");
        }
    }

    public override string GetStatusText(Building building)
    {
        double income = CalculateIncome(building);

        if (IsMaxLevel())
        {
            return $"Income: +{income:F1}/s\nLevel: {runtimeData.level}/{maxLevel}\nMAXED OUT!";
        }
        else
        {
            double nextCost = GetCurrentCost(runtimeData.timesActivated);
            return $"Income: +{income:F1}/s\nLevel: {runtimeData.level}/{maxLevel}\nNext: {nextCost:F0} Gold";
        }
    }

    public override string GetEffectDescription()
    {
        return effectDescription;
    }

    public double CalculateIncome(Building building)
    {
        return baseIncomeBonus * Mathf.Pow(incomeMultiplier, runtimeData.level - 1);
    }

    public double GetLastIncomeGenerated()
    {
        return _lastIncomeGenerated;
    }

    public override bool IsValid()
    {
        return base.IsValid() && baseIncomeBonus > 0 && incomeMultiplier > 0;
    }
}