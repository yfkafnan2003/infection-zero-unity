using UnityEngine;
using UnityEngine.AI;

public class ZombieAI : MonoBehaviour
{
    public NavMeshAgent agent;
    public Transform player;
    public Animator animator;
    public float attackDistance = 1.5f;
    public float attackRate = 1f;

    float nextAttackTime = 0f;
    public int damage = 10;
    PlayerHealth playerHealth;

    void Start()
    {
        // Ensure NavMeshAgent is enabled
        if (agent == null)
            agent = GetComponent<NavMeshAgent>();
        
        if (agent != null && !agent.enabled)
            agent.enabled = true;
        
        // Find player if not assigned
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                player = playerObj.transform;
        }
        
        if (player != null)
            playerHealth = player.GetComponent<PlayerHealth>();
        
        if (animator == null)
            animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (player == null || agent == null || !agent.enabled)
            return;

        agent.SetDestination(player.position);

        float speed = agent.velocity.magnitude;
        if (animator != null)
            animator.SetFloat("Speed", speed);

        float dist = Vector3.Distance(transform.position, player.position);

        if (dist <= attackDistance && Time.time >= nextAttackTime)
        {
            Attack();
        }
    }
    
    void Attack()
    {
        nextAttackTime = Time.time + attackRate;
        
        if (animator != null)
            animator.SetTrigger("Attack");
        
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(damage);
        }
    }
    
    void OnEnable()
    {
        // Re-enable NavMeshAgent when object is re-enabled
        if (agent != null && !agent.enabled)
            agent.enabled = true;
    }
}