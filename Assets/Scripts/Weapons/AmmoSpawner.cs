using UnityEngine;
using System.Collections;

public class AmmoSpawner : MonoBehaviour
{
    [Header("Spawner")]
    public GameObject ammoPickupPrefab;
    public float respawnTime = 30f;

    private GameObject currentAmmo;

    void Start()
    {
        SpawnAmmo();
    }

    void Update()
    {
        // If ammo has been picked up, start respawn
        if (currentAmmo == null)
        {
            currentAmmo = new GameObject("WaitingForRespawn"); // placeholder
            StartCoroutine(Respawn());
        }
    }

    IEnumerator Respawn()
    {
        yield return new WaitForSeconds(respawnTime);

        Destroy(currentAmmo); // remove placeholder
        SpawnAmmo();
    }

    void SpawnAmmo()
    {
        currentAmmo = Instantiate(
            ammoPickupPrefab,
            transform.position,
            transform.rotation
        );
    }

    // Optional: spawn at random rotation
    // currentAmmo = Instantiate(ammoPickupPrefab, transform.position, Random.rotation);
}