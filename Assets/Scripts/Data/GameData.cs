using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameData
{
    // ========== CURRENCY SYSTEM ==========
    [Header("Player Currency")]
    public double Gold = 10000;       // Main currency for buildings
    public double Honor = 0;          // Prestige currency
    public int Samurai = 0;           // Military units
    public int Peasants = 0;          // Workforce units

    // ========== BUILDING SYSTEM ==========
    [Header("Building Data")]
    public List<BuildingData> Buildings = new List<BuildingData>();

    // ========== GAME STATE ==========
    [Header("Game State")]
    public DateTime LastSaveTime;     // Used for offline earnings
    public int TotalPrestiges = 0;    // How many times player prestiged

    public GameData()
    {
        Debug.Log("🆕 Creating new game data with default values");

        // Initialize default buildings - ADD NEW BUILDINGS HERE!
        Buildings.Add(new BuildingData { ID = "rice_paddy", UnlockCost = 10 });
        Buildings.Add(new BuildingData { ID = "dojo", UnlockCost = 100 });
        Buildings.Add(new BuildingData { ID = "blacksmith", UnlockCost = 500 });
        Buildings.Add(new BuildingData { ID = "temple", UnlockCost = 1000 });

        Debug.Log($"🏗️ Initialized {Buildings.Count} default buildings");
    }
}

[System.Serializable]
public class BuildingData
{
    [Header("Building Identification")]
    public string ID;                 // Must match BuildingConfig ID exactly!

    [Header("Building State")]
    public int Level = 0;             // Current upgrade level
    public bool IsUnlocked = false;   // Has player purchased at least once?
    public double UnlockCost = 100;   // Cost to unlock (first purchase)

    // Helper method for debugging
    public override string ToString()
    {
        return $"{ID} (Level {Level}, Unlocked: {IsUnlocked})";
    }
}