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

    [Header("Troop Spawning")]
    public float spawnInterval = 60f;
    public int troopsPerSpawn = 1;

    public void CreateNewTroop()
    {
        GameObject troopObj = Instantiate(troopPrefab, troopContainer);
        TroopUnit troop = troopObj.GetComponent<TroopUnit>();
    }
}