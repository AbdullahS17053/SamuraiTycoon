using NUnit.Framework;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class WarManager : MonoBehaviour
{
    public static WarManager instance;

    public GameObject UI;
    public TextMeshProUGUI warPower;
    public TextMeshProUGUI warTroops;
    public int warPowerStore;
    public int warTroopsStore;

    [System.Serializable]
    public class TroopData
    {
        public int level;
        public int power;
    }

    private List<TroopData> troops = new List<TroopData>();

    private void Awake()
    {
        instance = this;
    }

    public void OpenWarPanel()
    {
        UI.SetActive(true);
    }

    public void AddTroop(TroopUnit troop)
    {
        warPowerStore += troop.currentPower;
        warTroopsStore++;
        warPower.text = warPowerStore.ToString();
        warTroops.text = warTroopsStore.ToString();

        troops.Add(new TroopData
        {
            level = troop.troopLevel,
            power = troop.currentPower
        });
    }

    public void SendToWar()
    {

    }
}
