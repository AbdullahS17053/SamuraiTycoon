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
    private int troopCount;

    private void Start()
    {
        StartCoroutine(TroopSpawn());
    }

    IEnumerator TroopSpawn()
    {
        for (; ;)
        {
            if(gate.waitingQueue.Count < gate.maxWait/2)
            {
                if (troopsPerSpawn > 1)
                {
                    for (int i = 0; i < troopsPerSpawn; i++)
                    {
                        CreateNewTroop();
                    }
                }
                else
                {
                    CreateNewTroop();
                }
            }
                yield return new WaitForSeconds(spawnInterval);
        }
    }

    public void CreateNewTroop()
    {
        GameObject troopObj = Instantiate(troopPrefab, spawnPosition.position, Quaternion.identity, troopContainer);
        TroopUnit troop = troopObj.GetComponent<TroopUnit>();
        troop.troopId = troopCount;
        troopCount++; 
    }
}