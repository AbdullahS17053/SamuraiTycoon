using UnityEngine;

public class LookAtCamera : MonoBehaviour
{
    private Camera cam;

    void Start()
    {
        cam = Camera.main;
    }

    void LateUpdate()
    {
        transform.LookAt(cam.transform);
    }
}
