using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TrainingBuilding : MonoBehaviour
{
    [Header("Basic Info")]
    public string ID;
    public string DisplayName;
    public bool locked;
    [TextArea] public string Description;

    [Header("Economics")]
    public int BaseIncomePerTrained = 100;
    public float IncomeMultiplier = 1.1f;
    public int PowerAddToTroop = 1;
    public int currentWorkers = 1;
    public int maxWorkers = 5;

    [Header("Visuals")]
    public Sprite Icon;
    public Sprite Banner;
    public Color ThemeColor = Color.white;

    [Header("Building Modules")]
    public List<BuildingModule> modules = new List<BuildingModule>();

    [Header("Building Configuration")]
    public int level = 1;
    public int maxLevel = 100;
    public float baseTrainingTime = 30f;
    public float trainingTimerReducer = 1.1f;

    [Header("Current State")]
    public List<TroopUnit> trainingQueue = new List<TroopUnit>();
    public List<TroopUnit> waitingQueue = new List<TroopUnit>();
    public Transform trainingArea;
    public Transform waitingArea;
    public float queueSpacing = 1.5f;
    public int maxWait = 5;

    public bool castle;
    public bool gate;

    // OPTIMIZED: Object pooling for troops
    private Queue<TroopUnit> troopPool = new Queue<TroopUnit>();
    private List<TroopUnit> activeTroops = new List<TroopUnit>();

    public GameObject Loading;
    public Slider LoadingSlider;
    public GameObject Loaded;

    // OPTIMIZED: Cache components
    private Coroutine currentUnlockCoroutine;

    public void StoreTroop(TroopUnit troop)
    {
        if (castle)
        {
            ReturnTroopToPool(troop);
            WarManager.instance.AddTroop(troop);
        }
    }

    public bool CanTrainTroop()
    {
        return trainingQueue.Count < currentWorkers;
    }

    public void AssignTroop(TroopUnit troop)
    {
        if (castle)
        {
            StoreTroop(troop);
            ReturnTroopToPool(troop);
            return;
        }

        if (locked)
        {
            troop.SkipTraining();
            return;
        }

        if (CanTrainTroop())
        {
            StartTraining(troop);
            trainingQueue.Add(troop);
        }
        else
        {
            if (waitingQueue.Count < maxWait)
            {
                waitingQueue.Add(troop);
                troop.Stand();
                UpdateWaitingQueuePositions();
            }
            else
            {
                troop.SkipTraining();
            }
        }
    }

    // OPTIMIZED: Object pooling methods
    public TroopUnit GetPooledTroop()
    {
        if (troopPool.Count > 0)
        {
            TroopUnit troop = troopPool.Dequeue();
            troop.gameObject.SetActive(true);
            activeTroops.Add(troop);
            return troop;
        }
        return null;
    }

    public void ReturnTroopToPool(TroopUnit troop)
    {
        troopPool.Enqueue(troop);
        activeTroops.Remove(troop);
        TroopManager.instance.ReturnTroopToPool(troop.gameObject);
    }

    void StartTraining(TroopUnit troop)
    {
        StartCoroutine(troop.StartTraining(baseTrainingTime, BaseIncomePerTrained));
    }

    public void CompleteCurrentTraining(TroopUnit troop)
    {
        // OPTIMIZED: More efficient removal
        for (int i = trainingQueue.Count - 1; i >= 0; i--)
        {
            if (trainingQueue[i].troopId == troop.troopId)
            {
                trainingQueue.RemoveAt(i);
                break;
            }
        }

        EconomyManager.Instance.AddGold(BaseIncomePerTrained);

        // OPTIMIZED: Process waiting queue more efficiently
        if (waitingQueue.Count > 0)
        {
            TroopUnit nextTroop = waitingQueue[0];
            waitingQueue.RemoveAt(0);
            AssignTroop(nextTroop);
            UpdateWaitingQueuePositions();
        }
    }

    private void UpdateWaitingQueuePositions()
    {
        for (int i = 0; i < waitingQueue.Count; i++)
        {
            Vector3 offset = new Vector3(0, 0, -i * queueSpacing);
            waitingQueue[i].Move(waitingArea.position + offset);
        }
    }

    public void RemoveTroop(int troopId)
    {
        // OPTIMIZED: Single pass removal
        for (int i = trainingQueue.Count - 1; i >= 0; i--)
        {
            if (trainingQueue[i].troopId == troopId)
            {
                trainingQueue.RemoveAt(i);
                break;
            }
        }

        for (int i = waitingQueue.Count - 1; i >= 0; i--)
        {
            if (waitingQueue[i].troopId == troopId)
            {
                waitingQueue.RemoveAt(i);
                UpdateWaitingQueuePositions();
                break;
            }
        }
    }

    public void UnlockBuilding(float duration)
    {
        if (currentUnlockCoroutine != null)
            StopCoroutine(currentUnlockCoroutine);

        currentUnlockCoroutine = StartCoroutine(Unlocking(duration));
    }

    IEnumerator Unlocking(float seconds)
    {
        Loading.SetActive(true);
        LoadingSlider.maxValue = seconds;
        LoadingSlider.value = 0;

        LoadingSlider.DOValue(seconds, seconds).SetEase(Ease.Linear);
        yield return new WaitForSeconds(seconds);

        Loading.SetActive(false);
        Loaded.SetActive(true);
        locked = false;
    }

    // OPTIMIZED: More efficient module lookup
    private Dictionary<string, BuildingModule> moduleCache;

    public BuildingModule GetModule(string moduleName)
    {
        if (moduleCache == null)
        {
            moduleCache = new Dictionary<string, BuildingModule>();
            foreach (var module in modules)
            {
                if (module != null)
                    moduleCache[module.moduleName] = module;
            }
        }

        moduleCache.TryGetValue(moduleName, out BuildingModule result);
        return result;
    }

    public void UpgradeIncome()
    {
        level++;
        BaseIncomePerTrained = Mathf.RoundToInt(BaseIncomePerTrained * IncomeMultiplier);
        VFXManager.instance.Income();
    }

    public void UpgradeEfficiency()
    {
        level++;
        baseTrainingTime /= trainingTimerReducer;
        VFXManager.instance.Speed();
    }

    public void UpgradeCapacity()
    {
        if (currentWorkers < maxWorkers)
        {
            level++;
            currentWorkers++;
            VFXManager.instance.Capacity();

            if (waitingQueue.Count > 0 && CanTrainTroop())
            {
                TroopUnit nextTroop = waitingQueue[0];
                waitingQueue.RemoveAt(0);
                AssignTroop(nextTroop);
                UpdateWaitingQueuePositions();
            }
        }
    }

    // OPTIMIZED: Cleanup on destroy
    private void OnDestroy()
    {
        if (currentUnlockCoroutine != null)
            StopCoroutine(currentUnlockCoroutine);
    }
}