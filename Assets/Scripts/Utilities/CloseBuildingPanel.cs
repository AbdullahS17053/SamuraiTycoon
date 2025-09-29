using UnityEngine;
using UnityEngine.UI;

public class CloseBuildingPanel : MonoBehaviour
{
    void Start()
    {
        GetComponent<Button>().onClick.AddListener(OnCloseClicked);
    }

    void OnCloseClicked()
    {
        BuildingManager.Instance.HideBuildingPanel();
    }
}