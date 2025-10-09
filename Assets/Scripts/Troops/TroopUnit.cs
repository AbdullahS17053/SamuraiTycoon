using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class TroopUnit : MonoBehaviour
{

    [Header("Troop Identification")]
    public string troopId;

    [Header("Troop Stats")]
    public float currentPower = 10f;
    public float basePower = 10f;

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
    }

    public void SkipTraining()
    {
        troopLevel++;
        Walk();

        MoveToBuilding();
    }

    public IEnumerator StartTraining(float time, int power)
    {
        navAgent.SetDestination(currentTrainingBuilding.trainingArea.position);

        Train();

        yield return new WaitForSeconds(time);
         
        currentTrainingBuilding.CompleteCurrentTraining(this);
        currentPower += power;
        troopLevel++;
        Walk();

        MoveToBuilding();
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Building") && other.gameObject.GetComponent<TrainingBuilding>().ID == currentTrainingBuilding.ID)
        {
            Stand();
            currentTrainingBuilding.AssignTroop(this);
        }
    }

    public void RestNow()
    {
        Stand();
        gameObject.SetActive(false);
    }

    void Stand()
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