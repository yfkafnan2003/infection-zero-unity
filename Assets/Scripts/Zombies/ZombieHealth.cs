using UnityEngine;

public class ZombieHealth : MonoBehaviour
{
    public int health = 50;

    public AudioSource audioSource;
    public AudioClip hitSound;
    public Animator animator;
    public GameObject bloodFX;
    
    private bool isDead = false; // Add this flag to prevent multiple kill registrations

    public void TakeDamage(int damage, Vector3 hitPoint)
    {
        if (isDead) return; // Don't process damage if already dead
        
        health -= damage;

        if(animator)
            animator.SetTrigger("Hit");

        if(audioSource && hitSound)
            audioSource.PlayOneShot(hitSound);

        if(bloodFX)
        {
            GameObject blood = Instantiate(
                bloodFX,
                hitPoint,
                Quaternion.identity
            );

            Destroy(blood, 0.5f);
        }

        if(health <= 0)
            Die();
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;
        
        if(animator)
            animator.SetTrigger("Die");

        UnityEngine.AI.NavMeshAgent agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (agent != null) agent.enabled = false;
        
        // Disable zombie components to prevent further actions
        MonoBehaviour[] scripts = GetComponents<MonoBehaviour>();
        foreach (MonoBehaviour script in scripts)
        {
            if (script != this)
                script.enabled = false;
        }
        
        // Register kill with LevelManager
        LevelManager levelManager = FindObjectOfType<LevelManager>();
        if (levelManager != null)
            levelManager.RegisterZombieKill();

        Destroy(gameObject, 10f);
    }
    
    public bool IsDead()
    {
        return isDead;
    }

}