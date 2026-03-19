using UnityEngine;

public class ZombieHealth : MonoBehaviour
{
    public int health = 50;

    public AudioSource audioSource;
    public AudioClip hitSound;

    public GameObject bloodFX;

    public void TakeDamage(int damage, Vector3 hitPoint)
    {
        health -= damage;

        if(audioSource && hitSound)
            audioSource.PlayOneShot(hitSound);

        if(bloodFX)
        {
            GameObject blood = Instantiate(
                bloodFX,
                hitPoint,
                Quaternion.identity
            );

            Destroy(blood, 2f);
        }

        if(health <= 0)
            Die();
    }

    void Die()
    {
        Destroy(gameObject);
    }
}