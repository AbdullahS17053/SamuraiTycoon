using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TroopUnit : MonoBehaviour
{
    public enum TroopType
    {
        Samurai,
        Archer,
        Cavalry,
        Ninja,
        Spearman,
        Ashigaru
    }

    public enum TroopRarity
    {
        Common,
        Uncommon,
        Rare,
        Epic,
        Legendary
    }

    public enum TroopState
    {
        Idle,
        MovingToBuilding,
        Training,
        MovingToCastle,
        InCastle,
        MovingToWar,
        InBattle
    }

    [Header("Troop Identification")]
    public string troopId;
    public string troopName;
    public TroopType troopType;
    public TroopRarity rarity;

    [Header("Troop Stats")]
    public float currentPower = 10f;
    public float basePower = 10f;
    public float trainingPowerBonus = 0f;
    public int trainingBuildingsCompleted = 0;

    [Header("State Management")]
    public TroopState currentState = TroopState.Idle;
    public TrainingBuilding currentTrainingBuilding;
    public int currentBuildingIndex = 0;

    // Movement
    [System.NonSerialized] public UnityEngine.AI.NavMeshAgent navAgent;
    private TroopManager troopManager;

    // Animation
    [Header("Animation")]
    public Animator animator; // Now public so you can assign from inspector
    private bool hasAnimator = false;
    private TroopState previousState; // Track previous state for animation changes

    // Training animation
    private Coroutine trainingAnimationCoroutine;
    private Vector3 originalScale;
    private bool hasTriggeredTrainingAnimation = false;

    // Debug tracking
    private string debugStatus = "Initialized";

    void Awake()
    {
        navAgent = GetComponent<UnityEngine.AI.NavMeshAgent>();

        // If animator is not assigned in inspector, try to get it
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

        hasAnimator = animator != null;

        originalScale = transform.localScale;
        previousState = currentState;

        if (!hasAnimator)
        {
            Debug.LogWarning($"⚠️ {gameObject.name}: No Animator component found. Animation states will not work.");
        }
        else
        {
            Debug.Log($"✅ {gameObject.name}: Animator found and ready");
        }
    }

    public void Initialize(string id, string name, TroopType type, TroopRarity troopRarity, TroopManager manager)
    {
        troopId = id;
        troopName = name;
        troopType = type;
        rarity = troopRarity;
        troopManager = manager;

        basePower = GetBasePowerByRarity();
        currentPower = basePower;

        debugStatus = "Initialized at barracks";
        Debug.Log($"⚔️ {troopName} initialized with {currentPower} power");

        // Set initial animation state
        UpdateAnimationState(true); // Force initial state
    }

    void Update()
    {
        UpdateState();

        // Only update animation if state changed
        if (currentState != previousState)
        {
            UpdateAnimationState(false);
            previousState = currentState;
        }
    }

    void UpdateState()
    {
        switch (currentState)
        {
            case TroopState.MovingToBuilding:
                if (navAgent != null && !navAgent.pathPending && navAgent.remainingDistance <= navAgent.stoppingDistance)
                {
                    debugStatus = "Reached building, starting training";
                    StartTraining();
                }
                break;

            case TroopState.MovingToCastle:
                if (navAgent != null && !navAgent.pathPending && navAgent.remainingDistance <= navAgent.stoppingDistance)
                {
                    debugStatus = "Reached castle, entering";
                    EnterCastle();
                }
                break;
        }
    }

    void UpdateAnimationState(bool forceUpdate = false)
    {
        if (!hasAnimator) return;

        try
        {
            // Only update if state changed or forced
            if (!forceUpdate && currentState == previousState) return;

            debugStatus += $", Anim: {currentState}";

            switch (currentState)
            {
                case TroopState.Idle:
                    SetIdleAnimation();
                    break;

                case TroopState.MovingToBuilding:
                case TroopState.MovingToCastle:
                case TroopState.MovingToWar:
                    SetWalkingAnimation();
                    break;

                case TroopState.Training:
                    SetTrainingAnimation();
                    break;

                case TroopState.InCastle:
                case TroopState.InBattle:
                    SetInactiveAnimation();
                    break;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"⚠️ {troopName}: Animation error - {e.Message}");
        }
    }

    void SetIdleAnimation()
    {
        if (!hasAnimator && animator.GetBool("Walking")) return;
        animator.SetBool("Walking", false);
        Debug.Log($"🛑 {troopName}: Setting Idle animation");
    }

    void SetWalkingAnimation()
    {
        if (!hasAnimator && !animator.GetBool("Walking")) return;
        animator.SetBool("Walking", true);
        animator.ResetTrigger("Train");
        hasTriggeredTrainingAnimation = false;
        Debug.Log($"🚶 {troopName}: Setting Walking animation");
    }

    void SetTrainingAnimation()
    {
        if (!hasAnimator) return;
        animator.SetBool("Walking", false);

        // Only trigger training animation once per training session
        if (!hasTriggeredTrainingAnimation)
        {
            animator.SetTrigger("Train");
            hasTriggeredTrainingAnimation = true;
            Debug.Log($"🏋️ {troopName}: Setting Training animation");
        }
    }

    void SetInactiveAnimation()
    {
        if (!hasAnimator) return;
        animator.SetBool("Walking", false);
        animator.ResetTrigger("Train");
        hasTriggeredTrainingAnimation = false;
        Debug.Log($"💤 {troopName}: Setting Inactive animation");
    }

    public void StartTrainingProgression()
    {
        currentState = TroopState.Idle;
        currentBuildingIndex = 0;
        hasTriggeredTrainingAnimation = false;
        debugStatus = "Starting training progression";
        UpdateAnimationState(true);
        FindNextTrainingBuilding();
    }

    public void FindNextTrainingBuilding()
    {
        if (troopManager == null)
        {
            Debug.LogError($"❌ {troopName}: TroopManager reference is null!");
            return;
        }

        List<TrainingBuilding> availableBuildings = troopManager.GetAllTrainingBuildingsInOrder();

        Debug.Log($"🔍 {troopName}: Looking for building {currentBuildingIndex + 1} of {availableBuildings.Count} total buildings");

        if (currentBuildingIndex >= availableBuildings.Count)
        {
            debugStatus = $"All {availableBuildings.Count} buildings completed, moving to castle";
            Debug.Log($"🎯 {troopName}: Completed all {availableBuildings.Count} buildings, going to castle");
            MoveToCastle();
            return;
        }

        if (currentBuildingIndex < availableBuildings.Count)
        {
            TrainingBuilding nextBuilding = availableBuildings[currentBuildingIndex];
            if (nextBuilding != null)
            {
                bool canTrain = nextBuilding.CanTrainTroopType(troopType);
                bool canAccept = nextBuilding.CanAcceptTroop();
                bool isUnlocked = nextBuilding.isUnlocked;

                debugStatus = $"Checking building {currentBuildingIndex + 1}: CanTrain={canTrain}, CanAccept={canAccept}, Unlocked={isUnlocked}";

                if (canTrain && canAccept && isUnlocked)
                {
                    currentTrainingBuilding = nextBuilding;
                    MoveToBuilding(currentTrainingBuilding.transform.position);
                }
                else
                {
                    debugStatus = $"Building {currentBuildingIndex + 1} not available, waiting...";
                    Debug.Log($"⏳ {troopName}: Building {currentBuildingIndex + 1} not available (CanTrain: {canTrain}, CanAccept: {canAccept}, Unlocked: {isUnlocked}). Retrying in 2s");
                    currentState = TroopState.Idle;
                    UpdateAnimationState(true);
                    Invoke("FindNextTrainingBuilding", 2f);
                }
            }
            else
            {
                debugStatus = $"Building {currentBuildingIndex + 1} is null, skipping";
                Debug.LogWarning($"⚠️ {troopName}: Building {currentBuildingIndex + 1} is null, skipping to next");
                currentBuildingIndex++;
                FindNextTrainingBuilding();
            }
        }
    }

    void MoveToBuilding(Vector3 position)
    {
        currentState = TroopState.MovingToBuilding;
        debugStatus = $"Moving to building {currentBuildingIndex + 1} at {position}";

        if (navAgent != null)
        {
            navAgent.SetDestination(position);
            Debug.Log($"🚶 {troopName} moving to building {currentBuildingIndex + 1} at {position}");
        }
        else
        {
            Debug.LogError($"❌ {troopName}: NavAgent is null, cannot move to building");
        }
    }

    void StartTraining()
    {
        currentState = TroopState.Training;
        debugStatus = $"Training at building {currentBuildingIndex + 1}";

        if (currentTrainingBuilding != null)
        {
            currentTrainingBuilding.AssignTroop(this);
            StartTrainingAnimation();

            float trainingTime = currentTrainingBuilding.GetTrainingTimeForTroop(troopType);
            Invoke("CompleteTraining", trainingTime);

            Debug.Log($"🏋️ {troopName} training at building {currentBuildingIndex + 1} for {trainingTime} seconds");
        }
        else
        {
            debugStatus = "Current training building is null, skipping";
            Debug.LogError($"❌ {troopName}: Current training building is null, skipping to next");
            currentBuildingIndex++;
            FindNextTrainingBuilding();
        }
    }

    void StartTrainingAnimation()
    {
        if (trainingAnimationCoroutine != null)
            StopCoroutine(trainingAnimationCoroutine);

        trainingAnimationCoroutine = StartCoroutine(TrainingAnimationRoutine());
    }

    IEnumerator TrainingAnimationRoutine()
    {
        Animator buildingAnimator = currentTrainingBuilding != null ? currentTrainingBuilding.GetComponent<Animator>() : null;
        bool hasBuildingAnimation = buildingAnimator != null && buildingAnimator.runtimeAnimatorController != null;

        if (hasBuildingAnimation)
        {
            buildingAnimator.SetBool("IsTraining", true);
        }

        // Wait for training to complete
        while (currentState == TroopState.Training)
        {
            yield return null;
        }

        // Clean up building animation
        if (hasBuildingAnimation && buildingAnimator != null)
        {
            buildingAnimator.SetBool("IsTraining", false);
        }
    }

    void StopTrainingAnimation()
    {
        if (trainingAnimationCoroutine != null)
        {
            StopCoroutine(trainingAnimationCoroutine);
            trainingAnimationCoroutine = null;
        }

        transform.localScale = originalScale;

        if (currentTrainingBuilding != null)
        {
            Animator buildingAnimator = currentTrainingBuilding.GetComponent<Animator>();
            if (buildingAnimator != null)
            {
                buildingAnimator.SetBool("IsTraining", false);
            }
        }
    }

    void CompleteTraining()
    {
        if (currentState != TroopState.Training)
        {
            Debug.LogWarning($"⚠️ {troopName}: CompleteTraining called but state is {currentState}");
            return;
        }

        StopTrainingAnimation();

        // Calculate power increase
        float powerIncrease = CalculatePowerIncrease();
        trainingPowerBonus += powerIncrease;
        currentPower = basePower + trainingPowerBonus;
        trainingBuildingsCompleted++;

        // Give one-time income for completing training
        if (troopManager != null)
        {
            troopManager.OnTroopTrained(this, powerIncrease);
        }

        debugStatus = $"Completed training at building {currentBuildingIndex + 1}, power +{powerIncrease}";
        Debug.Log($"✅ {troopName} completed training at building {currentBuildingIndex + 1}! Power +{powerIncrease}. Total: {currentPower}");

        // Release building
        if (currentTrainingBuilding != null)
        {
            currentTrainingBuilding.ReleaseTroop();
            currentTrainingBuilding = null;
        }

        CancelInvoke("CompleteTraining");

        // Move to next building
        currentBuildingIndex++;
        debugStatus = $"Moving to next building: {currentBuildingIndex + 1}";
        FindNextTrainingBuilding();
    }

    float CalculatePowerIncrease()
    {
        float baseIncrease = 5f;

        switch (rarity)
        {
            case TroopRarity.Common: return baseIncrease * 1f;
            case TroopRarity.Uncommon: return baseIncrease * 1.2f;
            case TroopRarity.Rare: return baseIncrease * 1.5f;
            case TroopRarity.Epic: return baseIncrease * 2f;
            case TroopRarity.Legendary: return baseIncrease * 3f;
            default: return baseIncrease;
        }
    }

    void MoveToCastle()
    {
        currentState = TroopState.MovingToCastle;
        Vector3 castlePosition = troopManager.GetCastlePosition();
        debugStatus = $"Moving to castle at {castlePosition}";

        if (navAgent != null)
        {
            navAgent.SetDestination(castlePosition);
            Debug.Log($"🏰 {troopName} completed all training! Moving to castle with {currentPower} power");
        }
        else
        {
            Debug.LogError($"❌ {troopName}: NavAgent is null, cannot move to castle");
            transform.position = castlePosition;
            EnterCastle();
        }
    }

    void EnterCastle()
    {
        currentState = TroopState.InCastle;
        debugStatus = "Stored in castle";

        // Stop all animations
        if (hasAnimator)
        {
            SetInactiveAnimation();
        }

        // Hide the troop completely
        gameObject.SetActive(false);

        // Store in castle
        troopManager.StoreTroopInCastle(this);

        Debug.Log($"🏰 {troopName} stored in castle with final power: {currentPower}");
    }

    float GetBasePowerByRarity()
    {
        switch (rarity)
        {
            case TroopRarity.Common: return 10f;
            case TroopRarity.Uncommon: return 15f;
            case TroopRarity.Rare: return 25f;
            case TroopRarity.Epic: return 40f;
            case TroopRarity.Legendary: return 65f;
            default: return 10f;
        }
    }

    // War methods
    public void SendToWar(Vector3 battlePosition)
    {
        gameObject.SetActive(true);
        currentState = TroopState.MovingToWar;
        if (navAgent != null)
        {
            navAgent.SetDestination(battlePosition);
        }
    }

    public bool IsReadyForBattle()
    {
        return currentState == TroopState.InCastle;
    }

    public bool IsTraining()
    {
        return currentState == TroopState.Training;
    }

    public float GetCombatValue()
    {
        return currentPower;
    }

    public int GetLevel()
    {
        return Mathf.FloorToInt(currentPower / 10f) + 1;
    }

    public void SetSelected(bool selected)
    {
        if (hasAnimator)
        {
            animator.SetBool("Selected", selected);
        }
    }

    public void CancelTraining()
    {
        CancelInvoke("CompleteTraining");
        CancelInvoke("FindNextTrainingBuilding");

        if (currentState == TroopState.Training)
        {
            StopTrainingAnimation();
            currentState = TroopState.Idle;
            UpdateAnimationState(true);
        }

        if (currentTrainingBuilding != null)
        {
            currentTrainingBuilding.ReleaseTroop();
            currentTrainingBuilding = null;
        }
    }

    // Debug method to check current status
    public string GetDebugStatus()
    {
        return $"{troopName}: {currentState} - {debugStatus} (Building {currentBuildingIndex + 1})";
    }

    // Animation helper method
    public void ForceAnimationUpdate()
    {
        UpdateAnimationState(true);
    }

    void OnDestroy()
    {
        if (trainingAnimationCoroutine != null)
        {
            StopCoroutine(trainingAnimationCoroutine);
        }
    }
}