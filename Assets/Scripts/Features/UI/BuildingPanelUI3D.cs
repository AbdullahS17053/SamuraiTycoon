using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class BuildingPanelUI3D : MonoBehaviour
{
    public static BuildingPanelUI3D Instance;

    [Header("Building Info UI")]
    public TextMeshProUGUI buildingNameText;
    public TextMeshProUGUI buildingLevelText;
    public TextMeshProUGUI buildingDescriptionText;
    public TextMeshProUGUI buildingIncomeText;
    public TextMeshProUGUI upgradeCostText;
    public Image buildingIconImage;
    public Button upgradeButton;
    public Button closeButton;

    [Header("Module Container")]
    public Transform moduleContainer;
    public GameObject moduleButtonPrefab;
    public TextMeshProUGUI moduleSectionTitle;

    [Header("Stats Display")]
    public TextMeshProUGUI levelStatText;
    public TextMeshProUGUI incomeStatText;
    public TextMeshProUGUI upgradeCostStatText;
    public TextMeshProUGUI statusStatText;
    public TextMeshProUGUI modulesStatText;
    public TextMeshProUGUI capacityStatText;
    public TextMeshProUGUI efficiencyStatText;
    public TextMeshProUGUI productionStatText;

    [Header("UI Elements")]
    public Slider buildingLevelSlider;

    [Header("Visual Settings")]
    public Color maxLevelColor = Color.yellow;
    public Color affordableColor = Color.green;
    public Color expensiveColor = Color.red;
    public Color normalColor = Color.white;

    [Header("Text Formatting")]
    public string currencyFormat = "{0} Gold";
    public string valueFormat = "{0}";
    public string capacityFormat = "{0}/{1}";
    public string percentageFormat = "{0}%";
    public string rateFormat = "{0}/s";

    // Private fields
    private Building _building;
    private BuildingConfig _config;
    private BuildingData _data;
    private EconomyManager _economy;
    private List<GameObject> _moduleButtons = new List<GameObject>();
    private List<ModuleButtonController> _moduleControllers = new List<ModuleButtonController>();
    private bool _isInitialized = false;

    // Event subscriptions
    private bool _isSubscribedToEvents = false;

    // Cache for performance
    private BuildingManager3D _buildingManager;

    public Image ThemedBuilding;
    public Image BuildingBanner;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

    }
    private void Start()
    {

        UpdateAllUI();

        // Debug initial slider state
        if (buildingLevelSlider == null)
        {
            Debug.LogError("❌ buildingLevelSlider is not assigned in the inspector!");
        }
        else
        {
            Debug.Log("✅ buildingLevelSlider is properly assigned");
        }
    }

    private void OnEnable()
    {
        SubscribeToEvents();
    }

    private void OnDisable()
    {
        UnsubscribeFromEvents();
    }

    private void OnDestroy()
    {
        UnsubscribeFromEvents();
        ClearModuleButtons();
    }

    public void Initialize(Building building, BuildingConfig config, BuildingData data, EconomyManager economy)
    {
        _building = building;
        _config = config;
        _data = data;
        _economy = economy;
        _buildingManager = BuildingManager3D.Instance;

        SetupEventListeners();
        UpdateAllUI();
        CreateModuleButtons();

        _isInitialized = true;

        Debug.Log($"✔️ BuildingPanelUI3D initialized for {config.DisplayName}");
    }

    private void SetupEventListeners()
    {
        // Safely setup upgrade button
        if (upgradeButton != null)
        {
            upgradeButton.onClick.RemoveAllListeners();
            upgradeButton.onClick.AddListener(OnUpgradeClicked);
        }
        else
        {
            Debug.LogWarning("🔴 UpgradeButton not assigned - upgrade functionality disabled");
        }

        // Safely setup close button
        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(OnCloseClicked);
        }
        else
        {
            Debug.LogWarning("🔴 CloseButton not assigned - manual close disabled");
        }

        SubscribeToEvents();
    }

    private void SubscribeToEvents()
    {
        if (_isSubscribedToEvents) return;

        // Subscribe to economy events
        if (_economy != null)
        {
            _economy.OnGoldChanged += OnCurrencyChanged;
        }

        // Subscribe to building manager events
        if (_buildingManager != null)
        {
            _buildingManager.OnBuildingUpgraded += OnBuildingUpgraded;
            _buildingManager.OnModuleActivated += OnModuleActivated;

            // ADD THIS NEW EVENT SUBSCRIPTION
            if (_buildingManager is BuildingManager3D buildingManager3D)
            {
                // Use reflection or add this event to BuildingManager3D
                // buildingManager3D.OnBuildingDataChanged += OnBuildingDataChanged;
            }
        }

        _isSubscribedToEvents = true;
        Debug.Log("✅ BuildingPanelUI3D events subscribed");
    }

    private void UnsubscribeFromEvents()
    {
        if (!_isSubscribedToEvents) return;

        // Unsubscribe from economy events
        if (_economy != null)
        {
            _economy.OnGoldChanged -= OnCurrencyChanged;
        }

        // Unsubscribe from building manager events
        if (_buildingManager != null)
        {
            _buildingManager.OnBuildingUpgraded -= OnBuildingUpgraded;
            _buildingManager.OnModuleActivated -= OnModuleActivated;
        }

        _isSubscribedToEvents = false;
    }

    private void OnBuildingUpgraded(string buildingId)
    {
        Debug.Log($"🔄 OnBuildingUpgraded received: {buildingId}, current building: {_config?.ID}");

        if (_config != null && _config.ID == buildingId)
        {
            // Force immediate data refresh
            RefreshData();

            // Update slider FIRST
            UpdateLevelSlider();

            // Then update everything else
            UpdateAllUI();

            Debug.Log($"✅ UI updated for building upgrade: {buildingId}");
        }
    }


    private void OnModuleActivated(string buildingId, string moduleName)
    {
        // If the module was activated on our building, update UI
        if (_config != null && _config.ID == buildingId)
        {
            RefreshData();
            UpdateAllUI();
        }
    }

    private void RefreshData()
    {
        // Refresh data references to ensure we have the latest data
        if (_buildingManager != null && _config != null)
        {
            var freshData = _buildingManager.GetData(_config.ID);
            if (freshData != null)
            {
                _data = freshData;
            }

            var freshBuilding = _buildingManager.GetBuildingInstance(_config.ID);
            if (freshBuilding != null)
            {
                _building = freshBuilding;
            }
        }
    }

    public void UpdateAllUI()
    {
        if (!_isInitialized)
        {
            Debug.LogWarning("❌ BuildingPanelUI3D not initialized in UpdateAllUI");
            return;
        }

        Debug.Log($"🔄 UpdateAllUI called for {_config?.DisplayName}");

        try
        {
            // UPDATE SLIDER FIRST - This is critical
            UpdateLevelSlider();

            // Then update other UI elements
            UpdateBuildingInfo();
            UpdateStatsDisplay();
            UpdateModuleSectionTitle();
            UpdateModuleButtons();

            Debug.Log($"✅ UpdateAllUI completed for {_config.DisplayName}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ Error in UpdateAllUI: {e.Message}");
        }
    }
    // ADD THIS METHOD for direct slider manipulation
    [ContextMenu("Force Slider Update")]
    public void ForceSliderUpdate()
    {
        Debug.Log("🔄 ForceSliderUpdate called");
        if (buildingLevelSlider != null && _data != null)
        {
            buildingLevelSlider.value = _data.Level;
            Debug.Log($"✅ Slider forcefully set to: {_data.Level}");
        }
        else
        {
            Debug.LogError($"❌ Cannot force slider update: slider={buildingLevelSlider != null}, data={_data != null}");
        }
    }
    private void OnBuildingDataChanged(string buildingId)
    {
        if (_config != null && _config.ID == buildingId)
        {
            Debug.Log($"🔄 OnBuildingDataChanged: {buildingId}");

            // Refresh data references
            RefreshData();

            // Update slider immediately
            UpdateLevelSlider();

            // Update all UI
            UpdateAllUI();
        }
    }

    private void UpdateBuildingInfo()
    {
        if (_config == null || _data == null) return;

        // Basic info - safely update each field
        SafeSetText(buildingNameText, _config.DisplayName);
        SafeSetText(buildingLevelText, $"Level {_data.Level}");
        SafeSetText(buildingDescriptionText, _config.Description);

        // Update themed elements
        if (ThemedBuilding != null)
            ThemedBuilding.color = _config.ThemeColor;

        if (BuildingBanner != null && _config.Banner != null)
            BuildingBanner.sprite = _config.Banner;

        // Income and costs
        double income = CalculateTotalIncome();
        double upgradeCost = CalculateUpgradeCost();

        SafeSetText(buildingIncomeText, string.Format(rateFormat, FormatNumber(income)));

        // Update upgrade cost text
        if (upgradeCostText != null)
        {
            bool isMaxLevel = _data.Level >= GetMaxLevel();
            upgradeCostText.text = isMaxLevel ?
                "MAX LEVEL" :
                string.Format(currencyFormat, FormatNumber(upgradeCost));

            upgradeCostText.color = isMaxLevel ?
                maxLevelColor :
                (_economy != null && _economy.CanAfford(upgradeCost) ? affordableColor : expensiveColor);
        }

        // Update icon
        if (buildingIconImage != null)
        {
            if (_config.Icon != null)
            {
                buildingIconImage.sprite = _config.Icon;
                buildingIconImage.color = Color.white;
            }
            else
            {
                buildingIconImage.color = Color.clear;
            }
        }

        // Upgrade button state
        if (upgradeButton != null)
        {
            bool isMaxLevel = _data.Level >= GetMaxLevel();
            bool canAfford = _economy != null && _economy.CanAfford(upgradeCost);

            upgradeButton.interactable = !isMaxLevel && canAfford;

            // Visual feedback
            var buttonImage = upgradeButton.GetComponent<Image>();
            if (buttonImage != null)
            {
                buttonImage.color = !isMaxLevel && canAfford ? affordableColor : expensiveColor;
            }
        }

        UpdateLevelSlider();
    }

    // ENHANCED SLIDER UPDATE METHOD
    public void UpdateLevelSlider()
    {
        if (buildingLevelSlider == null)
        {
            Debug.LogWarning("❌ buildingLevelSlider is null!");
            return;
        }

        if (_data == null)
        {
            Debug.LogWarning("❌ _data is null in UpdateLevelSlider!");
            return;
        }

        if (_config == null)
        {
            Debug.LogWarning("❌ _config is null in UpdateLevelSlider!");
            return;
        }

        try
        {
            int maxLevel = GetMaxLevel();
            int currentLevel = _data.Level;

            Debug.Log($"🔄 UpdateLevelSlider: {_config.DisplayName} - Level {currentLevel}/{maxLevel}");

            // Set slider values
            buildingLevelSlider.minValue = 0;
            buildingLevelSlider.maxValue = maxLevel;
            buildingLevelSlider.value = currentLevel;

            // Force slider visual update
            buildingLevelSlider.onValueChanged?.Invoke(currentLevel);

            // Update fill area color based on progress
            UpdateSliderVisuals(currentLevel, maxLevel);

            Debug.Log($"✅ Slider updated: {currentLevel}/{maxLevel}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ Error in UpdateLevelSlider: {e.Message}");
        }
    }
    private void UpdateSliderVisuals(int currentLevel, int maxLevel)
    {
        // Update slider fill color based on progress
        var fillArea = buildingLevelSlider.fillRect;
        if (fillArea != null)
        {
            var fillImage = fillArea.GetComponent<Image>();
            if (fillImage != null)
            {
                float progress = (float)currentLevel / maxLevel;
                fillImage.color = Color.Lerp(expensiveColor, affordableColor, progress);
            }
        }

        // Update background color
        var background = buildingLevelSlider.transform.Find("Background")?.GetComponent<Image>();
        if (background != null)
        {
            background.color = new Color(0.2f, 0.2f, 0.2f, 0.5f);
        }
    }

    private void UpdateStatsDisplay()
    {
        if (_building == null || _config == null || _data == null) return;

        double income = CalculateTotalIncome();
        double upgradeCost = CalculateUpgradeCost();
        int maxLevel = GetMaxLevel();

        // Only update stats if their Text components are assigned
        SafeSetText(levelStatText, string.Format(capacityFormat, _data.Level, maxLevel));
        SafeSetText(incomeStatText, string.Format(rateFormat, FormatNumber(income)));

        // Upgrade cost stat
        if (upgradeCostStatText != null)
        {
            bool isMaxLevel = _data.Level >= maxLevel;
            upgradeCostStatText.text = isMaxLevel ?
                "MAXED" :
                string.Format(currencyFormat, FormatNumber(upgradeCost));

            upgradeCostStatText.color = isMaxLevel ?
                maxLevelColor :
                (_economy != null && _economy.CanAfford(upgradeCost) ? affordableColor : expensiveColor);
        }

        // Status stat
        if (statusStatText != null)
        {
            statusStatText.text = _data.IsUnlocked ? "ACTIVE" : "LOCKED";
            statusStatText.color = _data.IsUnlocked ? affordableColor : expensiveColor;
        }

        // Modules stat
        if (modulesStatText != null)
        {
            int maxedModules = CountMaxedModules();
            int totalModules = _building.Modules.Count;
            modulesStatText.text = string.Format(capacityFormat, totalModules - maxedModules, totalModules);
        }

        // Capacity stat
        if (capacityStatText != null)
        {
            var capacityModule = _building.GetModule<CapacityModule>();
            if (capacityModule != null)
            {
                capacityStatText.text = string.Format(capacityFormat,
                    capacityModule.currentWorkers,
                    capacityModule.GetMaxCapacity(_building));
            }
            else
            {
                capacityStatText.text = "N/A";
            }
        }

        // Efficiency stat
        if (efficiencyStatText != null)
        {
            var speedModule = _building.GetModule<SpeedModule>();
            if (speedModule != null)
            {
                efficiencyStatText.text = string.Format(percentageFormat,
                    (speedModule.GetCurrentSpeedMultiplier() * 100).ToString("F0"));
            }
            else
            {
                efficiencyStatText.text = "100%";
            }
        }

        // Production stat
        if (productionStatText != null)
        {
            productionStatText.text = string.Format(valueFormat,
                CalculateProductionMultiplier().ToString("F1") + "x");
        }
    }

    private void CreateModuleButtons()
    {
        ClearModuleButtons();

        // Check if we have the necessary components
        if (moduleContainer == null)
        {
            Debug.LogWarning("🔴 ModuleContainer not assigned - module buttons disabled");
            return;
        }

        if (moduleButtonPrefab == null)
        {
            Debug.LogWarning("🔴 ModuleButtonPrefab not assigned - module buttons disabled");
            return;
        }

        if (_building?.Modules == null)
        {
            Debug.LogWarning("🔴 No modules found for building");
            return;
        }

        // Create buttons for each module
        foreach (var module in _building.Modules)
        {
            if (module != null && module.showInUI)
            {
                CreateModuleButton(module);
            }
        }

        UpdateModuleSectionTitle();
    }

    private void CreateModuleButton(BuildingModule module)
    {
        var buttonObj = Instantiate(moduleButtonPrefab, moduleContainer);
        if (buttonObj == null)
        {
            Debug.LogError("❌ Failed to instantiate module button");
            return;
        }

        var controller = buttonObj.GetComponent<ModuleButtonController>();
        if (controller != null)
        {
            controller.Initialize(module, _building, _economy);
            _moduleControllers.Add(controller);
        }
        else
        {
            Debug.LogWarning($"⚠️ ModuleButtonController not found on {moduleButtonPrefab.name}");
        }

        _moduleButtons.Add(buttonObj);
    }

    private void ClearModuleButtons()
    {
        foreach (var button in _moduleButtons)
        {
            if (button != null)
                Destroy(button);
        }
        _moduleButtons.Clear();
        _moduleControllers.Clear();
    }

    private void UpdateModuleButtons()
    {
        foreach (var controller in _moduleControllers)
        {
            if (controller != null)
                controller.UpdateUI();
        }
    }

    private void UpdateModuleSectionTitle()
    {
        if (moduleSectionTitle != null && _building?.Modules != null)
        {
            int maxedModules = CountMaxedModules();
            int totalModules = _building.Modules.Count;
            moduleSectionTitle.text = $"Modules ({totalModules - maxedModules}/{totalModules} Available)";
        }
    }

    // Event handlers
    private void OnUpgradeClicked()
    {
        if (_buildingManager != null && _config != null)
        {
            _buildingManager.UpgradeBuilding(_config.ID);
            // UI will update via event system
        }
    }

    private void OnCurrencyChanged(double change)
    {
        if (!_isInitialized) return;

        RefreshData();
        UpdateAllUI();
    }

    private void OnCloseClicked()
    {
        if (_buildingManager != null)
        {
            _buildingManager.HideBuildingPanel();
        }
    }

    // Helper methods
    private void SafeSetText(TextMeshProUGUI textField, string value)
    {
        if (textField != null)
        {
            textField.text = value;
        }
    }

    private double CalculateTotalIncome() => _config?.GetIncome(_data.Level) ?? 0;
    private double CalculateUpgradeCost() => _config?.GetCost(_data.Level) ?? 0;

    private int GetMaxLevel()
    {
        // You can make this configurable per building if needed
        return 20;
    }

    private double CalculateProductionMultiplier()
    {
        double multiplier = 1.0;

        var speedModule = _building?.GetModule<SpeedModule>();
        if (speedModule != null)
        {
            multiplier *= speedModule.GetCurrentSpeedMultiplier();
        }

        var capacityModule = _building?.GetModule<CapacityModule>();
        if (capacityModule != null && capacityModule.GetMaxCapacity(_building) > 0)
        {
            multiplier *= (double)capacityModule.currentWorkers / capacityModule.GetMaxCapacity(_building);
        }

        return multiplier;
    }

    private int CountMaxedModules()
    {
        int count = 0;
        if (_building?.Modules != null)
        {
            foreach (var module in _building.Modules)
            {
                if (module != null && module.IsMaxLevel())
                    count++;
            }
        }
        return count;
    }

    private string FormatNumber(double num)
    {
        if (num < 1000) return num.ToString("F0");

        string[] suffixes = { "", "K", "M", "B", "T" };
        int suffixIndex = 0;

        while (num >= 1000 && suffixIndex < suffixes.Length - 1)
        {
            num /= 1000;
            suffixIndex++;
        }

        return num.ToString("F2") + suffixes[suffixIndex];
    }

    [ContextMenu("Check Assigned Fields")]
    private void CheckAssignedFields()
    {
        Debug.Log("=== BuildingPanelUI3D Field Check ===");
        Debug.Log($"Building Name Text: {buildingNameText != null}");
        Debug.Log($"Building Level Text: {buildingLevelText != null}");
        Debug.Log($"Building Description Text: {buildingDescriptionText != null}");
        Debug.Log($"Building Income Text: {buildingIncomeText != null}");
        Debug.Log($"Upgrade Cost Text: {upgradeCostText != null}");
        Debug.Log($"Building Icon: {buildingIconImage != null}");
        Debug.Log($"Upgrade Button: {upgradeButton != null}");
        Debug.Log($"Close Button: {closeButton != null}");
        Debug.Log($"Module Container: {moduleContainer != null}");
        Debug.Log($"Module Button Prefab: {moduleButtonPrefab != null}");
        Debug.Log($"Module Section Title: {moduleSectionTitle != null}");
        Debug.Log($"Level Stat Text: {levelStatText != null}");
        Debug.Log($"Income Stat Text: {incomeStatText != null}");
        Debug.Log($"Upgrade Cost Stat Text: {upgradeCostStatText != null}");
        Debug.Log($"Status Stat Text: {statusStatText != null}");
        Debug.Log($"Modules Stat Text: {modulesStatText != null}");
        Debug.Log($"Capacity Stat Text: {capacityStatText != null}");
        Debug.Log($"Efficiency Stat Text: {efficiencyStatText != null}");
        Debug.Log($"Production Stat Text: {productionStatText != null}");
        Debug.Log($"Building Level Slider: {buildingLevelSlider != null}");
        Debug.Log("=== Field Check Complete ===");
    }

    [ContextMenu("Force UI Refresh")]
    public void ForceUIRefresh()
    {
        RefreshData();
        UpdateAllUI();
        Debug.Log("🔄 BuildingPanelUI3D forcefully refreshed");
    }

    [ContextMenu("Debug Slider Status")]
    public void DebugSliderStatus()
    {
        Debug.Log("=== SLIDER DEBUG INFO ===");
        Debug.Log($"Slider Object: {buildingLevelSlider != null}");

        if (buildingLevelSlider != null)
        {
            Debug.Log($"Slider Min: {buildingLevelSlider.minValue}");
            Debug.Log($"Slider Max: {buildingLevelSlider.maxValue}");
            Debug.Log($"Slider Value: {buildingLevelSlider.value}");
            Debug.Log($"Slider Active: {buildingLevelSlider.gameObject.activeInHierarchy}");
            Debug.Log($"Slider Interactable: {buildingLevelSlider.interactable}");
        }

        Debug.Log($"Data: {_data != null}");
        if (_data != null)
        {
            Debug.Log($"Building Level: {_data.Level}");
        }

        Debug.Log($"Config: {_config != null}");
        if (_config != null)
        {
            Debug.Log($"Building Name: {_config.DisplayName}");
        }

        Debug.Log($"Initialized: {_isInitialized}");
        Debug.Log("=== END SLIDER DEBUG ===");
    }
}