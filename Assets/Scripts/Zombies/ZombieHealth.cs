using UnityEngine;

public class ZombieHealth : MonoBehaviour
{
    public int health = 50;

    public AudioSource audioSource;
    public AudioClip hitSound;
    public Animator animator;
    public GameObject bloodFX;

    public void TakeDamage(int damage, Vector3 hitPoint)
    {
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
        if(animator)
            animator.SetTrigger("Die");

        GetComponent<UnityEngine.AI.NavMeshAgent>().enabled = false;

        Destroy(gameObject, 10f);
    }
}