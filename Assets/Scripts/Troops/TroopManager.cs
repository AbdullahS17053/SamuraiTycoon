using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TroopManager : MonoBehaviour
{
    public static TroopManager Instance { get; private set; }

    [Header("Troop Configuration")]
    public GameObject troopPrefab;
    public Transform troopContainer;
    public int baseMaxTroops = 20;

    [Header("Training Configuration")]
    public double baseTroopCost = 100.0;
    public float troopCostMultiplier = 1.15f;
    public float autoTrainInterval = 30.0f;

    [Header("Training Areas")]
    public List<Transform> trainingAreas = new List<Transform>();
    public Vector3 patrolBounds = new Vector3(50, 0, 50);

    [Header("Troop Types")]
    public TroopTypeConfig[] troopTypes;

    private List<TroopUnit> _troops = new List<TroopUnit>();
    private TroopUnit _selectedTroop;
    private double _totalTroopIncome = 0;
    private EconomyManager _economy;
    private ZoneManager _zoneManager;
    private float _autoTrainTimer = 0f;
    private int _troopsTrainedThisSession = 0;
    private bool _isInitialized = false;

    [System.Serializable]
    public class TroopTypeConfig
    {
        public TroopUnit.TroopType type;
        public string displayName;
        public double baseIncome;
        public float trainingTime;
        public GameObject prefab;
        public Color troopColor;
    }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void Initialize(GameData data, EconomyManager economy)
    {
        _economy = economy;
        _zoneManager = FindObjectOfType<ZoneManager>();

        // Load saved troops or create initial troops
        InitializeTroops();

        _isInitialized = true;
        Debug.Log($"🎖️ TroopManager initialized with {_troops.Count} troops");
    }

    public void ResetAllTroops()
    {
        foreach (var troop in _troops)
        {
            if (troop != null)
            {
                Destroy(troop.gameObject);
            }
        }
        _troops.Clear();
        _troopsTrainedThisSession = 0;

        // Reinitialize troops
        InitializeTroops();
        Debug.Log("🔄 All troops reset and reinitialized");
    }

    public void ReinitializeTroops()
    {
        InitializeTroops();
    }

    // Make InitializeTroops protected instead of private
    protected void InitializeTroops()
    {
        // Clear existing troops first
        foreach (var troop in _troops)
        {
            if (troop != null)
            {
                Destroy(troop.gameObject);
            }
        }
        _troops.Clear();

        // Create initial troops
        for (int i = 0; i < 3; i++)
        {
            CreateNewTroop(GetRandomTroopType(), GetRandomRarity());
        }

        Debug.Log($"🎖️ Troops reinitialized with {_troops.Count} troops");
    }

    public void CreateNewTroop(TroopUnit.TroopType type, TroopUnit.TroopRarity rarity)
    {
        if (!_isInitialized) return;

        if (GetTotalTroops() >= GetMaxTroops())
        {
            Debug.Log("❌ Maximum troop capacity reached!");
            return;
        }

        var typeConfig = troopTypes.FirstOrDefault(t => t.type == type);
        if (typeConfig == null || typeConfig.prefab == null)
        {
            Debug.LogError($"❌ No configuration found for troop type: {type}");
            return;
        }

        GameObject troopObj = Instantiate(typeConfig.prefab, troopContainer);
        TroopUnit troop = troopObj.GetComponent<TroopUnit>();

        if (troop != null)
        {
            string troopId = "troop_" + System.Guid.NewGuid().ToString().Substring(0, 8);
            string troopName = $"{typeConfig.displayName} {GetNextTroopNumber(type)}";

            troop.Initialize(troopId, troopName, type, rarity, this);

            // Position in random training area
            PositionTroop(troop);

            _troops.Add(troop);
            _troopsTrainedThisSession++;

            Debug.Log($"🎖️ New {rarity} {typeConfig.displayName} created: {troopName}");
        }
    }

    void PositionTroop(TroopUnit troop)
    {
        if (trainingAreas.Count > 0)
        {
            Transform randomArea = trainingAreas[Random.Range(0, trainingAreas.Count)];
            Vector3 randomPos = randomArea.position + new Vector3(
                Random.Range(-8f, 8f),
                0,
                Random.Range(-8f, 8f)
            );
            troop.transform.position = randomPos;
            troop.SetTargetPosition(randomPos);
        }
    }

    public void TrainTroop(TroopUnit troop)
    {
        if (troop != null && !troop.IsTraining())
        {
            troop.StartTraining();
        }
    }

    public void TrainAllTroops()
    {
        foreach (var troop in _troops)
        {
            if (!troop.IsTraining())
            {
                TrainTroop(troop);
            }
        }
    }

    public void OnTroopTrained(TroopUnit troop)
    {
        if (!_isInitialized) return;

        // Add income from trained troop
        double income = troop.GetCurrentIncome();
        _totalTroopIncome += income;

        // Visual feedback
        ShowIncomePopup(troop.transform.position, income);

        Debug.Log($"💰 {troop.troopName} now generates {income}/s");
    }

    public void PurchaseNewTroop()
    {
        if (!_isInitialized || _economy == null) return;

        double cost = GetNextTroopCost();

        if (_economy.SpendGold(cost))
        {
            CreateNewTroop(GetRandomTroopType(), GetRandomRarity());
            Debug.Log($"🎖️ Purchased new troop for {cost} gold");
        }
        else
        {
            Debug.Log("❌ Not enough gold to purchase new troop!");
        }
    }

    public void SelectTroop(TroopUnit troop)
    {
        // Deselect previous troop
        if (_selectedTroop != null)
        {
            _selectedTroop.SetSelected(false);
        }

        _selectedTroop = troop;

        if (_selectedTroop != null)
        {
            _selectedTroop.SetSelected(true);
            Debug.Log($"🎯 Selected troop: {_selectedTroop.troopName} Lvl {_selectedTroop.GetLevel()}");
        }
    }

    public void MoveSelectedTroop(Vector3 position)
    {
        if (_selectedTroop != null)
        {
            _selectedTroop.SetTargetPosition(position);
        }
    }

    public Vector3 GetRandomPatrolPoint()
    {
        Vector3 center = Vector3.zero;
        if (trainingAreas.Count > 0)
        {
            center = trainingAreas[Random.Range(0, trainingAreas.Count)].position;
        }

        return center + new Vector3(
            Random.Range(-patrolBounds.x, patrolBounds.x),
            0,
            Random.Range(-patrolBounds.z, patrolBounds.z)
        );
    }

    void ShowIncomePopup(Vector3 position, double amount)
    {
        // You can implement a floating text system here
        Debug.Log($"💰 +{amount} from troop at {position}");
    }

    public void Tick(float deltaTime)
    {
        if (!_isInitialized) return;

        // Handle troop movement input
        if (Input.GetMouseButtonDown(1) && _selectedTroop != null) // Right click
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                MoveSelectedTroop(hit.point);
            }
        }

        // Auto-train troops periodically
        _autoTrainTimer += deltaTime;
        if (_autoTrainTimer >= autoTrainInterval)
        {
            AutoTrainIdleTroops();
            _autoTrainTimer = 0f;
        }

        // Generate passive income from non-training troops
        GeneratePassiveIncome();
    }

    void AutoTrainIdleTroops()
    {
        int trained = 0;
        foreach (var troop in _troops)
        {
            if (!troop.IsTraining())
            {
                TrainTroop(troop);
                trained++;
            }
        }

        if (trained > 0)
        {
            Debug.Log($"🤖 Auto-trained {trained} idle troops");
        }
    }

    void GeneratePassiveIncome()
    {
        if (!_isInitialized || _economy == null) return;

        double passiveIncome = 0;

        foreach (var troop in _troops)
        {
            if (!troop.IsTraining())
            {
                // Apply zone multiplier
                double troopIncome = troop.GetCurrentIncome() * Time.deltaTime;
                if (_zoneManager != null)
                {
                    troopIncome *= _zoneManager.GetZoneIncomeMultiplier();
                }
                passiveIncome += troopIncome;
            }
        }

        if (passiveIncome > 0)
        {
            _economy.AddGold(passiveIncome);
        }
    }


    // Helper methods
    TroopUnit.TroopType GetRandomTroopType()
    {
        var availableTypes = System.Enum.GetValues(typeof(TroopUnit.TroopType));
        return (TroopUnit.TroopType)availableTypes.GetValue(Random.Range(0, availableTypes.Length));
    }

    TroopUnit.TroopRarity GetRandomRarity()
    {
        float rand = Random.Range(0f, 1f);
        if (rand < 0.01f) return TroopUnit.TroopRarity.Legendary;      // 1%
        else if (rand < 0.05f) return TroopUnit.TroopRarity.Epic;      // 4%
        else if (rand < 0.15f) return TroopUnit.TroopRarity.Rare;      // 10%
        else if (rand < 0.40f) return TroopUnit.TroopRarity.Uncommon;  // 25%
        else return TroopUnit.TroopRarity.Common;                      // 60%
    }

    int GetNextTroopNumber(TroopUnit.TroopType type)
    {
        return _troops.Count(t => t.troopType == type) + 1;
    }

    public double GetNextTroopCost()
    {
        return baseTroopCost * Mathf.Pow(troopCostMultiplier, _troopsTrainedThisSession);
    }

    public int GetTotalTroops()
    {
        return _troops.Count;
    }

    public int GetMaxTroops()
    {
        int baseMax = baseMaxTroops;
        if (_zoneManager != null)
        {
            baseMax += _zoneManager.GetExtraTroopCapacity();
        }
        return baseMax;
    }

    public int GetActiveTroops()
    {
        return _troops.Count(t => !t.IsTraining());
    }

    public double GetTotalTroopIncome()
    {
        return _totalTroopIncome;
    }

    public int GetTroopsTraining()
    {
        return _troops.Count(t => t.IsTraining());
    }

    [ContextMenu("Create Test Troop")]
    public void CreateTestTroop()
    {
        CreateNewTroop(GetRandomTroopType(), GetRandomRarity());
    }

    [ContextMenu("Train All Troops")]
    public void TrainAllTroopsCommand()
    {
        TrainAllTroops();
    }

    [ContextMenu("Purchase Troop")]
    public void PurchaseTroopCommand()
    {
        PurchaseNewTroop();
    }
}