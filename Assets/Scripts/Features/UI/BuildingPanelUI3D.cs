using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
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

    [Header("Stats Display - Optional Fields")]
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

    private Building _building;
    private BuildingConfig _config;
    private BuildingData _data;
    private EconomyManager _economy;
    private List<GameObject> _moduleButtons = new List<GameObject>();
    private List<ModuleButtonController> _moduleControllers = new List<ModuleButtonController>();
    private bool _isInitialized = false;

    public void Initialize(Building building, BuildingConfig config, BuildingData data, EconomyManager economy)
    {
        _building = building;
        _config = config;
        _data = data;
        _economy = economy;

        SetupEventListeners();
        UpdateAllUI();
        CreateModuleButtons();

        _isInitialized = true;

        Debug.Log($"✅ BuildingPanelUI3D initialized for {config.DisplayName}");
    }

    void SetupEventListeners()
    {
        // Safely setup upgrade button
        if (upgradeButton != null)
        {
            upgradeButton.onClick.RemoveAllListeners();
            upgradeButton.onClick.AddListener(OnUpgradeClicked);
        }
        else
        {
            Debug.LogWarning("⚠️ UpgradeButton not assigned - upgrade functionality disabled");
        }

        // Safely setup close button
        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(OnCloseClicked);
        }
        else
        {
            Debug.LogWarning("⚠️ CloseButton not assigned - manual close disabled");
        }

        // Setup economy events
        if (_economy != null)
        {
            _economy.OnGoldChanged -= OnCurrencyChanged;
            _economy.OnGoldChanged += OnCurrencyChanged;
        }
    }

    void UpdateAllUI()
    {
        UpdateBuildingInfo();
        UpdateStatsDisplay();
        UpdateModuleSectionTitle();
    }

    void UpdateBuildingInfo()
    {
        if (_config == null || _data == null) return;

        // Basic info - safely update each field
        SafeSetText(buildingNameText, _config.DisplayName);
        SafeSetText(buildingLevelText, $"Level {_data.Level}");
        SafeSetText(buildingDescriptionText, _config.Description);

        // Income and costs
        double income = CalculateTotalIncome();
        double upgradeCost = CalculateUpgradeCost();

        SafeSetText(buildingIncomeText, string.Format(rateFormat, FormatNumber(income)));

        if (upgradeCostText != null)
        {
            upgradeCostText.text = _data.Level >= 20 ?
                "MAX LEVEL" :
                string.Format(currencyFormat, FormatNumber(upgradeCost));

            upgradeCostText.color = _economy != null && _economy.Gold >= upgradeCost ?
                affordableColor : expensiveColor;
        }

        // Icon - safely update if assigned
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

        // Upgrade button state - only if button exists
        if (upgradeButton != null)
        {
            upgradeButton.interactable = _data.Level < 20 &&
                                       _economy != null &&
                                       _economy.Gold >= upgradeCost;
        }

        // Level slider - only if assigned
        if (buildingLevelSlider != null)
        {
            buildingLevelSlider.maxValue = 20;
            buildingLevelSlider.value = _data.Level;
        }
    }

    void UpdateStatsDisplay()
    {
        if (_building == null || _config == null || _data == null) return;

        double income = CalculateTotalIncome();
        double upgradeCost = CalculateUpgradeCost();

        // Only update stats if their Text components are assigned
        SafeSetText(levelStatText, string.Format(capacityFormat, _data.Level, 20));

        SafeSetText(incomeStatText, string.Format(rateFormat, FormatNumber(income)));

        if (upgradeCostStatText != null)
        {
            upgradeCostStatText.text = _data.Level >= 20 ?
                "MAXED" :
                string.Format(currencyFormat, FormatNumber(upgradeCost));

            upgradeCostStatText.color = _data.Level >= 20 ?
                maxLevelColor :
                (_economy != null && _economy.Gold >= upgradeCost ? affordableColor : expensiveColor);
        }

        if (statusStatText != null)
        {
            statusStatText.text = _data.IsUnlocked ? "ACTIVE" : "LOCKED";
            statusStatText.color = _data.IsUnlocked ? affordableColor : expensiveColor;
        }

        if (modulesStatText != null)
        {
            int maxedModules = CountMaxedModules();
            int totalModules = _building.Modules.Count;
            modulesStatText.text = string.Format(capacityFormat, totalModules - maxedModules, totalModules);
        }

        // Capacity stat - only calculate if text field exists
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

        // Efficiency stat - only calculate if text field exists
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

        // Production stat - only calculate if text field exists
        if (productionStatText != null)
        {
            productionStatText.text = string.Format(valueFormat,
                CalculateProductionMultiplier().ToString("F1") + "x");
        }
    }

    void CreateModuleButtons()
    {
        ClearModuleButtons();

        // Check if we have the necessary components
        if (moduleContainer == null)
        {
            Debug.LogWarning("⚠️ ModuleContainer not assigned - module buttons disabled");
            return;
        }

        if (moduleButtonPrefab == null)
        {
            Debug.LogWarning("⚠️ ModuleButtonPrefab not assigned - module buttons disabled");
            return;
        }

        if (_building?.Modules == null)
        {
            Debug.LogWarning("⚠️ No modules found for building");
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

    void CreateModuleButton(BuildingModule module)
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

    void ClearModuleButtons()
    {
        foreach (var button in _moduleButtons)
        {
            if (button != null)
                Destroy(button);
        }
        _moduleButtons.Clear();
        _moduleControllers.Clear();
    }

    void UpdateModuleButtons()
    {
        foreach (var controller in _moduleControllers)
        {
            if (controller != null)
                controller.UpdateUI();
        }
    }

    void UpdateModuleSectionTitle()
    {
        // Only update if the title field exists
        if (moduleSectionTitle != null && _building?.Modules != null)
        {
            int maxedModules = CountMaxedModules();
            int totalModules = _building.Modules.Count;
            moduleSectionTitle.text = $"Modules ({totalModules - maxedModules}/{totalModules} Available)";
        }
    }

    // Event handlers
    void OnUpgradeClicked()
    {
        var buildingManager = FindObjectOfType<BuildingManager3D>();
        if (buildingManager != null)
        {
            buildingManager.UpgradeBuilding(_config.ID);
            StartCoroutine(SmoothUIUpdate());
        }
    }

    void OnModuleClicked(string moduleName)
    {
        var buildingManager = FindObjectOfType<BuildingManager3D>();
        if (buildingManager != null)
        {
            buildingManager.ActivateModule(_config.ID, moduleName);
            StartCoroutine(SmoothUIUpdate());
        }
    }

    IEnumerator SmoothUIUpdate()
    {
        yield return new WaitForEndOfFrame();
        UpdateAllUI();
        UpdateModuleButtons();
    }

    void OnCurrencyChanged(double change)
    {
        if (!_isInitialized) return;

        UpdateBuildingInfo();
        UpdateModuleButtons();
        UpdateStatsDisplay();
    }

    void OnCloseClicked()
    {
        var buildingManager = FindObjectOfType<BuildingManager3D>();
        if (buildingManager != null)
        {
            buildingManager.HideBuildingPanel();
        }
    }

    // Helper methods
    void SafeSetText(TextMeshProUGUI textField, string value)
    {
        if (textField != null)
        {
            textField.text = value;
        }
    }

    double CalculateTotalIncome() => _config?.GetIncome(_data.Level) ?? 0;
    double CalculateUpgradeCost() => _config?.GetCost(_data.Level) ?? 0;

    double CalculateProductionMultiplier()
    {
        double multiplier = 1.0;

        var speedModule = _building.GetModule<SpeedModule>();
        if (speedModule != null)
        {
            multiplier *= speedModule.GetCurrentSpeedMultiplier();
        }

        var capacityModule = _building.GetModule<CapacityModule>();
        if (capacityModule != null && capacityModule.GetMaxCapacity(_building) > 0)
        {
            multiplier *= (double)capacityModule.currentWorkers / capacityModule.GetMaxCapacity(_building);
        }

        return multiplier;
    }

    int CountMaxedModules()
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
        // Clean up event listeners
        if (_economy != null)
            _economy.OnGoldChanged -= OnCurrencyChanged;

        // Clean up UI elements
        ClearModuleButtons();
    }

    // Debug method to check which fields are assigned
    [ContextMenu("Check Assigned Fields")]
    void CheckAssignedFields()
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
}