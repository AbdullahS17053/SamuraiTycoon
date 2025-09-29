using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class BuildingPanelUI : MonoBehaviour
{
    [Header("Building Info UI")]
    public TextMeshProUGUI buildingNameText;
    public TextMeshProUGUI buildingLevelText;
    public TextMeshProUGUI buildingDescriptionText;
    public TextMeshProUGUI buildingIncomeText;
    public TextMeshProUGUI upgradeCostText;
    public Image buildingIconImage;
    public Button upgradeButton;

    [Header("Module Container")]
    public Transform moduleContainer;
    public GameObject moduleButtonPrefab;

    [Header("Stats Display")]
    public TextMeshProUGUI statsText;
    public Transform statsContainer;

    private Building _building;
    private BuildingConfig _config;
    private BuildingData _data;
    private EconomyManager _economy;
    private List<GameObject> _moduleButtons = new List<GameObject>();

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
        _economy.OnGoldChanged += OnCurrencyChanged;

        Debug.Log($"✅ BuildingPanelUI initialized for: {config.DisplayName}");
    }

    void UpdateBuildingInfo()
    {
        if (_config == null || _data == null) return;

        buildingNameText.text = _config.DisplayName;
        buildingLevelText.text = $"Level {_data.Level}";
        buildingDescriptionText.text = _config.Description;

        double income = BuildingManager.Instance.GetIncome(_config.ID);
        buildingIncomeText.text = $"+{FormatNumber(income)}/s";

        double upgradeCost = BuildingManager.Instance.GetUpgradeCost(_config.ID);
        upgradeCostText.text = $"Upgrade: {FormatNumber(upgradeCost)} Gold";

        if (buildingIconImage != null && _config.Icon != null)
            buildingIconImage.sprite = _config.Icon;

        upgradeButton.interactable = _economy.Gold >= upgradeCost;
    }

    void CreateModuleButtons()
    {
        // Clear existing buttons
        foreach (var button in _moduleButtons)
            Destroy(button);
        _moduleButtons.Clear();

        var modules = BuildingManager.Instance.GetBuildingModules(_config.ID);
        foreach (var module in modules)
        {
            if (module.showInUI)
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
                Debug.Log($"🔧 Module button created: {module.moduleName}");
            }
        }
    }

    void UpdateStatsDisplay()
    {
        if (_building == null) return;

        string stats = $"<b>Building Statistics:</b>\n";
        stats += $"Level: {_data.Level}\n";
        stats += $"Income: {FormatNumber(BuildingManager.Instance.GetIncome(_config.ID))}/s\n";
        stats += $"Upgrade Cost: {FormatNumber(BuildingManager.Instance.GetUpgradeCost(_config.ID))}\n";
        stats += $"Unlocked: {_data.IsUnlocked}\n";

        // Add module-specific stats
        foreach (var module in _building.Modules)
        {
            stats += $"\n<b>{module.moduleName}:</b>\n";
            stats += $"{module.GetStatusText(_building)}\n";
        }

        if (statsText != null)
            statsText.text = stats;
    }

    void OnUpgradeClicked()
    {
        BuildingManager.Instance.UpgradeBuilding(_config.ID);
        UpdateBuildingInfo();
        UpdateStatsDisplay();
    }

    void OnModuleClicked(string moduleName)
    {
        BuildingManager.Instance.ActivateModule(_config.ID, moduleName);
        UpdateStatsDisplay();

        // Update module buttons
        var modules = BuildingManager.Instance.GetBuildingModules(_config.ID);
        for (int i = 0; i < modules.Count && i < _moduleButtons.Count; i++)
        {
            var module = modules[i];
            var buttonObj = _moduleButtons[i];
            var text = buttonObj.GetComponentInChildren<TextMeshProUGUI>();

            if (text != null)
            {
                text.text = $"{module.buttonText}\n{module.GetStatusText(_building)}";
            }
        }
    }

    void OnCurrencyChanged(double change)
    {
        UpdateBuildingInfo();
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