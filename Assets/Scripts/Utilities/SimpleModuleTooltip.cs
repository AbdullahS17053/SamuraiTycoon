using UnityEngine;
using UnityEngine.EventSystems;

public class SimpleModuleTooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private string _title;
    private string _description;

    public void SetTooltip(string title, string description)
    {
        _title = title;
        _description = description;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // Show tooltip in console for now
        Debug.Log($"🔍 {_title}: {_description}");
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // Tooltip hidden
    }
}