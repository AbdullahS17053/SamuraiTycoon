using UnityEngine;
using System.Collections.Generic;

public class TrainingBuilding : MonoBehaviour
{
    [Header("Building Configuration")]
    public string displayName;
    public string buildingId;
    public int buildingOrder = 1; // Order in progression (1, 2, 3, etc.)
    public bool isUnlocked = true; // Start with first building unlocked
    public TroopUnit.TroopType[] trainableTroopTypes;
    public float baseTrainingTime = 30f;
    public int trainingSlots = 1;

    [Header("Current State")]
    public TroopUnit currentTrainingTroop;
    public bool isTraining = false;
    public List<TroopUnit> trainingQueue = new List<TroopUnit>();

    // Training progress tracking
    private float trainingStartTime;
    private float currentTrainingDuration;

    void Start()
    {
        if (string.IsNullOrEmpty(buildingId))
        {
            buildingId = "building_" + GetInstanceID();
        }
    }

    void Update()
    {
        UpdateTrainingProgress();
    }

    void UpdateTrainingProgress()
    {
        if (isTraining && currentTrainingTroop != null)
        {
            if (Time.time - trainingStartTime >= currentTrainingDuration)
            {
                CompleteCurrentTraining();
            }
        }

        if (!isTraining && trainingQueue.Count > 0)
        {
            StartTraining(trainingQueue[0]);
            trainingQueue.RemoveAt(0);
        }
    }

    public bool CanTrainTroopType(TroopUnit.TroopType troopType)
    {
        if (trainableTroopTypes == null) return false;

        foreach (var type in trainableTroopTypes)
        {
            if (type == troopType)
                return true;
        }
        return false;
    }

    public bool CanAcceptTroop()
    {
        return isUnlocked && !isTraining && currentTrainingTroop == null;
    }

    public void AssignTroop(TroopUnit troop)
    {
        if (troop != null)
        {
            if (CanAcceptTroop())
            {
                StartTraining(troop);
            }
            else
            {
                trainingQueue.Add(troop);
                Debug.Log($"📋 {troop.troopName} added to queue at {displayName}");
            }
        }
    }

    void StartTraining(TroopUnit troop)
    {
        currentTrainingTroop = troop;
        isTraining = true;
        trainingStartTime = Time.time;
        currentTrainingDuration = GetTrainingTimeForTroop(troop.troopType);

        Debug.Log($"🏋️ Started training {troop.troopName} at {displayName} for {currentTrainingDuration} seconds");
    }

    void CompleteCurrentTraining()
    {
        if (currentTrainingTroop != null)
        {
            Debug.Log($"✅ Completed training {currentTrainingTroop.troopName} at {displayName}");
        }

        ReleaseTroop();
    }

    public void ReleaseTroop()
    {
        currentTrainingTroop = null;
        isTraining = false;
        trainingStartTime = 0;
    }

    public float GetTrainingTimeForTroop(TroopUnit.TroopType troopType)
    {
        return baseTrainingTime;
    }

    // UI methods
    public int GetTrainingCount()
    {
        return isTraining ? 1 : 0;
    }

    public int GetQueueCount()
    {
        return trainingQueue.Count;
    }

    public float GetTrainingProgress()
    {
        if (!isTraining || currentTrainingDuration == 0) return 0f;

        float elapsed = Time.time - trainingStartTime;
        return Mathf.Clamp01(elapsed / currentTrainingDuration);
    }

    public string GetCurrentTroopName()
    {
        return currentTrainingTroop != null ? currentTrainingTroop.troopName : "None";
    }

    // Unlock methods
    public void UnlockBuilding()
    {
        isUnlocked = true;
        Debug.Log($"🔓 Building {displayName} unlocked!");
    }

    public void LockBuilding()
    {
        isUnlocked = false;
        // Clear any current training
        if (isTraining && currentTrainingTroop != null)
        {
            ReleaseTroop();
        }
        trainingQueue.Clear();
    }
    /// <summary>
    /// Clear training queue and release current troop
    /// </summary>
    public void ClearTraining()
    {
        // Release current troop
        if (currentTrainingTroop != null)
        {
            currentTrainingTroop.CancelTraining();
            ReleaseTroop();
        }

        // Clear queue
        trainingQueue.Clear();

        Debug.Log($"🧹 Cleared training queue for {displayName}");
    }

    void OnDisable()
    {
        if (isTraining && currentTrainingTroop != null)
        {
            ReleaseTroop();
        }
    }

    [ContextMenu("Debug Building Info")]
    public void DebugBuildingInfo()
    {
        Debug.Log($"🏢 {displayName} (Order: {buildingOrder}, Unlocked: {isUnlocked})");
        Debug.Log($"- Training: {isTraining}, Queue: {trainingQueue.Count}");
        Debug.Log($"- Current Troop: {GetCurrentTroopName()}");
    }
}