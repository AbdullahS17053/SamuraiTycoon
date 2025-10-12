using TMPro;
using UnityEngine;

[System.Serializable]
public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    private void Awake()
    {
        instance = this;
    }
}