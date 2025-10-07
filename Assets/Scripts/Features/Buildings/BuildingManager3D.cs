using System.Collections.Generic;
using UnityEngine;

public class BuildingManager3D : MonoBehaviour
{
    public static BuildingManager3D Instance { get; private set; }

    [Header("Building Configs - DRAG BUILDINGS HERE!")]
    public List<BuildingConfig> AllBuildings = new List<BuildingConfig>();

    [Header("UI References")]
    public GameObject buildingPanel;
    public Transform buildingPanelContent;
    public GameObject buildingUIElementPrefab;

    [Header("Camera Control")]
    public Camera mainCamera;
    public float cameraMoveSpeed = 5f;
    public Vector3 panelCameraOffset = new Vector3(0, 2, -5);

    [Header("Runtime Info")]
    public Dictionary<string, Building> BuildingInstances = new Dictionary<string, Building>();
    public Dictionary<string, BuildingObject3D> BuildingObjects = new Dictionary<string, BuildingObject3D>();

    private GameData _data;
    private EconomyManager _economy;
    private Dictionary<string, BuildingConfig> _configDictionary = new Dictionary<string, BuildingConfig>();
    private float _tickTimer;
    private const float TICK_INTERVAL = 0.1f;
    private string _currentSelectedBuilding = "";
    private Vector3 _originalCameraPosition;
    private Quaternion _originalCameraRotation;
    private bool _isInitialized = false;

    // Events
    public System.Action<string> OnBuildingUpgraded;
    public System.Action<string, string> OnModuleActivated;
    public System.Action<string> OnBuildingSelected;
    public System.Action<string> OnBuildingDataChanged; // NEW EVENT

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            Debug.Log("✅ BuildingManager3D singleton created");
        }
        else
        {
            Destroy(gameObject);
        }

        // Store original camera position
        if (mainCamera != null)
        {
            _originalCameraPosition = mainCamera.transform.position;
            _originalCameraRotation = mainCamera.transform.rotation;
        }
    }

    public void Initialize(GameData data, EconomyManager economy)
    {
        _data = data;
        _economy = economy;

        // Create config dictionary
        _configDictionary.Clear();
        BuildingInstances.Clear();

        foreach (var config in AllBuildings)
        {
            if (config != null)
            {
                _configDictionary[config.ID] = config;

                // Create building instance with modules
                var buildingData = _data.Buildings.Find(b => b.ID == config.ID);
                if (buildingData != null)
                {
                    BuildingInstances[config.ID] = new Building(config, buildingData);
                    Debug.Log($"🏗️ 3D Building instance created: {config.DisplayName} with {config.modules.Count} modules");
                }
            }
        }

        _isInitialized = true;
        Debug.Log($"✅ BuildingManager3D initialized with {BuildingInstances.Count} buildings");
    }

    void Update()
    {
        if (!_isInitialized) return;

        _tickTimer += Time.deltaTime;
        if (_tickTimer >= TICK_INTERVAL)
        {
            foreach (var building in BuildingInstances.Values)
            {
                building.Tick(_tickTimer);
            }
            _tickTimer = 0f;
        }

        // Handle camera movement when panel is open
        if (buildingPanel != null && buildingPanel.activeInHierarchy && !string.IsNullOrEmpty(_currentSelectedBuilding))
        {
            MoveCameraToSelectedBuilding();
        }
    }

    // ========== BUILDING OBJECT MANAGEMENT ==========

    public void RegisterBuildingObject(BuildingObject3D buildingObject)
    {
        if (!BuildingObjects.ContainsKey(buildingObject.BuildingID))
        {
            BuildingObjects[buildingObject.BuildingID] = buildingObject;
            Debug.Log($"📝 3D BuildingObject registered: {buildingObject.BuildingID}");
        }
    }

    public void UnregisterBuildingObject(BuildingObject3D buildingObject)
    {
        if (BuildingObjects.ContainsKey(buildingObject.BuildingID))
        {
            BuildingObjects.Remove(buildingObject.BuildingID);
            Debug.Log($"📝 3D BuildingObject unregistered: {buildingObject.BuildingID}");
        }
    }

    // ========== BUILDING PANEL SYSTEM ==========

    public void ShowBuildingPanel(string buildingId)
    {
        if (!_isInitialized) return;

        _currentSelectedBuilding = buildingId;

        // Hide all building highlights
        foreach (var buildingObj in BuildingObjects.Values)
        {
            buildingObj.SetSelected(buildingObj.BuildingID == buildingId);
        }

        // Show the panel
        if (buildingPanel != null)
        {
            buildingPanel.SetActive(true);
            UpdateBuildingPanel();
            Debug.Log($"📊 3D Building panel shown for: {buildingId}");
        }
        else
        {
            Debug.LogError("❌ Building panel reference is null!");
        }

        // Move camera to focus on selected building
        if (mainCamera != null)
        {
            _originalCameraPosition = mainCamera.transform.position;
            _originalCameraRotation = mainCamera.transform.rotation;
        }

        OnBuildingSelected?.Invoke(buildingId);
    }

    public void HideBuildingPanel()
    {
        if (buildingPanel != null)
        {
            buildingPanel.SetActive(false);

            // Clear selection
            foreach (var buildingObj in BuildingObjects.Values)
            {
                buildingObj.SetSelected(false);
            }

            // Reset camera
            if (mainCamera != null)
            {
                mainCamera.transform.position = _originalCameraPosition;
                mainCamera.transform.rotation = _originalCameraRotation;
            }
            Destroy(buildingPanel.transform.GetChild(0).gameObject);
            _currentSelectedBuilding = "";
            Debug.Log("📊 3D Building panel hidden");
        }
    }
    // ADD THIS METHOD to ensure UI updates when building data changes
    public void ForceUIRefresh(string buildingId = null)
    {
        if (string.IsNullOrEmpty(buildingId))
        {
            buildingId = _currentSelectedBuilding;
        }

        if (!string.IsNullOrEmpty(buildingId) && buildingId == _currentSelectedBuilding)
        {
            UpdateBuildingPanel();
        }
    }

    private void MoveCameraToSelectedBuilding()
    {
        if (mainCamera == null || string.IsNullOrEmpty(_currentSelectedBuilding)) return;

        var buildingObj = GetBuildingObject(_currentSelectedBuilding);
        if (buildingObj == null) return;

        Vector3 targetPosition = buildingObj.transform.position + panelCameraOffset;
        Quaternion targetRotation = Quaternion.LookRotation(buildingObj.transform.position - targetPosition);

        mainCamera.transform.position = Vector3.Lerp(mainCamera.transform.position, targetPosition, Time.deltaTime * cameraMoveSpeed);
        mainCamera.transform.rotation = Quaternion.Lerp(mainCamera.transform.rotation, targetRotation, Time.deltaTime * cameraMoveSpeed);
    }

    private void UpdateBuildingPanel()
    {
        if (string.IsNullOrEmpty(_currentSelectedBuilding)) return;

        // Clear existing UI elements
        foreach (Transform child in buildingPanelContent)
        {
            Destroy(child.gameObject);
        }

        var building = GetBuildingInstance(_currentSelectedBuilding);
        var config = GetConfig(_currentSelectedBuilding);
        var data = GetData(_currentSelectedBuilding);

        if (building == null || config == null || data == null)
        {
            Debug.LogError($"❌ Cannot update panel - missing data for: {_currentSelectedBuilding}");
            return;
        }

        // Create main building info UI
        if (buildingUIElementPrefab != null)
        {
            var buildingUI = Instantiate(buildingUIElementPrefab, buildingPanelContent);
            var buildingUIComponent = buildingUI.GetComponent<BuildingPanelUI3D>();

            if (buildingUIComponent != null)
            {
                buildingUIComponent.Initialize(building, config, data, _economy);
                Debug.Log($"✅ BuildingPanelUI3D initialized successfully");
            }
            else
            {
                Debug.LogError($"❌ BuildingPanelUI3D component not found on prefab!");
            }
        }
        else
        {
            Debug.LogError($"❌ Building UI Element Prefab is null!");
        }

        Debug.Log($"🔄 3D Building panel updated for: {_currentSelectedBuilding}");
    }

    // ========== BUILDING ACTIONS ==========

    // IMPROVED UpgradeBuilding method
    public void UpgradeBuilding(string buildingId)
    {
        if (!_isInitialized || _economy == null) return;

        var buildingData = _data.Buildings.Find(b => b.ID == buildingId);
        var buildingInstance = GetBuildingInstance(buildingId);

        if (buildingData == null || buildingInstance == null)
        {
            Debug.LogError($"❌ Cannot upgrade building: {buildingId} - data not found");
            return;
        }

        double cost = GetUpgradeCost(buildingId);

        if (_economy.SpendGold(cost))
        {
            int oldLevel = buildingData.Level;
            buildingData.Level++;

            if (buildingData.Level == 1)
                buildingData.IsUnlocked = true;

            // Notify building instance about upgrade
            buildingInstance.OnUpgrade(oldLevel, buildingData.Level);

            Debug.Log($"⬆️ {buildingId} upgraded: Level {oldLevel} → {buildingData.Level}");

            // TRIGGER UI UPDATE IMMEDIATELY
            OnBuildingUpgraded?.Invoke(buildingId);

            // FORCE SLIDER UPDATE
            if (_currentSelectedBuilding == buildingId)
            {
                // Clear and recreate the panel to ensure fresh data
                UpdateBuildingPanel();

                // Additional direct slider update
                var panelUI = buildingPanelContent.GetComponentInChildren<BuildingPanelUI3D>();
                if (panelUI != null)
                {
                    panelUI.UpdateLevelSlider();
                    panelUI.UpdateAllUI();
                }
            }

            // Force save after upgrade
            if (GameManager.Instance != null && GameManager.Instance.Save != null)
            {
                GameManager.Instance.Save.DelayedSave(0.5f);
            }
        }
        else
        {
            Debug.Log($"❌ Not enough gold to upgrade {buildingId}. Need: {cost}, Have: {_economy.Gold}");
        }
    }

     public void ActivateModule(string buildingId, string moduleName)
    {
        if (!_isInitialized) return;

        var building = GetBuildingInstance(buildingId);
        if (building != null)
        {
            foreach (var module in building.Modules)
            {
                if (module != null && module.moduleName == moduleName)
                {
                    module.OnButtonClick(building);
                    OnModuleActivated?.Invoke(buildingId, moduleName);
                    OnBuildingDataChanged?.Invoke(buildingId); // NEW EVENT

                    // Refresh panel
                    if (_currentSelectedBuilding == buildingId)
                    {
                        UpdateBuildingPanel();
                    }

                    // Force save after module activation
                    if (GameManager.Instance != null && GameManager.Instance.Save != null)
                    {
                        GameManager.Instance.Save.SaveGame();
                    }
                    break;
                }
            }
        }
    }


    // ========== PUBLIC ACCESS METHODS ==========

    public BuildingConfig GetConfig(string buildingId)
    {
        return _configDictionary.ContainsKey(buildingId) ? _configDictionary[buildingId] : null;
    }

    public BuildingData GetData(string buildingId)
    {
        return _data?.Buildings?.Find(b => b.ID == buildingId);
    }

    public Building GetBuildingInstance(string buildingId)
    {
        return BuildingInstances.ContainsKey(buildingId) ? BuildingInstances[buildingId] : null;
    }

    public BuildingObject3D GetBuildingObject(string buildingId)
    {
        return BuildingObjects.ContainsKey(buildingId) ? BuildingObjects[buildingId] : null;
    }

    public List<BuildingModule> GetBuildingModules(string buildingId)
    {
        var building = GetBuildingInstance(buildingId);
        return building?.Modules ?? new List<BuildingModule>();
    }

    public double GetUpgradeCost(string buildingId)
    {
        var data = GetData(buildingId);
        var config = GetConfig(buildingId);
        return config?.GetCost(data?.Level ?? 0) ?? 0;
    }

    public double GetIncome(string buildingId)
    {
        var data = GetData(buildingId);
        var config = GetConfig(buildingId);
        return data?.IsUnlocked == true ? config?.GetIncome(data.Level) ?? 0 : 0;
    }

    public double GetTotalIncomePerSecond()
    {
        if (!_isInitialized) return 0;

        double totalIncome = 0;
        foreach (var buildingData in _data.Buildings)
        {
            if (buildingData.IsUnlocked)
            {
                totalIncome += GetIncome(buildingData.ID);
            }
        }
        return totalIncome;
    }

    // Add these methods to your existing BuildingManager3D class

    // ========== MODULE MANAGEMENT ==========
    public bool CanAddModule(string buildingId, BuildingModule module)
    {
        var config = GetConfig(buildingId);
        var building = GetBuildingInstance(buildingId);

        if (config == null || building == null) return false;

        // Check if building has slot available and doesn't already have this module
        return building.Modules.Count < config.maxModuleSlots &&
               !building.Modules.Exists(m => m.moduleName == module.moduleName);
    }

    public void AddModuleToBuilding(string buildingId, BuildingModule module)
    {
        if (CanAddModule(buildingId, module))
        {
            var building = GetBuildingInstance(buildingId);
            var config = GetConfig(buildingId);

            // Add module to config (for persistence)
            if (!config.modules.Contains(module))
            {
                config.modules.Add(module);
            }

            // Reinitialize building to include new module
            var buildingData = _data.Buildings.Find(b => b.ID == buildingId);
            BuildingInstances[buildingId] = new Building(config, buildingData);

            Debug.Log($"🔧 Added {module.moduleName} to {config.DisplayName}");

            // Refresh panel if this building is selected
            if (_currentSelectedBuilding == buildingId)
            {
                UpdateBuildingPanel();
            }
        }
    }

    public void RemoveModuleFromBuilding(string buildingId, BuildingModule module)
    {
        var config = GetConfig(buildingId);
        var building = GetBuildingInstance(buildingId);

        if (config != null && building != null)
        {
            config.modules.Remove(module);

            // Reinitialize building without the module
            var buildingData = _data.Buildings.Find(b => b.ID == buildingId);
            BuildingInstances[buildingId] = new Building(config, buildingData);

            Debug.Log($"🔧 Removed {module.moduleName} from {config.DisplayName}");

            // Refresh panel if this building is selected
            if (_currentSelectedBuilding == buildingId)
            {
                UpdateBuildingPanel();
            }
        }
    }

    // Add this method to BuildingManager3D.cs for debugging
    [ContextMenu("Debug Building Setup")]
    public void DebugBuildingSetup()
    {
        Debug.Log("=== BUILDING MANAGER DEBUG ===");
        Debug.Log($"All Buildings Count: {AllBuildings.Count}");
        Debug.Log($"Initialized: {_isInitialized}");
        Debug.Log($"Data: {_data != null}");

        foreach (var config in AllBuildings)
        {
            if (config != null)
            {
                Debug.Log($"Building: {config.DisplayName} (ID: {config.ID})");
                Debug.Log($"- Modules: {config.modules?.Count ?? 0}");
                if (config.modules != null)
                {
                    foreach (var module in config.modules)
                    {
                        if (module != null)
                        {
                            Debug.Log($"  - Module: {module.moduleName} (ShowInUI: {module.showInUI})");
                        }
                    }
                }

                var buildingData = _data?.Buildings?.Find(b => b.ID == config.ID);
                Debug.Log($"- Building Data: {(buildingData != null ? "Found" : "Not Found")}");
            }
        }

        Debug.Log($"Building Instances: {BuildingInstances.Count}");
        Debug.Log($"Building Objects: {BuildingObjects.Count}");
        Debug.Log($"Current Selected: {_currentSelectedBuilding}");
        Debug.Log("=== DEBUG COMPLETE ===");
    }

    // Add this method to BuildingManager3D class
    public void RefreshUI()
    {
        if (!string.IsNullOrEmpty(_currentSelectedBuilding))
        {
            UpdateBuildingPanel();
        }
    }
    public void RefreshBuildingUI(string buildingId)
    {
        if (_currentSelectedBuilding == buildingId)
        {
            UpdateBuildingPanel();
        }
    }
}