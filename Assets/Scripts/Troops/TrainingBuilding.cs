using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

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
    public float PowerIncrease = 1.1f;
    public int currentWorkers = 1;
    public int maxWorkers;

    [Header("Visuals")]
    public Sprite Icon;
    public Sprite Banner;
    public Color ThemeColor = Color.white;

    [Header("Building Modules - DRAG MODULES HERE!")]
    [Tooltip("Add modules to create upgrade buttons in the building panel")]
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
    public List<TroopUnit> troops = new List<TroopUnit>();

    public GameObject Loading;
    public UnityEngine.UI.Slider LoadingSlider;
    public GameObject Loaded;
    public void StoreTroops(TroopUnit troop)
    {
        troops.Add(troop);
        troop.RestNow();

        WarManager.instance.AddTroop(troop);


    }

    public bool CanTrainTroop()
    {
        if (trainingQueue.Count >= currentWorkers) return false;

        return true;
    }

    public void AssignTroop(TroopUnit troop)
    {
        if (castle)
        {
            StoreTroops(troop);
            return;
        }

        if (!locked)
        {
            if (CanTrainTroop())
            {
                StartTraining(troop);
                trainingQueue.Add(troop);
            }
            else
            {
                if(waitingQueue.Count >= maxWait)
                {
                    troop.SkipTraining();
                }
                else
                {
                    waitingQueue.Add(troop);
                    troop.Stand();
                    UpdateWaitingQueuePositions();
                }
            }
        }
        else
        {
            troop.SkipTraining();
        }
    }

    void StartTraining(TroopUnit troop)
    {
        StartCoroutine(troop.StartTraining(baseTrainingTime, BaseIncomePerTrained));
    }

    public void CompleteCurrentTraining(TroopUnit troop)
    {
        for(int i = 0; i < trainingQueue.Count; i++)
        {
            if(trainingQueue[i].troopId == troop.troopId)
            {
                trainingQueue.RemoveAt(i);

                EconomyManager.Instance.AddGold(BaseIncomePerTrained);

                if(waitingQueue.Count > 0)
                {
                    for(int e = 0; e < waitingQueue.Count; e++)
                    {
                        AssignTroop(waitingQueue[e]);
                        waitingQueue.RemoveAt(e);

                        UpdateWaitingQueuePositions();

                        break;
                    }
                }

                break;
            }
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
    public void Remove(int ID)
    {
        for (int i = 0; i < trainingQueue.Count; i++)
        {
            if (trainingQueue[i].troopId == ID)
            {
                trainingQueue.RemoveAt(i);

                break;
            }
        }
        for (int e = 0; e < waitingQueue.Count; e++)
        {
            if (waitingQueue[e].troopId == ID)
            {
                waitingQueue.RemoveAt(e);

                UpdateWaitingQueuePositions();

                break;
            }
        }
    }

    public void UnlockBuilding(float duration)
    {
        Debug.Log("unlocking");
        StartCoroutine(Unlocking(duration));
    }

    IEnumerator Unlocking(float seconds)
    {
        Loading.SetActive(true);

        LoadingSlider.maxValue = seconds;
        LoadingSlider.value = 1;
        LoadingSlider.DOValue(seconds, seconds).SetEase(Ease.Linear);

        yield return new WaitForSeconds(seconds);


        Loading.SetActive(false);
        Loaded.SetActive(true);
        locked = false;
    }

    public BuildingModule GetModule<T>(string moduleName)
    {
        foreach (var module in modules)
        {
            if (module.moduleName == moduleName)
                return module;
        }
        return null;
    }

    public void UpgradeIncome()
    {
        float baseIncome = BaseIncomePerTrained;

        BaseIncomePerTrained = Mathf.RoundToInt(baseIncome *= IncomeMultiplier);

        VFXManager.instance.Income();
    }

    public void UpgradeEfficiency()
    {
        baseTrainingTime = Mathf.RoundToInt(baseTrainingTime /= IncomeMultiplier);
        VFXManager.instance.Speed();
    }

    public void UpgradeCapcity()
    {
        currentWorkers++;
        VFXManager.instance.Capacity();

        if (waitingQueue.Count > 0)
        {
            for (int e = 0; e < trainingQueue.Count; e++)
            {
                AssignTroop(waitingQueue[e]);
                waitingQueue.RemoveAt(e);

                break;
            }
        }
    }

}