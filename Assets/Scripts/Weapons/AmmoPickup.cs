using UnityEngine;

public class AmmoPickup : MonoBehaviour
{
    public AmmoType ammoType;
    public int ammoAmount = 30;

    public float rotateSpeed = 120f;

    public AudioClip pickupSound;

    void Update()
    {
        transform.Rotate(Vector3.up * rotateSpeed * Time.deltaTime);
    }

    void OnTriggerEnter(Collider other)
    {
        Gun gun = other.GetComponentInChildren<Gun>();

        if(gun != null)
        {
            gun.AddAmmo(ammoAmount, ammoType);

            AudioSource.PlayClipAtPoint(pickupSound, transform.position);

            Destroy(gameObject);
        }
    }
}