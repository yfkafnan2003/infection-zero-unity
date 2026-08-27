using UnityEngine;
using UnityEngine.UI;
using System.Collections;
public class WeaponManager : MonoBehaviour
{
    [Header("Weapon Slots")]
    public Gun[] weapons = new Gun[3]; // 0 pistol, 1 heavy, 2 heavy
    int currentWeaponIndex = 0;

    [Header("Weapon Holder")]
    public Transform weaponHolder;

    [Header("UI")]
    public Image[] weaponIcons; // 3 buttons icons
    public Color selectedColor = Color.white;
    public Color normalColor = Color.gray;
    
    [Header("Weapon Switch Animation")]
    public float switchSpeed = 8f;
    public float downOffset = -0.6f;

    bool switching = false;
    public AimSystem aimSystem;

    public void InitializeWeapons()
    {
        // First, deactivate all weapons and setup references
        for(int i = 0; i < weapons.Length; i++)
        {
            if (weapons[i] != null)
            {
                weapons[i].gameObject.SetActive(false);
                weapons[i].aimSystem = aimSystem;
                weapons[i].playerCamera = aimSystem.playerCamera;
                
                // Make sure Animator reference is set (find in child if needed)
                if (weapons[i].gunAnimator == null && weapons[i].transform.childCount > 0)
                {
                    weapons[i].gunAnimator = weapons[i].transform.GetChild(0).GetComponent<Animator>();
                }
            }
        }

        // Find first valid weapon to activate
        currentWeaponIndex = -1;
        for (int i = 0; i < weapons.Length; i++)
        {
            if (weapons[i] != null)
            {
                currentWeaponIndex = i;
                break;
            }
        }
        
        if (currentWeaponIndex >= 0 && weapons[currentWeaponIndex] != null)
        {
            weapons[currentWeaponIndex].gameObject.SetActive(true);
            if (aimSystem != null)
                aimSystem.gun = weapons[currentWeaponIndex].transform;
        }

        UpdateUI();
    }
    public void UpdateWeaponUIVisibility()
    {
        // Only show weapon icons for weapons that exist
        for (int i = 0; i < weaponIcons.Length; i++)
        {
            if (weaponIcons[i] != null)
            {
                // Hide icon if weapon doesn't exist
                weaponIcons[i].gameObject.SetActive(weapons[i] != null);
            }
        }
    }
    public void EquipWeapon(int index)
    {
        if(index < 0 || index >= weapons.Length) return;
        if(index == currentWeaponIndex) return;

        StartCoroutine(SwitchWeaponRoutine(index));
    }
    IEnumerator SwitchWeaponRoutine(int newIndex)
    {
        switching = true;

        Transform currentGun = weapons[currentWeaponIndex].transform;

        Vector3 startPos = currentGun.localPosition;
        Vector3 downPos = startPos + new Vector3(0, downOffset, 0);

        float t = 0;

        // Move current gun down

        while(t < 1)
        {
            t += Time.deltaTime * switchSpeed;
            currentGun.localPosition = Vector3.Lerp(startPos, downPos, t);
            yield return null;
        }

        weapons[currentWeaponIndex].OnWeaponSwitch();
        weapons[currentWeaponIndex].gameObject.SetActive(false);

        currentWeaponIndex = newIndex;

        Gun newGun = weapons[currentWeaponIndex];

        newGun.gameObject.SetActive(true);

        if(aimSystem != null)
            aimSystem.gun = newGun.transform;

        Transform newGunTransform = newGun.transform;

        newGunTransform.localPosition = downPos;

        t = 0;

        // Move new gun up
        while(t < 1)
        {
            t += Time.deltaTime * switchSpeed;
            newGunTransform.localPosition = Vector3.Lerp(downPos, startPos, t);
            yield return null;
        }

        newGunTransform.localPosition = startPos;

        newGun.UpdateAmmoUI();
        UpdateUI();

        switching = false;
    }
    public void ShootCurrentWeapon()
    {
        Gun gun = GetCurrentWeapon();
        if (gun != null)
        {
            gun.StartShooting(); // Changed from gun.Shoot()
        }
    }

    public void StopShootingCurrentWeapon()
    {
        Gun gun = GetCurrentWeapon();
        if (gun != null)
        {
            gun.StopShooting();
        }
    }
    public void ReloadCurrentWeapon()
    {
        Gun gun = GetCurrentWeapon();

        if(gun != null)
            gun.ReloadButton();
    }
    public void UpdateUI()
    {
        for(int i = 0; i < weaponIcons.Length; i++)
        {
            if(i == currentWeaponIndex)
                weaponIcons[i].color = selectedColor;
            else
                weaponIcons[i].color = normalColor;
        }
    }

    public Gun GetCurrentWeapon()
    {
        return weapons[currentWeaponIndex];
    }
}