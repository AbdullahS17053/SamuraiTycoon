using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class VFXManager : MonoBehaviour
{
    public static VFXManager instance;

    public Canvas uiCanvas;

    public ParticleSystem trainingDone;
    public ParticleSystem buildingupgrade;
    public ParticleSystem buildingTrained;
    public ParticleSystem income;
    public ParticleSystem speed;
    public ParticleSystem capacity;

    private void Awake()
    {
        instance = this;
    }

    public void Trained(Vector3 pos)
    {
        Play(trainingDone, pos);
    }
    public void BuildingUpgrade(Vector3 pos)
    {
        Play(buildingupgrade, new Vector3(pos.x, 0.2f, pos.z));
    }
    public void BuildingTrained(Vector3 pos)
    {
        Play(buildingTrained, pos);
    }
    public void Income()
    {
        PlayUI(income);
    }
    public void Speed()
    {
        PlayUI(speed);
    }
    public void Capacity()
    {
        PlayUI(capacity);
    }

    private void Play(ParticleSystem p, Vector3 pos)
    {
        Instantiate(p, pos, p.transform.rotation);
    }

    private void PlayUI(ParticleSystem psPrefab)
    {
        
    }

    private void CamShake()
    {
        StartCoroutine(Shake(0.2f, 0.05f));
    }

    IEnumerator Shake(float duration, float magnitude)
    {
        Camera cam = Camera.main;
        Vector3 originalPos = cam.transform.localPosition;

        float elapsed = 0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            cam.transform.localPosition = originalPos + new Vector3(x, y, 0);

            elapsed += Time.deltaTime;
            yield return null;
        }

        cam.transform.localPosition = new Vector3(0,0, -10);
    }
}
