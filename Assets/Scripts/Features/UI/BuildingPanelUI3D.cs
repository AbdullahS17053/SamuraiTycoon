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

    [Header("Stats Display")]
    public TextMeshProUGUI statsText;
    public Slider buildingLevelSlider;

    [Header("Visual Settings")]
    public Color maxLevelColor = Color.yellow;
    public Color affordableColor = Color.green;
    public Color expensiveColor = Color.red;
    public Color normalColor = Color.white;

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
    }

    void SetupEventListeners()
    {
        // Remove existing listeners to prevent duplicates
        if (upgradeButton != null)
        {
            upgradeButton.onClick.RemoveAllListeners();
            upgradeButton.onClick.AddListener(OnUpgradeClicked);
        }

        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(OnCloseClicked);
        }

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

        // Basic info
        if (buildingNameText != null)
            buildingNameText.text = _config.DisplayName;

        if (buildingLevelText != null)
            buildingLevelText.text = $"Level {_data.Level}";

        if (buildingDescriptionText != null)
            buildingDescriptionText.text = _config.Description;

        // Income and costs
        double income = CalculateTotalIncome();
        double upgradeCost = CalculateUpgradeCost();

        if (buildingIncomeText != null)
            buildingIncomeText.text = $"+{FormatNumber(income)}/s";

        if (upgradeCostText != null)
        {
            upgradeCostText.text = _data.Level >= 20 ?
                "MAX LEVEL" :
                $"Upgrade: {FormatNumber(upgradeCost)} Gold";

            // Color coding for affordability
            upgradeCostText.color = _economy != null && _economy.Gold >= upgradeCost ?
                affordableColor : expensiveColor;
        }

        // Icon
        if (buildingIconImage != null && _config.Icon != null)
            buildingIconImage.sprite = _config.Icon;

        // Upgrade button state
        if (upgradeButton != null)
        {
            upgradeButton.interactable = _data.Level < 20 &&
                                       _economy != null &&
                                       _economy.Gold >= upgradeCost;
        }

        // Level slider
        if (buildingLevelSlider != null)
        {
            buildingLevelSlider.maxValue = 20;
            buildingLevelSlider.value = _data.Level;
        }
    }

    void CreateModuleButtons()
    {
        ClearModuleButtons();

        if (moduleContainer == null || moduleButtonPrefab == null || _building?.Modules == null)
            return;

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
        if (buttonObj == null) return;

        var controller = buttonObj.GetComponent<ModuleButtonController>();
        if (controller != null)
        {
            controller.Initialize(module, _building, _economy);
            _moduleControllers.Add(controller);
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
        if (moduleSectionTitle != null && _building?.Modules != null)
        {
            int maxedModules = CountMaxedModules();
            int totalModules = _building.Modules.Count;
            moduleSectionTitle.text = $"Modules ({totalModules - maxedModules}/{totalModules} Available)";
        }
    }

    void UpdateStatsDisplay()
    {
        if (_building == null || _config == null || _data == null) return;

        var stats = new System.Text.StringBuilder();

        stats.AppendLine($"<size=18><b>{_config.DisplayName} Statistics</b></size>\n");
        stats.AppendLine($"<b>Level:</b> {_data.Level}/20");
        stats.AppendLine($"<b>Income:</b> {FormatNumber(CalculateTotalIncome())}/s");
        stats.AppendLine($"<b>Next Upgrade:</b> {FormatNumber(CalculateUpgradeCost())} Gold");
        stats.AppendLine($"<b>Status:</b> {(_data.IsUnlocked ? "🟢 Active" : "🔴 Locked")}");

        int maxedModules = CountMaxedModules();
        stats.AppendLine($"<b>Modules:</b> {_building.Modules.Count - maxedModules}/{_building.Modules.Count} Active");

        // Module details
        if (_building.Modules.Count > 0)
        {
            stats.AppendLine($"\n<size=16><b>Active Modules:</b></size>");
            foreach (var module in _building.Modules)
            {
                if (module != null)
                {
                    string maxedIndicator = module.IsMaxLevel() ? " 🏆" : "";
                    stats.AppendLine($"\n<b>{module.moduleName}{maxedIndicator}</b>");
                    stats.AppendLine(module.GetStatusText(_building));
                }
            }
        }

        if (statsText != null)
            statsText.text = stats.ToString();
    }

    // Event handlers
    void OnUpgradeClicked()
    {
        var buildingManager = FindObjectOfType<BuildingManager3D>();
        buildingManager?.UpgradeBuilding(_config.ID);

        // Smooth UI update
        StartCoroutine(SmoothUIUpdate());
    }

    void OnModuleClicked(string moduleName)
    {
        var buildingManager = FindObjectOfType<BuildingManager3D>();
        buildingManager?.ActivateModule(_config.ID, moduleName);

        StartCoroutine(SmoothUIUpdate());
    }

    IEnumerator SmoothUIUpdate()
    {
        yield return new WaitForEndOfFrame(); // Wait one frame for data to update
        UpdateAllUI();
        UpdateModuleButtons();
    }

    void OnCurrencyChanged(double change)
    {
        if (!_isInitialized) return;

        UpdateBuildingInfo();
        UpdateModuleButtons();
    }

    void OnCloseClicked()
    {
        FindObjectOfType<BuildingManager3D>()?.HideBuildingPanel();
    }

    // Helper methods
    double CalculateTotalIncome() => _config?.GetIncome(_data.Level) ?? 0;
    double CalculateUpgradeCost() => _config?.GetCost(_data.Level) ?? 0;

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
        if (_economy != null)
            _economy.OnGoldChanged -= OnCurrencyChanged;

        ClearModuleButtons();
    }
}