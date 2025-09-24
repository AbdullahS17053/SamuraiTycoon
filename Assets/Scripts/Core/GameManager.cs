using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public EconomyManager Economy;
    public BuildingManager Buildings;
    public UIManager UI;
    public SaveManager Save;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeManagers();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void InitializeManagers()
    {
        Save.Initialize();

        // Calculate offline earnings before economy starts
        double offlineEarnings = Save.CalculateOfflineEarnings(Buildings);

        Economy.Initialize(Save.Data);
        Buildings.Initialize(Save.Data, Economy);
        UI.Initialize(Save.Data, Economy, Buildings);

        // Add offline earnings to economy
        if (offlineEarnings > 0)
        {
            Economy.AddGold(offlineEarnings);
        }

        Debug.Log("All managers initialized successfully");
    }

    void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus) Save.SaveGame();
    }

    void OnApplicationQuit()
    {
        Save.SaveGame();
    }

    // Call this from UI buttons for manual saving
    public void ManualSave()
    {
        Save.SaveGame();
        Debug.Log("Game manually saved");
    }
}