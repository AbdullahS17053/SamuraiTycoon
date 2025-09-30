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
    public VerticalLayoutGroup moduleGridLayout;
    public int maxModulesPerRow = 2;

    [Header("Max Level Colors")]
    public Color maxLevelColor = Color.yellow;
    public Color normalColor = Color.white;

    private Building _building;
    private BuildingConfig _config;
    private BuildingData _data;
    private EconomyManager _economy;
    private List<GameObject> _moduleButtons = new List<GameObject>();
    private bool _isInitialized = false;

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
                Debug.Log($"🔍 Module: {module?.moduleName} (ShowInUI: {module?.showInUI})");
            }
        }


        UpdateBuildingInfo();
        CreateModuleButtons();
        UpdateStatsDisplay();

        // Subscribe to events
        upgradeButton.onClick.AddListener(OnUpgradeClicked);
        closeButton.onClick.AddListener(OnCloseClicked);

        if (_economy != null)
        {
            _economy.OnGoldChanged += OnCurrencyChanged;
        }
        else
        {
            Debug.LogWarning("❌ EconomyManager is null!");
        }

        _isInitialized = true;
        Debug.Log($"✅ BuildingPanelUI3D initialized for: {config.DisplayName}");
    }

    void UpdateBuildingInfo()
    {
        if (!_isInitialized || _config == null || _data == null)
        {
            Debug.LogError("❌ Cannot update building info - not initialized!");
            return;
        }

        buildingNameText.text = _config.DisplayName;
        buildingLevelText.text = $"Level {_data.Level}";
        buildingDescriptionText.text = _config.Description;

        double income = CalculateTotalIncome();
        buildingIncomeText.text = $"+{FormatNumber(income)}/s";

        double upgradeCost = CalculateUpgradeCost();
        upgradeCostText.text = $"Upgrade: {FormatNumber(upgradeCost)} Gold";

        if (buildingIconImage != null && _config.Icon != null)
            buildingIconImage.sprite = _config.Icon;

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

        if (!_isInitialized)
        {
            Debug.LogError("❌ Not initialized!");
            return;
        }

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

        // Clear existing buttons
        foreach (var button in _moduleButtons)
            Destroy(button);
        _moduleButtons.Clear();

        if (_building == null)
        {
            Debug.LogError("❌ Building is null!");
            return;
        }

        if (_building.Modules == null)
        {
            Debug.LogWarning("⚠️ No modules found for building");
            return;
        }

        Debug.Log($"🔧 Processing {_building.Modules.Count} modules for {_config.DisplayName}");

        int createdButtons = 0;
        foreach (var module in _building.Modules)
        {
            if (module != null && module.showInUI)
            {
                CreateModuleButton(module);
                createdButtons++;
            }
            else
            {
                Debug.Log($"⚠️ Module is null or showInUI is false: {module?.moduleName}");
            }
        }

        Debug.Log($"🔧 Created {createdButtons} module buttons for {_config.DisplayName}");
    }

    void CreateModuleButton(BuildingModule module)
    {
        Debug.Log($"🔧 Creating button for module: {module.moduleName}");

        if (moduleButtonPrefab == null)
        {
            Debug.LogError("❌ Module button prefab is null!");
            return;
        }

        var buttonObj = Instantiate(moduleButtonPrefab, moduleContainer);
        if (buttonObj == null)
        {
            Debug.LogError("❌ Failed to instantiate button object!");
            return;
        }

        var button = buttonObj.GetComponent<Button>();
        var text = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
        var image = buttonObj.GetComponent<Image>();

        if (button == null) Debug.LogError("❌ Button component missing!");
        if (text == null) Debug.LogError("❌ TextMeshPro component missing!");
        if (image == null) Debug.LogError("❌ Image component missing!");

        // Set button appearance based on max level
        if (image != null)
        {
            image.color = module.IsMaxLevel() ? maxLevelColor : module.buttonColor;
        }

        if (text != null)
        {
            text.text = $"{module.buttonText}\n{module.GetStatusText(_building)}";

            // Change text color for max level
            if (module.IsMaxLevel())
            {
                text.color = maxLevelColor;
            }
        }

        // Add click handler (only if not max level)
        if (!module.IsMaxLevel())
        {
            string moduleName = module.moduleName;
            button.onClick.AddListener(() => OnModuleClicked(moduleName));
            Debug.Log($"🔧 Added click listener for module: {moduleName}");
        }
        else
        {
            button.interactable = false;
            Debug.Log($"🔧 Module {module.moduleName} is max level - button disabled");
        }

        _moduleButtons.Add(buttonObj);
        Debug.Log($"✅ Successfully created button for module: {module.moduleName}");
    }

    // ... rest of the methods remain the same as before ...

    void UpdateStatsDisplay()
    {
        if (!_isInitialized || _building == null) return;

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
        if (!_isInitialized) return;

        var buildingManager = FindObjectOfType<BuildingManager3D>();
        if (buildingManager != null)
        {
            buildingManager.UpgradeBuilding(_config.ID);
            UpdateBuildingInfo();
            UpdateStatsDisplay();
            UpdateModuleButtonsText();
        }
    }

    void OnModuleClicked(string moduleName)
    {
        if (!_isInitialized) return;

        var buildingManager = FindObjectOfType<BuildingManager3D>();
        if (buildingManager != null)
        {
            buildingManager.ActivateModule(_config.ID, moduleName);
            UpdateStatsDisplay();
            UpdateModuleButtonsText();
            UpdateModuleButtonAppearance();
        }
    }

    void OnCloseClicked()
    {
        var buildingManager = FindObjectOfType<BuildingManager3D>();
        if (buildingManager != null)
        {
            buildingManager.HideBuildingPanel();
        }
    }

    void UpdateModuleButtonsText()
    {
        if (!_isInitialized || _building == null || _building.Modules == null) return;

        for (int i = 0; i < _building.Modules.Count && i < _moduleButtons.Count; i++)
        {
            var module = _building.Modules[i];
            var buttonObj = _moduleButtons[i];
            var text = buttonObj.GetComponentInChildren<TextMeshProUGUI>();

            if (text != null && module != null)
            {
                text.text = $"{module.buttonText}\n{module.GetStatusText(_building)}";
            }
        }
    }

    void UpdateModuleButtonAppearance()
    {
        if (!_isInitialized || _building == null || _building.Modules == null) return;

        for (int i = 0; i < _building.Modules.Count && i < _moduleButtons.Count; i++)
        {
            var module = _building.Modules[i];
            var buttonObj = _moduleButtons[i];
            var button = buttonObj.GetComponent<Button>();
            var image = buttonObj.GetComponent<Image>();
            var text = buttonObj.GetComponentInChildren<TextMeshProUGUI>();

            if (module != null)
            {
                // Update colors based on max level
                if (image != null)
                {
                    image.color = module.IsMaxLevel() ? maxLevelColor : module.buttonColor;
                }

                if (text != null)
                {
                    text.color = module.IsMaxLevel() ? maxLevelColor : Color.white;
                }

                // Update interactability
                if (button != null)
                {
                    button.interactable = !module.IsMaxLevel() && module.CanActivate(_building, _economy.Gold);
                }
            }
        }
    }

    void OnCurrencyChanged(double change)
    {
        UpdateBuildingInfo();
        UpdateModuleButtonsText();
        UpdateModuleButtonAppearance();
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
    }
}