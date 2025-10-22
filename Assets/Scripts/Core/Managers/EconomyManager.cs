using System;
using TMPro;
using UnityEngine;

[System.Serializable]
public class EconomyManager : MonoBehaviour
{
    public static EconomyManager Instance;

    public int Gold;
    public int Gem;

    public TextMeshProUGUI goldText;
    public TextMeshProUGUI gemText;

    private void Awake()
    {
        Instance = this;
        goldText.text = Gold.ToString();
        gemText.text = Gem.ToString();
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
    public void AddGem(int amount)
    {
        Gem += amount;

        gemText.text = Gem.ToString();
    }

    public bool SpendGem(int amount)
    {
        if (Gem >= amount)
        {
            Gem -= amount;

            gemText.text = Gem.ToString();

            return true;
        }
        else
        {
            return false;
        }
    }
}