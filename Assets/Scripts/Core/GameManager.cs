using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

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
        Debug.Log("=== SAMURAI TYCOON INITIALIZING ===");

        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }


    public void AddGold(int gold)
    {
        Economy.AddGold(gold);
    }
}