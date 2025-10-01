using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Manager References - DRAG THIS OBJECT TO ALL FIELDS!")]
    public EconomyManager Economy;
    public BuildingManager3D Buildings;
    public UIManager UI;
    public SaveManager Save;

    void Awake()
    {
        Debug.Log("=== GAME MANAGER STARTING ===");

        // Singleton pattern - only one instance allowed
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeManagers();
            Debug.Log("✅ GameManager initialized as singleton");
        }
        else
        {
            Debug.Log("⚠️ Duplicate GameManager destroyed");
            Destroy(gameObject);
        }
    }

    void InitializeManagers()
    {
        Debug.Log("🔄 Initializing managers...");

        // Initialize SaveManager FIRST
        Save.Initialize();
        Debug.Log("✅ SaveManager initialized");

        // Calculate offline earnings before economy starts
        double offlineEarnings = Save.CalculateOfflineEarnings(Buildings);
        Debug.Log($"💰 Offline earnings calculated: {offlineEarnings}");

        // Initialize other managers
        Economy.Initialize(Save.Data);
        Debug.Log("✅ EconomyManager initialized");

        Buildings.Initialize(Save.Data, Economy);
        Debug.Log("✅ BuildingManager3D initialized");

        // Link building manager to economy for income calculation
        Economy.SetBuildingManager(Buildings);
        Debug.Log("🔗 BuildingManager3D linked to EconomyManager");

        UI.Initialize(Save.Data, Economy, Buildings);
        Debug.Log("✅ UIManager initialized");

        // Add offline earnings to economy (unless this was a reset)
        if (offlineEarnings > 0 && !Save.resetSaveOnStart)
        {
            Economy.AddGold(offlineEarnings);
            Debug.Log($"💸 Added offline earnings to player: {offlineEarnings}");
        }
        else if (Save.resetSaveOnStart)
        {
            Debug.Log("🔄 Reset mode - skipping offline earnings");
        }

        Debug.Log("🎉 ALL MANAGERS INITIALIZED SUCCESSFULLY!");
    }

    void Update()
    {
        // Tick economy and building managers
        Economy.Tick(Time.deltaTime);
    }

    void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            Save.SaveGame();
            Debug.Log("📱 Game paused - auto-saved");
        }
    }

    void OnApplicationQuit()
    {
        Save.SaveGame();
        Debug.Log("👋 Game quit - auto-saved");
    }

    [ContextMenu("Reset Game Runtime")]
    public void ResetGameRuntime()
    {
        Debug.Log("🔄 Resetting game runtime...");

        Save.ResetGameData();

        // Re-initialize all managers with fresh data
        Economy.Initialize(Save.Data);
        Buildings.Initialize(Save.Data, Economy);
        UI.Initialize(Save.Data, Economy, Buildings);

        Debug.Log("🎮 Game reset complete! All managers reinitialized.");
    }

    [ContextMenu("Force Reset Game")]
    public void ForceResetGame()
    {
        Debug.Log("💥 FORCE RESETTING GAME...");
        Save.ForceResetGameData();
        ResetGameRuntime();
    }
}