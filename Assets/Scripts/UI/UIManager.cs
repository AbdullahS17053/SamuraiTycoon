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

    private GameData _data;
    private EconomyManager _economy;
    private BuildingManager3D _buildings;

    public void Initialize(GameData data, EconomyManager economy, BuildingManager3D buildings)
    {
        _data = data;
        _economy = economy;
        _buildings = buildings;

        // Validate UI references
        if (goldText == null) Debug.LogError("❌ goldText is not assigned!");
        if (honorText == null) Debug.LogError("❌ honorText is not assigned!");
        if (samuraiText == null) Debug.LogError("❌ samuraiText is not assigned!");
        if (peasantsText == null) Debug.LogError("❌ peasantsText is not assigned!");

        // Subscribe to economy events
        _economy.OnGoldChanged += UpdateCurrencyDisplay;
        _economy.OnHonorChanged += UpdateCurrencyDisplay;

        // Initial UI update
        UpdateCurrencyDisplay(0);

        Debug.Log("✅ UIManager initialized successfully with BuildingManager3D");
    }

    private void UpdateCurrencyDisplay(double change)
    {
        if (goldText != null)
            goldText.text = FormatNumber(_data.Gold);

        if (honorText != null)
            honorText.text = FormatNumber(_data.Honor);

        if (samuraiText != null)
            samuraiText.text = _data.Samurai + " Samurai";

        if (peasantsText != null)
            peasantsText.text = _data.Peasants + " Peasants";

        //* Debug.Log($"🔄 UI Updated: Gold={_data.Gold}, Honor={_data.Honor}");
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