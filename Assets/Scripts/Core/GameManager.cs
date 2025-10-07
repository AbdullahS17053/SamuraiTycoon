using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Core Managers")]
    public EconomyManager Economy;
    public BuildingManager3D Buildings;
    public UIManager UI;
    public SaveManager Save;

    [Header("New Systems")]
    public ZoneManager Zone;
    public TroopManager Troops;
    public PrestigeManager Prestige;
    public ThemeManager Theme;
    public Web3Manager Web3;

    [Header("Game Settings")]
    public bool enableWeb3Features = false;
    public bool enableJapaneseTheme = true;
    public bool enableTroopSystem = true;
    public bool passiveIncomeActive = false;

    void Awake()
    {
        Debug.Log("=== SAMURAI TYCOON INITIALIZING ===");

        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeGame();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void InitializeGame()
    {
        // 1. Initialize Save System FIRST
        Save.Initialize();
        Debug.Log("✅ SaveManager initialized");

        // 2. Initialize Core Systems
        Economy.Initialize(Save.Data);
        Debug.Log("✅ EconomyManager initialized");

        Buildings.Initialize(Save.Data, Economy);
        Debug.Log("✅ BuildingManager3D initialized");

        // 3. Link Economy with Building Manager
        Economy.SetBuildingManager(Buildings);

        // 4. Calculate offline earnings AFTER systems are initialized
        double offlineEarnings = Save.CalculateOfflineEarnings(Buildings);
        Debug.Log($"💰 Offline earnings: {offlineEarnings}");

        // 5. Initialize New Systems
        if (Zone != null)
        {
            Zone.Initialize(Save.Data, Economy, Buildings);
            Debug.Log("✅ ZoneManager initialized");
        }

        if (Troops != null && enableTroopSystem)
        {
            Troops.Initialize();
            Debug.Log("✅ TroopManager initialized");
        }

        if (Prestige != null)
        {
            // ADD THIS LINE:
            Prestige.Initialize();
            Debug.Log("✅ PrestigeManager initialized");
        }

        // 6. Initialize Theme
        if (Theme != null && enableJapaneseTheme)
        {
            Theme.Initialize(Save.Data);
            Debug.Log("🎌 ThemeManager initialized");
        }

        // 7. Initialize Web3 (Optional)
        if (Web3 != null && enableWeb3Features)
        {
            Web3.Initialize(Save.Data);
            Debug.Log("✅ Web3Manager initialized");
        }

        // 8. Initialize UI LAST
        UI.Initialize(Save.Data, Economy, Buildings);
        Debug.Log("✅ UIManager initialized");

        // 9. Add offline earnings
        if (offlineEarnings > 0 && !Save.resetSaveOnStart)
        {
            Economy.AddGold(offlineEarnings);
            Debug.Log($"💸 Added offline earnings: {offlineEarnings}");
        }

        Debug.Log("🎉 ALL SYSTEMS INITIALIZED!");
    }

    void Update()
    {
        // Tick economy
        if (passiveIncomeActive)
        {
            Economy.Tick(Time.deltaTime);
        }

        // FIXED: Tick TroopManager if enabled
        if (Troops != null && enableTroopSystem)
        {
            Troops.Tick(Time.deltaTime);
        }

        // Auto-save every 30 seconds
        if (Time.time % 30f < Time.deltaTime)
        {
            Save.SaveGame();
        }
    }

    void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus) Save.SaveGame();
    }

    void OnApplicationQuit()
    {
        Save.SaveGame();
    }
}