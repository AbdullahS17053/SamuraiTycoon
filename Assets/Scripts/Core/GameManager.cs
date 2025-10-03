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

        // 2. Calculate offline earnings
        double offlineEarnings = Save.CalculateOfflineEarnings(Buildings);
        Debug.Log($"💰 Offline earnings: {offlineEarnings}");

        // 3. Initialize Core Systems
        Economy.Initialize(Save.Data);
        Debug.Log("✅ EconomyManager initialized");

        Buildings.Initialize(Save.Data, Economy);
        Debug.Log("✅ BuildingManager3D initialized");

        // 4. Initialize New Systems
        if (Zone != null)
        {
            Zone.Initialize(Save.Data, Economy, Buildings);
            Debug.Log("✅ ZoneManager initialized");
        }

        if (Troops != null && enableTroopSystem)
        {
            // Troops will auto-initialize in Start()
            Debug.Log("✅ TroopManager enabled");
        }

        if (Prestige != null)
        {
            // Prestige auto-initializes in Start()
            Debug.Log("✅ PrestigeManager initialized");
        }

        // 5. Initialize Theme
        if (Theme != null && enableJapaneseTheme)
        {
            Theme.ApplyJapaneseThemeToAll();
            Debug.Log("🎌 Japanese theme applied");
        }

        // 6. Initialize Web3 (Optional)
        if (Web3 != null && enableWeb3Features)
        {
            Debug.Log("✅ Web3Manager initialized (Placeholder)");
        }

        // 7. Initialize UI LAST
        UI.Initialize(Save.Data, Economy, Buildings);
        Debug.Log("✅ UIManager initialized");

        // 8. Add offline earnings
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
        Economy.Tick(Time.deltaTime);

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