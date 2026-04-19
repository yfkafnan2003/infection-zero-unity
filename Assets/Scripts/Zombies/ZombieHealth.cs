using UnityEngine;

public class ZombieHealth : MonoBehaviour
{
    public int health = 50;
    private GameObject radarDot;
    public AudioSource audioSource;
    public AudioClip hitSound;
    public Animator animator;
    public GameObject bloodFX;
    
    [Header("Ammo Drop Settings")]
    public GameObject ammoDropPrefab;
    [Range(0f, 100f)]
    public float dropChance = 70f;
    public int minAmmoDrop = 15;
    public int maxAmmoDrop = 45;
    
    [Header("Money Drop Settings")]
    public GameObject moneyDropPrefab;
    [Range(0f, 100f)]
    public float moneyDropChance = 80f; // 80% chance to drop money
    public int minMoneyDrop = 10;
    public int maxMoneyDrop = 50;
    public AnimationCurve moneyAmountCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f); // For weighted drops
    
    private bool isDead = false;

    public void TakeDamage(int damage)
    {
        if (isDead) return;
        ZombieAI zombieAI = GetComponent<ZombieAI>();
        if (zombieAI != null)
                zombieAI.OnDamagedByPlayer();
            
        health -= damage;

        if (health <= 0)
        {
            Die();
            return;
        }

        if(animator)
            animator.SetTrigger("Hit");

        if(audioSource && hitSound)
            audioSource.PlayOneShot(hitSound);

        if(bloodFX)
        {
            GameObject blood = Instantiate(bloodFX, transform.position, Quaternion.identity);
            Destroy(blood, 0.5f);
        }
    }
    
    public void SetRadarDot(GameObject dot)
    {
        radarDot = dot;
    }
    
    void Die()
    {
        if (isDead) return;
        isDead = true;

        if (radarDot != null)
            Destroy(radarDot);
        
        if(animator)
            animator.SetTrigger("Die");

        // Drop ONLY ONE item (either ammo OR money, not both)
        DropRandomItem();

        UnityEngine.AI.NavMeshAgent agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (agent != null) agent.enabled = false;
        
        MonoBehaviour[] scripts = GetComponents<MonoBehaviour>();
        foreach (MonoBehaviour script in scripts)
        {
            if (script != this)
                script.enabled = false;
        }
        
        LevelManager levelManager = FindObjectOfType<LevelManager>();
        if (levelManager != null)
            levelManager.RegisterZombieKill();

        Destroy(gameObject, 10f);
    }

    void DropRandomItem()
    {
        // First decide if anything drops at all (70% chance total)
        float totalDropChance = 70f;
        float randomChance = Random.Range(0f, 100f);
        
        if (randomChance > totalDropChance) return;
        
        // Now decide WHAT to drop (50% ammo, 50% money)
        bool dropAmmo = Random.value < 0.5f;
        
        if (dropAmmo)
        {
            DropAmmo();
        }
        else
        {
            DropMoney();
        }
    }
    void DropAmmo()
    {
        float randomChance = Random.Range(0f, 100f);
        if (randomChance > dropChance) return;
        
        if (ammoDropPrefab == null)
        {
            Debug.LogWarning("Ammo drop prefab not assigned!");
            return;
        }
        
        int ammoAmount = Random.Range(minAmmoDrop, maxAmmoDrop + 1);
        AmmoType ammoType = GetRandomAmmoType();
        
        // Spawn ammo slightly above ground
        Vector3 dropPosition = transform.position + Vector3.up * 0.5f;
        GameObject ammoObj = Instantiate(ammoDropPrefab, dropPosition, Quaternion.identity);
        AmmoPickup ammoPickup = ammoObj.GetComponent<AmmoPickup>();
        
        if (ammoPickup != null)
        {
            ammoPickup.ammoType = ammoType;
            ammoPickup.ammoAmount = ammoAmount;
            Debug.Log($"Zombie dropped {ammoAmount} {ammoType} ammo");
        }
    }
    
    void DropMoney()
    {
        float randomChance = Random.Range(0f, 100f);
        if (randomChance > moneyDropChance) return;
        
        if (moneyDropPrefab == null)
        {
            Debug.LogWarning("Money drop prefab not assigned!");
            return;
        }
        
        // Use curve for weighted random (higher amounts are rarer)
        float curveValue = Random.value;
        float weightedAmount = moneyAmountCurve.Evaluate(curveValue);
        int moneyAmount = Mathf.RoundToInt(Mathf.Lerp(minMoneyDrop, maxMoneyDrop, weightedAmount));
        
        // Ensure minimum of 1
        moneyAmount = Mathf.Max(1, moneyAmount);
        
        // Spawn money slightly above ground
        Vector3 dropPosition = transform.position + Vector3.up * 0.5f;
        GameObject moneyObj = Instantiate(moneyDropPrefab, dropPosition, Quaternion.identity);
        MoneyPickup moneyPickup = moneyObj.GetComponent<MoneyPickup>();
        
        if (moneyPickup != null)
        {
            moneyPickup.moneyAmount = moneyAmount;
            Debug.Log($"Zombie dropped ${moneyAmount} money");
        }
    }
    
    AmmoType GetRandomAmmoType()
    {
        float random = Random.Range(0f, 100f);
        
        if (random < 50f)
            return AmmoType.Pistol;
        else if (random < 80f)
            return AmmoType.Shotgun;
        else
            return AmmoType.Machinegun;
    }
    
    public bool IsDead()
    {
        return isDead;
    }
}