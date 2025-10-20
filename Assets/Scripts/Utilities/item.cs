using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class item : MonoBehaviour
{
    public TextMeshProUGUI title;
    public TextMeshProUGUI level;
    public Image image;
    private ItemData data;

    public void OpenPanel(ItemData _data)
    {
        data = _data;

        title.text = data.title;
        level.text = data.level;
        image.sprite = data.icon;
    }
}
