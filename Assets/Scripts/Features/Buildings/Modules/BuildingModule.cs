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
    public int maxLevel = 10;
    public bool showMaxLevelWarning = true;

    [Header("Cost Settings")]
    public double activationCost = 100;
    public float costMultiplier = 1.2f;

    // Events for module lifecycle
    public event Action<string> OnModuleStarted;    // BuildingID
    public event Action<string> OnModuleCompleted; // BuildingID
    public event Action<string> OnModuleProgress;  // BuildingID

    // Runtime data for this module instance
    [System.NonSerialized]
    protected BuildingModuleData runtimeData;

    // Main methods that modules override
    public abstract void Initialize(BuildingModuleData data);
    public abstract void OnBuildingTick(Building building, double deltaTime);
    public abstract void OnUpgrade(Building building, int oldLevel, int newLevel);
    public abstract void OnButtonClick(Building building);
    public abstract string GetStatusText(Building building);
    public abstract string GetEffectDescription();

    // Helper methods
    protected void TriggerStarted(string buildingId) => OnModuleStarted?.Invoke(buildingId);
    protected void TriggerCompleted(string buildingId) => OnModuleCompleted?.Invoke(buildingId);
    protected void TriggerProgress(string buildingId) => OnModuleProgress?.Invoke(buildingId);

    // Cost calculation
    public virtual double GetCurrentCost(int timesActivated)
    {
        return activationCost * Mathf.Pow(costMultiplier, timesActivated);
    }

    // Can this module be activated?
    public virtual bool CanActivate(Building building, double currentGold)
    {
        return !IsMaxLevel() && currentGold >= GetCurrentCost(GetActivationCount(building));
    }

    // Get how many times this module has been activated
    protected virtual int GetActivationCount(Building building)
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
}