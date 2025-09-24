using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BuildingManager
{
    [Header("Configs - DRAG NEW BUILDINGS HERE!")]
    public List<BuildingConfig> AllBuildings = new List<BuildingConfig>();

    [Header("Runtime Info")]
    public Dictionary<string, BuildingConfig> BuildingDictionary = new Dictionary<string, BuildingConfig>();

    private GameData _data;
    private EconomyManager _economy;

    // Events - UI can listen to these
    public System.Action<string> OnBuildingUpgraded; // BuildingID

    public void Initialize(GameData data, EconomyManager economy)
    {
        _data = data;
        _economy = economy;

        // Create quick lookup dictionary
        foreach (var config in AllBuildings)
        {
            BuildingDictionary[config.ID] = config;
        }
    }

    // CALL THIS WHEN PLAYER CLICKS UPGRADE BUTTON
    public void UpgradeBuilding(string buildingId)
    {
        var buildingData = _data.Buildings.Find(b => b.ID == buildingId);
        var config = GetConfig(buildingId);

        if (buildingData == null || config == null) return;

        double cost = GetUpgradeCost(buildingId);

        if (_economy.SpendGold(cost))
        {
            buildingData.Level++;

            // Unlock on first purchase
            if (buildingData.Level == 1)
                buildingData.IsUnlocked = true;

            OnBuildingUpgraded?.Invoke(buildingId);
        }
    }

    // THESE METHODS ARE USED BY UI SYSTEM
    public BuildingConfig GetConfig(string buildingId)
    {
        return BuildingDictionary.ContainsKey(buildingId) ? BuildingDictionary[buildingId] : null;
    }

    public BuildingData GetData(string buildingId)
    {
        return _data.Buildings.Find(b => b.ID == buildingId);
    }

    public double GetUpgradeCost(string buildingId)
    {
        var data = GetData(buildingId);
        var config = GetConfig(buildingId);
        return config?.GetCost(data?.Level ?? 0) ?? 0;
    }

    public double GetIncome(string buildingId)
    {
        var data = GetData(buildingId);
        var config = GetConfig(buildingId);
        return data?.IsUnlocked == true ? config?.GetIncome(data.Level) ?? 0 : 0;
    }
}