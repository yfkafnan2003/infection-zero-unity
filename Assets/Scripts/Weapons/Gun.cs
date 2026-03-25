using UnityEngine;
using TMPro;
using System.Collections;

public enum GunType
{
    Pistol,
    Shotgun,
    Machinegun
}

public enum AmmoType
{
    Pistol,
    Shotgun,
    Machinegun
}

public class Gun : MonoBehaviour
{
    [Header("Gun Info")]
    public string gunName;
    public GunType gunType;
    public AmmoType ammoType;
    public Camera playerCamera;
    public float shootDistance = 200f;
    public GameObject bulletPrefab;
    public Transform muzzle;
    [Header("Muzzle Aim Fix")]
    public Vector3 normalMuzzleRotationOffset;
    public Vector3 aimMuzzleRotationOffset;
    float nextFireTime = 0f;
    [Header("Recoil")]
    public float recoilKick = 0.05f;
    public float recoilRotation = 3f;
    public float recoilRecoverSpeed = 10f;

Vector3 recoilOffset;
float recoilRot;
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

    [Header("Animation")]
    public Animator gunAnimator;
    public AimSystem aimSystem;

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
    void Update()
    {
        RecoverRecoil();
    }
    public void Shoot()
    {

        if(isReloading) return;
        if(Time.time < nextFireTime) return;
        if(currentAmmo <= 0) return;

        nextFireTime = Time.time + fireRate;

        if(gunType == GunType.Shotgun)
        {
            ShootShotgun();
        }
        else
        {
            Quaternion shootRotation = muzzle.rotation;

            if(aimSystem != null && aimSystem.IsAiming())
            {
                shootRotation *= Quaternion.Euler(aimMuzzleRotationOffset);
            }
            else
            {
                shootRotation *= Quaternion.Euler(normalMuzzleRotationOffset);
            }

            ShootBullet();
        }

        audioSource.PlayOneShot(shootSound);

        currentAmmo--;
        UpdateAmmoUI();

        if(crosshair != null)
            crosshair.ShootExpand();

        ApplyRecoil();

        if(currentAmmo <= 0)
            StartCoroutine(Reload());
    }
    public void OnWeaponSwitch()
    {
        StopAllCoroutines();
        isReloading = false;
        recoilOffset = Vector3.zero;
        recoilRot = 0f;
    }
    public Vector3 GetRecoilPosition()
    {
        return recoilOffset;
    }
    void RecoverRecoil()
    {
        recoilOffset = Vector3.Lerp(recoilOffset, Vector3.zero, Time.deltaTime * recoilRecoverSpeed);
        recoilRot = Mathf.Lerp(recoilRot, 0f, Time.deltaTime * recoilRecoverSpeed);
    }
    public float GetRecoilRotation()
    {
        return recoilRot;
    }
    void ShootBullet()
    {
        Vector3 direction = GetShootDirection();
        Quaternion rot = Quaternion.LookRotation(direction);

        Instantiate(bulletPrefab, muzzle.position, rot);
    }
    Vector3 GetShootDirection()
    {
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        Vector3 targetPoint;

        if (Physics.Raycast(ray, out hit, shootDistance))
            targetPoint = hit.point;
        else
            targetPoint = ray.GetPoint(shootDistance);

        return (targetPoint - muzzle.position).normalized;
    }
    void ShootShotgun()
    {
        for(int i = 0; i < pelletCount; i++)
        {
            Vector3 dir = GetShootDirection();

            dir = Quaternion.Euler(
                Random.Range(-spread, spread),
                Random.Range(-spread, spread),
                0
            ) * dir;

            Quaternion rot = Quaternion.LookRotation(dir);

            Instantiate(bulletPrefab, muzzle.position, rot);
        }
    }

    void ApplyRecoil()
    {
        recoilOffset -= new Vector3(
            Random.Range(-0.01f,0.01f),
            Random.Range(-0.01f,0.01f),
            recoilKick
        );

        recoilRot -= recoilRotation + Random.Range(-0.5f,0.5f);
    }

    public void ReloadButton()
    {
        if(!gameObject.activeInHierarchy) return;

        if(!isReloading)
            StartCoroutine(Reload());
    }

    IEnumerator Reload()
    {
        if(reserveAmmo <= 0 || currentAmmo == magazineSize)
            yield break;

        isReloading = true;

        bool wasAiming = aimSystem != null && aimSystem.IsAiming();

        if(wasAiming)
            aimSystem.StopAim();

        if(gunAnimator)
            gunAnimator.SetTrigger("Reload");

        audioSource.PlayOneShot(reloadSound);

        yield return new WaitForSeconds(reloadTime);

        int neededAmmo = magazineSize - currentAmmo;
        int ammoToLoad = Mathf.Min(neededAmmo, reserveAmmo);

        currentAmmo += ammoToLoad;
        reserveAmmo -= ammoToLoad;

        UpdateAmmoUI();

        isReloading = false;

        if(wasAiming)
            aimSystem.ForceAim();
    }

    public void UpdateAmmoUI()
    {
        ammoText.text = currentAmmo + " / " + reserveAmmo;
    }
    public bool IsReloading()
    {
        return isReloading;
    }
    public void AddAmmo(int amount, AmmoType type)
    {
        if(type != ammoType) return;

        reserveAmmo += amount;
        UpdateAmmoUI();
    }
}