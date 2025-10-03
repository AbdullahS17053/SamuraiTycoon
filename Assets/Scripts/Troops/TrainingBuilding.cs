using System.Collections.Generic;
using UnityEngine;

public class TrainingBuilding : MonoBehaviour
{
    [Header("Training Building Configuration")]
    public string buildingId = "training_ground";
    public string displayName = "Training Grounds";
    public int trainingSlots = 3;
    public int queueLimit = 10;

    [Header("Training Configuration")]
    public float baseTrainingTime = 30f;
    public double trainingCost = 100f;

    private List<TroopUnit> _trainingTroops = new List<TroopUnit>();
    private Queue<TroopUnit> _trainingQueue = new Queue<TroopUnit>();
    private float[] _trainingProgress;
    private bool _isInitialized = false;

    void Start()
    {
        Initialize();
    }

    void Initialize()
    {
        _trainingProgress = new float[trainingSlots];
        _isInitialized = true;
        Debug.Log($"🏯 TrainingBuilding initialized: {displayName} with {trainingSlots} slots");
    }

    void Update()
    {
        if (!_isInitialized) return;

        UpdateTrainingProgress();
        ProcessTrainingQueue();
    }

    public bool AddToTraining(TroopUnit troop)
    {
        if (!_isInitialized || troop == null) return false;

        // Check if there's an available slot
        if (_trainingTroops.Count < trainingSlots)
        {
            StartTrainingTroop(troop, _trainingTroops.Count);
            return true;
        }
        else if (_trainingQueue.Count < queueLimit)
        {
            _trainingQueue.Enqueue(troop);
            troop.SetState(TroopUnit.TroopState.Idle); // Use the enum from TroopUnit
            Debug.Log($"📋 {troop.troopName} added to training queue");
            return true;
        }

        Debug.Log($"❌ Training queue full for {displayName}");
        return false;
    }

    private void StartTrainingTroop(TroopUnit troop, int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= trainingSlots) return;

        _trainingTroops.Add(troop);
        _trainingProgress[slotIndex] = 0f;

        // Use the TroopUnit's built-in training system
        troop.StartTraining();

        Debug.Log($"🎯 Started training {troop.troopName} in slot {slotIndex}");
    }

    private void UpdateTrainingProgress()
    {
        for (int i = _trainingTroops.Count - 1; i >= 0; i--)
        {
            var troop = _trainingTroops[i];
            if (troop == null)
            {
                _trainingTroops.RemoveAt(i);
                continue;
            }

            // Check if troop is still training using its internal state
            if (!troop.IsTraining())
            {
                CompleteTraining(troop, i);
            }
        }
    }

    private void CompleteTraining(TroopUnit troop, int slotIndex)
    {
        if (troop != null)
        {
            Debug.Log($"✅ Training completed for {troop.troopName}");

            // The troop already handles its own completion logic
            // Just remove it from our tracking
        }

        _trainingTroops.RemoveAt(slotIndex);
        _trainingProgress[slotIndex] = 0f;
    }

    private void ProcessTrainingQueue()
    {
        while (_trainingQueue.Count > 0 && _trainingTroops.Count < trainingSlots)
        {
            TroopUnit nextTroop = _trainingQueue.Dequeue();
            if (nextTroop != null)
            {
                StartTrainingTroop(nextTroop, _trainingTroops.Count);
            }
        }
    }

    // Public access methods
    public int GetTrainingCount()
    {
        return _trainingTroops.Count;
    }

    public int GetQueueCount()
    {
        return _trainingQueue.Count;
    }

    public float GetTrainingProgress(int slotIndex)
    {
        if (slotIndex >= 0 && slotIndex < trainingSlots && slotIndex < _trainingTroops.Count)
        {
            var troop = _trainingTroops[slotIndex];
            if (troop != null && troop.IsTraining())
            {
                // You would need to expose progress from TroopUnit or calculate it here
                return 0.5f; // Placeholder
            }
        }
        return 0f;
    }

    public bool CanAcceptTraining()
    {
        return _trainingTroops.Count < trainingSlots || _trainingQueue.Count < queueLimit;
    }

    [ContextMenu("Debug Training Info")]
    public void DebugTrainingInfo()
    {
        Debug.Log($"=== {displayName} Training Info ===");
        Debug.Log($"Training: {_trainingTroops.Count}/{trainingSlots}");
        Debug.Log($"Queue: {_trainingQueue.Count}/{queueLimit}");

        for (int i = 0; i < _trainingTroops.Count; i++)
        {
            if (_trainingTroops[i] != null)
            {
                Debug.Log($"Slot {i}: {_trainingTroops[i].troopName} (Lvl {_trainingTroops[i].GetLevel()})");
            }
        }
    }
}