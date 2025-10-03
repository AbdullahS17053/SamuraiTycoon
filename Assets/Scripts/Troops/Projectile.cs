using Unity.VisualScripting;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float speed = 10f;
    public float damage = 10f;
    public GameObject impactEffect;

    private Transform _target;
    private Vector3 _lastTargetPosition;

    public void Initialize(Transform target, float projectileDamage)
    {
        _target = target;
        damage = projectileDamage;
        _lastTargetPosition = target.position;
    }

    void Update()
    {
        if (_target != null)
        {
            _lastTargetPosition = _target.position;
        }

        Vector3 direction = (_lastTargetPosition - transform.position).normalized;
        float distanceThisFrame = speed * Time.deltaTime;

        // Check if we hit the target
        if (Vector3.Distance(transform.position, _lastTargetPosition) <= distanceThisFrame)
        {
            HitTarget();
            return;
        }

        // Move towards target
        transform.Translate(direction * distanceThisFrame, Space.World);
        transform.LookAt(_lastTargetPosition);
    }

    void HitTarget()
    {
        // Show impact effect
        if (impactEffect != null)
        {
            Instantiate(impactEffect, transform.position, transform.rotation);
        }

        // Apply damage to target if it still exists
        if (_target != null)
        {
            TroopUnit targetTroop = _target.GetComponent<TroopUnit>();
            if (targetTroop != null)
            {
                targetTroop.TakeDamage(damage);
            }
        }

        Destroy(gameObject);
    }
}