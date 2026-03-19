using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 40f;
    public float lifeTime = 3f;

    public int damage = 20;
    public float headshotMultiplier = 2f;

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }

    void OnTriggerEnter(Collider collision)
    {
        ZombieHealth zombie = collision.GetComponentInParent<ZombieHealth>();

        if(zombie != null)
        {
            int finalDamage = damage;

            if(collision.CompareTag("ZombieHead"))
                finalDamage = Mathf.RoundToInt(damage * headshotMultiplier);

            zombie.TakeDamage(finalDamage, transform.position);
        }

        Destroy(gameObject);
    }
}