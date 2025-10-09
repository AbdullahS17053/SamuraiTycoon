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
    private TrainingBuilding _currentBuilding;
    private EconomyManager _currentEconomy;

    private void Awake()
    {
        AutoAssignReferences();
    }

    private void AutoAssignReferences()
    {
        // Auto-assign with better fallbacks
        if (button == null) button = GetComponentInChildren<Button>();
        if (titleText == null) titleText = FindTextComponent("title", "name", "header");
        if (descriptionText == null) descriptionText = FindTextComponent("description", "desc", "info");
        if (costText == null) costText = FindTextComponent("cost", "price", "value");
        if (levelText == null) levelText = FindTextComponent("level", "lvl", "count");
        if (backgroundImage == null) backgroundImage = GetComponent<Image>() ?? GetComponentInChildren<Image>();
        if (iconImage == null) iconImage = transform.Find("Icon")?.GetComponent<Image>() ?? GetComponentInChildren<Image>();
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

    private void OnButtonClicked()
    {
        if (_currentModule != null && _currentBuilding != null && BuildingManager3D.Instance != null)
        {
            _currentModule.OnButtonClick(_currentBuilding);

            // Update UI immediately for better feedback
            StartCoroutine(DelayedUIUpdate());
        }
    }

    private System.Collections.IEnumerator DelayedUIUpdate()
    {
        yield return new WaitForEndOfFrame();
        UpdateUI();
    }

    public void UpdateUI()
    {
        if (_currentModule == null || _currentBuilding == null || _currentEconomy == null) return;

        // Update title
        SafeSetText(titleText, _currentModule.buttonText);

        // Update description
        SafeSetText(descriptionText, _currentModule.GetEffectDescription());

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

        // Update colors
        if (backgroundImage != null)
        {
            Color targetColor = _currentModule.IsMaxLevel() ?
                Color.yellow :
                (_currentModule.buttonColor != Color.clear ? _currentModule.buttonColor : Color.white);

            backgroundImage.color = targetColor; // Immediate color change
        }

        // Update icon
        if (iconImage != null && _currentModule.icon != null)
        {
            iconImage.sprite = _currentModule.icon;
            iconImage.color = _currentModule.IsMaxLevel() ? new Color(1, 1, 1, 0.7f) : Color.white;
        }

    }

    private void SafeSetText(TextMeshProUGUI textField, string value)
    {
        if (textField != null)
        {
            textField.text = value ?? string.Empty;
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

    private void OnEnable()
    {
        // Refresh UI when enabled
        if (_currentModule != null)
        {
            UpdateUI();
        }
    }
}