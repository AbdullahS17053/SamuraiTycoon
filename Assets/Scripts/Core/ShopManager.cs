using System.Collections.Generic;
using UnityEngine;

public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance;

    public PurchasePanel PurchasePanel;
    public PurchasePanel PurchaseAddPanel;
    public GameObject itemSpawn;
    public Transform itemSpawnArea;

    public List<ItemData> Items;


    private void Awake()
    {
        Instance = this;
    }

    public void OpenPanel(ItemData data)
    {
        PurchasePanel.gameObject.SetActive(true);
        PurchasePanel.OpenPanel(data);
    }
    public void OpenADPanel()
    {
        PurchaseAddPanel.gameObject.SetActive(true);
        PurchaseAddPanel.OpenPanel(new ItemData());
    }
    public void ClosePanel()
    {
        PurchasePanel.gameObject.SetActive(false);
    }

    public void AddItem(ItemData item)
    {
        Items.Add(item);

        item _item;

        _item = Instantiate(itemSpawn, itemSpawnArea).GetComponent<item>();

        _item.OpenPanel(item);
    }
}
