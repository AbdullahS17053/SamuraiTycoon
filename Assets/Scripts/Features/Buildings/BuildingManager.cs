using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BuildingManager
{
    [Header("Building Configs - DRAG CONFIGS HERE!")]
    public List<BuildingConfig> AllBuildings = new List<BuildingConfig>();

    [Header("Runtime Info - VIEW ONLY!")]
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
        BuildingDictionary.Clear();
        foreach (var config in AllBuildings)
        {
            if (config != null)
            {
                BuildingDictionary[config.ID] = config;
                Debug.Log($"📚 Loaded building config: {config.ID} - {config.DisplayName}");
            }
            else
            {
                Debug.LogError("❌ Null building config found in list!");
            }
        }

        // Validate that all BuildingData has corresponding configs
        foreach (var buildingData in _data.Buildings)
        {
            if (!BuildingDictionary.ContainsKey(buildingData.ID))
            {
                Debug.LogError($"❌ No config found for building data: {buildingData.ID}");
            }
        }

        Debug.Log($"✅ BuildingManager initialized with {BuildingDictionary.Count} buildings");
    }

    // CALL THIS WHEN PLAYER CLICKS UPGRADE BUTTON
    public void UpgradeBuilding(string buildingId)
    {
        Debug.Log($"🔨 Attempting to upgrade: {buildingId}");

        var buildingData = _data.Buildings.Find(b => b.ID == buildingId);
        var config = GetConfig(buildingId);

        if (buildingData == null)
        {
            Debug.LogError($"❌ Building data not found: {buildingId}");
            return;
        }

        if (config == null)
        {
            Debug.LogError($"❌ Building config not found: {buildingId}");
            return;
        }

        double cost = GetUpgradeCost(buildingId);
        Debug.Log($"💳 {buildingId} upgrade cost: {cost}, player gold: {_economy.Gold}");

        if (_economy.SpendGold(cost))
        {
            int oldLevel = buildingData.Level;
            buildingData.Level++;

            // Unlock on first purchase
            if (buildingData.Level == 1)
            {
                buildingData.IsUnlocked = true;
                Debug.Log($"🔓 Building unlocked: {buildingId}");
            }

            Debug.Log($"⬆️ {buildingId} upgraded: Level {oldLevel} → {buildingData.Level}");

            // Notify everyone that building was upgraded
            OnBuildingUpgraded?.Invoke(buildingId);
            Debug.Log($"📢 Upgrade event fired for: {buildingId}");
        }
        else
        {
            Debug.Log($"❌ Upgrade failed - not enough gold for {buildingId}");
        }
    }

    // ========== PUBLIC METHODS FOR OTHER SYSTEMS ==========

    public BuildingConfig GetConfig(string buildingId)
    {
        if (BuildingDictionary.ContainsKey(buildingId))
            return BuildingDictionary[buildingId];

        Debug.LogWarning($"⚠️ Building config not found: {buildingId}");
        return null;
    }

    public BuildingData GetData(string buildingId)
    {
        var data = _data.Buildings.Find(b => b.ID == buildingId);
        if (data == null)
            Debug.LogWarning($"⚠️ Building data not found: {buildingId}");
        return data;
    }

    public double GetUpgradeCost(string buildingId)
    {
        var data = GetData(buildingId);
        var config = GetConfig(buildingId);

        if (data == null || config == null)
        {
            Debug.LogWarning($"⚠️ Cannot get cost for {buildingId} - data or config missing");
            return 0;
        }

        double cost = config.GetCost(data.Level);
        return cost;
    }

    public double GetIncome(string buildingId)
    {
        var data = GetData(buildingId);
        var config = GetConfig(buildingId);

        if (data == null || config == null || !data.IsUnlocked)
        {
            return 0;
        }

        double income = config.GetIncome(data.Level);
        return income;
    }

    public double GetTotalIncomePerSecond()
    {
        double totalIncome = 0;
        foreach (var buildingData in _data.Buildings)
        {
            if (buildingData.IsUnlocked)
            {
                totalIncome += GetIncome(buildingData.ID);
            }
        }
        return totalIncome;
    }
}