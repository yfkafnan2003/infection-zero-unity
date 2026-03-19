using UnityEngine;
using UnityEngine.AI;

public class ZombieAI : MonoBehaviour
{
    public NavMeshAgent agent;

    public Transform player;

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

        float dist = Vector3.Distance(transform.position, player.position);

        if(dist <= attackDistance && Time.time >= nextAttackTime)
        {
            Attack();
        }
    }

    void Attack()
    {
        nextAttackTime = Time.time + attackRate;

        if(playerHealth != null)
        {
            playerHealth.TakeDamage(damage);
        }
    }
}