using System;
using Unity.VisualScripting;
using UnityEngine;

[System.Serializable]
public class EconomyManager
{
    [Header("Debug View - VIEW ONLY!")]
    public double Gold;
    public double Honor;
    public int Samurai;
    public int Peasants;

    // Events - other systems can listen to these
    public event Action<double> OnGoldChanged;
    public event Action<double> OnHonorChanged;
    public event Action<int> OnSamuraiChanged;
    public event Action<int> OnPeasantsChanged;

    private GameData _data;
    private float _tickTimer;
    private const float TICK_INTERVAL = 1.0f; // Update every second
    private BuildingManager _buildingManager; // Added for income calculation


    
    public void Initialize(GameData data)
    {
        _data = data;
        UpdateDebugView();
        Debug.Log("✅ EconomyManager initialized with starting gold: " + _data.Gold);
    }

    // Set building manager reference for income calculation
    public void SetBuildingManager(BuildingManager buildingManager)
    {
        _buildingManager = buildingManager;
        Debug.Log("🔗 BuildingManager linked to EconomyManager");

        AddGold(1000);
    }

    // Call this in Update() from a MonoBehaviour wrapper
    public void Tick(float deltaTime)
    {
        _tickTimer += deltaTime;
        if (_tickTimer >= TICK_INTERVAL)
        {
            CalculatePassiveIncome();
            _tickTimer = 0f;
        }
    }

    private void CalculatePassiveIncome()
    {
        if (_buildingManager == null)
        {
            Debug.LogWarning("⚠️ BuildingManager not set - cannot calculate income");
            return;
        }

        double income = _buildingManager.GetTotalIncomePerSecond();

        if (income > 0)
        {
            AddGold(income);
            Debug.Log($"💵 Passive income: +{income} gold");
        }
    }

    public void AddGold(double amount)
    {
        if (amount <= 0)
        {
            Debug.LogWarning("⚠️ Attempted to add negative or zero gold: " + amount);
            return;
        }

        double oldGold = _data.Gold;
        _data.Gold += amount;
        UpdateDebugView();

        Debug.Log($"💰 Gold: {oldGold} → {_data.Gold} (+{amount})");
        OnGoldChanged?.Invoke(amount);
    }

    public bool SpendGold(double amount)
    {
        if (amount <= 0)
        {
            Debug.LogError("❌ Attempted to spend negative gold: " + amount);
            return false;
        }

        if (_data.Gold >= amount)
        {
            double oldGold = _data.Gold;
            _data.Gold -= amount;
            UpdateDebugView();

            Debug.Log($"💸 Gold: {oldGold} → {_data.Gold} (-{amount})");
            OnGoldChanged?.Invoke(-amount);
            return true;
        }

        Debug.Log($"❌ Not enough gold! Need: {amount}, Have: {_data.Gold}");
        return false;
    }

    public void AddHonor(double amount)
    {
        if (amount <= 0)
        {
            Debug.LogWarning("⚠️ Attempted to add negative honor: " + amount);
            return;
        }

        double oldHonor = _data.Honor;
        _data.Honor += amount;
        UpdateDebugView();

        Debug.Log($"🎖️ Honor: {oldHonor} → {_data.Honor} (+{amount})");
        OnHonorChanged?.Invoke(amount);
    }

    private void UpdateDebugView()
    {
        Gold = _data.Gold;
        Honor = _data.Honor;
        Samurai = _data.Samurai;
        Peasants = _data.Peasants;
    }
}