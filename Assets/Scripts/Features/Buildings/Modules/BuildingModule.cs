using System;
using UnityEngine;

// Base class for all building modules
public abstract class BuildingModule : ScriptableObject
{
    [Header("Module Basics")]
    public string moduleName;
    public string description;
    public Sprite icon;
    public bool isActive = true;

    [Header("UI Settings")]
    public bool showInUI = true;
    public string buttonText = "Activate";
    public Color buttonColor = Color.white;

    [Header("Upgrade Limits")]
    public int level = 1;
    public int maxLevel = 10;
    public bool showMaxLevelWarning = true;

    [Header("Cost Settings")]
    public int cost = 100;
    public float costMultiplier = 1.1f;

    // Events for module lifecycle
    public event Action<string> OnModuleStarted;
    public event Action<string> OnModuleCompleted;
    public event Action<string> OnModuleProgress;

    // Runtime data for this module instance
    [System.NonSerialized]
    protected BuildingModuleData runtimeData;

    // Main methods that modules override
    public abstract void OnButtonClick(TrainingBuilding building);
    public abstract string GetStatusText(TrainingBuilding building);
    public abstract string GetEffectDescription();

    // Helper methods
    protected void TriggerStarted(string buildingId) => OnModuleStarted?.Invoke(buildingId);
    protected void TriggerCompleted(string buildingId) => OnModuleCompleted?.Invoke(buildingId);
    protected void TriggerProgress(string buildingId) => OnModuleProgress?.Invoke(buildingId);

    // Cost calculation
    public virtual int GetCurrentCost()
    {
        return Mathf.RoundToInt(cost * (costMultiplier * level));
    }

    // Get how many times this module has been activated
    protected virtual int GetActivationCount(TrainingBuilding building)
    {
        return runtimeData?.level ?? 0;
    }

    // Check if module has reached max level
    public virtual bool IsMaxLevel()
    {
        return runtimeData != null && runtimeData.level >= maxLevel;
    }

    // Get current level
    public virtual int GetCurrentLevel()
    {
        return runtimeData?.level ?? 0;
    }

    // Get max level text for UI
    public virtual string GetMaxLevelText()
    {
        return IsMaxLevel() ? "MAXED OUT!" : $"Max: {maxLevel}";
    }

    // Set runtime data
    public void SetRuntimeData(BuildingModuleData data)
    {
        runtimeData = data;
    }

    // Get runtime data
    public BuildingModuleData GetRuntimeData()
    {
        return runtimeData;
    }

    // Helper method for derived classes to check economy
    protected bool SpendGoldForActivation(int cost, string actionName)
    {
        if (GameManager.Instance?.Economy?.SpendGold(cost) == true)
        {
            Debug.Log($"💰 Spent {cost} gold for {actionName}");
            return true;
        }
        return false;
    }

    // Validation method
    public virtual bool IsValid()
    {
        return !string.IsNullOrEmpty(moduleName) && maxLevel > 0;
    }
}