using System.Collections.Generic;
using UnityEngine;

public class TroopManager : MonoBehaviour
{
    public static TroopManager instance;    

    [Header("Troop Configuration")]
    public GameObject troopPrefab;
    public Transform troopContainer;
    public Transform spawnPosition;
    public TrainingBuilding gate;

    [Header("Troop Spawning")]
    public float spawnInterval = 60f;
    public int troopsPerSpawn = 1;
    private int troopCount = 0;

    // OPTIMIZED: Object pooling
    private Queue<GameObject> troopPool = new Queue<GameObject>();
    private List<GameObject> activeTroops = new List<GameObject>();
    private const int INITIAL_POOL_SIZE = 65;

    private Coroutine spawnCoroutine;

    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        InitializePool();
        spawnCoroutine = StartCoroutine(TroopSpawn());
    }

    void InitializePool()
    {
        for (int i = 0; i < INITIAL_POOL_SIZE; i++)
        {
            CreatePooledTroop();
        }
    }

    void CreatePooledTroop()
    {
        GameObject troopObj = Instantiate(troopPrefab, troopContainer);
        TroopUnit troop = troopObj.GetComponent<TroopUnit>();
        troop.troopId = troopCount++;
        troopObj.SetActive(false);
        troopPool.Enqueue(troopObj);
    }

    System.Collections.IEnumerator TroopSpawn()
    {
        while (true)
        {
            if (gate.waitingQueue.Count < gate.maxWait / 2)
            {
                int spawnCount = Mathf.Min(troopsPerSpawn, gate.maxWait - gate.waitingQueue.Count);

                for (int i = 0; i < spawnCount; i++)
                {
                    CreateNewTroop();
                }
            }
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    public void CreateNewTroop()
    {
        if (troopPool.Count == 0)
        {
            CreatePooledTroop();
        }

        GameObject troopObj = troopPool.Dequeue();
        troopObj.transform.position = spawnPosition.position;
        troopObj.SetActive(true);
        activeTroops.Add(troopObj);

        TroopUnit troop = troopObj.GetComponent<TroopUnit>();
        troop.troopLevel = -1;
        troop.currentPower = 10;
        troop.troopId = troopCount++;
        troop.Reset();
    }

    // OPTIMIZED: Return troop to pool
    public void ReturnTroopToPool(GameObject troopObj)
    {
        troopObj.SetActive(false);
        troopPool.Enqueue(troopObj);
        activeTroops.Remove(troopObj);
    }

    void OnDestroy()
    {
        if (spawnCoroutine != null)
            StopCoroutine(spawnCoroutine);
    }
}