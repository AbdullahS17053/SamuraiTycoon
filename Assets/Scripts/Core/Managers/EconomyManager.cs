using System;
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
    private BuildingManager3D _buildingManager;
    private float _tickTimer;
    private const float TICK_INTERVAL = 1.0f;
    private bool _isInitialized = false;

    public void Initialize(GameData data)
    {
        _data = data;
        _isInitialized = true;
        UpdateDebugView();
        Debug.Log("✅ EconomyManager initialized with starting gold: " + _data.Gold);
    }

    public void SetBuildingManager(BuildingManager3D buildingManager)
    {
        _buildingManager = buildingManager;
        Debug.Log("🔗 BuildingManager3D linked to EconomyManager");
    }

    public void Tick(float deltaTime)
    {
        if (!_isInitialized || _buildingManager == null) return;

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
            return;
        }

        double income = _buildingManager.GetTotalIncomePerSecond();

        if (income > 0)
        {
            AddGold(income);
        }
    }

    public void AddGold(double amount)
    {
        if (!_isInitialized || amount <= 0) return;

        double oldGold = _data.Gold;
        _data.Gold += amount;
        UpdateDebugView();

        Debug.Log($"💰 Gold: {oldGold} → {_data.Gold} (+{amount})");
        OnGoldChanged?.Invoke(amount);
    }

    public bool SpendGold(double amount)
    {
        if (!_isInitialized || amount <= 0) return false;

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
        if (!_isInitialized || amount <= 0) return;

        double oldHonor = _data.Honor;
        _data.Honor += amount;
        UpdateDebugView();

        Debug.Log($"🎖️ Honor: {oldHonor} → {_data.Honor} (+{amount})");
        OnHonorChanged?.Invoke(amount);
    }

    private void UpdateDebugView()
    {
        if (!_isInitialized) return;

        Gold = _data.Gold;
        Honor = _data.Honor;
        Samurai = _data.Samurai;
        Peasants = _data.Peasants;
    }

    // Helper methods for other systems
    public bool CanAfford(double amount)
    {
        return _isInitialized && _data.Gold >= amount;
    }

    public double GetCurrentGold()
    {
        return _isInitialized ? _data.Gold : 0;
    }
}