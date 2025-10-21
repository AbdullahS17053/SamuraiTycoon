using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class WarManager : MonoBehaviour
{
    public static WarManager instance;

    public bool isPanning = false;
    public GameObject cam1;
    public GameObject cam2;
    public GameObject WarPanel;
    public GameObject zonePanel;
    public GameObject clearedPanel;
    public GameObject warOnButton;
    public GameObject warWonButton;
    public GameObject warLostButton;
    public TextMeshProUGUI textRewardGot;
    public TextMeshProUGUI textLostPower;
    public TextMeshProUGUI textLostTroops;
    public TextMeshProUGUI textPower;
    public TextMeshProUGUI textCurrentPower;
    public TextMeshProUGUI textReward;
    public TextMeshProUGUI textTime;
    public bool onWar;
    private WarZoneSites currentWarSite;
    private WarZoneSites currentOnWar;



    public GameObject UI;
    public TextMeshProUGUI warPower;
    public TextMeshProUGUI warTroops;
    public int warPowerStore;
    public int warTroopsStore;
    public int onWarTroops;
    public int onWarPower;

    [System.Serializable]
    public class TroopData
    {
        public int level;
        public int power;
    }

    private List<TroopData> troops = new List<TroopData>();
    public List<WarZoneSites> warZones = new List<WarZoneSites>();

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


    public void OpenWarPanelGo()
    {
        cam1.SetActive(false);
        cam2.SetActive(true);
        WarPanel.SetActive(true);

    }
    public void CloseWarPanelGo()
    {
        cam2.SetActive(false);
        WarPanel.SetActive(false);
        cam1.SetActive(true);
    }

    public void OpenWar(int ID)
    {

        if (onWar)
        {
            warOnButton.SetActive(true);
        }
        else
        {
            warOnButton.SetActive(false);
        }

            foreach (WarZoneSites war in warZones)
            {
                if (war.ID == ID)
                {
                clearedPanel.SetActive(war.clear);
                currentWarSite = war;
                zonePanel.SetActive(true);
                textPower.text = war.Power.ToString();
                textReward.text = war.Reward.ToString();
                textCurrentPower.text = warPowerStore.ToString();
                textTime.text = war.Time.ToString();

                break;
                }
            }
    }


    public void SendToWar()
    {
        currentOnWar = currentWarSite;
        onWar = true;

        onWarTroops = warTroopsStore;
        onWarPower = warPowerStore;

        warTroopsStore = 0;
        warPowerStore = 0;

        StartCoroutine(GoneOnWar());
        AdManager.instance.OpenAd();
    }

    IEnumerator GoneOnWar()
    {
        bool won = false;

        if(currentOnWar.Power < onWarPower)
        {
            won = true;
        }

        currentOnWar.StartWar();
        zonePanel.SetActive(false);

        yield return new WaitForSeconds(currentOnWar.Time);

        WarEnds(!won);
    }

    public void WarEnds(bool lost)
    {
        if (lost)
        {
            warPowerStore -= currentOnWar.Power;
            warTroopsStore -= (int)(warTroopsStore * 0.6f);

            textLostPower.text = currentOnWar.Power.ToString();
            textLostTroops.text = (warTroopsStore * 0.6f).ToString();
            warLostButton.SetActive(true);

            currentOnWar.WarEnded();
            onWar = false;
        }
        else
        {
            warWonButton.SetActive(true);
            textRewardGot.text = currentOnWar.Reward.ToString();
            currentOnWar.clear = true;
        }
    }

    public void CollectReward()
    {
        warWonButton.SetActive(false);

        EconomyManager.Instance.AddGold(currentOnWar.Reward);

        warPowerStore += onWarPower;
        warTroopsStore += onWarTroops;

        currentOnWar.WarEnded();
        onWar = false;
    }
}
