using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TrainingBuildingUI : MonoBehaviour
{
    [Header("UI References")]
    public GameObject buildingPanel;
    public TextMeshProUGUI buildingNameText;
    public TextMeshProUGUI trainingStatusText;
    public TextMeshProUGUI queueStatusText;
    public TextMeshProUGUI trainingProgressText;
    public Slider trainingProgressSlider;
    public Button closeButton;

    [Header("Troop Info")]
    public TextMeshProUGUI currentTroopText;
    public TextMeshProUGUI troopTypeText;
    public TextMeshProUGUI troopPowerText;

    private TrainingBuilding currentBuilding;
    private Camera mainCamera;
    private Canvas canvas;

    void Start()
    {
        mainCamera = Camera.main;
        canvas = GetComponent<Canvas>();

        if (canvas != null)
        {
            canvas.worldCamera = mainCamera;
        }

        SetupUI();
        HidePanel();
    }

    void SetupUI()
    {
        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(HidePanel);
        }
    }

    void Update()
    {
        if (buildingPanel.activeInHierarchy && currentBuilding != null)
        {
            UpdateUI();
        }
    }

    public void ShowPanel(TrainingBuilding building)
    {
        currentBuilding = building;

        if (buildingPanel != null)
        {
            buildingPanel.SetActive(true);
            UpdateUI();
        }
    }

    public void HidePanel()
    {
        if (buildingPanel != null)
        {
            buildingPanel.SetActive(false);
        }
        currentBuilding = null;
    }

    void UpdateUI()
    {
        if (currentBuilding == null) return;

        // Update basic building info
        if (buildingNameText != null)
            buildingNameText.text = currentBuilding.displayName;

        // Update training status
        if (trainingStatusText != null)
        {
            string status = currentBuilding.isTraining ? "Training In Progress" : "Available";
            trainingStatusText.text = $"Status: {status}";
        }

        // Update queue info - FIXED: Using correct method names
        if (queueStatusText != null)
        {
            int trainingCount = currentBuilding.GetTrainingCount();
            int queueCount = currentBuilding.GetQueueCount();
            int totalSlots = currentBuilding.trainingSlots;

            queueStatusText.text = $"Training: {trainingCount}/{totalSlots} | Queue: {queueCount}";
        }

        // Update training progress - FIXED: Using correct method names
        if (trainingProgressText != null && trainingProgressSlider != null)
        {
            float progress = currentBuilding.GetTrainingProgress();
            trainingProgressSlider.value = progress;
            trainingProgressText.text = $"Progress: {progress:P0}";
        }

        // Update current troop info
        if (currentTroopText != null)
        {
            currentTroopText.text = $"Current: {currentBuilding.GetCurrentTroopName()}";
        }

        // Update troop type and power if training
        if (troopTypeText != null && troopPowerText != null)
        {
            if (currentBuilding.currentTrainingTroop != null)
            {
                var troop = currentBuilding.currentTrainingTroop;
                troopTypeText.text = $"Type: {troop.troopType}";
                troopPowerText.text = $"Power: {troop.GetCombatValue():F1}";
            }
            else
            {
                troopTypeText.text = "Type: None";
                troopPowerText.text = "Power: 0";
            }
        }
    }

    // Method to handle building selection
    public void OnBuildingSelected(TrainingBuilding building)
    {
        if (building != null)
        {
            ShowPanel(building);
        }
    }

    [ContextMenu("Test UI With Random Building")]
    public void TestUI()
    {
        // Find any training building in the scene for testing
        TrainingBuilding[] buildings = FindObjectsOfType<TrainingBuilding>();
        if (buildings.Length > 0)
        {
            ShowPanel(buildings[0]);
            Debug.Log("Testing UI with building: " + buildings[0].displayName);
        }
        else
        {
            Debug.LogWarning("No TrainingBuilding found in scene for testing");
        }
    }
}