using System.Collections;
using UnityEngine;

public class VFXManager : MonoBehaviour
{
    public static VFXManager instance;

    public ParticleSystem swordSlash;
    public ParticleSystem hitSword;
    public ParticleSystem takeDamageEnemy;
    public ParticleSystem enemyDie;
    public ParticleSystem arrowHit;
    public ParticleSystem playerDamage;

    private void Awake()
    {
        instance = this;
    }

    public void SwordSlash(Vector3 pos)
    {
        Play(swordSlash, pos);
    }
    public void EnemyDamage(Vector3 pos)
    {
        Play(takeDamageEnemy, pos);
    }
    public void SwordHit(Vector3 pos)
    {
        Play(hitSword, pos);
        CamShake();
    }
    public void EnemyDie(Vector3 pos)
    {
        Play(enemyDie, pos);
    }
    public void ArrowHit(Vector3 pos)
    {
        Play(arrowHit, pos);
    }
    public void PlayerDamage(Vector3 pos)
    {
        Play(playerDamage, pos);
        CamShake();
    }

    private void Play(ParticleSystem p, Vector3 pos)
    {
        Instantiate(p, pos, Quaternion.identity);
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
