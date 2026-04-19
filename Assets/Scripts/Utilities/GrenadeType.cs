using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum GrenadeType
{
    Explosive,
    Smoke,
    Fire
}

public class GrenadeTypes : MonoBehaviour
{
    public GrenadeType grenadeType;
    
    [Header("Explosive Grenade")]
    public float explosiveRadius = 5f;
    public int explosiveDamage = 100;
    public GameObject explosionEffect;
    public AudioClip explosionSound;
    
    [Header("Smoke Grenade")]
    public float smokeRadius = 6f;
    public float smokeDuration = 5f;
    public float slowFactor = 0.3f;
    public GameObject smokeEffect;
    
    [Header("Fire Grenade")]
    public float fireRadius = 4f;
    public float fireDuration = 5f;
    public int fireDamagePerSecond = 20;
    public GameObject fireEffect;
    public AudioClip fireSound;
    
    [Header("VFX Height")]
    public float vfxHeightFromGround = 0.1f;
    
    private bool hasExploded = false;
    private GameObject activeSmokeEffect;
    private List<GameObject> activeFireZones = new List<GameObject>();
    
    void Start()
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = transform.forward * 15f;
        }
        Destroy(gameObject, 5f);
    }
    
    void OnCollisionEnter(Collision collision)
    {
        if (!hasExploded)
        {
            hasExploded = true;
            
            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.isKinematic = true;
            }
            
            // Get ground position with customizable height
            Vector3 groundPos = transform.position;
            RaycastHit hit;
            if (Physics.Raycast(transform.position + Vector3.up * 0.5f, Vector3.down, out hit, 2f))
            {
                groundPos = hit.point + Vector3.up * vfxHeightFromGround;
            }
            
            switch (grenadeType)
            {
                case GrenadeType.Explosive:
                    ExplosiveGrenade(groundPos);
                    break;
                case GrenadeType.Smoke:
                    SmokeGrenade(groundPos);
                    break;
                case GrenadeType.Fire:
                    FireGrenade(groundPos);
                    break;
            }
        }
    }
    
    void ExplosiveGrenade(Vector3 groundPos)
    {
        if (explosionSound != null)
            AudioSource.PlayClipAtPoint(explosionSound, groundPos);
        
        if (explosionEffect != null)
        {
            GameObject effect = Instantiate(explosionEffect, groundPos, Quaternion.identity);
            Destroy(effect, 2f);
        }
        
        // Damage zombies
        Collider[] hitColliders = Physics.OverlapSphere(groundPos, explosiveRadius);
        foreach (Collider hit in hitColliders)
        {
            ZombieHealth zombie = hit.GetComponentInParent<ZombieHealth>();
            if (zombie != null && !zombie.IsDead())
            {
                zombie.TakeDamage(explosiveDamage);
            }
            
            // Damage player
            PlayerHealth player = hit.GetComponentInParent<PlayerHealth>();
            if (player != null)
            {
                player.TakeDamage(explosiveDamage);
            }
        }
        
        Destroy(gameObject);
    }
    
    void SmokeGrenade(Vector3 groundPos)
    {
        if (explosionSound != null)
            AudioSource.PlayClipAtPoint(explosionSound, groundPos);
        
        // Spawn smoke effect at ground level
        if (smokeEffect != null)
        {
            activeSmokeEffect = Instantiate(smokeEffect, groundPos, Quaternion.identity);
            activeSmokeEffect.transform.localScale = Vector3.one * smokeRadius;
            activeSmokeEffect.transform.position = groundPos;
            Destroy(activeSmokeEffect, smokeDuration);
        }
        
        // Start smoke effect coroutine
        StartCoroutine(SmokeEffectCoroutine(groundPos));
        
        Destroy(gameObject);
    }
    
    IEnumerator SmokeEffectCoroutine(Vector3 centerPos)
    {
        float elapsedTime = 0f;
        // Store slowed zombies with their original speeds
        Dictionary<ZombieHealth, float> slowedZombies = new Dictionary<ZombieHealth, float>();
        
        while (elapsedTime < smokeDuration)
        {
            // Check all zombies in radius
            Collider[] hitColliders = Physics.OverlapSphere(centerPos, smokeRadius);
            
            foreach (Collider hit in hitColliders)
            {
                ZombieHealth zombie = hit.GetComponentInParent<ZombieHealth>();
                if (zombie != null && !zombie.IsDead())
                {
                    UnityEngine.AI.NavMeshAgent agent = zombie.GetComponent<UnityEngine.AI.NavMeshAgent>();
                    if (agent != null && agent.isActiveAndEnabled)
                    {
                        // If zombie not already slowed, store original speed and slow it
                        if (!slowedZombies.ContainsKey(zombie))
                        {
                            // Store the ORIGINAL speed (e.g., 3.5f)
                            slowedZombies.Add(zombie, agent.speed);
                            // Apply slow factor
                            agent.speed = slowedZombies[zombie] * slowFactor;
                            Debug.Log($"Zombie slowed from {slowedZombies[zombie]} to {agent.speed}");
                        }
                    }
                }
            }
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        // Restore all slowed zombies to their ORIGINAL speed
        foreach (var entry in slowedZombies)
        {
            if (entry.Key != null && !entry.Key.IsDead())
            {
                UnityEngine.AI.NavMeshAgent agent = entry.Key.GetComponent<UnityEngine.AI.NavMeshAgent>();
                if (agent != null)
                {
                    // Restore the exact original speed
                    agent.speed = entry.Value;
                    Debug.Log($"Zombie speed restored to: {agent.speed}");
                }
            }
        }
        
        slowedZombies.Clear();
    }
    
    void FireGrenade(Vector3 groundPos)
    {
        if (fireSound != null)
            AudioSource.PlayClipAtPoint(fireSound, groundPos);
        
        // Create multiple fire zones to cover the area
        if (fireEffect != null)
        {
            // Create main fire zone
            GameObject mainFire = Instantiate(fireEffect, groundPos, Quaternion.identity);
            mainFire.transform.localScale = Vector3.one * fireRadius;
            mainFire.transform.position = groundPos;
            
            FireZone mainZone = mainFire.AddComponent<FireZone>();
            mainZone.Initialize(fireRadius, fireDuration, fireDamagePerSecond, vfxHeightFromGround);
            activeFireZones.Add(mainFire);
            
            // Create additional fire zones around the area for full coverage
            int extraZones = 4;
            for (int i = 0; i < extraZones; i++)
            {
                float angle = (360f / extraZones) * i;
                Vector3 offset = new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad), 0, Mathf.Sin(angle * Mathf.Deg2Rad)) * (fireRadius * 0.6f);
                Vector3 extraPos = groundPos + offset;
                
                GameObject extraFire = Instantiate(fireEffect, extraPos, Quaternion.identity);
                extraFire.transform.localScale = Vector3.one * (fireRadius * 0.8f);
                extraFire.transform.position = extraPos;
                
                FireZone extraZone = extraFire.AddComponent<FireZone>();
                extraZone.Initialize(fireRadius * 0.8f, fireDuration, fireDamagePerSecond, vfxHeightFromGround);
                activeFireZones.Add(extraFire);
            }
        }
        
        Destroy(gameObject);
    }
}