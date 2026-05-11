using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;
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
    private Dictionary<ZombieHealth, float> zombieHitTimers = new Dictionary<ZombieHealth, float>();
    private Dictionary<ZombieHealth, int> zombieHitCounts = new Dictionary<ZombieHealth, int>();
    public float hitResetTime = 1f; // Time to reset hit count for same zombie
    [Header("Gun Info")]
    public string gunName;
    public GunType gunType;
    public AmmoType ammoType;
    public Camera playerCamera;
    public float shootDistance = 200f;
    
    float nextFireTime = 0f;
    private bool isShooting = false;
    private Coroutine autoFireCoroutine;
    [Header("Recoil")]
    public float recoilKick = 0.05f;
    public float recoilRotation = 3f;
    public float recoilRecoverSpeed = 10f;

    Vector3 recoilOffset;
    float recoilRot;
    
    [Header("Stats")]
    public float fireRate = 0.2f;
    public int damage = 20;

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
    public AudioClip dryFireSound;
    public AudioClip reloadSound;

    public DynamicCrosshair crosshair;

    Vector3 originalGunPos;

    void Start()
    {
        currentAmmo = magazineSize;
        originalGunPos = transform.localPosition;
        UpdateAmmoUI();
        Debug.Log($"Gun initialized: {gunName}, Ammo: {currentAmmo}, FireRate: {fireRate}");
    }
    
    void Update()
    {
        RecoverRecoil();
    }
    
    public void Shoot()
    {
        if(isReloading) return;
        if(Time.time < nextFireTime) return;
        
        // Handle no ammo
        if(currentAmmo <= 0)
        {
            // Show "No Ammo" warning
            if (HitTextManager.Instance != null)
            {
                HitTextManager.Instance.ShowNoAmmoText();
            }
            
            if (audioSource != null && dryFireSound != null)
                audioSource.PlayOneShot(dryFireSound);
            
            return;
        }

        nextFireTime = Time.time + fireRate;

        if(gunType == GunType.Shotgun)
        {
            ShootShotgun();
        }
        else
        {
            ShootBullet();
        }

        if (audioSource != null && shootSound != null)
            audioSource.PlayOneShot(shootSound);

        currentAmmo--;
        UpdateAmmoUI();

        if(crosshair != null)
            crosshair.ShootExpand();

        ApplyRecoil();

        if(currentAmmo <= 0)
            StartCoroutine(Reload());
    }
    
    void ShootBullet()
    {
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;
        
        if (Physics.Raycast(ray, out hit, shootDistance))
        {
            // Check for Weakpoint first (Boss weakpoint)
            Weakpoint weakpoint = hit.collider.GetComponent<Weakpoint>();
            if (weakpoint != null)
            {
                weakpoint.OnHit(damage);
                Debug.Log($"Hit boss weakpoint for {damage} damage!");
                
                // Show hit text for weakpoint
                if (HitTextManager.Instance != null)
                {
                    HitTextManager.Instance.ShowHitText(hit.point, 1, false);
                }
                return;
            }
            
            // Check for Boss Health
            BossHealth bossHealth = hit.collider.GetComponentInParent<BossHealth>();
            if (bossHealth != null)
            {
                // Normal body hit (reduced damage or no damage)
                bossHealth.TakeDamage(Mathf.RoundToInt(damage * 0.5f), false);
                Debug.Log($"Hit boss body for {damage * 0.5f} damage!");
                
                // Show hit text
                if (HitTextManager.Instance != null)
                {
                    HitTextManager.Instance.ShowHitText(hit.point, 1, false);
                }
                return;
            }
            
            // Check for regular Zombie
            ZombieHealth enemy = hit.collider.GetComponentInParent<ZombieHealth>();
            
            if (enemy != null)
            {
                bool isHeadshot = hit.collider.CompareTag("ZombieHead");
                int finalDamage = isHeadshot ? damage * 5 : damage;
                
                int hitCount = UpdateHitCount(enemy);
                
                if (HitTextManager.Instance != null)
                {
                    HitTextManager.Instance.ShowHitText(hit.point, hitCount, isHeadshot);
                }
                
                enemy.TakeDamage(finalDamage);
                
                if (isHeadshot)
                    Debug.Log($"HEADSHOT! Dealt {finalDamage} damage!");
                else
                    Debug.Log($"Hit {enemy.name} for {finalDamage} damage! Hit count: {hitCount}");
            }
        }
    }

    void ShootShotgun()
    {
        for(int i = 0; i < pelletCount; i++)
        {
            Vector3 direction = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0)).direction;
            
            direction = Quaternion.Euler(
                Random.Range(-spread, spread),
                Random.Range(-spread, spread),
                0
            ) * direction;
            
            Ray ray = new Ray(playerCamera.transform.position, direction);
            RaycastHit hit;
            
            if (Physics.Raycast(ray, out hit, shootDistance))
            {
                // Check for Weakpoint first
                Weakpoint weakpoint = hit.collider.GetComponent<Weakpoint>();
                if (weakpoint != null)
                {
                    weakpoint.OnHit(damage);
                    if (HitTextManager.Instance != null)
                    {
                        HitTextManager.Instance.ShowHitText(hit.point, 1, false);
                    }
                    continue;
                }
                
                // Check for Boss Health
                BossHealth bossHealth = hit.collider.GetComponentInParent<BossHealth>();
                if (bossHealth != null)
                {
                    bossHealth.TakeDamage(Mathf.RoundToInt(damage * 0.5f), false);
                    if (HitTextManager.Instance != null)
                    {
                        HitTextManager.Instance.ShowHitText(hit.point, 1, false);
                    }
                    continue;
                }
                
                // Check for regular Zombie
                ZombieHealth enemy = hit.collider.GetComponentInParent<ZombieHealth>();
                
                if (enemy != null)
                {
                    bool isHeadshot = hit.collider.CompareTag("ZombieHead");
                    int finalDamage = isHeadshot ? damage * 5 : damage;
                    
                    int hitCount = UpdateHitCount(enemy);
                    
                    if (HitTextManager.Instance != null)
                    {
                        HitTextManager.Instance.ShowHitText(hit.point, hitCount, isHeadshot);
                    }
                    
                    enemy.TakeDamage(finalDamage);
                }
            }
        }
    }
    // Add this helper method to track hit counts
    int UpdateHitCount(ZombieHealth zombie)
    {
        float currentTime = Time.time;
        
        if (zombieHitTimers.ContainsKey(zombie))
        {
            if (currentTime - zombieHitTimers[zombie] <= hitResetTime)
            {
                // Same zombie hit within time window
                int newCount = zombieHitCounts[zombie] + 1;
                zombieHitCounts[zombie] = newCount;
                zombieHitTimers[zombie] = currentTime;
                return newCount;
            }
            else
            {
                // Reset for this zombie
                zombieHitCounts[zombie] = 1;
                zombieHitTimers[zombie] = currentTime;
                return 1;
            }
        }
        else
        {
            // First hit on this zombie
            zombieHitTimers.Add(zombie, currentTime);
            zombieHitCounts.Add(zombie, 1);
            return 1;
        }
    }

    // Add cleanup in OnDisable or when gun is destroyed
    void OnDisable()
    {
        zombieHitTimers.Clear();
        zombieHitCounts.Clear();
    }


    public void SetupGun(Camera cam, AimSystem aim)
    {
        playerCamera = cam;
        aimSystem = aim;
        originalGunPos = transform.localPosition;
    }
    
    public void OnWeaponSwitch()
    {
        StopShooting(); // Add this line
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

    void ApplyRecoil()
    {
        recoilOffset -= new Vector3(
            Random.Range(-0.01f, 0.01f),
            Random.Range(-0.01f, 0.01f),
            recoilKick
        );

        recoilRot -= recoilRotation + Random.Range(-0.5f, 0.5f);
    }
    public void StartShooting()
    {
        if (isReloading) return;
        
        // Handle no ammo BEFORE the coroutine
        if (currentAmmo <= 0)
        {
            // Show "No Ammo" warning
            if (HitTextManager.Instance != null)
            {
                HitTextManager.Instance.ShowNoAmmoText();
            }
            
            if (audioSource != null && dryFireSound != null)
                audioSource.PlayOneShot(dryFireSound);
            
            return;  // Don't start shooting
        }
        
        if (!isShooting)
        {
            isShooting = true;
            if (autoFireCoroutine != null)
                StopCoroutine(autoFireCoroutine);
            autoFireCoroutine = StartCoroutine(AutoFire());
        }
    }

    public void StopShooting()
    {
        isShooting = false;
        if (autoFireCoroutine != null)
        {
            StopCoroutine(autoFireCoroutine);
            autoFireCoroutine = null;
        }
    }

    IEnumerator AutoFire()
    {
        while (isShooting && currentAmmo > 0 && !isReloading)
        {
            Shoot();
            yield return new WaitForSeconds(fireRate);
        }
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

        // Stop shooting while reloading
        StopShooting();
        
        isReloading = true;

        bool wasAiming = aimSystem != null && aimSystem.IsAiming();

        if(wasAiming)
            aimSystem.StopAim();

        if(gunAnimator)
            gunAnimator.SetTrigger("Reload");

        if(audioSource != null && reloadSound != null)
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
        if (ammoText != null)
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