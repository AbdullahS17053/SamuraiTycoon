using System;
using UnityEngine;
using UnityEngine.UIElements;

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

    // Events for module lifecycle
    public event Action<string> OnModuleStarted;    // BuildingID
    public event Action<string> OnModuleCompleted; // BuildingID
    public event Action<string> OnModuleProgress;  // BuildingID

    // Runtime data for this module instance
    protected BuildingModuleData runtimeData;

    // Main methods that modules override
    public abstract void Initialize(BuildingModuleData data);
    public abstract void OnBuildingTick(Building building, double deltaTime);
    public abstract void OnUpgrade(Building building, int oldLevel, int newLevel);
    public abstract void OnButtonClick(Building building);
    public abstract string GetStatusText(Building building);

    // Helper methods
    protected void TriggerStarted(string buildingId) => OnModuleStarted?.Invoke(buildingId);
    protected void TriggerCompleted(string buildingId) => OnModuleCompleted?.Invoke(buildingId);
    protected void TriggerProgress(string buildingId) => OnModuleProgress?.Invoke(buildingId);
}

// Base data container for module runtime state
[System.Serializable]
public class BuildingModuleData
{
    public string moduleId;
    public bool isUnlocked;
    public int level;
    public double progress;
    public double duration;
    public JsonData customData; // For module-specific data
}

// Simple JSON-like data storage
[System.Serializable]
public class JsonData
{
    public string data;
    public string GetString(string key, string defaultValue = "") => /* implementation */ defaultValue;
    public void SetString(string key, string value) { /* implementation */ }
    public double GetDouble(string key, double defaultValue = 0) => /* implementation */ defaultValue;
    public void SetDouble(string key, double value) { /* implementation */ }
}