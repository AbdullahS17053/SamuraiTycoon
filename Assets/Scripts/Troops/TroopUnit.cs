using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TroopUnit : MonoBehaviour
{

    [Header("Troop Identification")]
    public string troopId;

    [Header("Troop Stats")]
    public int currentPower = 10;
    public Slider slider;

    [Header("State Management")]
    public TrainingBuilding currentTrainingBuilding;
    public int troopLevel = 0;

    public UnityEngine.AI.NavMeshAgent navAgent;

    public Animator animator;

    void Start()
    {
        MoveToBuilding();
    }

    void MoveToBuilding()
    {
        currentTrainingBuilding = BuildingManager3D.Instance.GetBuilding(troopLevel);
        navAgent.SetDestination(currentTrainingBuilding.waitingArea.position);
        Walk();
    }
    public void Move(Vector3 here)
    {
        navAgent.SetDestination(here);
        animator.SetBool("Walking", true);
        StartCoroutine(CheckArrival());
    }

    private IEnumerator CheckArrival()
    {
        while (navAgent.pathPending || navAgent.remainingDistance > navAgent.stoppingDistance)
            yield return null;

        animator.SetBool("Walking", false);
    }

    public void SkipTraining()
    {
        troopLevel++;
        
        MoveToBuilding();
    }

    public IEnumerator StartTraining(float time, int power)
    {
        navAgent.SetDestination(currentTrainingBuilding.trainingArea.position);

        while (navAgent.pathPending || navAgent.remainingDistance > navAgent.stoppingDistance)
        {
            yield return null;
        }

        slider.gameObject.SetActive(true);
        slider.maxValue = 100;
        slider.value = 1;
        slider.DOValue(100, currentTrainingBuilding.baseTrainingTime).SetEase(Ease.Linear);
        Train();

        yield return new WaitForSeconds(time);


        if (!currentTrainingBuilding.gate)
        {
            UIManager.instance.SpawnAndAnimate(transform.position);
            VFXManager.instance.Trained(transform.position);
        }
        VFXManager.instance.BuildingTrained(transform.position);

        slider.gameObject.SetActive(false);
        currentTrainingBuilding.CompleteCurrentTraining(this);
        currentPower += power;
        troopLevel++;
        Walk();

        MoveToBuilding();
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Building"))
        {
            if(other.gameObject.GetComponent<TrainingBuilding>().ID == currentTrainingBuilding.ID)
            {
                currentTrainingBuilding.AssignTroop(this);
            }
        }
    }

    public void RestNow()
    {
        Stand();
        //gameObject.SetActive(false);
        Destroy(gameObject);
    }

    public void Stand()
    {
        animator.SetBool("Walking", false);
    }

    void Walk()
    {
        animator.SetBool("Walking", true);
        animator.ResetTrigger("Train");
    }

    void Train()
    {
        animator.SetBool("Walking", false);

        animator.SetTrigger("Train");
    }
}