using UnityEngine;
using UnityEngine.AI;

public class ZombieAI : MonoBehaviour
{
    public NavMeshAgent agent;
    public Transform player;
    public Animator animator;
    public float attackDistance = 1.5f;
    public float attackRate = 1f;
    
    // Add these variables at the top
    private float lastPlayerDamageTime = 0f;
    private float aggroDuration = 5f;
    private float playerAggroDistance = 3f;
    
    float nextAttackTime = 0f;
    public int damage = 10;
    PlayerHealth playerHealth;
    public float doorAttackDistance = 2f;
    public int doorDamage = 10;
    private DoorHealth targetDoor;
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
        
        // Apply POI customization
        ApplyPOISettings();
    }

    void ApplyPOISettings()
    {
        LevelManager levelManager = FindObjectOfType<LevelManager>();
        if (levelManager != null && levelManager.currentPOIData != null)
        {
            POIData poi = levelManager.currentPOIData;
            
            // Apply zombie speed
            if (agent != null)
            {
                agent.speed = poi.zombieSpeed;
                Debug.Log($"Zombie speed set to {poi.zombieSpeed} from POI");
            }
            
            // Apply zombie damage
            damage = poi.GetZombieDamage();
            Debug.Log($"Zombie damage set to {damage} from POI");
        }
    }

    void Update()
    {
        if (player == null || agent == null || !agent.enabled)
            return;
        
        LevelManager lm = FindObjectOfType<LevelManager>();
        bool isProtectDoorMission = (lm != null && lm.currentPOIData != null && 
                                    lm.currentPOIData.poiType == POIType.ProtectDoor);
        
        // Check if player is attacking (has aggro)
        bool hasAggro = HasPlayerAggro();
        
        if (isProtectDoorMission && !hasAggro && targetDoor != null && targetDoor.currentHealth > 0)
        {
            // Attack door
            agent.SetDestination(targetDoor.transform.position);
            float distToDoor = Vector3.Distance(transform.position, targetDoor.transform.position);
            
            if (distToDoor <= doorAttackDistance && Time.time >= nextAttackTime)
            {
                AttackDoor();
            }
        }
        else
        {
            // Attack player
            agent.SetDestination(player.position);
            float distToPlayer = Vector3.Distance(transform.position, player.position);
            
            if (distToPlayer <= attackDistance && Time.time >= nextAttackTime)
            {
                AttackPlayer();
            }
        }
        
        // Update door target if needed
        if (targetDoor == null || targetDoor.currentHealth <= 0)
        {
            FindNearestDoor();
        }
        
        // Animations
        float speed = agent.velocity.magnitude;
        if (animator != null)
            animator.SetFloat("Speed", speed);
    }

    bool HasPlayerAggro()
    {
        // Check if recently took damage from player
        if (Time.time - lastPlayerDamageTime < aggroDuration)
            return true;
        
        // Check if player is too close
        float distToPlayer = Vector3.Distance(transform.position, player.position);
        if (distToPlayer < playerAggroDistance)
            return true;
        
        return false;
    }

    public void OnDamagedByPlayer()
    {
        lastPlayerDamageTime = Time.time;
    }
        void FindNearestDoor()
    {
        DoorHealth[] doors = FindObjectsOfType<DoorHealth>();
        float closestDistance = Mathf.Infinity;
        
        foreach (DoorHealth door in doors)
        {
            if (door.currentHealth > 0)
            {
                float dist = Vector3.Distance(transform.position, door.transform.position);  // Add .transform here too
                if (dist < closestDistance)
                {
                    closestDistance = dist;
                    targetDoor = door;
                }
            }
        }
    }

    void AttackDoor()
    {
        nextAttackTime = Time.time + attackRate;
        
        if (animator != null)
            animator.SetTrigger("Attack");
        
        if (targetDoor != null)
        {
            targetDoor.TakeDamage(doorDamage);
            Debug.Log($"Zombie attacked door for {doorDamage} damage!");
        }
    }

    void AttackPlayer()
    {
        nextAttackTime = Time.time + attackRate;
        
        if (animator != null)
            animator.SetTrigger("Attack");
        
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(damage);
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