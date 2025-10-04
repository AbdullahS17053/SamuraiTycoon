using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Collections;

public class TroopManager : MonoBehaviour
{
    public static TroopManager Instance { get; private set; }

    [Header("Troop Configuration")]
    public GameObject troopPrefab;
    public Transform troopContainer;
    public int baseMaxTroops = 20;

    [Header("Training & Barracks")]
    public Transform barracksPosition;
    public Transform castleStoragePoint;

    [Header("Troop Spawning")]
    public float spawnInterval = 60f;
    public int troopsPerSpawn = 1;
    public bool autoSpawnEnabled = true;

    [Header("Troop Types")]
    public TroopTypeConfig[] troopTypes;

    [Header("Income Settings")]
    public float incomePerTraining = 25f;
    public float incomeMultiplierByRarity = 1.5f;

    private List<TroopUnit> allTroops = new List<TroopUnit>();
    private List<TroopUnit> castleTroops = new List<TroopUnit>();
    private List<TrainingBuilding> trainingBuildings = new List<TrainingBuilding>();
    private TroopUnit selectedTroop;
    private EconomyManager economy;
    private BuildingManager3D buildingManager;
    private Coroutine spawnCoroutine;

    [System.Serializable]
    public class TroopTypeConfig
    {
        public TroopUnit.TroopType type;
        public string displayName;
        public float baseTrainingTime;
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

    void Start()
    {
        economy = GameManager.Instance != null ? GameManager.Instance.Economy : null;
        buildingManager = BuildingManager3D.Instance;

        RefreshTrainingBuildings();

        if (autoSpawnEnabled)
        {
            StartAutoSpawning();
        }

        Debug.Log($"⚔️ TroopManager initialized with {trainingBuildings.Count} training buildings");
    }

    public void Initialize()
    {
        Debug.Log("⚔️ TroopManager initialized via GameManager");
    }

    public void Tick(float deltaTime)
    {
        // Update logic if needed
    }

    // CRITICAL FIX: Get training buildings in proper order
    public List<TrainingBuilding> GetAllTrainingBuildingsInOrder()
    {
        if (trainingBuildings == null || trainingBuildings.Count == 0)
        {
            Debug.LogWarning("⚠️ No training buildings found!");
            return new List<TrainingBuilding>();
        }

        // Sort by building order, then by name as fallback
        var orderedBuildings = trainingBuildings
            .Where(b => b != null && b.isUnlocked)
            .OrderBy(b => b.buildingOrder)
            .ThenBy(b => b.displayName)
            .ToList();

        Debug.Log($"🏗️ Found {orderedBuildings.Count} unlocked training buildings in order: {string.Join(", ", orderedBuildings.Select(b => $"{b.displayName}({b.buildingOrder})"))}");

        return orderedBuildings;
    }

    public void RefreshTrainingBuildings()
    {
        trainingBuildings.Clear();

        // Find all TrainingBuilding components in the scene
        TrainingBuilding[] sceneBuildings = FindObjectsOfType<TrainingBuilding>();
        foreach (TrainingBuilding building in sceneBuildings)
        {
            if (!trainingBuildings.Contains(building))
            {
                trainingBuildings.Add(building);
                Debug.Log($"🏗️ Found training building: {building.displayName} (Order: {building.buildingOrder}, Unlocked: {building.isUnlocked})");
            }
        }

        // Also check BuildingManager if it exists
        if (BuildingManager3D.Instance != null)
        {
            foreach (var buildingObj in BuildingManager3D.Instance.BuildingObjects.Values)
            {
                if (buildingObj != null)
                {
                    TrainingBuilding trainingBuilding = buildingObj.GetComponent<TrainingBuilding>();
                    if (trainingBuilding != null && !trainingBuildings.Contains(trainingBuilding))
                    {
                        trainingBuildings.Add(trainingBuilding);
                        Debug.Log($"🏗️ Found training building from BuildingManager: {trainingBuilding.displayName}");
                    }
                }
            }
        }

        Debug.Log($"🔧 Refreshed training buildings: {trainingBuildings.Count} total found");
    }

    // Auto-spawning system
    public void StartAutoSpawning()
    {
        if (spawnCoroutine != null)
            StopCoroutine(spawnCoroutine);

        spawnCoroutine = StartCoroutine(SpawnRoutine());
    }

    IEnumerator SpawnRoutine()
    {
        while (autoSpawnEnabled)
        {
            yield return new WaitForSeconds(spawnInterval);

            for (int i = 0; i < troopsPerSpawn; i++)
            {
                if (GetTotalTroops() < GetMaxTroops())
                {
                    CreateNewTroop(GetRandomTroopType(), GetRandomRarity());
                }
                else
                {
                    Debug.Log("⏹️ Max troop capacity reached, cannot spawn more");
                }
            }
        }
    }

    public TroopUnit CreateNewTroop(TroopUnit.TroopType type, TroopUnit.TroopRarity rarity)
    {
        if (GetTotalTroops() >= GetMaxTroops())
        {
            Debug.Log("❌ Maximum troop capacity reached!");
            return null;
        }

        var typeConfig = troopTypes.FirstOrDefault(t => t.type == type);
        if (typeConfig == null || typeConfig.prefab == null)
        {
            Debug.LogError($"❌ No configuration or prefab found for troop type: {type}");
            return null;
        }

        GameObject troopObj = Instantiate(typeConfig.prefab, troopContainer);
        TroopUnit troop = troopObj.GetComponent<TroopUnit>();
        if (troop != null)
        {
            string troopId = "troop_" + System.Guid.NewGuid().ToString().Substring(0, 8);
            string troopName = $"{typeConfig.displayName} {GetNextTroopNumber(type)}";
            troop.Initialize(troopId, troopName, type, rarity, this);
            allTroops.Add(troop);

            PositionTroopAtBarracks(troop);
            troop.StartTrainingProgression();

            Debug.Log($"✅ New {rarity} {typeConfig.displayName} created: {troopName}");
            return troop;
        }

        Debug.LogError("❌ Failed to get TroopUnit component from prefab");
        Destroy(troopObj);
        return null;
    }

    // One-time income when troop completes training
    public void OnTroopTrained(TroopUnit troop, float powerIncrease)
    {
        if (economy != null)
        {
            float income = incomePerTraining;

            switch (troop.rarity)
            {
                case TroopUnit.TroopRarity.Uncommon: income *= 1.2f; break;
                case TroopUnit.TroopRarity.Rare: income *= 1.5f; break;
                case TroopUnit.TroopRarity.Epic: income *= 2f; break;
                case TroopUnit.TroopRarity.Legendary: income *= 3f; break;
            }

            income += powerIncrease * 2f;

            economy.AddGold(income);
            Debug.Log($"💰 Earned {income} gold from {troop.troopName} training completion");
        }
    }

    public Vector3 GetBarracksPosition()
    {
        return barracksPosition != null ? barracksPosition.position : Vector3.zero;
    }

    public Vector3 GetCastlePosition()
    {
        return castleStoragePoint != null ? castleStoragePoint.position : Vector3.zero;
    }

    public void StoreTroopInCastle(TroopUnit troop)
    {
        if (troop != null && !castleTroops.Contains(troop))
        {
            castleTroops.Add(troop);
            troop.transform.position = GetCastlePosition();
            troop.gameObject.SetActive(false);

            Debug.Log($"🏰 Troop {troop.troopName} stored in castle. Total castle troops: {castleTroops.Count}");
        }
    }

    // PRESTIGE METHODS
    public void ResetAllTroops()
    {
        Debug.Log("🔄 Resetting all troops for prestige...");

        // Clear castle troops
        foreach (var troop in castleTroops.ToList())
        {
            if (troop != null)
            {
                Destroy(troop.gameObject);
            }
        }
        castleTroops.Clear();

        // Reset all active troops
        foreach (var troop in allTroops.ToList())
        {
            if (troop != null)
            {
                troop.CancelTraining();
                troop.currentState = TroopUnit.TroopState.Idle;
                troop.currentBuildingIndex = 0;
                troop.trainingPowerBonus = 0;
                troop.currentPower = troop.basePower;
                troop.trainingBuildingsCompleted = 0;

                troop.transform.position = GetBarracksPosition() + new Vector3(
                    Random.Range(-3f, 3f),
                    0,
                    Random.Range(-3f, 3f)
                );

                if (troop.navAgent != null)
                {
                    troop.navAgent.ResetPath();
                }

                troop.gameObject.SetActive(true);
                troop.StartTrainingProgression();
            }
        }

        // Clear training building queues
        foreach (var building in trainingBuildings)
        {
            if (building != null)
            {
                building.ReleaseTroop();
                building.trainingQueue.Clear();
            }
        }

        Debug.Log($"🔄 Reset {allTroops.Count} troops for prestige");
    }

    public void ReinitializeTroops()
    {
        Debug.Log("🔁 Reinitializing troops after soft reset...");

        // Keep castle troops but reset their state
        foreach (var troop in castleTroops.ToList())
        {
            if (troop != null)
            {
                troop.gameObject.SetActive(true);
                troop.currentState = TroopUnit.TroopState.Idle;
                troop.StartTrainingProgression();
            }
        }
        castleTroops.Clear();

        // Reset all active troops but keep their power and progression
        foreach (var troop in allTroops)
        {
            if (troop != null && troop.currentState != TroopUnit.TroopState.InCastle)
            {
                troop.currentState = TroopUnit.TroopState.Idle;
                troop.CancelTraining();
                troop.transform.position = GetBarracksPosition() + new Vector3(
                    Random.Range(-3f, 3f),
                    0,
                    Random.Range(-3f, 3f)
                );

                if (troop.navAgent != null)
                {
                    troop.navAgent.ResetPath();
                }
                troop.FindNextTrainingBuilding();
            }
        }

        foreach (var building in trainingBuildings)
        {
            if (building != null)
            {
                building.trainingQueue.Clear();
            }
        }

        Debug.Log($"🔁 Reinitialized {allTroops.Count} troops (soft reset)");
    }

    // Helper methods
    private void PositionTroopAtBarracks(TroopUnit troop)
    {
        Vector3 randomOffset = new Vector3(
            Random.Range(-3f, 3f),
            0,
            Random.Range(-3f, 3f)
        );
        troop.transform.position = GetBarracksPosition() + randomOffset;
    }

    private TroopUnit.TroopType GetRandomTroopType()
    {
        if (troopTypes == null || troopTypes.Length == 0)
        {
            Debug.LogError("❌ No troop types configured!");
            return TroopUnit.TroopType.Samurai;
        }

        var availableTypes = troopTypes.Where(t => t.prefab != null).Select(t => t.type).ToArray();
        if (availableTypes.Length == 0)
        {
            Debug.LogError("❌ No valid troop types with prefabs found!");
            return TroopUnit.TroopType.Samurai;
        }

        return availableTypes[Random.Range(0, availableTypes.Length)];
    }

    private TroopUnit.TroopRarity GetRandomRarity()
    {
        float rand = Random.Range(0f, 1f);
        if (rand < 0.01f) return TroopUnit.TroopRarity.Legendary;
        else if (rand < 0.05f) return TroopUnit.TroopRarity.Epic;
        else if (rand < 0.15f) return TroopUnit.TroopRarity.Rare;
        else if (rand < 0.40f) return TroopUnit.TroopRarity.Uncommon;
        else return TroopUnit.TroopRarity.Common;
    }

    private int GetNextTroopNumber(TroopUnit.TroopType type)
    {
        return allTroops.Count(t => t.troopType == type) + 1;
    }

    public int GetTotalTroops() => allTroops.Count;
    public int GetReadyTroops() => castleTroops.Count;
    public int GetTrainingTroops() => allTroops.Count(t => t.IsTraining());
    public int GetMaxTroops() => baseMaxTroops;

    [ContextMenu("Spawn Test Troop")]
    public void SpawnTestTroop()
    {
        CreateNewTroop(GetRandomTroopType(), GetRandomRarity());
    }

    [ContextMenu("Debug All Troops Status")]
    public void DebugAllTroops()
    {
        Debug.Log("=== TROOP STATUS ===");
        foreach (var troop in allTroops)
        {
            if (troop != null)
            {
                Debug.Log(troop.GetDebugStatus());
            }
        }
        Debug.Log($"Total troops: {allTroops.Count}, In castle: {castleTroops.Count}");
    }

    [ContextMenu("Refresh Buildings")]
    public void RefreshBuildings()
    {
        RefreshTrainingBuildings();
    }

    [ContextMenu("Force All Troops to Next Building")]
    public void ForceAllToNextBuilding()
    {
        foreach (var troop in allTroops)
        {
            if (troop != null && troop.currentState == TroopUnit.TroopState.Training)
            {
                troop.CancelTraining();
                troop.currentBuildingIndex++;
                troop.FindNextTrainingBuilding();
            }
        }
    }
}