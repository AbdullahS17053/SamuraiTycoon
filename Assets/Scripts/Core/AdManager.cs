using UnityEngine;

public class AdManager : MonoBehaviour
{
    public static AdManager instance;

    public bool ads = true;

    private void Awake()
    {
        instance = this;
    }
}
