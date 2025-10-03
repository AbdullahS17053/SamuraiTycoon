using UnityEngine;
using System.Collections.Generic;

public class TrainingBuilding : MonoBehaviour
{
    [Header("Training Configuration")]
    public string buildingId;
    public int trainingSlots = 3;
    public float trainingTime = 10f;
    public Transform[] trainingPositions;
    public Transform exitPoint;
    public Transform defensivePosition; // Where troops stand after training

    [Header("Visual Effects")]
    public ParticleSystem trainingEffect;
    public AudioClip trainingSound;
    public Light trainingLight;

    private List<TroopUnit> _trainingTroops = new List<TroopUnit>();
    private List<float> _trainingProgress = new List<float>();
    private Queue<TroopUnit> _waitingQueue = new Queue<TroopUnit>();
    private bool[] _slotOccupied;

    void Start()
    {
        _slotOccupied = new bool[trainingSlots];

        // Auto-find positions if not assigned
        if (trainingPositions == null || trainingPositions.Length == 0)
        {
            FindTrainingPositions();
        }
    }

    void FindTrainingPositions()
    {
        List<Transform> positions = new List<Transform>();

        // Look for child objects named "TrainingPosition"
        foreach (Transform child in transform)
        {
            if (child.name.Contains("TrainingPosition") || child.name.Contains("TrainPos"))
            {
                positions.Add(child);
            }
        }

        trainingPositions = positions.ToArray();

        // If still no positions, create them automatically
        if (trainingPositions.Length == 0)
        {
            CreateDefaultPositions();
        }
    }

    void CreateDefaultPositions()
    {
        trainingPositions = new Transform[trainingSlots];
        for (int i = 0; i < trainingSlots; i++)
        {
            GameObject pos = new GameObject($"TrainingPosition_{i}");
            pos.transform.SetParent(transform);
            pos.transform.localPosition = new Vector3((i - trainingSlots / 2f) * 2f, 0, 0);
            trainingPositions[i] = pos.transform;
        }
    }

    public bool CanAcceptTroop()
    {
        return _trainingTroops.Count < trainingSlots || _waitingQueue.Count < 5; // Max queue size
    }

    public void SendTroopForTraining(TroopUnit troop)
    {
        if (_trainingTroops.Count < trainingSlots)
        {
            StartTraining(troop, _trainingTroops.Count);
        }
        else
        {
            _waitingQueue.Enqueue(troop);
            troop.SetState(TroopUnit.TroopState.Queued);
            troop.SetTargetPosition(GetQueuePosition(_waitingQueue.Count));
        }
    }

    void StartTraining(TroopUnit troop, int slotIndex)
    {
        _trainingTroops.Add(troop);
        _trainingProgress.Add(0f);
        _slotOccupied[slotIndex] = true;

        // Move troop to training position
        troop.SetState(TroopUnit.TroopState.Training);
        troop.SetTargetPosition(trainingPositions[slotIndex].position);

        // Start training visual effects
        if (trainingEffect != null)
            trainingEffect.Play();

        if (trainingLight != null)
            trainingLight.enabled = true;

        Debug.Log($"?? {troop.troopName} started training at {buildingId}");
    }

    void Update()
    {
        // Update training progress
        for (int i = _trainingTroops.Count - 1; i >= 0; i--)
        {
            if (_trainingTroops[i] == null)
            {
                _trainingTroops.RemoveAt(i);
                _trainingProgress.RemoveAt(i);
                continue;
            }

            // Check if troop reached training position
            if (Vector3.Distance(_trainingTroops[i].transform.position, trainingPositions[i].position) < 1f)
            {
                _trainingProgress[i] += Time.deltaTime;

                // Update troop training progress
                _trainingTroops[i].UpdateTrainingProgress(_trainingProgress[i] / trainingTime);

                // Training complete
                if (_trainingProgress[i] >= trainingTime)
                {
                    CompleteTraining(_trainingTroops[i], i);
                }
            }
        }

        // Process waiting queue
        if (_waitingQueue.Count > 0 && _trainingTroops.Count < trainingSlots)
        {
            TroopUnit nextTroop = _waitingQueue.Dequeue();
            StartTraining(nextTroop, GetAvailableSlot());
        }
    }

    void CompleteTraining(TroopUnit troop, int slotIndex)
    {
        // Training complete effects
        if (trainingEffect != null)
            trainingEffect.Stop();

        // Level up the troop
        troop.CompleteTraining();

        // Move troop to exit, then to defensive position
        troop.SetState(TroopUnit.TroopState.MovingToPosition);
        troop.SetTargetPosition(exitPoint.position);

        // Remove from training lists
        _trainingTroops.RemoveAt(slotIndex);
        _trainingProgress.RemoveAt(slotIndex);
        _slotOccupied[slotIndex] = false;

        Debug.Log($"??? {troop.troopName} completed training! Now level {troop.GetLevel()}");

        // Start coroutine to send to defensive position
        StartCoroutine(SendToDefensivePosition(troop));
    }

    System.Collections.IEnumerator SendToDefensivePosition(TroopUnit troop)
    {
        // Wait for troop to reach exit point
        yield return new WaitUntil(() => Vector3.Distance(troop.transform.position, exitPoint.position) < 1f);

        // Send to defensive position
        troop.SetState(TroopUnit.TroopState.Guarding);
        troop.SetTargetPosition(defensivePosition.position);
    }

    int GetAvailableSlot()
    {
        for (int i = 0; i < trainingSlots; i++)
        {
            if (!_slotOccupied[i]) return i;
        }
        return 0;
    }

    Vector3 GetQueuePosition(int queueIndex)
    {
        Vector3 basePosition = transform.position + transform.forward * 3f;
        return basePosition + transform.right * (queueIndex * 2f);
    }

    public int GetQueueCount()
    {
        return _waitingQueue.Count;
    }

    public int GetTrainingCount()
    {
        return _trainingTroops.Count;
    }

    public float GetTrainingProgress(int slotIndex)
    {
        if (slotIndex < _trainingProgress.Count)
            return _trainingProgress[slotIndex] / trainingTime;
        return 0f;
    }

    void OnDrawGizmos()
    {
        // Draw training positions
        if (trainingPositions != null)
        {
            Gizmos.color = Color.yellow;
            foreach (var pos in trainingPositions)
            {
                if (pos != null)
                {
                    Gizmos.DrawWireSphere(pos.position, 0.5f);
                    Gizmos.DrawIcon(pos.position, "Training.png", true);
                }
            }
        }

        // Draw exit point
        if (exitPoint != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(exitPoint.position, 0.3f);
            Gizmos.DrawIcon(exitPoint.position, "Exit.png", true);
        }

        // Draw defensive position
        if (defensivePosition != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(defensivePosition.position, 0.5f);
            Gizmos.DrawIcon(defensivePosition.position, "Guard.png", true);
        }
    }
}