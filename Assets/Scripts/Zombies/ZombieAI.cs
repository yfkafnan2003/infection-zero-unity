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
        playerHealth = player.GetComponent<PlayerHealth>();
    }

    void Update()
    {
        if(player == null) return;

        agent.SetDestination(player.position);

        float speed = agent.velocity.magnitude;
        animator.SetFloat("Speed", speed);

        float dist = Vector3.Distance(transform.position, player.position);

        if(dist <= attackDistance && Time.time >= nextAttackTime)
        {
            Attack();
        }
    }
    public void DealDamage()
    {
        if(playerHealth != null)
            playerHealth.TakeDamage(damage);
    }
    void Attack()
    {
        nextAttackTime = Time.time + attackRate;

        animator.SetTrigger("Attack");

        if(playerHealth != null)
        {
            playerHealth.TakeDamage(damage);
        }
    }
}