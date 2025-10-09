using System;
using TMPro;
using UnityEngine;

[System.Serializable]
public class EconomyManager : MonoBehaviour
{
    public static EconomyManager Instance;

    public int Gold;

    public TextMeshProUGUI goldText;

    private void Awake()
    {
        Instance = this;
    }


    public void AddGold(int amount)
    {
        Gold += amount;

        goldText.text = Gold.ToString();
    }

    public bool SpendGold(int amount)
    {
        if (Gold >= amount)
        {
            Gold -= amount;

            goldText.text = Gold.ToString();

            return true;
        }
        else
        {
            return false;
        }
    }
}