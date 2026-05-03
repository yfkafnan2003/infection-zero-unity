using UnityEngine;

public class ZombieHealth : MonoBehaviour
{
    public int health = 50;
    private GameObject radarDot;
    public AudioSource audioSource;
    // Add these variables with your other header variables
    [Header("Idle Sounds")]
    public AudioClip[] idleSounds;
    [Range(0f, 30f)]
    public float minIdleSoundDelay = 5f;
    [Range(0f, 30f)]
    public float maxIdleSoundDelay = 15f;
    private float nextIdleSoundTime = 0f;


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
    public float moneyDropChance = 80f;
    public int minMoneyDrop = 10;
    public int maxMoneyDrop = 50;
    public AnimationCurve moneyAmountCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
    
    private bool isDead = false;

    // Add this method to Start() or create a Start() method if you don't have one
    void Start()
    {
        // Set random time for first idle sound
        nextIdleSoundTime = Time.time + Random.Range(minIdleSoundDelay, maxIdleSoundDelay);
    }

    // Add this to Update() - create Update() method if you don't have one
    void Update()
    {
        // Play random idle sounds when not dead
        if (!isDead && audioSource != null && idleSounds.Length > 0)
        {
            if (Time.time >= nextIdleSoundTime)
            {
                // Pick random sound from array
                AudioClip randomSound = idleSounds[Random.Range(0, idleSounds.Length)];
                audioSource.PlayOneShot(randomSound);
                
                // Set next random time
                nextIdleSoundTime = Time.time + Random.Range(minIdleSoundDelay, maxIdleSoundDelay);
            }
        }
    }
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

        // Disable collider so player can pass through
        Collider col = GetComponent<Collider>();
        if (col != null)
            col.enabled = false;

        if (radarDot != null)
            Destroy(radarDot);
        
        if(animator)
            animator.SetTrigger("Die");

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

    public void ForceDeath()
    {
        if (isDead) return;
        isDead = true;

        // Disable collider so player can pass through
        Collider col = GetComponent<Collider>();
        if (col != null)
            col.enabled = false;

        if (radarDot != null)
            Destroy(radarDot);
        
        if (animator != null)
            animator.SetTrigger("Die");
        
        // Disable components but DON'T drop items or register kill
        UnityEngine.AI.NavMeshAgent agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (agent != null) agent.enabled = false;
        
        MonoBehaviour[] scripts = GetComponents<MonoBehaviour>();
        foreach (MonoBehaviour script in scripts)
        {
            if (script != this)
                script.enabled = false;
        }
        
        // Destroy after animation (no item drop)
        Destroy(gameObject, 10f);
    }
    void DropRandomItem()
    {
        float totalDropChance = 70f;
        float randomChance = Random.Range(0f, 100f);
        
        if (randomChance > totalDropChance) return;
        
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
        
        // REMOVED: ammoAmount variable (not needed anymore)
        AmmoType ammoType = GetRandomAmmoType();
        
        Vector3 dropPosition = transform.position + Vector3.up * 0.5f;
        GameObject ammoObj = Instantiate(ammoDropPrefab, dropPosition, Quaternion.identity);
        AmmoPickup ammoPickup = ammoObj.GetComponent<AmmoPickup>();
        
        if (ammoPickup != null)
        {
            ammoPickup.ammoType = ammoType;
            // REMOVED: ammoPickup.ammoAmount = ammoAmount;  <- THIS WAS THE ERROR
            Debug.Log($"Zombie dropped {ammoType} ammo");
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
        
        float curveValue = Random.value;
        float weightedAmount = moneyAmountCurve.Evaluate(curveValue);
        int moneyAmount = Mathf.RoundToInt(Mathf.Lerp(minMoneyDrop, maxMoneyDrop, weightedAmount));
        
        moneyAmount = Mathf.Max(1, moneyAmount);
        
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