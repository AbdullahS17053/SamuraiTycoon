using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.AI;

public class TroopUnit : MonoBehaviour
{
    [Header("Troop Configuration")]
    public string troopId;
    public string troopName;
    public TroopType troopType = TroopType.Samurai;
    public TroopRarity rarity = TroopRarity.Common;

    [Header("Stats")]
    public double baseIncomePerSecond = 1.0;
    public float trainingTime = 5.0f;
    public float moveSpeed = 3.5f;
    public float attackRange = 2.0f;
    public float attackDamage = 10f;
    public float attackSpeed = 1.0f;

    [Header("Visual Elements")]
    public Animator animator;
    public TextMeshPro levelText;
    public GameObject selectionEffect;
    public Renderer troopRenderer;
    public GameObject rarityEffect;
    public NavMeshAgent navAgent;

    [Header("Combat")]
    public GameObject attackProjectile;
    public Transform attackPoint;
    public AudioClip attackSound;

    // Current state
    private int _level = 1;
    private bool _isTraining = false;
    private bool _isSelected = false;
    private bool _inCombat = false;
    private TroopUnit _currentTarget;
    private float _attackCooldown = 0f;

    // References
    private TroopManager _manager;
    private AudioSource _audioSource;

    // Add these fields to the TroopUnit class
    private TroopState _currentState = TroopState.Idle;
    private float _trainingProgress = 0f;

    public enum TroopType
    {
        Samurai,
        Ashigaru,
        Archer,
        Spearman,
        Cavalry,
        Ninja
    }

    public enum TroopRarity
    {
        Common,
        Uncommon,
        Rare,
        Epic,
        Legendary
    }
    // Add this enum to the TroopUnit class
    public enum TroopState
    {
        Idle,
        Training,
        Combat,
        Moving,
        Patrolling
    }

    void Start()
    {
        _manager = FindObjectOfType<TroopManager>();
        _audioSource = GetComponent<AudioSource>();
        if (_audioSource == null) _audioSource = gameObject.AddComponent<AudioSource>();

        if (navAgent != null)
        {
            navAgent.speed = moveSpeed;
        }

        UpdateVisuals();
        SetRandomPatrolPoint();
    }

    void Update()
    {
        if (_inCombat && _currentTarget != null)
        {
            HandleCombat();
        }
        else if (!_isTraining && navAgent != null && !navAgent.hasPath)
        {
            // Random patrol behavior when not in combat
            if (Random.Range(0f, 1f) < 0.01f) // 1% chance each frame to get new destination
            {
                SetRandomPatrolPoint();
            }
        }

        // Update attack cooldown
        if (_attackCooldown > 0f)
        {
            _attackCooldown -= Time.deltaTime;
        }
    }

    public void Initialize(string id, string name, TroopType type, TroopRarity troopRarity, TroopManager manager)
    {
        troopId = id;
        troopName = name;
        troopType = type;
        rarity = troopRarity;
        _manager = manager;

        // Apply rarity bonuses
        ApplyRarityBonuses();
        UpdateVisuals();
    }



    void ApplyRarityBonuses()
    {
        switch (rarity)
        {
            case TroopRarity.Common:
                baseIncomePerSecond *= 1.0;
                break;
            case TroopRarity.Uncommon:
                baseIncomePerSecond *= 1.5;
                moveSpeed *= 1.1f;
                break;
            case TroopRarity.Rare:
                baseIncomePerSecond *= 2.0;
                moveSpeed *= 1.2f;
                trainingTime *= 0.9f;
                break;
            case TroopRarity.Epic:
                baseIncomePerSecond *= 3.0;
                moveSpeed *= 1.3f;
                trainingTime *= 0.8f;
                break;
            case TroopRarity.Legendary:
                baseIncomePerSecond *= 5.0;
                moveSpeed *= 1.5f;
                trainingTime *= 0.7f;
                break;
        }
    }

    // Add these methods to the TroopUnit class
    public void SetState(TroopState newState)
    {
        TroopState previousState = _currentState;
        _currentState = newState;

        // Handle state transitions
        OnStateChanged(previousState, newState);
    }

    public TroopState GetCurrentState()
    {
        return _currentState;
    }

    public void UpdateTrainingProgress(float progress)
    {
        _trainingProgress = Mathf.Clamp01(progress);

        // Visual feedback for training progress
        if (troopRenderer != null)
        {
            Color trainingColor = Color.Lerp(Color.gray, GetRarityColor(), _trainingProgress);
            troopRenderer.material.color = trainingColor;
        }
    }

    public void CompleteTraining()
    {
        _isTraining = false;
        _level++;
        _trainingProgress = 1f;

        // Level up bonuses
        baseIncomePerSecond *= 1.2f;
        moveSpeed *= 1.05f;

        // Reset visuals
        transform.localScale = Vector3.one;
        if (animator != null)
            animator.SetBool("IsTraining", false);

        UpdateVisuals();
        SetState(TroopState.Idle);

        // Generate income and notify manager
        if (_manager != null)
        {
            _manager.OnTroopTrained(this);
        }

        Debug.Log($"🎖️ {troopName} trained to level {_level}! Income: {GetCurrentIncome()}/s");
    }

    private void OnStateChanged(TroopState fromState, TroopState toState)
    {
        Debug.Log($"🔄 {troopName} state changed: {fromState} -> {toState}");

        // Handle animation states
        if (animator != null)
        {
            animator.SetBool("IsTraining", toState == TroopState.Training);
            animator.SetBool("InCombat", toState == TroopState.Combat);
            animator.SetBool("IsMoving", toState == TroopState.Moving);
        }

        // Handle navigation
        if (navAgent != null)
        {
            navAgent.isStopped = (toState == TroopState.Training || toState == TroopState.Idle);
        }
    }

    // Modify the existing StartTraining method to use the state system
    public void StartTraining()
    {
        if (!_isTraining && _currentState != TroopState.Training)
        {
            _isTraining = true;
            SetState(TroopState.Training);
            StartCoroutine(TrainingCoroutine());
        }
    }

    // Modify the TrainingCoroutine to use the new progress system
    IEnumerator TrainingCoroutine()
    {
        float timer = 0f;
        _trainingProgress = 0f;

        while (timer < trainingTime)
        {
            timer += Time.deltaTime;
            _trainingProgress = timer / trainingTime;

            UpdateTrainingProgress(_trainingProgress);

            // Pulse effect when nearly done
            if (_trainingProgress > 0.8f)
            {
                float pulse = Mathf.PingPong(Time.time * 4f, 0.3f) + 0.7f;
                transform.localScale = Vector3.one * pulse;
            }

            yield return null;
        }

        // Use the new CompleteTraining method
        CompleteTraining();
    }

    void HandleCombat()
    {
        if (_currentTarget == null)
        {
            _inCombat = false;
            return;
        }

        // Face target
        Vector3 direction = (_currentTarget.transform.position - transform.position).normalized;
        transform.rotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));

        // Check attack range
        float distance = Vector3.Distance(transform.position, _currentTarget.transform.position);
        if (distance <= attackRange)
        {
            // Stop moving
            if (navAgent != null && navAgent.isActiveAndEnabled)
                navAgent.isStopped = true;

            // Attack if cooldown is ready
            if (_attackCooldown <= 0f)
            {
                Attack();
                _attackCooldown = 1f / attackSpeed;
            }
        }
        else
        {
            // Move towards target
            if (navAgent != null && navAgent.isActiveAndEnabled)
            {
                navAgent.isStopped = false;
                navAgent.SetDestination(_currentTarget.transform.position);
            }
        }
    }

    void Attack()
    {
        if (animator != null)
            animator.SetTrigger("Attack");

        // Play attack sound
        if (attackSound != null && _audioSource != null)
        {
            _audioSource.PlayOneShot(attackSound);
        }

        // Create projectile for ranged troops
        if (attackProjectile != null && attackPoint != null && troopType == TroopType.Archer)
        {
            GameObject projectile = Instantiate(attackProjectile, attackPoint.position, attackPoint.rotation);
            Projectile proj = projectile.GetComponent<Projectile>();
            if (proj != null)
            {
                proj.Initialize(_currentTarget.transform, attackDamage);
            }
        }
        else
        {
            // Melee attack - directly damage target
            _currentTarget.TakeDamage(attackDamage);
        }

        Debug.Log($"⚔️ {troopName} attacks for {attackDamage} damage!");
    }

    public void TakeDamage(float damage)
    {
        // Visual feedback
        StartCoroutine(DamageFlash());

        // You could add health system here
        Debug.Log($"{troopName} takes {damage} damage!");
    }

    IEnumerator DamageFlash()
    {
        Material mat = troopRenderer.material;
        Color originalColor = mat.color;
        mat.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        mat.color = originalColor;
    }

    void SetRandomPatrolPoint()
    {
        if (navAgent != null && _manager != null)
        {
            Vector3 randomPoint = _manager.GetRandomPatrolPoint();
            navAgent.SetDestination(randomPoint);
        }
    }

    public void SetTargetPosition(Vector3 position)
    {
        if (navAgent != null && navAgent.isActiveAndEnabled)
        {
            navAgent.SetDestination(position);

            // Face the direction of movement
            Vector3 direction = (position - transform.position).normalized;
            if (direction != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
            }
        }
    }

    public void SetSelected(bool selected)
    {
        _isSelected = selected;
        if (selectionEffect != null)
        {
            selectionEffect.SetActive(selected);
        }
    }

    void UpdateVisuals()
    {
        // Update level text
        if (levelText != null)
        {
            levelText.text = $"Lvl {_level}";
        }

        // Update color based on rarity
        if (troopRenderer != null)
        {
            troopRenderer.material.color = GetRarityColor();
        }

        // Update rarity effect
        if (rarityEffect != null)
        {
            rarityEffect.SetActive(rarity >= TroopRarity.Rare);
        }
    }

    Color GetRarityColor()
    {
        switch (rarity)
        {
            case TroopRarity.Common: return Color.white;
            case TroopRarity.Uncommon: return Color.green;
            case TroopRarity.Rare: return Color.blue;
            case TroopRarity.Epic: return Color.magenta;
            case TroopRarity.Legendary: return Color.yellow;
            default: return Color.white;
        }
    }

    void OnMouseDown()
    {
        SetSelected(true);
        if (_manager != null)
        {
            _manager.SelectTroop(this);
        }
    }

    public double GetCurrentIncome()
    {
        return baseIncomePerSecond * _level * GetRarityMultiplier();
    }

    float GetRarityMultiplier()
    {
        switch (rarity)
        {
            case TroopRarity.Common: return 1.0f;
            case TroopRarity.Uncommon: return 1.5f;
            case TroopRarity.Rare: return 2.0f;
            case TroopRarity.Epic: return 3.0f;
            case TroopRarity.Legendary: return 5.0f;
            default: return 1.0f;
        }
    }

    public bool IsTraining()
    {
        return _isTraining;
    }

    public int GetLevel()
    {
        return _level;
    }

    public TroopRarity GetRarity()
    {
        return rarity;
    }
}