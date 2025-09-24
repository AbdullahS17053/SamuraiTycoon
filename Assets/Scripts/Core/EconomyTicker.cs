using UnityEngine;

public class EconomyTicker : MonoBehaviour
{
    void Update()
    {
        // Tick economy once per second
        GameManager.Instance.Economy.Tick(Time.deltaTime);
    }
}