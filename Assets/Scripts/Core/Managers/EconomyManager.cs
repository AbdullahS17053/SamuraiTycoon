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
    private float _tickTimer;
    private const float TICK_INTERVAL = 1.0f; // Update every second

    public void Initialize(GameData data)
    {
        _data = data;
        UpdateDebugView();
        Debug.Log("EconomyManager initialized");
    }

    // Call this in Update() from a MonoBehaviour wrapper if needed
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
        double income = 0;
        // Note: Building income is now calculated by BuildingManager
        // This method can be expanded for other income sources

        if (income > 0)
        {
            AddGold(income);
        }
    }

    public void AddGold(double amount)
    {
        if (amount <= 0) return;

        _data.Gold += amount;
        UpdateDebugView();
        OnGoldChanged?.Invoke(amount);
    }

    public bool SpendGold(double amount)
    {
        if (_data.Gold >= amount)
        {
            _data.Gold -= amount;
            UpdateDebugView();
            OnGoldChanged?.Invoke(-amount);
            return true;
        }
        return false;
    }

    public void AddHonor(double amount)
    {
        if (amount <= 0) return;

        _data.Honor += amount;
        UpdateDebugView();
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