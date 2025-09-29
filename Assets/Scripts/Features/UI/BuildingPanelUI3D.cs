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

    [Header("Stats Display")]
    public TextMeshProUGUI statsText;
    public Slider buildingLevelSlider;

    private Building _building;
    private BuildingConfig _config;
    private BuildingData _data;
    private EconomyManager _economy;
    private List<GameObject> _moduleButtons = new List<GameObject>();
    private bool _isInitialized = false;

    // Initialize method that was missing
    public void Initialize(Building building, BuildingConfig config, BuildingData data, EconomyManager economy)
    {
        _building = building;
        _config = config;
        _data = data;
        _economy = economy;

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

        _isInitialized = true;
        Debug.Log($"✅ BuildingPanelUI3D initialized for: {config.DisplayName}");
    }

    void UpdateBuildingInfo()
    {
        if (!_isInitialized || _config == null || _data == null) return;

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
            buildingLevelSlider.maxValue = 10; // Max level
            buildingLevelSlider.value = _data.Level;
        }
    }

    void CreateModuleButtons()
    {
        if (!_isInitialized || moduleContainer == null || moduleButtonPrefab == null) return;

        // Clear existing buttons
        foreach (var button in _moduleButtons)
            Destroy(button);
        _moduleButtons.Clear();

        if (_building == null || _building.Modules == null) return;

        foreach (var module in _building.Modules)
        {
            if (module != null && module.showInUI)
            {
                var buttonObj = Instantiate(moduleButtonPrefab, moduleContainer);
                var button = buttonObj.GetComponent<Button>();
                var text = buttonObj.GetComponentInChildren<TextMeshProUGUI>();

                if (text != null)
                {
                    text.text = $"{module.buttonText}\n{module.GetStatusText(_building)}";
                }

                // Add click handler
                string moduleName = module.moduleName;
                button.onClick.AddListener(() => OnModuleClicked(moduleName));

                _moduleButtons.Add(buttonObj);
                Debug.Log($"🔧 3D Module button created: {module.moduleName}");
            }
        }
    }

    void UpdateStatsDisplay()
    {
        if (!_isInitialized || _building == null) return;

        string stats = $"<size=18><b>{_config.DisplayName} Statistics</b></size>\n\n";
        stats += $"<b>Level:</b> {_data.Level}\n";
        stats += $"<b>Income:</b> {FormatNumber(CalculateTotalIncome())}/s\n";
        stats += $"<b>Next Upgrade:</b> {FormatNumber(CalculateUpgradeCost())} Gold\n";
        stats += $"<b>Status:</b> {(_data.IsUnlocked ? "🟢 Active" : "🔴 Locked")}\n";

        // Add module-specific stats
        if (_building.Modules != null && _building.Modules.Count > 0)
        {
            stats += $"\n<size=16><b>Active Modules:</b></size>\n";
            foreach (var module in _building.Modules)
            {
                if (module != null)
                {
                    stats += $"\n<b>{module.moduleName}:</b>\n";
                    stats += $"{module.GetStatusText(_building)}\n";
                }
            }
        }

        if (statsText != null)
            statsText.text = stats;
    }

    void OnUpgradeClicked()
    {
        if (!_isInitialized) return;

        // Use the BuildingManager3D to upgrade
        var buildingManager = FindObjectOfType<BuildingManager3D>();
        if (buildingManager != null)
        {
            buildingManager.UpgradeBuilding(_config.ID);
            UpdateBuildingInfo();
            UpdateStatsDisplay();
            UpdateModuleButtonsText();
        }
        else
        {
            Debug.LogError("❌ BuildingManager3D not found!");
        }
    }

    void OnModuleClicked(string moduleName)
    {
        if (!_isInitialized) return;

        // Use the BuildingManager3D to activate module
        var buildingManager = FindObjectOfType<BuildingManager3D>();
        if (buildingManager != null)
        {
            buildingManager.ActivateModule(_config.ID, moduleName);
            UpdateStatsDisplay();
            UpdateModuleButtonsText();
        }
        else
        {
            Debug.LogError("❌ BuildingManager3D not found!");
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

    void OnCurrencyChanged(double change)
    {
        UpdateBuildingInfo();
    }

    double CalculateTotalIncome()
    {
        if (_building == null || _building.Modules == null) return 0;

        double totalIncome = 0;

        // Calculate base income from config
        if (_config != null && _data != null && _data.IsUnlocked)
        {
            totalIncome += _config.GetIncome(_data.Level);
        }

        // Add income from modules
        foreach (var module in _building.Modules)
        {
            if (module is IncomeModule incomeModule)
            {
                // Income modules handle their own income in their Tick method
                // This is just for display
            }
        }

        return totalIncome;
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