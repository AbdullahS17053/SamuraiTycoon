using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class WarZoneSites : MonoBehaviour, IPointerClickHandler
{
    [Header("Warzone Details")]
    public int ID;
    public int Power;
    public int Reward;
    public float Time;
    public bool clear = false;
    public Slider slider;

    public void OnPointerClick(PointerEventData eventData)
    {
        WarManager.instance.OpenWar(ID);
    }

    public void StartWar()
    {
        slider.gameObject.SetActive(true);
        slider.maxValue = 100;
        slider.value = 1;
        slider.DOValue(100, Time).SetEase(Ease.Linear);
    }
    public void WarEnded()
    {
        slider.gameObject.SetActive(false);
    }
}
