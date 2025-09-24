using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class UIManager
{
    [Header("Currency Display - DRAG UI TEXT ELEMENTS HERE!")]
    public Text goldText;
    public Text honorText;
    public Text samuraiText;
    public Text peasantsText;

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

        // Subscribe to economy events
        _economy.OnGoldChanged += UpdateCurrencyDisplay;
        _economy.OnHonorChanged += UpdateCurrencyDisplay;

        // Initial UI update
        UpdateCurrencyDisplay(0);

        Debug.Log("UIManager initialized successfully");
    }

    private void UpdateCurrencyDisplay(double change)
    {
        // This method is called automatically when currency changes
        // No need to manually call it anywhere!

        if (goldText != null)
            goldText.text = FormatNumber(_data.Gold) + " Gold";

        if (honorText != null)
            honorText.text = FormatNumber(_data.Honor) + " Honor";

        if (samuraiText != null)
            samuraiText.text = _data.Samurai + " Samurai";

        if (peasantsText != null)
            peasantsText.text = _data.Peasants + " Peasants";
    }

    public void OnBuildingUpgraded(string buildingId)
    {
        // This is called by BuildingManager when a building is upgraded
        // We don't need to manually update currency here because events handle it
        Debug.Log($"UI notified of building upgrade: {buildingId}");

        // If you have building-specific UI, update it here
        // For now, currency updates are handled automatically by events
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