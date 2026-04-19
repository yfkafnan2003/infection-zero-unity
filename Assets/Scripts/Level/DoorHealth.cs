using UnityEngine;
using TMPro;

public class DoorHealth : MonoBehaviour
{
    [Header("Door Settings")]
    public int maxHealth = 100;
    public int currentHealth;
    public GameObject doorVisual;
    public ParticleSystem damageEffect;
    
    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip damageSound;
    
    [Header("Healing Settings")]
    public float healDistance = 5f;
    public int healAmountPerSecond = 10;
    private Transform player;
    
    private ProtectDoorManager doorManager;
    private float lastHealTime;
    private int doorIndex;
    
    void Start()
    {
        currentHealth = maxHealth;
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        doorManager = FindObjectOfType<ProtectDoorManager>();
        
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }
    
    void Update()
    {
        // Healing logic only if mission is active
        if (doorManager != null && doorManager.IsMissionActive() && player != null && currentHealth < maxHealth && currentHealth > 0)
        {
            float distance = Vector3.Distance(transform.position, player.position);
            
            if (distance <= healDistance && Time.time >= lastHealTime + 1f)
            {
                Heal(healAmountPerSecond);
                lastHealTime = Time.time;
            }
        }
    }
    
    public void TakeDamage(int damage)
    {
        if (currentHealth <= 0) return;
        
        currentHealth -= damage;
        
        // Update UI
        if (doorManager != null)
            doorManager.UpdateDoorUI(doorIndex, currentHealth, maxHealth);
        
        // Play damage sound
        if (audioSource != null && damageSound != null)
            audioSource.PlayOneShot(damageSound);
        
        if (damageEffect != null)
            damageEffect.Play();
        
        Debug.Log($"Door took {damage} damage! Health: {currentHealth}/{maxHealth}");
        
        if (currentHealth <= 0)
        {
            DestroyDoor();
        }
    }
    
    void Heal(int amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        
        if (doorManager != null)
        {
            doorManager.UpdateDoorUI(doorIndex, currentHealth, maxHealth);
            doorManager.ShowDoorRepairing(doorIndex); // Add this line
        }
        Debug.Log($"Door healed by {amount}! Health: {currentHealth}/{maxHealth}");
    }
    
    void DestroyDoor()
    {
        Debug.Log("Door destroyed!");
        
        if (doorVisual != null)
            doorVisual.SetActive(false);
        
        if (doorManager != null)
            doorManager.OnDoorDestroyed();
    }
    
    public void Setup(int health, int regenRate, float distance)
    {
        maxHealth = health;
        currentHealth = health;
        healAmountPerSecond = regenRate;
        healDistance = distance;
    }
    
    public void SetDoorIndex(int index)
    {
        doorIndex = index;
    }
    
    public void SetDoorManager(ProtectDoorManager manager)
    {
        doorManager = manager;
    }
}