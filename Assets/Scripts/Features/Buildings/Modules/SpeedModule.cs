using UnityEngine;

[CreateAssetMenu(fileName = "SpeedModule", menuName = "Samurai Tycoon/Modules/Speed")]
public class SpeedModule : BuildingModule
{
    [Header("Speed Settings")]
    public float baseSpeedMultiplier = 1.1f;
    public float speedIncreasePerLevel = 0.1f;
    public float maxSpeedMultiplier = 3.0f;

    [Header("Effect Description")]
    public string effectDescription = "Increases production speed";

    public override void Initialize(BuildingModuleData data)
    {
        SetRuntimeData(data);
        if (runtimeData.level == 0) runtimeData.level = 1;
        Debug.Log($"⚡ SpeedModule initialized - Level: {runtimeData.level}/{maxLevel}");
    }

    public override void OnBuildingTick(Building building, double deltaTime) { }

    public override void OnUpgrade(Building building, int oldLevel, int newLevel)
    {
        Debug.Log($"⚡ Speed increased: {GetCurrentSpeedMultiplier():F2}x");
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
            // Update UI
            if (BuildingPanelUI3D.Instance != null)
            {
                BuildingPanelUI3D.Instance.UpdateLevelSlider();
                BuildingPanelUI3D.Instance.UpdateAllUI();
            }

            runtimeData.timesActivated++;
            runtimeData.level++;

            Debug.Log($"⚡ SpeedModule upgraded to level {runtimeData.level}/{maxLevel} for {building.Config.DisplayName}");
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
        float speed = GetCurrentSpeedMultiplier();

        if (IsMaxLevel())
        {
            return $"Speed: {speed:F2}x\nLevel: {runtimeData.level}/{maxLevel}\nMAXED OUT!";
        }
        else
        {
            double nextCost = GetCurrentCost(runtimeData.timesActivated);
            return $"Speed: {speed:F2}x\nLevel: {runtimeData.level}/{maxLevel}\nNext: {nextCost:F0} Gold";
        }
    }

    public override string GetEffectDescription()
    {
        return effectDescription;
    }

    public float GetCurrentSpeedMultiplier()
    {
        float speed = baseSpeedMultiplier + (speedIncreasePerLevel * (runtimeData.level - 1));
        return Mathf.Min(speed, maxSpeedMultiplier);
    }
}