using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class PurchasePanel : MonoBehaviour
{
    public TextMeshProUGUI title;
    public TextMeshProUGUI description;
    public TextMeshProUGUI cost;
    public Image image;
    private ItemData data;

    public GameObject loading;
    public GameObject loaded;

    public GameObject purchasedAds;
    public bool adPanel;

    public void OpenPanel(ItemData _data)
    {
        data = _data;

        title.text = data.title;
        description.text = data.description;
        cost.text = $"${data.cost.ToString()}";
        image.sprite = data.icon;
    }

    public void PurchaseMade()
    {
        StartCoroutine(StartPurchase());
    }
    IEnumerator StartPurchase()
    {
        loading.SetActive(true);

        yield return new WaitForSeconds(3);

        loading.SetActive(false);
        loaded.SetActive(true);

        yield return new WaitForSeconds(2);

        loaded.SetActive(false);

        if (adPanel)
        {
            AdManager.instance.ads = false;
            purchasedAds.SetActive(true);
        }
        else
        {


            if (data.gold)
            {
                EconomyManager.Instance.AddGold(data.reward);
            }
            else if (data.gem)
            {
                EconomyManager.Instance.AddGem(data.reward);
            }
            else
            {
                ShopManager.Instance.AddItem(data);
            }

            ShopManager.Instance.ClosePanel();
        }
    }
}
