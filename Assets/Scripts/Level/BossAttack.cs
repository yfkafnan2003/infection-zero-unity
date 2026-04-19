using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class BossAttack : MonoBehaviour
{
    [Header("References (Assign in Inspector)")]
    public Transform playerTransform; // Drag player Transform here
    
    public float attackRange = 15f;
    public float attackCooldown = 3f;
    public int damage = 30;
    public Transform attackPoint;
    public float attackRadius = 3f;
    public float moveSpeed = 3.5f;
    public float collisionDamage = 20f;
    public float collisionDamageCooldown = 1f;
    
    [Header("Jump Attack")]
    public float jumpCooldown = 10f;
    public float jumpRange = 8f;
    public float jumpHeight = 5f;
    public float jumpDuration = 1.5f;
    public float jumpDamage = 50f;
    public float jumpRadius = 3f;
    public float jumpChance = 0.5f;
    
    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip walkSound;
    public AudioClip jumpSound;
    public AudioClip landSound;
    
    private float nextAttackTime = 0f;
    private float nextCollisionDamageTime = 0f;
    private float nextJumpTime = 0f;
    private Animator animator;
    private NavMeshAgent agent;
    private bool isAttacking = false;
    private bool isScreaming = false;
    private bool isJumping = false;
    private Vector3 jumpTargetPosition;
    private float originalSpeed;
    private bool isWalking = false;
    private float walkSoundTimer = 0f;

    void Start()
    {
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        nextJumpTime = Time.time + 5f;
        if (agent != null)
        {
            agent.baseOffset = -0.1f;
            originalSpeed = moveSpeed;
            agent.speed = moveSpeed;
            agent.stoppingDistance = attackRange - 2f;
            if (animator != null)
                animator.applyRootMotion = true;
            agent.velocity = Vector3.zero;
        }
        
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
        
    }

    IEnumerator InitialDelayJump()
    {
        yield return new WaitForSeconds(2f);
        if (playerTransform != null && !isJumping)
        {
            jumpTargetPosition = playerTransform.position;
            StartCoroutine(DelayedJump());
        }
    }
    
    void Update()
    {
        if (playerTransform == null) return;
        
        float distance = Vector3.Distance(transform.position, playerTransform.position);
        
        bool isInAction = isAttacking || isScreaming || isJumping;
        
        HandleWalkSound();
        
        if (agent != null && distance > attackRange && !isInAction)
        {
            bool shouldJump = (distance >= jumpRange && 
                            Time.time >= nextJumpTime && 
                            !isJumping &&
                            Random.value <= jumpChance);
            
            if (shouldJump)
            {
                StartJumpAttack();
            }
            else
            {
                agent.isStopped = false;
                agent.SetDestination(playerTransform.position);
                
                if (animator != null)
                    animator.SetFloat("Speed", agent.velocity.magnitude);
                
                if (agent.velocity.magnitude > 0.5f && !isWalking)
                    isWalking = true;
                else if (agent.velocity.magnitude <= 0.5f && isWalking)
                    isWalking = false;
            }
        }
        else if (agent != null && !isInAction)
        {
            agent.isStopped = true;
            if (animator != null)
                animator.SetFloat("Speed", 0);
            isWalking = false;
        }
        
        if (distance <= attackRange && Time.time >= nextAttackTime && !isAttacking && !isScreaming && !isJumping)
        {
            Attack();
        }
        
        if (distance <= 2.5f && Time.time >= nextCollisionDamageTime && !isAttacking)
        {
            nextCollisionDamageTime = Time.time + collisionDamageCooldown;
            
            PlayerHealth playerHealth = playerTransform.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(Mathf.RoundToInt(collisionDamage));
            }
        }
    }
    
    void HandleWalkSound()
    {
        if (walkSound != null && isWalking && !isAttacking && !isScreaming && !isJumping)
        {
            walkSoundTimer += Time.deltaTime;
            if (walkSoundTimer >= 0.5f)
            {
                walkSoundTimer = 0f;
                audioSource.PlayOneShot(walkSound);
            }
        }
        else
        {
            walkSoundTimer = 0f;
        }
    }
    
    void Attack()
    {
        nextAttackTime = Time.time + attackCooldown;
        isAttacking = true;
        
        if (animator != null)
        {
            animator.SetTrigger("Attack");
            float attackLength = GetAnimationLength("Attack");
            StartCoroutine(ResetActionAfterTime(attackLength, "Attack"));
        }
        
        // Direct distance check instead of layer
        if (playerTransform != null)
        {
            float distanceToPlayer = Vector3.Distance(attackPoint.position, playerTransform.position);
            if (distanceToPlayer <= attackRadius)
            {
                PlayerHealth playerHealth = playerTransform.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(damage);
                }
            }
        }
    }
    
    void StartJumpAttack()
    {
        if (isJumping) return;
        nextJumpTime = Time.time + jumpCooldown;
        isJumping = true;
        
        if (agent != null)
            agent.isStopped = true;
        
        if (audioSource != null && jumpSound != null)
            audioSource.PlayOneShot(jumpSound);
        
        jumpTargetPosition = playerTransform.position;
        
        if (animator != null)
            animator.SetTrigger("Jump");
        
        StartCoroutine(DelayedJump());
    }

    IEnumerator DelayedJump()
    {
        float jumpAnimationDelay = 0.3f;
        yield return new WaitForSeconds(jumpAnimationDelay);
        StartCoroutine(PerformJump());
    }
    
    IEnumerator PerformJump()
    {
        Vector3 startPosition = transform.position;
        Vector3 endPosition = new Vector3(jumpTargetPosition.x, startPosition.y, jumpTargetPosition.z);
        float elapsedTime = 0f;
        bool hasLanded = false;
        
        while (elapsedTime < jumpDuration)
        {
            float t = elapsedTime / jumpDuration;
            float height = Mathf.Sin(t * Mathf.PI) * jumpHeight;
            
            Vector3 newPos = Vector3.Lerp(startPosition, endPosition, t);
            newPos.y = startPosition.y + height;
            transform.position = newPos;
            
            if (!hasLanded && t >= 0.95f)
            {
                hasLanded = true;
                if (animator != null)
                    animator.SetTrigger("Land");
            }
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        transform.position = endPosition;
        ShakeCameraOnLand();
        
        if (audioSource != null && landSound != null)
            audioSource.PlayOneShot(landSound);
        
        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
        if (distanceToPlayer <= jumpRadius)
        {
            PlayerHealth playerHealth = playerTransform.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(Mathf.RoundToInt(jumpDamage));
            }
        }
        
        ShowLandingEffect();
        yield return new WaitForSeconds(0.2f);
        isJumping = false;
        
        if (agent != null && !isAttacking && !isScreaming)
            agent.isStopped = false;
    }
    
    void ShakeCameraOnLand()
    {
        PlayerHealth playerHealth = FindObjectOfType<PlayerHealth>();
        if (playerHealth != null)
            playerHealth.ShakeCamera(0.5f, 0.5f);
    }
    
    void ShowLandingEffect()
    {
        GameObject effect = new GameObject("LandingEffect");
        effect.transform.position = transform.position;
        SphereCollider collider = effect.AddComponent<SphereCollider>();
        collider.radius = jumpRadius;
        collider.isTrigger = true;
        Destroy(effect, 0.5f);
    }
    
    public void OnScream()
    {
        isScreaming = true;
        if (agent != null) agent.isStopped = true;
        
        float screamLength = GetAnimationLength("Scream");
        StartCoroutine(ResetActionAfterTime(screamLength, "Scream"));
    }
    
    IEnumerator ResetActionAfterTime(float time, string action)
    {
        yield return new WaitForSeconds(time);
        
        if (action == "Attack") isAttacking = false;
        else if (action == "Scream") isScreaming = false;
        
        if (!isAttacking && !isScreaming && !isJumping)
        {
            if (agent != null)
            {
                agent.isStopped = false;
                agent.velocity = Vector3.zero;
            }
        }
    }
    
    float GetAnimationLength(string animationName)
    {
        if (animator != null)
        {
            RuntimeAnimatorController ac = animator.runtimeAnimatorController;
            AnimationClip[] clips = ac.animationClips;
            for (int i = 0; i < clips.Length; i++)
            {
                if (clips[i].name == animationName)
                    return clips[i].length;
            }
        }
        return 0.5f;
    }
    
    void OnDrawGizmosSelected()
    {
        if (attackPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackPoint.position, attackRadius);
        }
        
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, jumpRadius);
        
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, jumpRange);
    }
}