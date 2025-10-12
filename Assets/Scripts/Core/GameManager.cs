using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Core Managers")]
    public EconomyManager Economy;
    public BuildingManager3D Buildings;
    public UIManager UI;

    [Header("New Systems")]
    public TroopManager Troops;

    [Header("Game Settings")]
    public bool enableTroopSystem = true;

    void Awake()
    {
        Instance = this;
    }


    public void AddGold(int gold)
    {
        Economy.AddGold(gold);
    }
}