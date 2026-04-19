using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BossHealth : MonoBehaviour
{
    [Header("Boss Settings")]
    public int maxHealth = 1000;
    public int currentHealth;
    public Slider healthBar;
    public TextMeshProUGUI healthText;
    
    [Header("Phase Settings")]
    public GameObject[] spawnPoints;
    public GameObject zombiePrefab;
    public int zombiesToSpawn = 5;
    
    [Header("Attack Settings")]
    public float attackCooldown = 3f;
    public int attackDamage = 30;
    public float attackRange = 10f;
    
    [Header("Weakpoint")]
    public GameObject weakpointObject;
    public float weakpointMultiplier = 3f;
    
    [Header("UI")]
    public GameObject bossUIPanel;
    
    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip roarSound;  // ADD THIS - Assign roar sound in Inspector
    
    private Animator animator;
    private Transform player;
    private float nextAttackTime = 0f;
    private bool isDead = false;
    private bool phase90Triggered = false;
    private bool phase75Triggered = false;
    private bool phase50Triggered = false;
    private bool phase30Triggered = false;
    private bool phase10Triggered = false;

    
    void Start()
    {
        currentHealth = maxHealth;
        animator = GetComponent<Animator>();
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
        
        if (healthBar != null)
        {
            healthBar.maxValue = maxHealth;
            healthBar.value = currentHealth;
        }
        
        UpdateHealthUI();
        
        if (bossUIPanel != null)
            bossUIPanel.SetActive(true);
    }
    
    void Update()
    {
        if (isDead) return;
        
        float healthPercent = (float)currentHealth / maxHealth;
        
        if (!phase90Triggered && healthPercent <= 0.90f)
        {
            phase90Triggered = true;
            TriggerPhase(90);
        }
        else if (!phase75Triggered && healthPercent <= 0.75f)
        {
            phase75Triggered = true;
            TriggerPhase(75);
        }
        else if (!phase50Triggered && healthPercent <= 0.50f)
        {
            phase50Triggered = true;
            TriggerPhase(50);
        }
        else if (!phase30Triggered && healthPercent <= 0.30f)
        {
            phase30Triggered = true;
            TriggerPhase(30);
        }
        else if (!phase10Triggered && healthPercent <= 0.10f)
        {
            phase10Triggered = true;
            TriggerPhase(10);
        }
    }
    
    public void TakeDamage(int damage, bool isWeakpointHit = false)
    {
        if (isDead) return;
        
        int finalDamage = isWeakpointHit ? Mathf.RoundToInt(damage * weakpointMultiplier) : damage;
        currentHealth -= finalDamage;
        
        if (healthBar != null)
            healthBar.value = currentHealth;
        
        UpdateHealthUI();
        
        // Play hit animation (keep this)
        if (animator != null)
            animator.SetTrigger("Hit");

        Debug.Log($"Boss took {finalDamage} damage! Health: {currentHealth}/{maxHealth}");
        
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    
    void TriggerPhase(int phasePercent)
    {
        Debug.Log($"BOSS PHASE {phasePercent}% TRIGGERED!");
        
        // Play scream animation
        if (animator != null)
            animator.SetTrigger("Scream");
        
        // Play roar sound
        if (audioSource != null && roarSound != null)
        {
            audioSource.PlayOneShot(roarSound);
            Debug.Log("Boss roared!");
        }
        
        // Call OnScream to stop movement
        BossAttack bossAttack = GetComponent<BossAttack>();
        if (bossAttack != null)
            bossAttack.OnScream();
        
        // Shake camera
        ShakeCamera();
        
        // Spawn zombies after 2 seconds
        StartCoroutine(SpawnZombiesWithDelay());
    }
    
    System.Collections.IEnumerator SpawnZombiesWithDelay()
    {
        yield return new WaitForSeconds(2f); // Wait 2 seconds after scream
        
        for (int i = 0; i < zombiesToSpawn; i++)
        {
            if (spawnPoints != null && spawnPoints.Length > 0)
            {
                Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)].transform;
                Instantiate(zombiePrefab, spawnPoint.position, Quaternion.identity);
            }
            yield return new WaitForSeconds(0.5f);
        }
    }
    void ShakeCamera()
    {
        PlayerHealth playerHealth = FindObjectOfType<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.ShakeCamera(3f, 0.3f);
            Debug.Log("Camera shake triggered!");
        }
    }
    void Die()
    {
        isDead = true;
        
        if (animator != null)
            animator.SetTrigger("Die");
        
        if (bossUIPanel != null)
            bossUIPanel.SetActive(false);
        
        if (weakpointObject != null)
            weakpointObject.SetActive(false);
        
        Collider col = GetComponent<Collider>();
        if (col != null)
            col.enabled = false;
        
        LevelManager levelManager = FindObjectOfType<LevelManager>();
        if (levelManager != null)
        {
            levelManager.CompleteMission(true);
        }
        
        Destroy(gameObject, 5f);
    }
    
    void UpdateHealthUI()
    {
        if (healthText != null)
        {
            healthText.text = $"{currentHealth}/{maxHealth}";
        }
    }
    
    public bool IsDead()
    {
        return isDead;
    }
}