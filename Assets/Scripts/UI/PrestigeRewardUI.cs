using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PrestigeRewardUI : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI rewardNameText;
    public TextMeshProUGUI rewardDescriptionText;
    public Image rewardIcon;
    public Button selectButton;
    public GameObject selectedIndicator;

    private PrestigeManager.PrestigeBonus _bonus;
    private PrestigeManager _prestigeManager;
    private bool _isSelected = false;

    public void Initialize(PrestigeManager.PrestigeBonus bonus, PrestigeManager manager)
    {
        _bonus = bonus;
        _prestigeManager = manager;

        UpdateUI();

        if (selectButton != null)
        {
            selectButton.onClick.RemoveAllListeners();
            selectButton.onClick.AddListener(ToggleSelection);
        }
    }

    void UpdateUI()
    {
        if (rewardNameText != null)
            rewardNameText.text = _bonus.displayName;

        if (rewardDescriptionText != null)
            rewardDescriptionText.text = GetBonusDescription();

        if (rewardIcon != null && _bonus.icon != null)
            rewardIcon.sprite = _bonus.icon;

        if (selectedIndicator != null)
            selectedIndicator.SetActive(_isSelected);

        if (selectButton != null)
        {
            selectButton.GetComponentInChildren<TextMeshProUGUI>().text =
                _isSelected ? "SELECTED" : "SELECT";
        }
    }

    string GetBonusDescription()
    {
        string valueText = _bonus.value > 0 ? $"+{_bonus.value}" : _bonus.value.ToString();

        switch (_bonus.type)
        {
            case PrestigeManager.PrestigeBonus.BonusType.GlobalIncomeMultiplier:
                return $"{valueText}% Global Income";
            case PrestigeManager.PrestigeBonus.BonusType.TroopTrainingSpeed:
                return $"{valueText}% Training Speed";
            case PrestigeManager.PrestigeBonus.BonusType.BuildingCostReduction:
                return $"{valueText}% Building Cost Reduction";
            case PrestigeManager.PrestigeBonus.BonusType.OfflineEarnings:
                return $"{valueText}% Offline Earnings";
            case PrestigeManager.PrestigeBonus.BonusType.TroopCapacity:
                return $"{valueText} Max Troops";
            case PrestigeManager.PrestigeBonus.BonusType.AutoTrainSpeed:
                return $"{valueText}% Auto Train Speed";
            default:
                return _bonus.description;
        }
    }

    void ToggleSelection()
    {
        _isSelected = !_isSelected;

        if (_isSelected)
        {
            _prestigeManager.SelectBonus(_bonus);
        }
        else
        {
            _prestigeManager.DeselectBonus(_bonus);
        }

        UpdateUI();
    }
}