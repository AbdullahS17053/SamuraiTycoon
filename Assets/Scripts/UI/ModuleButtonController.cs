using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ModuleButtonController : MonoBehaviour
{
    [Header("UI References - Auto-assigned if not set")]
    public Button button;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI descriptionText;
    public TextMeshProUGUI costText;
    public TextMeshProUGUI levelText;
    public Image backgroundImage;
    public Image iconImage;

    void Awake()
    {
        // Auto-assign references if not set
        if (button == null) button = GetComponentInChildren<Button>();
        if (titleText == null) titleText = FindTextComponent("title", "name");
        if (descriptionText == null) descriptionText = FindTextComponent("description", "desc");
        if (costText == null) costText = FindTextComponent("cost", "price");
        if (levelText == null) levelText = FindTextComponent("level", "lvl");
        if (backgroundImage == null) backgroundImage = GetComponentInChildren<Image>();
    }

    private TextMeshProUGUI FindTextComponent(params string[] possibleNames)
    {
        TextMeshProUGUI[] allTexts = GetComponentsInChildren<TextMeshProUGUI>(true);

        foreach (var text in allTexts)
        {
            foreach (string name in possibleNames)
            {
                if (text.name.ToLower().Contains(name))
                    return text;
            }
        }

        // Return first text if no specific one found
        return allTexts.Length > 0 ? allTexts[0] : null;
    }

    public void Initialize(BuildingModule module, Building building, EconomyManager economy)
    {
        // Update all UI elements based on module data
        UpdateUI(module, building, economy);
    }

    public void UpdateUI(BuildingModule module, Building building, EconomyManager economy)
    {
        // Update title
        if (titleText != null)
            titleText.text = module.buttonText;

        // Update description
        if (descriptionText != null)
            descriptionText.text = module.GetEffectDescription();

        // Update cost and level
        string status = module.GetStatusText(building);
        if (costText != null)
        {
            // Extract cost from status or use module data
            costText.text = module.IsMaxLevel() ? "MAXED OUT" : $"Cost: {module.GetCurrentCost(module.GetCurrentLevel())}";
        }

        if (levelText != null)
        {
            levelText.text = $"Level: {module.GetCurrentLevel()}/{module.maxLevel}";
        }

        // Update colors
        if (backgroundImage != null)
        {
            backgroundImage.color = module.IsMaxLevel() ? Color.yellow :
                                  (module.buttonColor != Color.clear ? module.buttonColor : Color.white);
        }

        // Update icon
        if (iconImage != null && module.icon != null)
        {
            iconImage.sprite = module.icon;
        }

        // Update button interactability
        if (button != null)
        {
            button.interactable = !module.IsMaxLevel() && module.CanActivate(building, economy.Gold);
        }
    }
}