using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameData
{
    // ========== CURRENCY SYSTEM ==========
    [Header("Player Currency")]
    public double Gold = 10000;
    public double Honor = 0;
    public int Samurai = 0;
    public int Peasants = 0;

    // ========== BUILDING SYSTEM ==========
    [Header("Building Data")]
    public List<BuildingData> Buildings = new List<BuildingData>();

    // ========== PRESTIGE SYSTEM ==========
    [Header("Prestige Bonuses")]
    public double GlobalIncomeMultiplier = 1.0;
    public float TroopTrainingSpeedMultiplier = 1.0f;
    public double BuildingCostReduction = 0.0;
    public double OfflineEarningsMultiplier = 1.0;
    public int ExtraTroopCapacity = 0;
    public float AutoTrainSpeedMultiplier = 1.0f;
    public int TotalPrestiges = 0;

    // ========== ZONE SYSTEM ==========
    [Header("Zone Progress")]
    public string CurrentZoneId = "training_ground";
    public List<string> UnlockedZones = new List<string>();

    // ========== GAME STATE ==========
    [Header("Game State")]
    public DateTime LastSaveTime;
    public string PlayerName = "Shogun";
    public int TotalPlayTime = 0;

    public GameData()
    {
        Debug.Log("🆕 Creating new game data with Japanese theme");

        // Initialize Japanese-themed buildings
        Buildings.Add(new BuildingData
        {
            ID = "dojo",
            DisplayName = "Samurai Dojo",
            UnlockCost = 100,
            Description = "Train elite samurai warriors"
        });
        Buildings.Add(new BuildingData
        {
            ID = "archery_range",
            DisplayName = "Kyūdōjo",
            UnlockCost = 500,
            Description = "Master the way of the bow"
        });
        Buildings.Add(new BuildingData
        {
            ID = "stable",
            DisplayName = "Bajutsu Dojo",
            UnlockCost = 1000,
            Description = "Train cavalry for mounted combat"
        });
        Buildings.Add(new BuildingData
        {
            ID = "ninja_camp",
            DisplayName = "Shinobi Camp",
            UnlockCost = 5000,
            Description = "Train stealthy ninja operatives"
        });
        Buildings.Add(new BuildingData
        {
            ID = "castle",
            DisplayName = "Shiro Castle",
            UnlockCost = 25000,
            Description = "Your mighty fortress headquarters"
        });

        // Add starting zone
        UnlockedZones.Add("training_ground");

        Debug.Log($"🏯 Initialized {Buildings.Count} Japanese-themed buildings");
    }
}

[System.Serializable]
public class BuildingData
{
    [Header("Building Identification")]
    public string ID;
    public string DisplayName;
    public bool IsUnlocked = false;
    public int Level = 0;
    public double UnlockCost = 100;

    [Header("Japanese Theme")]
    public string JapaneseName;
    public string Description;
    public string Lore;

    [Header("Building State")]
    public DateTime LastCollectionTime;
    public double StoredResources = 0;

    public override string ToString()
    {
        return $"{DisplayName} (Level {Level}, Unlocked: {IsUnlocked})";
    }
}