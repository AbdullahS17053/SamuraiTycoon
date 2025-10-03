using System;
using System.Collections.Generic;
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

    private Dictionary<string, string> _stringData = new Dictionary<string, string>();
    private Dictionary<string, double> _doubleData = new Dictionary<string, double>();
    private Dictionary<string, int> _intData = new Dictionary<string, int>();
    private Dictionary<string, bool> _boolData = new Dictionary<string, bool>();

    public string GetString(string key, string defaultValue = "")
    {
        return _stringData.ContainsKey(key) ? _stringData[key] : defaultValue;
    }

    public void SetString(string key, string value)
    {
        _stringData[key] = value;
        UpdateSerializedData();
    }

    public double GetDouble(string key, double defaultValue = 0)
    {
        return _doubleData.ContainsKey(key) ? _doubleData[key] : defaultValue;
    }

    public void SetDouble(string key, double value)
    {
        _doubleData[key] = value;
        UpdateSerializedData();
    }

    public int GetInt(string key, int defaultValue = 0)
    {
        return _intData.ContainsKey(key) ? _intData[key] : defaultValue;
    }

    public void SetInt(string key, int value)
    {
        _intData[key] = value;
        UpdateSerializedData();
    }

    public bool GetBool(string key, bool defaultValue = false)
    {
        return _boolData.ContainsKey(key) ? _boolData[key] : defaultValue;
    }

    public void SetBool(string key, bool value)
    {
        _boolData[key] = value;
        UpdateSerializedData();
    }

    private void UpdateSerializedData()
    {
        // Simple serialization - you might want to use proper JSON serialization
        data = "CustomData";
    }
}