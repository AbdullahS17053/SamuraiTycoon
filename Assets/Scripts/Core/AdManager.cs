using UnityEngine;

public class AdManager : MonoBehaviour
{
    public static AdManager instance;

    public GameObject Ad;

    public bool ads = true;

    private void Awake()
    {
        instance = this;
    }

    public void OpenAd()
    {
        if (ads)
        {
            Ad.SetActive(true);
        }
    }
}
