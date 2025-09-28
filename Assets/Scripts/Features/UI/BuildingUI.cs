using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BuildingUI : MonoBehaviour
{
    [Header("UI References - DRAG UI ELEMENTS HERE!")]
    public TextMeshProUGUI NameText;
    public TextMeshProUGUI LevelText;
    public TextMeshProUGUI CostText;
    public TextMeshProUGUI IncomeText;
    public Image IconImage;
    public Button UpgradeButton;

    [Header("Building ID - SET THIS IN INSPECTOR!")]
    public string BuildingID;

    private BuildingManager _buildings;
    private EconomyManager _economy;
    private bool _isInitialized = false;

    void Start()
    {
        Debug.Log($"🏗️ BuildingUI Start called for: {BuildingID}");

        // Wait for GameManager to be ready
        if (GameManager.Instance == null)
        {
            Debug.LogError($"❌ GameManager not ready for {BuildingID}");
            return;
        }

        Initialize();
    }

    public void Initialize()
    {
        if (_isInitialized)
        {
            Debug.LogWarning($"⚠️ BuildingUI already initialized for {BuildingID}");
            return;
        }

        if (GameManager.Instance == null)
        {
            Debug.LogError($"❌ GameManager instance is null for {BuildingID}");
            return;
        }

        _buildings = GameManager.Instance.Buildings;
        _economy = GameManager.Instance.Economy;

        if (_buildings == null)
        {
            Debug.LogError($"❌ BuildingManager is null for {BuildingID}");
            return;
        }

        if (_economy == null)
        {
            Debug.LogError($"❌ EconomyManager is null for {BuildingID}");
            return;
        }

        // Validate UI elements
        if (NameText == null) Debug.LogError($"❌ NameText is null for {BuildingID}");
        if (LevelText == null) Debug.LogError($"❌ LevelText is null for {BuildingID}");
        if (CostText == null) Debug.LogError($"❌ CostText is null for {BuildingID}");
        if (IncomeText == null) Debug.LogError($"❌ IncomeText is null for {BuildingID}");
        if (IconImage == null) Debug.LogError($"❌ IconImage is null for {BuildingID}");
        if (UpgradeButton == null) Debug.LogError($"❌ UpgradeButton is null for {BuildingID}");

        // Subscribe to events
        _economy.OnGoldChanged += RefreshUI;
        _buildings.OnBuildingUpgraded += OnBuildingUpgraded;

        // Set up button click
        UpgradeButton.onClick.RemoveAllListeners(); // Clear any existing listeners
        UpgradeButton.onClick.AddListener(OnUpgradeClicked);


        _isInitialized = true;
        // Initial UI update
        RefreshUI(0);
        Debug.Log($"✅ BuildingUI initialized successfully for: {BuildingID}");
    }

    void RefreshUI(double goldChange)
    {
        if (!_isInitialized) return;

        var config = _buildings.GetConfig(BuildingID);
        var data = _buildings.GetData(BuildingID);

        if (config == null)
        {
            Debug.LogError($"❌ Config not found for: {BuildingID}");
            return;
        }

        if (data == null)
        {
            Debug.LogError($"❌ Data not found for: {BuildingID}");
            return;
        }

        try
        {
            NameText.text = config.DisplayName;
            LevelText.text = $"Level {data.Level}";

            double cost = _buildings.GetUpgradeCost(BuildingID);
            CostText.text = $"Cost: {FormatNumber(cost)} Gold";

            double income = _buildings.GetIncome(BuildingID);
            IncomeText.text = $"+{FormatNumber(income)}/s";

            if (IconImage != null && config.Icon != null)
                IconImage.sprite = config.Icon;

            // Update button interactability
            bool canAfford = _economy.Gold >= cost;
            UpgradeButton.interactable = canAfford;

            Debug.Log($"🔄 {BuildingID} UI updated - Level: {data.Level}, Cost: {cost}, CanAfford: {canAfford}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ Error updating UI for {BuildingID}: {e.Message}");
        }
    }

    void OnUpgradeClicked()
    {
        if (!_isInitialized) return;

        Debug.Log($"🖱️ Upgrade clicked for: {BuildingID}");
        _buildings.UpgradeBuilding(BuildingID);
    }

    void OnBuildingUpgraded(string buildingId)
    {
        if (buildingId == BuildingID)
        {
            Debug.Log($"📢 Building upgraded event received for: {BuildingID}");
            RefreshUI(0);
        }
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
        // Clean up event subscriptions
        if (_economy != null)
            _economy.OnGoldChanged -= RefreshUI;

        if (_buildings != null)
            _buildings.OnBuildingUpgraded -= OnBuildingUpgraded;
    }
}