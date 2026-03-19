using UnityEngine;
using TMPro;
using System.Collections;

public enum GunType
{
    Pistol,
    Shotgun,
    Machinegun,
    Sniper
}

public enum AmmoType
{
    Pistol,
    Shotgun,
    Machinegun,
    Sniper
}

public class Gun : MonoBehaviour
{
    [Header("Gun Info")]
    public string gunName;
    public GunType gunType;
    public AmmoType ammoType;

    public GameObject bulletPrefab;
    public Transform muzzle;

    float nextFireTime = 0f;

    [Header("Stats")]
    public float fireRate = 0.2f;
    public int damage = 20;
    public float recoilForce = 0.05f;

    [Header("Shotgun Only")]
    public int pelletCount = 6;
    public float spread = 5f;

    [Header("Ammo")]
    public int magazineSize = 30;
    public int currentAmmo;
    public int reserveAmmo = 90;

    public TextMeshProUGUI ammoText;

    [Header("Reload")]
    public float reloadTime = 1.5f;
    bool isReloading = false;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip shootSound;
    public AudioClip reloadSound;

    public DynamicCrosshair crosshair;

    Vector3 originalGunPos;

    void Start()
    {
        currentAmmo = magazineSize;

        originalGunPos = transform.localPosition;

        UpdateAmmoUI();
    }

    public void Shoot()
    {
        if(isReloading) return;
        if(Time.time < nextFireTime) return;
        if(currentAmmo <= 0) return;

        nextFireTime = Time.time + fireRate;

        if(gunType == GunType.Shotgun)
            ShootShotgun();
        else
            ShootBullet(muzzle.rotation);

        audioSource.PlayOneShot(shootSound);

        currentAmmo--;
        UpdateAmmoUI();

        if(crosshair != null)
            crosshair.ShootExpand();

        ApplyRecoil();

        if(currentAmmo <= 0)
            StartCoroutine(Reload());
    }

    void ShootBullet(Quaternion rotation)
    {
        Instantiate(bulletPrefab, muzzle.position, rotation);
    }

    void ShootShotgun()
    {
        for(int i=0;i<pelletCount;i++)
        {
            float x = Random.Range(-spread, spread);
            float y = Random.Range(-spread, spread);

            Quaternion spreadRot = muzzle.rotation * Quaternion.Euler(x,y,0);

            ShootBullet(spreadRot);
        }
    }

    void ApplyRecoil()
    {
        transform.localPosition -= new Vector3(0,0,recoilForce);

        StopCoroutine("RecoilRecover");
        StartCoroutine("RecoilRecover");
    }

    IEnumerator RecoilRecover()
    {
        while(Vector3.Distance(transform.localPosition, originalGunPos) > 0.001f)
        {
            transform.localPosition = Vector3.Lerp(
                transform.localPosition,
                originalGunPos,
                Time.deltaTime * 8f
            );

            yield return null;
        }

        transform.localPosition = originalGunPos;
    }

    public void ReloadButton()
    {
        if(!isReloading)
            StartCoroutine(Reload());
    }

    IEnumerator Reload()
    {
        if(reserveAmmo <= 0 || currentAmmo == magazineSize)
            yield break;

        isReloading = true;

        audioSource.PlayOneShot(reloadSound);

        yield return new WaitForSeconds(reloadTime);

        int neededAmmo = magazineSize - currentAmmo;
        int ammoToLoad = Mathf.Min(neededAmmo, reserveAmmo);

        currentAmmo += ammoToLoad;
        reserveAmmo -= ammoToLoad;

        UpdateAmmoUI();

        isReloading = false;
    }

    void UpdateAmmoUI()
    {
        ammoText.text = currentAmmo + " / " + reserveAmmo;
    }

    public void AddAmmo(int amount, AmmoType type)
    {
        if(type != ammoType) return;

        reserveAmmo += amount;
        UpdateAmmoUI();
    }
}