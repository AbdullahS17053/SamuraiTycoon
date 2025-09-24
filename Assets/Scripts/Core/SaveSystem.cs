using System;
using System.IO;
using UnityEngine;

[System.Serializable]
public class SaveManager
{
    private string SavePath => Application.persistentDataPath + "/samuraitycoon.save";

    public GameData Data { get; private set; }

    public void Initialize()
    {
        Data = LoadGame() ?? new GameData();
    }

    public void SaveGame()
    {
        if (Data == null) return;

        Data.LastSaveTime = DateTime.Now;

        try
        {
            string json = JsonUtility.ToJson(Data, true);
            File.WriteAllText(SavePath, json);
            Debug.Log("Game saved successfully");
        }
        catch (Exception e)
        {
            Debug.LogError($"Save failed: {e.Message}");
        }
    }

    private GameData LoadGame()
    {
        if (!File.Exists(SavePath))
        {
            Debug.Log("No save file found, creating new game");
            return null;
        }

        try
        {
            string json = File.ReadAllText(SavePath);
            GameData data = JsonUtility.FromJson<GameData>(json);
            Debug.Log("Game loaded successfully");
            return data;
        }
        catch (Exception e)
        {
            Debug.LogError($"Load failed: {e.Message}");
            return null;
        }
    }

    // Call this when game starts to calculate offline earnings
    public double CalculateOfflineEarnings(BuildingManager buildingManager)
    {
        if (Data == null || Data.LastSaveTime == default) return 0;

        TimeSpan offlineTime = DateTime.Now - Data.LastSaveTime;
        double offlineSeconds = Math.Min(offlineTime.TotalSeconds, 86400); // Cap at 24 hours

        double offlineIncome = 0;

        foreach (var buildingData in Data.Buildings)
        {
            if (buildingData.IsUnlocked)
            {
                var config = buildingManager.GetConfig(buildingData.ID);
                if (config != null)
                {
                    offlineIncome += config.GetIncome(buildingData.Level) * offlineSeconds;
                }
            }
        }

        Debug.Log($"Offline earnings: {offlineIncome} gold from {offlineSeconds} seconds");
        return offlineIncome;
    }
}