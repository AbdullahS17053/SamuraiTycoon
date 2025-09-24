using System;
using System.Collections.Generic;

[System.Serializable]
public class GameData
{
    // Currency - ADD NEW CURRENCIES HERE!
    public double Gold = 0;
    public double Honor = 0;
    public int Samurai = 0;
    public int Peasants = 0;

    // Buildings - AUTO-SAVES ALL BUILDINGS!
    public List<BuildingData> Buildings = new List<BuildingData>();

    // Settings
    public DateTime LastSaveTime;
    public int TotalPrestiges = 0;

    public GameData()
    {
        // Initialize default buildings - ADD NEW BUILDINGS HERE!
        Buildings.Add(new BuildingData { ID = "rice_paddy", UnlockCost = 10 });
        Buildings.Add(new BuildingData { ID = "dojo", UnlockCost = 100 });
        Buildings.Add(new BuildingData { ID = "blacksmith", UnlockCost = 500 });
        Buildings.Add(new BuildingData { ID = "temple", UnlockCost = 1000 });
    }
}

[System.Serializable]
public class BuildingData
{
    public string ID;          // Must match BuildingConfig ID
    public int Level = 0;
    public bool IsUnlocked = false;
    public double UnlockCost = 100;
}