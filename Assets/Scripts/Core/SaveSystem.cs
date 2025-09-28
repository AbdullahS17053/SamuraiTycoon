using System;
using System.IO;
using UnityEngine;

[System.Serializable]
public class SaveManager
{
    private string SavePath => Application.persistentDataPath + "/samuraitycoon.save";

    public GameData Data { get; private set; }

    [Header("Debug Options - EXPAND ME!")]
    [Tooltip("Check this to delete save file and start fresh")]
    public bool resetSaveOnStart = false;
    [Tooltip("Starting gold when resetting (if resetSaveOnStart is true)")]
    public double startingGold = 0;

    public void Initialize()
    {
        // Handle reset option
        if (resetSaveOnStart)
        {
            ResetGameData();
        }

        Data = LoadGame() ?? new GameData();

        // Apply starting gold if reset
        if (resetSaveOnStart && startingGold > 0)
        {
            Data.Gold = startingGold;
            Debug.Log($"💰 Set starting gold to: {startingGold}");
        }
    }

    public void SaveGame()
    {
        if (Data == null)
        {
            Debug.LogError("❌ Cannot save - Data is null!");
            return;
        }

        Data.LastSaveTime = DateTime.Now;

        try
        {
            string json = JsonUtility.ToJson(Data, true);
            File.WriteAllText(SavePath, json);
            Debug.Log($"💾 Game saved successfully to: {SavePath}");
            Debug.Log($"📊 Save stats - Gold: {Data.Gold}, Buildings: {Data.Buildings.Count}");
        }
        catch (Exception e)
        {
            Debug.LogError($"❌ Save failed: {e.Message}");
        }
    }

    private GameData LoadGame()
    {
        if (!File.Exists(SavePath))
        {
            Debug.Log("🆕 No save file found, creating new game");
            return null;
        }

        try
        {
            string json = File.ReadAllText(SavePath);
            GameData data = JsonUtility.FromJson<GameData>(json);

            if (data == null)
            {
                Debug.LogError("❌ Save file corrupted, creating new game");
                return null;
            }

            Debug.Log($"📂 Game loaded successfully: {data.Buildings.Count} buildings, {data.Gold} gold");
            return data;
        }
        catch (Exception e)
        {
            Debug.LogError($"❌ Load failed: {e.Message}");
            return null;
        }
    }

    // Call this when game starts to calculate offline earnings
    public double CalculateOfflineEarnings(BuildingManager buildingManager)
    {
        if (Data == null || Data.LastSaveTime == default)
        {
            Debug.Log("⏰ No previous save time found, skipping offline earnings");
            return 0;
        }

        TimeSpan offlineTime = DateTime.Now - Data.LastSaveTime;
        double offlineSeconds = Math.Min(offlineTime.TotalSeconds, 86400); // Cap at 24 hours

        double offlineIncome = 0;
        int earningBuildings = 0;

        foreach (var buildingData in Data.Buildings)
        {
            if (buildingData.IsUnlocked)
            {
                var config = buildingManager.GetConfig(buildingData.ID);
                if (config != null)
                {
                    double buildingIncome = config.GetIncome(buildingData.Level) * offlineSeconds;
                    offlineIncome += buildingIncome;
                    earningBuildings++;

                    if (buildingIncome > 0)
                    {
                        Debug.Log($"🏗️ {buildingData.ID} earned {buildingIncome} gold offline");
                    }
                }
            }
        }

        Debug.Log($"💰 Offline earnings: {offlineIncome} gold from {earningBuildings} buildings over {offlineSeconds:F0} seconds");
        return offlineIncome;
    }

    // ========== DEBUG/RESET METHODS ==========

    [ContextMenu("Reset Game Data NOW")]
    public void ResetGameData()
    {
        try
        {
            if (File.Exists(SavePath))
            {
                File.Delete(SavePath);
                Debug.Log("🗑️ Save file deleted: " + SavePath);
            }
            else
            {
                Debug.Log("ℹ️ No save file to delete");
            }

            // Create fresh data
            Data = new GameData();
            if (startingGold > 0)
            {
                Data.Gold = startingGold;
            }

            Debug.Log("🆕 Game data reset complete!");
            Debug.Log($"💰 Starting gold: {Data.Gold}");
            Debug.Log($"🏗️ Buildings initialized: {Data.Buildings.Count}");
        }
        catch (Exception e)
        {
            Debug.LogError($"❌ Reset failed: {e.Message}");
        }
    }

    [ContextMenu("Print Save File Info")]
    public void PrintSaveInfo()
    {
        Debug.Log($"📁 Save path: {SavePath}");
        Debug.Log($"📊 File exists: {File.Exists(SavePath)}");

        if (File.Exists(SavePath))
        {
            try
            {
                string json = File.ReadAllText(SavePath);
                var tempData = JsonUtility.FromJson<GameData>(json);
                Debug.Log($"📈 Save info - Gold: {tempData.Gold}, Buildings: {tempData.Buildings.Count}, Last Save: {tempData.LastSaveTime}");
            }
            catch (Exception e)
            {
                Debug.LogError($"❌ Cannot read save file: {e.Message}");
            }
        }
    }
}