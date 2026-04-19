using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class OwnedGunData
{
    public string gunName;
    public GunType gunType;
    public int upgradeLevel;
    public bool isOwned;
}

public class WeaponInventory : MonoBehaviour
{
    public static WeaponInventory instance;
    
    [Header("Guns Database")]
    public List<GunData> allGuns; // Reference to same ScriptableObjects or list from ShopManager
    
    [Header("Equipped Guns")]
    public string[] equippedGunNames = new string[3]; // Store names only
    
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        LoadInventory();
    }
    
    public void LoadInventory()
    {
        // Load owned guns
        foreach (GunData gun in allGuns)
        {
            gun.isOwned = PlayerPrefs.GetInt(gun.gunName + "_Owned", 0) == 1;
            gun.currentUpgradeLevel = PlayerPrefs.GetInt(gun.gunName + "_Upgrade", 0);
        }
        
        // Load equipped guns
        for (int i = 0; i < 3; i++)
        {
            equippedGunNames[i] = PlayerPrefs.GetString($"EquippedGun_{i}", "");
        }
        
        // Ensure first pistol is owned if nothing is equipped
        if (string.IsNullOrEmpty(equippedGunNames[0]))
        {
            foreach (GunData gun in allGuns)
            {
                if (gun.gunType == GunType.Pistol && gun.isOwned)
                {
                    equippedGunNames[0] = gun.gunName;
                    break;
                }
            }
        }
    }
    
    public void SaveInventory()
    {
        foreach (GunData gun in allGuns)
        {
            PlayerPrefs.SetInt(gun.gunName + "_Owned", gun.isOwned ? 1 : 0);
            PlayerPrefs.SetInt(gun.gunName + "_Upgrade", gun.currentUpgradeLevel);
        }
        
        for (int i = 0; i < 3; i++)
        {
            PlayerPrefs.SetString($"EquippedGun_{i}", equippedGunNames[i]);
        }
        
        PlayerPrefs.Save();
    }
    
    public GunData GetGunData(string gunName)
    {
        return allGuns.Find(g => g.gunName == gunName);
    }
    
    public string[] GetEquippedGunNames()
    {
        return equippedGunNames;
    }
    
    public void EquipGun(string gunName, int slotIndex)
    {
        if (slotIndex >= 0 && slotIndex < 3)
        {
            equippedGunNames[slotIndex] = gunName;
            SaveInventory();
        }
    }
    
    public bool IsGunOwned(string gunName)
    {
        GunData gun = GetGunData(gunName);
        return gun != null && gun.isOwned;
    }
    
    public int GetGunUpgradeLevel(string gunName)
    {
        GunData gun = GetGunData(gunName);
        return gun != null ? gun.currentUpgradeLevel : 0;
    }
}