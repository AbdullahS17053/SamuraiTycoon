using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Manager References - DRAG THIS OBJECT TO ALL FIELDS!")]
    public EconomyManager Economy;
    public BuildingManager3D Buildings; // ← CHANGED to 3D manager
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

        // Initialize in correct order
        Save.Initialize();
        Debug.Log("✅ SaveManager initialized");

        // Calculate offline earnings before economy starts
        double offlineEarnings = Save.CalculateOfflineEarnings(Buildings);
        Debug.Log($"💰 Offline earnings calculated: {offlineEarnings}");

        Economy.Initialize(Save.Data);
        Debug.Log("✅ EconomyManager initialized");

        Buildings.Initialize(Save.Data, Economy);
        Debug.Log("✅ BuildingManager3D initialized");

        // Link building manager to economy for income calculation
        Economy.SetBuildingManager(Buildings);
        Debug.Log("🔗 BuildingManager3D linked to EconomyManager");

        UI.Initialize(Save.Data, Economy, Buildings);
        Debug.Log("✅ UIManager initialized");

        // Add offline earnings to economy
        if (offlineEarnings > 0)
        {
            Economy.AddGold(offlineEarnings);
            Debug.Log($"💸 Added offline earnings to player");
        }

        Debug.Log("🎉 ALL MANAGERS INITIALIZED SUCCESSFULLY!");
    }

    void Update()
    {
        // Tick economy and building managers
        Economy.Tick(Time.deltaTime);
        // BuildingManager3D now handles its own Update tick
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
        InitializeManagers();

        Debug.Log("🎮 Game reset complete! All managers reinitialized.");
    }
}