using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Manager References - DRAG THIS OBJECT TO ALL FIELDS!")]
    public EconomyManager Economy;
    public BuildingManager Buildings;
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

        Economy.SetBuildingManager(Buildings);
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
        Debug.Log("✅ BuildingManager initialized");

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

    // Call this from UI buttons for manual saving
    public void ManualSave()
    {
        Save.SaveGame();
        Debug.Log("💾 Manual save completed");
    }
}