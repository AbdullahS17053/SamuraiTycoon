using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ModuleStatUI : MonoBehaviour
{
    [Header("Module Stat UI Elements")]
    public TextMeshProUGUI moduleNameText;
    public TextMeshProUGUI moduleLevelText;
    public TextMeshProUGUI moduleValueText;
    public TextMeshProUGUI moduleStatusText;
    public Image moduleIcon;
    public Slider moduleProgressSlider;

    [Header("Visual Settings")]
    public Color activeColor = Color.green;
    public Color maxedColor = Color.yellow;
    public Color inactiveColor = Color.gray;

    private BuildingModule _module;
    private Building _building;

    public void Initialize(BuildingModule module, Building building)
    {
        _module = module;
        _building = building;
        UpdateDisplay();
    }

    public void UpdateDisplay()
    {
        if (_module == null) return;

        // Module name
        if (moduleNameText != null)
        {
            moduleNameText.text = _module.moduleName;
            moduleNameText.color = _module.IsMaxLevel() ? maxedColor :
                                 _module.isActive ? activeColor : inactiveColor;
        }

        // Module level
        if (moduleLevelText != null)
        {
            moduleLevelText.text = $"Lvl {_module.GetCurrentLevel()}/{_module.maxLevel}";
        }

        // Module-specific value display
        if (moduleValueText != null)
        {
            moduleValueText.text = GetModuleSpecificValue();
        }

        // Module status
        if (moduleStatusText != null)
        {
            moduleStatusText.text = _module.IsMaxLevel() ? "MAXED" :
                                  _module.isActive ? "ACTIVE" : "INACTIVE";
            moduleStatusText.color = _module.IsMaxLevel() ? maxedColor :
                                   _module.isActive ? activeColor : inactiveColor;
        }

        // Module icon
        if (moduleIcon != null && _module.icon != null)
        {
            moduleIcon.sprite = _module.icon;
            moduleIcon.color = _module.IsMaxLevel() ? maxedColor :
                              _module.isActive ? activeColor : Color.white;
        }

        // Progress slider
        if (moduleProgressSlider != null)
        {
            moduleProgressSlider.maxValue = _module.maxLevel;
            moduleProgressSlider.value = _module.GetCurrentLevel();
        }
    }

    private string GetModuleSpecificValue()
    {
        if (_module is CapacityModule capacityModule)
        {
            return $"{capacityModule.currentWorkers}/{capacityModule.GetMaxCapacity(_building)} Workers";
        }
        else if (_module is SpeedModule speedModule)
        {
            return $"{speedModule.GetCurrentSpeedMultiplier():F1}x Speed";
        }
        else if (_module is IncomeModule incomeModule)
        {
            return $"+{incomeModule.baseIncomeBonus * Mathf.Pow(incomeModule.incomeMultiplier, _module.GetCurrentLevel() - 1):F1}/s";
        }

        return _module.GetEffectDescription();
    }
}