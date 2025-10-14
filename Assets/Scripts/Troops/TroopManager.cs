using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Collections;

public class TroopManager : MonoBehaviour
{
    [Header("Troop Configuration")]
    public GameObject troopPrefab;
    public Transform troopContainer;
    public Transform spawnPosition;
    public TrainingBuilding gate;

    [Header("Troop Spawning")]
    public float spawnInterval = 60f;
    public int troopsPerSpawn = 1;

    private void Start()
    {
        StartCoroutine(TroopSpawn());
    }

    IEnumerator TroopSpawn()
    {
        for (; ;)
        {
            if(gate.waitingQueue.Count < gate.maxWait)
            {
                if (troopsPerSpawn > 1)
                {
                    for (int i = 0; i < troopsPerSpawn; i++)
                    {
                        Instantiate(troopPrefab, spawnPosition.position, Quaternion.identity, troopContainer);
                    }
                }
                else
                {
                    Instantiate(troopPrefab, spawnPosition.position, Quaternion.identity, troopContainer);
                }
            }
                yield return new WaitForSeconds(spawnInterval);
        }
    }

    public void CreateNewTroop()
    {
        GameObject troopObj = Instantiate(troopPrefab, troopContainer);
        TroopUnit troop = troopObj.GetComponent<TroopUnit>();
    }
}