using UnityEngine;
using UnityEngine.UI;

public class WeaponIconLoader : MonoBehaviour
{
    public WeaponManager weaponManager;
    public LevelWeaponLoader levelWeaponLoader;
    
    void Start()
    {
        LoadWeaponIcons();
    }
    
    void LoadWeaponIcons()
    {
        if (weaponManager == null)
            weaponManager = FindObjectOfType<WeaponManager>();
        
        if (levelWeaponLoader == null)
            levelWeaponLoader = FindObjectOfType<LevelWeaponLoader>();
        
        if (weaponManager == null || levelWeaponLoader == null) return;
        
        // Assign icons to weapon manager slots
        for (int i = 0; i < weaponManager.weapons.Length && i < weaponManager.weaponIcons.Length; i++)
        {
            if (weaponManager.weapons[i] != null && weaponManager.weaponIcons[i] != null)
            {
                // Find the icon for this gun
                LevelWeaponLoader.GunPrefabMapping mapping = levelWeaponLoader.gunPrefabMappings.Find(m => m.gunName == weaponManager.weapons[i].gunName);
                
                if (mapping != null && mapping.gunIcon != null)
                {
                    weaponManager.weaponIcons[i].sprite = mapping.gunIcon;
                    Debug.Log($"Set icon for {weaponManager.weapons[i].gunName}");
                }
            }
        }
    }
}