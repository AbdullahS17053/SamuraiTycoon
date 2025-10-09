using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BuildingPanelUI3D : MonoBehaviour
{
    public static BuildingPanelUI3D Instance;

    [Header("Building Info UI")]
    public TextMeshProUGUI buildingNameText;
    public TextMeshProUGUI buildingLevelText;
    public TextMeshProUGUI buildingDescriptionText;
    public Image buildingIconImage;
    public Image BuildingBanner;
    public Image ThemedBuilding;

    [Header("Module Container")]
    public Transform moduleContainer;
    public GameObject moduleButtonPrefab;
    public TextMeshProUGUI moduleSectionTitle;

    [Header("Stats Display")]
    public TextMeshProUGUI levelStatText;
    public TextMeshProUGUI maxLevelStatText;
    public TextMeshProUGUI incomeStatText;
    public TextMeshProUGUI capacityStatText;
    public TextMeshProUGUI efficiencyStatText;

    [Header("UI Elements")]
    public Slider buildingLevelSlider;

    private TrainingBuilding currentBuilding;
    private List<GameObject> _moduleButtons = new List<GameObject>();


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

    }

    public void OnBuildingUpgraded(TrainingBuilding building)
    {
        if(building == null)
        {
            building = currentBuilding;
        }
        else
        {
            currentBuilding = building;
            ClearModuleButtons();
            CreateModuleButtons();
        }



        buildingNameText.text = building.DisplayName;
        buildingDescriptionText.text = building.Description;

        buildingIconImage.sprite = building.Icon;
        BuildingBanner.sprite = building.Banner;

        ThemedBuilding.color = building.ThemeColor;

        buildingLevelSlider.minValue = 0;
        buildingLevelSlider.maxValue = building.maxLevel;
        buildingLevelSlider.value = building.level;
        levelStatText.text = building.level.ToString();
        maxLevelStatText.text = building.maxLevel.ToString();

        capacityStatText.text = $"{building.currentWorkers}/{building.maxWorkers}";

        efficiencyStatText.text = $"{building.baseTrainingTime}s";

        incomeStatText.text = $"{building.BaseIncomePerTrained} Gold";

    }

    private void CreateModuleButtons()
    {

        foreach (BuildingModule module in currentBuilding.modules)
        {
            if (module != null && module.showInUI)
            {
                GameObject buttonObj = Instantiate(moduleButtonPrefab, moduleContainer);
                buttonObj.GetComponent<ModuleButtonController>()._currentModule = module; 

                _moduleButtons.Add(buttonObj);
            }
        }
    }

    public void ClearModuleButtons()
    {
        foreach (var button in _moduleButtons)
        {
            if (button != null)
                Destroy(button);
        }
        _moduleButtons.Clear();
    }
}