using UnityEngine;
using System.Collections;

public class FireZone : MonoBehaviour
{
    private float radius;
    private float duration;
    private int damagePerSecond;
    private float lastDamageTime;
    private bool isActive = true;
    private ParticleSystem[] particleSystems;
    private float vfxHeight;
    
    public void Initialize(float fireRadius, float fireDuration, int damagePerSec, float vfxHeightFromGround)
    {
        radius = fireRadius;
        duration = fireDuration;
        damagePerSecond = damagePerSec;
        lastDamageTime = Time.time;
        vfxHeight = vfxHeightFromGround;
        
        // Position the fire effect on ground with customizable height
        RaycastHit hit;
        if (Physics.Raycast(transform.position + Vector3.up * 0.5f, Vector3.down, out hit, 2f))
        {
            transform.position = hit.point + Vector3.up * vfxHeight;
        }
        
        // Get all particle systems and make them play
        particleSystems = GetComponentsInChildren<ParticleSystem>();
        foreach (ParticleSystem ps in particleSystems)
        {
            var main = ps.main;
            main.simulationSpeed = 1f;
            ps.Play();
        }
        
        Destroy(gameObject, duration);
        StartCoroutine(ShrinkEffect());
    }
    
    void Update()
    {
        if (!isActive) return;
        
        if (Time.time >= lastDamageTime + 1f)
        {
            lastDamageTime = Time.time;
            
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, radius);
            foreach (Collider hit in hitColliders)
            {
                // Damage zombies
                ZombieHealth zombie = hit.GetComponentInParent<ZombieHealth>();
                if (zombie != null && !zombie.IsDead())
                {
                    zombie.TakeDamage(damagePerSecond);
                }
                
                // Damage player
                PlayerHealth player = hit.GetComponentInParent<PlayerHealth>();
                if (player != null)
                {
                    player.TakeDamage(damagePerSecond);
                }
            }
        }
    }
    
    IEnumerator ShrinkEffect()
    {
        float elapsedTime = 0f;
        Vector3 startScale = transform.localScale;
        
        while (elapsedTime < duration && isActive)
        {
            elapsedTime += Time.deltaTime;
            float t = 1f - (elapsedTime / duration);
            transform.localScale = startScale * t;
            
            // Keep fire on ground with customizable height
            RaycastHit hit;
            if (Physics.Raycast(transform.position + Vector3.up * 0.5f, Vector3.down, out hit, 2f))
            {
                transform.position = hit.point + Vector3.up * vfxHeight;
            }
            
            yield return null;
        }
        
        isActive = false;
    }
}