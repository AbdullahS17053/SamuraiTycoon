using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ModuleButtonController : MonoBehaviour
{
    [Header("UI References")]
    public Button button;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI descriptionText;
    public TextMeshProUGUI costText;
    public TextMeshProUGUI levelText;
    public Image backgroundImage;
    public Image iconImage;

    private BuildingModule _currentModule;
    private Building _currentBuilding;
    private EconomyManager _currentEconomy;

    void Awake()
    {
        // Auto-assign with better fallbacks
        if (button == null) button = GetComponentInChildren<Button>();
        if (titleText == null) titleText = FindTextComponent("title", "name", "header");
        if (descriptionText == null) descriptionText = FindTextComponent("description", "desc", "info");
        if (costText == null) costText = FindTextComponent("cost", "price", "value");
        if (levelText == null) levelText = FindTextComponent("level", "lvl", "count");
        if (backgroundImage == null) backgroundImage = GetComponent<Image>() ?? GetComponentInChildren<Image>();
    }

    private TextMeshProUGUI FindTextComponent(params string[] possibleNames)
    {
        TextMeshProUGUI[] allTexts = GetComponentsInChildren<TextMeshProUGUI>(true);

        if (allTexts.Length == 0) return null;

        foreach (var text in allTexts)
        {
            string lowerName = text.name.ToLower();
            foreach (string name in possibleNames)
            {
                if (lowerName.Contains(name.ToLower()))
                    return text;
            }
        }

        // Return first text if no specific one found
        return allTexts[0];
    }

    public void Initialize(BuildingModule module, Building building, EconomyManager economy)
    {
        _currentModule = module;
        _currentBuilding = building;
        _currentEconomy = economy;

        UpdateUI();

        // Set up button click
        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            if (!module.IsMaxLevel())
            {
                button.onClick.AddListener(OnButtonClicked);
            }
        }
    }

    private void OnButtonClicked()
    {
        if (_currentModule != null && _currentBuilding != null)
        {
            _currentModule.OnButtonClick(_currentBuilding);
            UpdateUI(); // Immediate feedback
        }
    }

    public void UpdateUI()
    {
        if (_currentModule == null || _currentBuilding == null || _currentEconomy == null) return;

        // Update title
        if (titleText != null)
            titleText.text = _currentModule.buttonText;

        // Update description
        if (descriptionText != null)
            descriptionText.text = _currentModule.GetEffectDescription();

        // Update cost and level
        if (costText != null)
        {
            costText.text = _currentModule.IsMaxLevel() ?
                "MAXED OUT" :
                $"Cost: {FormatNumber(_currentModule.GetCurrentCost(_currentModule.GetCurrentLevel()))}";
        }

        if (levelText != null)
        {
            levelText.text = $"Level: {_currentModule.GetCurrentLevel()}/{_currentModule.maxLevel}";
        }

        // Update colors with smooth transitions
        if (backgroundImage != null)
        {
            Color targetColor = _currentModule.IsMaxLevel() ?
                Color.yellow :
                (_currentModule.buttonColor != Color.clear ? _currentModule.buttonColor : Color.white);

            backgroundImage.color = Color.Lerp(backgroundImage.color, targetColor, Time.deltaTime * 10f);
        }

        // Update icon
        if (iconImage != null && _currentModule.icon != null)
        {
            iconImage.sprite = _currentModule.icon;
            iconImage.color = _currentModule.IsMaxLevel() ? new Color(1, 1, 1, 0.7f) : Color.white;
        }

        // Update button interactability with smooth transition
        if (button != null)
        {
            bool shouldBeInteractable = !_currentModule.IsMaxLevel() &&
                                      _currentModule.CanActivate(_currentBuilding, _currentEconomy.Gold);

            button.interactable = shouldBeInteractable;

            // Visual feedback for affordable/not affordable
            if (!shouldBeInteractable && !_currentModule.IsMaxLevel())
            {
                button.image.color = Color.Lerp(button.image.color, Color.gray, Time.deltaTime * 8f);
            }
            else
            {
                button.image.color = Color.Lerp(button.image.color, Color.white, Time.deltaTime * 8f);
            }
        }
    }

    void Update()
    {
        // Smooth continuous updates for affordability states
        if (_currentModule != null && !_currentModule.IsMaxLevel())
        {
            UpdateUI();
        }
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