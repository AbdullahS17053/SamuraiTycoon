using TMPro;
using UnityEngine;

[System.Serializable]
public class UIManager
{
    [Header("Currency Display - DRAG TEXT ELEMENTS HERE!")]
    public TextMeshProUGUI goldText;
    public TextMeshProUGUI honorText;
    public TextMeshProUGUI samuraiText;
    public TextMeshProUGUI peasantsText;

    [Header("UI Panels")]
    public GameObject buildingPanel;

    private GameData _data;
    private EconomyManager _economy;
    private BuildingManager _buildings;

    public void Initialize(GameData data, EconomyManager economy, BuildingManager buildings)
    {
        _data = data;
        _economy = economy;
        _buildings = buildings;

        // Validate UI references
        if (goldText == null) Debug.LogError("❌ goldText is not assigned!");
        if (honorText == null) Debug.LogError("❌ honorText is not assigned!");
        if (samuraiText == null) Debug.LogError("❌ samuraiText is not assigned!");
        if (peasantsText == null) Debug.LogError("❌ peasantsText is not assigned!");
        if (buildingPanel == null) Debug.LogError("❌ buildingPanel is not assigned!");

        // Subscribe to economy events
        _economy.OnGoldChanged += UpdateCurrencyDisplay;
        _economy.OnHonorChanged += UpdateCurrencyDisplay;
        _economy.OnSamuraiChanged += UpdateCurrencyDisplay;
        _economy.OnPeasantsChanged += UpdateCurrencyDisplay;

        // Subscribe to building events
        _buildings.OnBuildingUpgraded += OnBuildingUpgraded;

        // Initial UI update
        UpdateCurrencyDisplay(0);

        Debug.Log("✅ UIManager initialized and events subscribed");
    }

    private void UpdateCurrencyDisplay(double change)
    {
        // This method is called automatically when currency changes
        if (goldText != null)
            goldText.text = FormatNumber(_data.Gold) + " Gold";

        if (honorText != null)
            honorText.text = FormatNumber(_data.Honor) + " Honor";

        if (samuraiText != null)
            samuraiText.text = _data.Samurai + " Samurai";

        if (peasantsText != null)
            peasantsText.text = _data.Peasants + " Peasants";

        Debug.Log($"🔄 UI Updated: Gold={_data.Gold}, Honor={_data.Honor}");
    }

    private void UpdateCurrencyDisplay(int change)
    {
        // Overload for integer changes (samurai/peasants)
        UpdateCurrencyDisplay((double)change);
    }

    public void OnBuildingUpgraded(string buildingId)
    {
        Debug.Log($"🏗️ UI notified of building upgrade: {buildingId}");
        // Currency display updates automatically via events
        // Add building-specific UI updates here if needed
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
}