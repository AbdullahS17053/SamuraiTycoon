using UnityEngine;
using UnityEngine.UIElements;

[CreateAssetMenu(fileName = "IncomeModule", menuName = "Samurai Tycoon/Modules/Income")]
public class IncomeModule : BuildingModule
{
    [Header("Income Settings")]
    public double baseIncome = 1.0;
    public float incomeMultiplier = 1.1f;
    public float incomeInterval = 1.0f; // Seconds between income

    private double timer;

    public override void Initialize(BuildingModuleData data)
    {
        runtimeData = data;
        Debug.Log($"💰 IncomeModule initialized for building");
    }

    public override void OnBuildingTick(Building building, double deltaTime)
    {
        if (!isActive) return;

        timer += deltaTime;
        if (timer >= incomeInterval)
        {
            double income = CalculateIncome(building);
            GameManager.Instance.Economy.AddGold(income);
            timer = 0;

            Debug.Log($"💰 {building.Config.DisplayName} generated {income} gold");
        }
    }

    public override void OnUpgrade(Building building, int oldLevel, int newLevel)
    {
        Debug.Log($"⬆️ IncomeModule upgraded: {oldLevel} → {newLevel}");
        // Income automatically increases due to level in CalculateIncome
    }

    public override void OnButtonClick(Building building)
    {
        // Toggle income boost
        isActive = !isActive;
        Debug.Log($"🔧 IncomeModule {(isActive ? "activated" : "deactivated")} for {building.Config.DisplayName}");
    }

    public override string GetStatusText(Building building)
    {
        double income = CalculateIncome(building);
        return $"Income: {income}/s {(isActive ? "🟢" : "🔴")}";
    }

    private double CalculateIncome(Building building)
    {
        return baseIncome * Mathf.Pow(incomeMultiplier, building.Data.Level) * (isActive ? 1 : 0);
    }
}