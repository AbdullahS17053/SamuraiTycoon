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
    public Image iconImage;

    public BuildingModule _currentModule;
    public TrainingBuilding _currentBuilding;

    private void Start()
    {
        Setup();
    }

    private void Setup()
    {
        button.onClick.AddListener(Upgrade);
        UpdateButton();


        iconImage.sprite = _currentModule.icon;
    }

    public void Upgrade()
    {
        _currentModule.OnButtonClick(_currentBuilding);
        UpdateButton();
    }

    public void UpdateButton()
    {
        titleText.text = _currentModule.moduleName;
        descriptionText.text = _currentModule.description;
        levelText.text = _currentModule.level.ToString();
        costText.text = _currentModule.GetCurrentCost().ToString();

    }
}