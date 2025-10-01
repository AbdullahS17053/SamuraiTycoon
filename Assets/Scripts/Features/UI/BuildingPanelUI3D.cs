using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class BuildingPanelUI3D : MonoBehaviour
{
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
    public TextMeshProUGUI statsText;
    public Slider buildingLevelSlider;

    [Header("Module Grid Layout")]
    public GridLayoutGroup moduleGridLayout;

    [Header("Max Level Colors")]
    public Color maxLevelColor = Color.yellow;
    public Color normalColor = Color.white;

    private Building _building;
    private BuildingConfig _config;
    private BuildingData _data;
    private EconomyManager _economy;
    private List<GameObject> _moduleButtons = new List<GameObject>();

    public void Initialize(Building building, BuildingConfig config, BuildingData data, EconomyManager economy)
    {
        Debug.Log($"🔄 BuildingPanelUI3D.Initialize called for: {config.DisplayName}");

        _building = building;
        _config = config;
        _data = data;
        _economy = economy;


        // Debug module information
        Debug.Log($"🔍 Building: {config.DisplayName}");
        Debug.Log($"🔍 Building Modules Count: {_building?.Modules?.Count ?? 0}");
        if (_building?.Modules != null)
        {
            foreach (var module in _building.Modules)
            {
                if (module != null)
                {
                    Debug.Log($"🔍 Module: {module.moduleName} (ShowInUI: {module.showInUI})");
                }
            }
        }

        // Initialize UI components
        UpdateBuildingInfo();
        CreateModuleButtons();
        UpdateStatsDisplay();

        // Subscribe to events
        if (upgradeButton != null)
            upgradeButton.onClick.AddListener(OnUpgradeClicked);
        else
            Debug.LogError("❌ UpgradeButton is null!");

        if (closeButton != null)
            closeButton.onClick.AddListener(OnCloseClicked);
        else
            Debug.LogError("❌ CloseButton is null!");

        if (_economy != null)
        {
            _economy.OnGoldChanged += OnCurrencyChanged;
        }

        Debug.Log($"✅ BuildingPanelUI3D initialized successfully for: {config.DisplayName}");
    }

    void UpdateBuildingInfo()
    {
        if (_config == null || _data == null)
        {
            Debug.LogError("❌ Cannot update building info - config or data is null!");
            return;
        }

        // Update basic building info
        if (buildingNameText != null)
            buildingNameText.text = _config.DisplayName;

        if (buildingLevelText != null)
            buildingLevelText.text = $"Level {_data.Level}";

        if (buildingDescriptionText != null)
            buildingDescriptionText.text = _config.Description;

        double income = CalculateTotalIncome();
        if (buildingIncomeText != null)
            buildingIncomeText.text = $"+{FormatNumber(income)}/s";

        double upgradeCost = CalculateUpgradeCost();
        if (upgradeCostText != null)
            upgradeCostText.text = $"Upgrade: {FormatNumber(upgradeCost)} Gold";

        if (buildingIconImage != null && _config.Icon != null)
            buildingIconImage.sprite = _config.Icon;

        if (upgradeButton != null)
            upgradeButton.interactable = _economy != null && _economy.Gold >= upgradeCost;

        // Update level slider
        if (buildingLevelSlider != null)
        {
            buildingLevelSlider.maxValue = 20;
            buildingLevelSlider.value = _data.Level;
        }

        // Update module section title
        if (moduleSectionTitle != null)
        {
            int maxedModules = CountMaxedModules();
            moduleSectionTitle.text = $"Upgrade Modules ({_building.Modules.Count - maxedModules}/{_config.maxModuleSlots} Available)";
        }
    }

    void CreateModuleButtons()
    {
        Debug.Log($"🔧 CreateModuleButtons called");

        // Clear existing buttons
        foreach (var button in _moduleButtons)
        {
            if (button != null)
                Destroy(button);
        }
        _moduleButtons.Clear();

        if (moduleContainer == null)
        {
            Debug.LogError("❌ Module container is null!");
            return;
        }

        if (moduleButtonPrefab == null)
        {
            Debug.LogError("❌ Module button prefab is null!");
            return;
        }

        if (_building == null)
        {
            Debug.LogError("❌ Building is null!");
            return;
        }

        if (_building.Modules == null || _building.Modules.Count == 0)
        {
            Debug.LogWarning($"⚠️ No modules found for building: {_config.DisplayName}");
            return;
        }

        Debug.Log($"🔧 Processing {_building.Modules.Count} modules for {_config.DisplayName}");

        int createdButtons = 0;
        foreach (var module in _building.Modules)
        {
            if (module != null && module.showInUI)
            {
                if (CreateModuleButton(module))
                {
                    createdButtons++;
                }
            }
            else
            {
                Debug.Log($"⚠️ Module is null or showInUI is false: {module?.moduleName}");
            }
        }

        Debug.Log($"✅ Created {createdButtons} module buttons for {_config.DisplayName}");
    }

    bool CreateModuleButton(BuildingModule module)
    {
        Debug.Log($"🔧 Creating button for module: {module.moduleName}");

        if (moduleButtonPrefab == null)
        {
            Debug.LogError("❌ Module button prefab is null!");
            return false;
        }

        var buttonObj = Instantiate(moduleButtonPrefab, moduleContainer);
        if (buttonObj == null)
        {
            Debug.LogError("❌ Failed to instantiate button object!");
            return false;
        }

        // Try to use ModuleButtonController if available
        ModuleButtonController buttonController = buttonObj.GetComponent<ModuleButtonController>();
        if (buttonController != null)
        {
            // Use the controller to set up the button
            buttonController.Initialize(module, _building, _economy);

            // Set up click listener
            if (!module.IsMaxLevel())
            {
                string moduleName = module.moduleName;
                buttonController.button.onClick.AddListener(() => OnModuleClicked(moduleName));
            }
            else
            {
                buttonController.button.interactable = false;
            }
        }
        else
        {
            // Fallback to the flexible system
            Button button = FindButtonInChildren(buttonObj);
            if (button == null)
            {
                Debug.LogError("❌ No Button component found in module button prefab!");
                Destroy(buttonObj);
                return false;
            }

            TextMeshProUGUI[] textComponents = buttonObj.GetComponentsInChildren<TextMeshProUGUI>(true);
            TextMeshProUGUI mainText = textComponents.Length > 0 ? textComponents[0] : null;

            UpdateModuleButtonUI(buttonObj, button, mainText, module);

            if (!module.IsMaxLevel())
            {
                string moduleName = module.moduleName;
                button.onClick.AddListener(() => OnModuleClicked(moduleName));
            }
            else
            {
                button.interactable = false;
            }
        }

        _moduleButtons.Add(buttonObj);
        Debug.Log($"✅ Successfully created button for module: {module.moduleName}");
        return true;
    }

    Button FindButtonInChildren(GameObject parent)
    {
        // First try to find a button in the entire hierarchy
        Button button = parent.GetComponentInChildren<Button>(true);

        if (button != null)
        {
            return button;
        }

        // If no button found, try to find by common names
        Transform purchaseChild = parent.transform.Find("purchase");
        if (purchaseChild != null)
        {
            button = purchaseChild.GetComponent<Button>();
            if (button != null) return button;
        }

        Transform buttonChild = parent.transform.Find("button");
        if (buttonChild != null)
        {
            button = buttonChild.GetComponent<Button>();
            if (button != null) return button;
        }

        Transform btnChild = parent.transform.Find("btn");
        if (btnChild != null)
        {
            button = btnChild.GetComponent<Button>();
            if (button != null) return button;
        }

        return null;
    }

    void UpdateModuleButtonUI(GameObject buttonObj, Button button, TextMeshProUGUI mainText, BuildingModule module)
    {
        if (mainText != null)
        {
            // Create a formatted string with module information
            string status = module.GetStatusText(_building);
            string formattedText = $"{module.buttonText}\n{status}";
            mainText.text = formattedText;

            // Change text color for max level
            if (module.IsMaxLevel())
            {
                mainText.color = maxLevelColor;
            }
        }

        // Update button appearance
        Image buttonImage = button.GetComponent<Image>();
        if (buttonImage != null)
        {
            Color buttonColor = module.buttonColor;
            if (buttonColor == Color.clear) // Use default if not set
                buttonColor = normalColor;

            buttonImage.color = module.IsMaxLevel() ? maxLevelColor : buttonColor;
        }

        // Update button interactability
        button.interactable = !module.IsMaxLevel() && module.CanActivate(_building, _economy.Gold);
    }

    void UpdateStatsDisplay()
    {
        if (_building == null) return;

        string stats = $"<size=18><b>{_config.DisplayName} Statistics</b></size>\n\n";
        stats += $"<b>Level:</b> {_data.Level}\n";
        stats += $"<b>Income:</b> {FormatNumber(CalculateTotalIncome())}/s\n";
        stats += $"<b>Next Upgrade:</b> {FormatNumber(CalculateUpgradeCost())} Gold\n";
        stats += $"<b>Status:</b> {(_data.IsUnlocked ? "🟢 Active" : "🔴 Locked")}\n";
        stats += $"<b>Modules:</b> {_building.Modules.Count}/{_config.maxModuleSlots}\n";

        int maxedModules = CountMaxedModules();
        if (maxedModules > 0)
        {
            stats += $"<b>Maxed Modules:</b> {maxedModules}\n";
        }

        // Add module-specific stats
        if (_building.Modules != null && _building.Modules.Count > 0)
        {
            stats += $"\n<size=16><b>Active Modules:</b></size>\n";
            foreach (var module in _building.Modules)
            {
                if (module != null)
                {
                    string maxedIndicator = module.IsMaxLevel() ? " 🏆" : "";
                    stats += $"\n<b>{module.moduleName}{maxedIndicator}:</b>\n";
                    stats += $"{module.GetStatusText(_building)}\n";
                }
            }
        }

        if (statsText != null)
            statsText.text = stats;
    }

    int CountMaxedModules()
    {
        int count = 0;
        if (_building?.Modules != null)
        {
            foreach (var module in _building.Modules)
            {
                if (module != null && module.IsMaxLevel())
                {
                    count++;
                }
            }
        }
        return count;
    }

    void OnUpgradeClicked()
    {
        var buildingManager = FindObjectOfType<BuildingManager3D>();
        if (buildingManager != null)
        {
            buildingManager.UpgradeBuilding(_config.ID);
            UpdateBuildingInfo();
            UpdateStatsDisplay();
            UpdateModuleButtons();
        }
    }

    void OnModuleClicked(string moduleName)
    {
        var buildingManager = FindObjectOfType<BuildingManager3D>();
        if (buildingManager != null)
        {
            buildingManager.ActivateModule(_config.ID, moduleName);
            UpdateStatsDisplay();
            UpdateModuleButtons();
        }
    }

    void UpdateModuleButtons()
    {
        // Recreate all module buttons to reflect updated state
        CreateModuleButtons();
    }

    void OnCloseClicked()
    {
        var buildingManager = FindObjectOfType<BuildingManager3D>();
        if (buildingManager != null)
        {
            buildingManager.HideBuildingPanel();
        }
    }

    void OnCurrencyChanged(double change)
    {
        UpdateBuildingInfo();
        UpdateModuleButtons();
    }

    double CalculateTotalIncome()
    {
        if (_config != null && _data != null && _data.IsUnlocked)
        {
            return _config.GetIncome(_data.Level);
        }
        return 0;
    }

    double CalculateUpgradeCost()
    {
        if (_config != null && _data != null)
        {
            return _config.GetCost(_data.Level);
        }
        return 0;
    }

    string FormatNumber(double num)
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

    void OnDestroy()
    {
        if (_economy != null)
            _economy.OnGoldChanged -= OnCurrencyChanged;

        // Clean up spawned buttons
        foreach (var button in _moduleButtons)
        {
            if (button != null)
                Destroy(button);
        }
        _moduleButtons.Clear();
    }

    // Enhanced test method for your specific prefab structure
    [ContextMenu("Test Prefab Structure")]
    public void TestPrefabStructure()
    {
        if (moduleButtonPrefab == null)
        {
            Debug.LogError("❌ No module button prefab assigned!");
            return;
        }

        Debug.Log("🧪 Testing Prefab Structure...");

        // Test instantiation
        var testObj = Instantiate(moduleButtonPrefab);

        // Find button
        Button button = FindButtonInChildren(testObj);
        if (button != null)
        {
            Debug.Log($"✅ Found Button: {button.name}");
        }
        else
        {
            Debug.LogError("❌ No button found in prefab!");
        }

        // Find texts
        TextMeshProUGUI[] texts = testObj.GetComponentsInChildren<TextMeshProUGUI>(true);
        Debug.Log($"✅ Found {texts.Length} TextMeshPro components:");
        foreach (var text in texts)
        {
            Debug.Log($"   - {text.name}: '{text.text}'");
        }

        // Clean up
        DestroyImmediate(testObj);
        Debug.Log("✅ Prefab structure test completed");
    }
}