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

    private void Awake()
    {
        instance = this;
    }

    public void OpenWarPanel()
    {
        UI.SetActive(true);
        warPower.text = warPowerStore.ToString();
        warTroops.text = warTroopsStore.ToString();
    }

    public void AddTroop(int power)
    {
        warPowerStore += power;
        warTroopsStore++;
    }

    public void SendToWar()
    {

    }
}
