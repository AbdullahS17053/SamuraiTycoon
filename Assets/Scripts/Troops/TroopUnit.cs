using DG.Tweening;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using static UnityEngine.InputManagerEntry;

public class TroopUnit : MonoBehaviour
{
    [Header("Troop Identification")]
    public int troopId;

    public GameObject[] skins;

    [Header("Troop Stats")]
    public int currentPower = 10;
    public Slider slider;

    [Header("State Management")]
    public TrainingBuilding currentTrainingBuilding;
    public int troopLevel = -1;
    public int troopSkin = 0;

    // OPTIMIZED: Cached components
    private NavMeshAgent navAgent;
    public Animator animator;
    private Coroutine movementCoroutine;

    void Awake()
    {
        navAgent = GetComponent<NavMeshAgent>();

        // OPTIMIZED: Configure NavMeshAgent for performance
        if (navAgent != null)
        {
            navAgent.avoidancePriority = Random.Range(1, 100);
        }
    }

    void Start()
    {
        MoveToBuilding();
    }

    void MoveToBuilding()
    {
        if (currentTrainingBuilding != null)
        {
            currentTrainingBuilding.RemoveTroop(troopId);
        }

        troopLevel++;
        currentTrainingBuilding = BuildingManager3D.Instance.GetBuilding(troopLevel);

        if (currentTrainingBuilding != null)
        {
            Move(currentTrainingBuilding.waitingArea.position);
        }
    }

    public void Move(Vector3 position)
    {
        if (movementCoroutine != null)
            StopCoroutine(movementCoroutine);

        movementCoroutine = StartCoroutine(MoveToPosition(position));
    }

    private IEnumerator MoveToPosition(Vector3 position)
    {
        navAgent.SetDestination(position);
        Walk();

        // OPTIMIZED: More efficient arrival checking
        float timeout = 10f;
        float startTime = Time.time;

        while (navAgent.pathPending ||
               (navAgent.hasPath && navAgent.remainingDistance > navAgent.stoppingDistance + 0.1f))
        {
            if (Time.time - startTime > timeout)
            {
                Debug.LogWarning($"Troop {troopId} movement timeout");
                break;
            }
            yield return new WaitForSeconds(0.1f); // Check less frequently
        }

        Stand();
        movementCoroutine = null;
    }

    public void SkipTraining()
    {
        MoveToBuilding();
    }

    public IEnumerator StartTraining(float time, int power)
    {
        Move(currentTrainingBuilding.trainingArea.position);

        // Wait for arrival
        yield return new WaitUntil(() => movementCoroutine == null);

        slider.gameObject.SetActive(true);
        slider.maxValue = 100;
        slider.value = 0;

        // OPTIMIZED: Use less expensive tween
        slider.DOValue(100, time).SetEase(Ease.Linear);
        Train();

        yield return new WaitForSeconds(time);

        if (!currentTrainingBuilding.gate)
        {
            UIManager.instance.SpawnAndAnimate(transform.position);
            VFXManager.instance.Trained(transform.position);
        }

        VFXManager.instance.BuildingTrained(transform.position);

        slider.gameObject.SetActive(false);

        if (currentTrainingBuilding != null)
        {
            currentTrainingBuilding.CompleteCurrentTraining(this);
        }

        foreach(GameObject g in skins)
        {
            g.SetActive(false);
        }
        skins[troopLevel].SetActive(true);
        currentPower += power;
        MoveToBuilding();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Building"))
        {
            TrainingBuilding building = other.GetComponent<TrainingBuilding>();
            if (building != null && building.ID == currentTrainingBuilding.ID)
            {
                currentTrainingBuilding.AssignTroop(this);
            }
        }
    }

    public void RestNow()
    {
        Stand();
        if (currentTrainingBuilding != null)
        {
            currentTrainingBuilding.ReturnTroopToPool(this);
        }
    }

    public void Stand()
    {
        animator.SetBool("Walking", false);
        if (navAgent != null && navAgent.isActiveAndEnabled)
        {
            navAgent.isStopped = true;
        }
    }

    void Walk()
    {
        animator.SetBool("Walking", true);
        animator.ResetTrigger("Train");
        if (navAgent != null && navAgent.isActiveAndEnabled)
        {
            navAgent.isStopped = false;
        }
    }

    void Train()
    {
        animator.SetBool("Walking", false);
        animator.SetTrigger("Train");
        if (navAgent != null)
        {
            navAgent.isStopped = true;
        }
    }

    // OPTIMIZED: Cleanup
    private void OnDisable()
    {
        if (movementCoroutine != null)
        {
            StopCoroutine(movementCoroutine);
            movementCoroutine = null;
        }

        if (slider != null)
            slider.gameObject.SetActive(false);
    }
}