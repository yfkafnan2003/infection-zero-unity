using UnityEngine;
using UnityEngine.AI;

public class ZombieAI : MonoBehaviour
{
    [Header("References")]
    public NavMeshAgent agent;
    public Transform player;
    public Animator animator;

    [Header("Attack Settings")]
    public float attackDistance = 1.5f;
    public float attackRate = 1f;
    public int damage = 10;

    [Header("Aggro Settings")]
    [SerializeField] private float aggroDuration = 5f;
    [SerializeField] private float playerAggroDistance = 3f;

    [Header("Door Attack")]
    public float doorAttackDistance = 2f;
    public int doorDamage = 10;

    [Header("NavMesh Performance")]
    [Tooltip("How often the zombie recalculates its path.")]
    [SerializeField] private float pathUpdateRate = 0.15f;

    [Tooltip("How often the zombie searches for a door.")]
    [SerializeField] private float doorSearchRate = 1f;

    private float lastPlayerDamageTime = 0f;
    private float nextAttackTime = 0f;
    private float nextPathUpdateTime = 0f;
    private float nextDoorSearchTime = 0f;

    private PlayerHealth playerHealth;
    private DoorHealth targetDoor;
    private LevelManager levelManager;


    // =========================================================
    // START
    // =========================================================

    void Start()
    {
        // Get NavMeshAgent
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


        // Get PlayerHealth
        if (player != null)
        {
            playerHealth = player.GetComponent<PlayerHealth>();
        }


        // Get Animator
        if (animator == null)
            animator = GetComponent<Animator>();


        // Cache LevelManager
        levelManager = FindObjectOfType<LevelManager>();


        // Apply POI settings
        ApplyPOISettings();


        // Slightly randomize the first path update
        // This prevents every zombie from calculating a path
        // on exactly the same frame.
        nextPathUpdateTime = Time.time + Random.Range(0f, pathUpdateRate);


        // Randomize door search
        nextDoorSearchTime = Time.time + Random.Range(0f, doorSearchRate);
    }


    // =========================================================
    // POI SETTINGS
    // =========================================================

    void ApplyPOISettings()
    {
        if (levelManager == null)
            return;

        if (levelManager.currentPOIData == null)
            return;

        POIData poi = levelManager.currentPOIData;


        // Zombie speed
        if (agent != null)
        {
            agent.speed = poi.zombieSpeed;
        }


        // Zombie damage
        damage = poi.GetZombieDamage();
    }


    // =========================================================
    // UPDATE
    // =========================================================

    void Update()
    {
        // Safety checks
        if (player == null)
            return;

        if (agent == null)
            return;

        if (!agent.enabled)
            return;


        // Determine mission type
        bool isProtectDoorMission =
            levelManager != null &&
            levelManager.currentPOIData != null &&
            levelManager.currentPOIData.poiType == POIType.ProtectDoor;


        // Check player aggro
        bool hasAggro = HasPlayerAggro();


        // -----------------------------------------------------
        // DOOR BEHAVIOR
        // -----------------------------------------------------

        if (isProtectDoorMission && !hasAggro)
        {
            // Search for a door occasionally
            // instead of every frame.
            if (Time.time >= nextDoorSearchTime)
            {
                nextDoorSearchTime = Time.time + doorSearchRate;

                FindNearestDoor();
            }


            // Attack door if we have a valid target
            if (targetDoor != null &&
                targetDoor.currentHealth > 0)
            {
                float distanceSqr =
                    (transform.position - targetDoor.transform.position).sqrMagnitude;


                // Attack
                if (distanceSqr <= doorAttackDistance * doorAttackDistance)
                {
                    if (Time.time >= nextAttackTime)
                    {
                        AttackDoor();
                    }
                }
                else
                {
                    // Update path only occasionally
                    UpdateDoorPath();
                }

                UpdateAnimation();
                return;
            }
        }


        // -----------------------------------------------------
        // PLAYER BEHAVIOR
        // -----------------------------------------------------

        UpdatePlayerPath();


        // Calculate distance to player
        float playerDistanceSqr =
            (transform.position - player.position).sqrMagnitude;


        // Attack player
        if (playerDistanceSqr <= attackDistance * attackDistance)
        {
            if (Time.time >= nextAttackTime)
            {
                AttackPlayer();
            }
        }


        // Update animation
        UpdateAnimation();
    }


    // =========================================================
    // PLAYER PATH
    // =========================================================

    void UpdatePlayerPath()
    {
        // Don't recalculate path every frame.
        if (Time.time < nextPathUpdateTime)
            return;


        nextPathUpdateTime = Time.time + pathUpdateRate;


        if (player == null)
            return;


        agent.SetDestination(player.position);
    }


    // =========================================================
    // DOOR PATH
    // =========================================================

    void UpdateDoorPath()
    {
        if (targetDoor == null)
            return;


        if (Time.time < nextPathUpdateTime)
            return;


        nextPathUpdateTime = Time.time + pathUpdateRate;


        agent.SetDestination(targetDoor.transform.position);
    }


    // =========================================================
    // AGGRO
    // =========================================================

    bool HasPlayerAggro()
    {
        // Recently damaged by player
        if (Time.time - lastPlayerDamageTime < aggroDuration)
        {
            return true;
        }


        if (player == null)
            return false;


        // Check distance without Vector3.Distance()
        float distanceSqr =
            (transform.position - player.position).sqrMagnitude;


        return distanceSqr <
               playerAggroDistance * playerAggroDistance;
    }


    // =========================================================
    // PLAYER DAMAGED ZOMBIE
    // =========================================================

    public void OnDamagedByPlayer()
    {
        lastPlayerDamageTime = Time.time;
    }


    // =========================================================
    // FIND NEAREST DOOR
    // =========================================================

    void FindNearestDoor()
    {
        DoorHealth[] doors = FindObjectsOfType<DoorHealth>();

        if (doors == null || doors.Length == 0)
        {
            targetDoor = null;
            return;
        }


        float closestDistanceSqr = Mathf.Infinity;

        DoorHealth closestDoor = null;


        Vector3 zombiePosition = transform.position;


        foreach (DoorHealth door in doors)
        {
            if (door == null)
                continue;


            if (door.currentHealth <= 0)
                continue;


            float distanceSqr =
                (zombiePosition - door.transform.position).sqrMagnitude;


            if (distanceSqr < closestDistanceSqr)
            {
                closestDistanceSqr = distanceSqr;
                closestDoor = door;
            }
        }


        targetDoor = closestDoor;
    }


    // =========================================================
    // ATTACK DOOR
    // =========================================================

    void AttackDoor()
    {
        nextAttackTime = Time.time + attackRate;


        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }


        if (targetDoor != null &&
            targetDoor.currentHealth > 0)
        {
            targetDoor.TakeDamage(doorDamage);
        }
    }


    // =========================================================
    // ATTACK PLAYER
    // =========================================================

    void AttackPlayer()
    {
        nextAttackTime = Time.time + attackRate;


        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }


        if (playerHealth != null)
        {
            playerHealth.TakeDamage(damage);
        }
    }


    // =========================================================
    // ANIMATION
    // =========================================================

    void UpdateAnimation()
    {
        if (animator == null)
            return;


        float speed = agent.velocity.magnitude;

        animator.SetFloat("Speed", speed);
    }


    // =========================================================
    // ANIMATION EVENT
    // =========================================================

    // If your Attack animation has an Animation Event
    // calling "Attack()", keep this function.

    void Attack()
    {
        nextAttackTime = Time.time + attackRate;


        if (playerHealth != null)
        {
            playerHealth.TakeDamage(damage);
        }
    }


    // =========================================================
    // ON ENABLE
    // =========================================================

    void OnEnable()
    {
        if (agent != null && !agent.enabled)
        {
            agent.enabled = true;
        }


        // Give each zombie a slightly different update time.
        // This prevents all zombies from recalculating their
        // paths on the exact same frame.

        nextPathUpdateTime =
            Time.time + Random.Range(0f, pathUpdateRate);

        nextDoorSearchTime =
            Time.time + Random.Range(0f, doorSearchRate);
    }
}