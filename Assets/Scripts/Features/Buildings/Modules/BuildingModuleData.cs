using System;
using UnityEngine;

[System.Serializable]
public class BuildingModuleData
{
    [Header("Module Identification")]
    public string moduleId;

    [Header("Module State")]
    public bool isUnlocked;
    public int level;
    public double progress;
    public double duration;
    public int timesActivated;

    [Header("Custom Data")]
    public JsonData customData;

    public BuildingModuleData()
    {
        customData = new JsonData();
        level = 1; // Start at level 1
        isUnlocked = true;
    }
}

[System.Serializable]
public class JsonData
{
    [SerializeField]
    private string data = "{}";

    public string GetString(string key, string defaultValue = "")
    {
        // Simple implementation - you can expand this with proper JSON parsing
        try
        {
            // For now, return default - implement proper JSON parsing if needed
            return defaultValue;
        }
        catch (Exception)
        {
            return defaultValue;
        }
    }

    public void SetString(string key, string value)
    {
        // Simple implementation
    }

    public double GetDouble(string key, double defaultValue = 0)
    {
        return defaultValue;
    }

    public void SetDouble(string key, double value) { }

    public int GetInt(string key, int defaultValue = 0)
    {
        return defaultValue;
    }

    public void SetInt(string key, int value) { }

    public bool GetBool(string key, bool defaultValue = false)
    {
        return defaultValue;
    }

    public void SetBool(string key, bool value) { }
}