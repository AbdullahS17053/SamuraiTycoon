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

    // Passive income tracking
    private double _lastPassiveIncome = 0;
    private float _passiveIncomeTimer = 0f;

    public void Initialize(GameData data)
    {
        _data = data;
        _isInitialized = true;
        UpdateDebugView();
        Debug.Log($"💰 EconomyManager initialized with starting gold: {_data.Gold}");
    }

    public void SetBuildingManager(BuildingManager3D buildingManager)
    {
        _buildingManager = buildingManager;
        Debug.Log("💰 BuildingManager3D linked to EconomyManager");
    }

    public void Tick(float deltaTime)
    {
        if (!_isInitialized || _buildingManager == null) return;

        _tickTimer += deltaTime;
        _passiveIncomeTimer += deltaTime;

        // Calculate passive income every second
        if (_tickTimer >= TICK_INTERVAL)
        {
            CalculatePassiveIncome();
            _tickTimer = 0f;
        }

        // Update debug view less frequently for performance
        if (_passiveIncomeTimer >= 5f) // Every 5 seconds
        {
            UpdateDebugView();
            _passiveIncomeTimer = 0f;
        }
    }

    private void CalculatePassiveIncome()
    {
        if (_buildingManager == null) return;

        double income = _buildingManager.GetTotalIncomePerSecond();

        if (income > 0)
        {
            AddGold(income);
            _lastPassiveIncome = income;
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

        // Auto-save on significant gold changes
        if (amount >= 1000)
        {
            TriggerAutoSave();
        }
    }

    public bool SpendGold(double amount)
    {
        if (!_isInitialized || amount <= 0) return false;

        if (_data.Gold >= amount)
        {
            double oldGold = _data.Gold;
            _data.Gold -= amount;
            UpdateDebugView();

            Debug.Log($"💰 Gold: {oldGold} → {_data.Gold} (-{amount})");
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

        Debug.Log($"🟢 Honor: {oldHonor} → {_data.Honor} (+{amount})");
        OnHonorChanged?.Invoke(amount);

        // Auto-save on honor changes
        TriggerAutoSave();
    }

    private void UpdateDebugView()
    {
        if (!_isInitialized) return;

        Gold = _data.Gold;
        Honor = _data.Honor;
        Samurai = _data.Samurai;
        Peasants = _data.Peasants;
    }

    private void TriggerAutoSave()
    {
        if (GameManager.Instance != null && GameManager.Instance.Save != null)
        {
            // Use a small delay to avoid multiple rapid saves
            GameManager.Instance.Save.DelayedSave(1f);
        }
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

    public double GetCurrentHonor()
    {
        return _isInitialized ? _data.Honor : 0;
    }

    public double GetPassiveIncomePerSecond()
    {
        return _lastPassiveIncome;
    }

    // Method to force immediate debug view update
    public void ForceDebugUpdate()
    {
        UpdateDebugView();
    }

    [ContextMenu("Add Test Gold")]
    public void AddTestGold()
    {
        AddGold(1000);
        Debug.Log($"💰 Added test gold. Total: {_data.Gold}");
    }

    [ContextMenu("Add Test Honor")]
    public void AddTestHonor()
    {
        AddHonor(100);
        Debug.Log($"🟢 Added test honor. Total: {_data.Honor}");
    }
}