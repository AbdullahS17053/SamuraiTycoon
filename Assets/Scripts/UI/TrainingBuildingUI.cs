using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TrainingBuildingUI : MonoBehaviour
{
    [Header("Training Building UI")]
    public TextMeshProUGUI buildingNameText;
    public TextMeshProUGUI trainingSlotsText;
    public TextMeshProUGUI queueCountText;
    public Slider[] trainingProgressSliders;
    public TextMeshProUGUI[] troopNameTexts;
    public Image[] troopIcons;

    private TrainingBuilding _trainingBuilding;

    public void Initialize(TrainingBuilding building)
    {
        _trainingBuilding = building;
        UpdateUI();
    }

    void Update()
    {
        if (_trainingBuilding != null)
        {
            UpdateUI();
        }
    }

    void UpdateUI()
    {
        if (buildingNameText != null)
            buildingNameText.text = _trainingBuilding.buildingId;

        if (trainingSlotsText != null)
            trainingSlotsText.text = $"Training: {_trainingBuilding.GetTrainingCount()}/{_trainingBuilding.trainingSlots}";

        if (queueCountText != null)
            queueCountText.text = $"Queue: {_trainingBuilding.GetQueueCount()}";

        // Update individual training slots
        for (int i = 0; i < trainingProgressSliders.Length; i++)
        {
            if (i < _trainingBuilding.trainingSlots)
            {
                trainingProgressSliders[i].gameObject.SetActive(true);
                trainingProgressSliders[i].value = _trainingBuilding.GetTrainingProgress(i);

                if (troopNameTexts[i] != null)
                {
                    // You would get the actual troop name from the building
                    troopNameTexts[i].text = $"Training... {(_trainingBuilding.GetTrainingProgress(i) * 100):F0}%";
                }
            }
            else
            {
                trainingProgressSliders[i].gameObject.SetActive(false);
                if (troopNameTexts[i] != null)
                    troopNameTexts[i].text = "Empty";
            }
        }
    }
}