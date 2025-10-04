using UnityEngine;
using System.Collections.Generic;

public class PrestigeManager : MonoBehaviour
{
    [Header("Prestige Configuration")]
    public double baseHonorMultiplier = 1.0;
    public double goldToHonorRatio = 1000.0;
    public double minimumPrestigeGold = 10000.0;

    [Header("Prestige Bonuses")]
    public List<PrestigeBonus> availableBonuses = new List<PrestigeBonus>();

    [Header("Current State")]
    public int prestigeCount = 0;
    public double totalHonorEarned = 0;
    public double availableHonor = 0;

    // Private
    private int _prestigeCount = 0;
    private EconomyManager _economy;
    private SaveManager _saveManager;

    [System.Serializable]
    public class PrestigeBonus
    {
        public string bonusName;
        public string description;
        public double honorCost;
        public bool isPurchased = false;
        public System.Action onPurchase;

        // ADDED: UI Properties
        public string displayName;
        public Sprite icon;
        public float value;
        public BonusType type;

        public enum BonusType
        {
            IncomeMultiplier,
            TrainingSpeed,
            TroopCapacity,
            BuildingCostReduction,
            OfflineEarnings,
            AutoTrainSpeed
        }
    }

    void Start()
    {
        _economy = GameManager.Instance.Economy;
        _saveManager = GameManager.Instance.Save;

        // Load prestige data from save
        LoadPrestigeData();

        Debug.Log("✅ PrestigeManager initialized");
    }

    // ADDED: Initialize method for GameManager
    public void Initialize()
    {
        Debug.Log("✅ PrestigeManager initialized via GameManager");

        // Initialize default bonuses if none exist
        if (availableBonuses.Count == 0)
        {
            CreateDefaultBonuses();
        }
    }

    private void CreateDefaultBonuses()
    {
        availableBonuses = new List<PrestigeBonus>
        {
            new PrestigeBonus
            {
                bonusName = "Income Boost",
                displayName = "Gold Income +25%",
                description = "Increase all gold income by 25%",
                honorCost = 50,
                value = 0.25f,
                type = PrestigeBonus.BonusType.IncomeMultiplier,
                onPurchase = () => { ApplyIncomeMultiplier(0.25f); }
            },
            new PrestigeBonus
            {
                bonusName = "Training Speed",
                displayName = "Training Speed +20%",
                description = "Train troops 20% faster",
                honorCost = 75,
                value = 0.20f,
                type = PrestigeBonus.BonusType.TrainingSpeed,
                onPurchase = () => { ApplyTrainingSpeed(0.20f); }
            },
            new PrestigeBonus
            {
                bonusName = "Troop Capacity",
                displayName = "+5 Troop Capacity",
                description = "Increase maximum troop capacity by 5",
                honorCost = 100,
                value = 5f,
                type = PrestigeBonus.BonusType.TroopCapacity,
                onPurchase = () => { ApplyTroopCapacity(5); }
            }
        };
    }

    // ADDED: Selection methods for UI
    public void SelectBonus(PrestigeBonus bonus)
    {
        if (bonus != null && !bonus.isPurchased)
        {
            Debug.Log($"🎯 Selected prestige bonus: {bonus.displayName}");
        }
    }

    public void DeselectBonus(PrestigeBonus bonus)
    {
        if (bonus != null)
        {
            Debug.Log($"🎯 Deselected prestige bonus: {bonus.displayName}");
        }
    }

    public bool CanPurchaseBonus(PrestigeBonus bonus)
    {
        return bonus != null && !bonus.isPurchased && availableHonor >= bonus.honorCost;
    }

    public bool CanPrestige()
    {
        if (_economy == null) return false;

        return _economy.Gold >= minimumPrestigeGold;
    }

    public void PrestigeReset()
    {
        if (!CanPrestige())
        {
            Debug.Log("❌ Not enough gold to prestige!");
            return;
        }

        Debug.Log("👑 Starting prestige reset...");

        // Calculate honor reward
        double honorReward = CalculateHonorReward();

        // Add honor
        availableHonor += honorReward;
        totalHonorEarned += honorReward;
        _prestigeCount++;
        prestigeCount = _prestigeCount;

        // Reset troops through TroopManager
        if (GameManager.Instance != null && GameManager.Instance.Troops != null)
        {
            GameManager.Instance.Troops.ResetAllTroops();
        }

        // Reset economy (keep honor)
        if (_economy != null)
        {
            double currentHonor = _economy.Honor;
            _economy.SpendGold(_economy.Gold); // Spend all gold
            _economy.AddHonor(honorReward);
        }

        // Reset buildings
        ResetBuildings();

        // Save game
        if (_saveManager != null)
        {
            _saveManager.SaveGame();
        }

        Debug.Log($"🎉 Prestige complete! Gained {honorReward} honor. Total prestige: {_prestigeCount}");
    }

    public void SoftReset()
    {
        Debug.Log("🔄 Soft reset in progress...");

        // Reset troops but keep some progress
        if (GameManager.Instance != null && GameManager.Instance.Troops != null)
        {
            GameManager.Instance.Troops.ReinitializeTroops();
        }

        // Partial economy reset
        if (_economy != null)
        {
            // Keep 10% of gold
            double goldToKeep = _economy.Gold * 0.1;
            _economy.SpendGold(_economy.Gold - goldToKeep);
        }

        // Save game
        if (_saveManager != null)
        {
            _saveManager.SaveGame();
        }

        Debug.Log("✅ Soft reset complete!");
    }

    private double CalculateHonorReward()
    {
        if (GameManager.Instance == null || GameManager.Instance.Economy == null)
        {
            Debug.LogError("❌ Cannot calculate honor reward - GameManager or Economy not found");
            return 0;
        }

        double currentGold = GameManager.Instance.Economy.Gold;
        double currentHonor = GameManager.Instance.Economy.Honor;

        // Base formula: honor = sqrt(gold / 1000) + (prestigeCount * 10)
        double baseHonor = System.Math.Sqrt(currentGold / 1000.0);
        double prestigeBonus = _prestigeCount * 10;
        double totalHonor = baseHonor + prestigeBonus;

        // Minimum honor reward
        totalHonor = System.Math.Max(totalHonor, 10);

        Debug.Log($"💰 Honor Calculation: Gold={currentGold}, Base={baseHonor:F2}, PrestigeBonus={prestigeBonus}, Total={totalHonor:F2}");

        return totalHonor;
    }

    public bool PurchaseBonus(PrestigeBonus bonus)
    {
        if (bonus == null || bonus.isPurchased) return false;

        if (availableHonor >= bonus.honorCost)
        {
            availableHonor -= bonus.honorCost;
            bonus.isPurchased = true;

            // Apply bonus effect
            bonus.onPurchase?.Invoke();

            Debug.Log($"✅ Purchased prestige bonus: {bonus.displayName}");
            return true;
        }

        Debug.Log($"❌ Not enough honor to purchase {bonus.displayName}");
        return false;
    }

    public double GetHonorRewardPreview()
    {
        if (_economy == null) return 0;
        return CalculateHonorReward();
    }

    // Bonus application methods
    private void ApplyIncomeMultiplier(float multiplier)
    {
        if (GameManager.Instance != null && GameManager.Instance.Save != null)
        {
            GameManager.Instance.Save.Data.GlobalIncomeMultiplier += multiplier;
            Debug.Log($"💰 Income multiplier increased by {multiplier:P0}");
        }
    }

    private void ApplyTrainingSpeed(float speedBonus)
    {
        if (GameManager.Instance != null && GameManager.Instance.Save != null)
        {
            GameManager.Instance.Save.Data.TroopTrainingSpeedMultiplier += speedBonus;
            Debug.Log($"⚡ Training speed increased by {speedBonus:P0}");
        }
    }

    private void ApplyTroopCapacity(int capacityBonus)
    {
        if (GameManager.Instance != null && GameManager.Instance.Save != null)
        {
            GameManager.Instance.Save.Data.ExtraTroopCapacity += capacityBonus;
            Debug.Log($"🎖️ Troop capacity increased by {capacityBonus}");
        }
    }

    private void ResetBuildings()
    {
        // Reset all buildings to level 1 but keep them unlocked
        var buildingManager = BuildingManager3D.Instance;
        if (buildingManager != null)
        {
            Debug.Log("🏗️ Resetting buildings for prestige...");
        }
    }

    private void LoadPrestigeData()
    {
        // Load prestige data from save
        if (_saveManager != null && _saveManager.Data != null)
        {
            _prestigeCount = _saveManager.Data.TotalPrestiges;
            // Load other prestige data as needed
        }
    }

    private void SavePrestigeData()
    {
        // Save prestige data
        if (_saveManager != null && _saveManager.Data != null)
        {
            _saveManager.Data.TotalPrestiges = _prestigeCount;
        }
    }

    [ContextMenu("Debug Prestige Info")]
    public void DebugPrestigeInfo()
    {
        Debug.Log("=== PRESTIGE INFO ===");
        Debug.Log($"Prestige Count: {_prestigeCount}");
        Debug.Log($"Available Honor: {availableHonor}");
        Debug.Log($"Total Honor Earned: {totalHonorEarned}");
        Debug.Log($"Can Prestige: {CanPrestige()}");
        Debug.Log($"Honor Reward Preview: {GetHonorRewardPreview():F2}");
        Debug.Log($"Available Bonuses: {availableBonuses?.Count ?? 0}");
    }

    [ContextMenu("Test Prestige")]
    public void TestPrestige()
    {
        if (CanPrestige())
        {
            PrestigeReset();
        }
        else
        {
            Debug.Log("❌ Cannot prestige - not enough gold");
        }
    }

    [ContextMenu("Add Test Honor")]
    public void AddTestHonor()
    {
        availableHonor += 100;
        Debug.Log($"➕ Added 100 test honor. Total: {availableHonor}");
    }
}