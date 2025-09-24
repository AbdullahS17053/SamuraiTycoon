using UnityEngine;
using UnityEngine.UI;

public class BuildingUI : MonoBehaviour
{
    [Header("UI References - DRAG UI ELEMENTS HERE!")]
    public Text NameText;
    public Text LevelText;
    public Text CostText;
    public Text IncomeText;
    public Image IconImage;
    public Button UpgradeButton;

    [Header("Building ID - SET THIS!")]
    public string BuildingID;

    private BuildingManager _buildings;
    private EconomyManager _economy;

    public void Initialize(BuildingManager buildings, EconomyManager economy)
    {
        _buildings = buildings;
        _economy = economy;

        // Subscribe to events
        _economy.OnGoldChanged += RefreshUI;
        _buildings.OnBuildingUpgraded += OnBuildingUpgraded;

        RefreshUI(0);
        UpgradeButton.onClick.AddListener(OnUpgradeClicked);
    }

    void RefreshUI(double goldChange)
    {
        var config = _buildings.GetConfig(BuildingID);
        var data = _buildings.GetData(BuildingID);

        if (config == null || data == null) return;

        NameText.text = config.DisplayName;
        LevelText.text = $"Level {data.Level}";

        double cost = _buildings.GetUpgradeCost(BuildingID);
        CostText.text = $"Cost: {FormatNumber(cost)} Gold";

        double income = _buildings.GetIncome(BuildingID);
        IncomeText.text = $"+{FormatNumber(income)}/s";

        IconImage.sprite = config.Icon;
        UpgradeButton.interactable = _economy.Gold >= cost;
    }

    void OnUpgradeClicked() => _buildings.UpgradeBuilding(BuildingID);
    void OnBuildingUpgraded(string buildingId) => RefreshUI(0);

    string FormatNumber(double num) => NumberFormatter.Format(num);
}