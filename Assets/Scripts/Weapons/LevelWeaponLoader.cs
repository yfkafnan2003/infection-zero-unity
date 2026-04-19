using UnityEngine;
using TMPro;
using System.Collections.Generic; 
[System.Serializable]
public class WeaponCustomization
{
    public Vector3 position = new Vector3(0.36f, -0.18f, 0.81f);
    public Vector3 rotation = new Vector3(0f, -98.495f, 0f);
}

public class LevelWeaponLoader : MonoBehaviour
{
    [Header("References")]
    public WeaponManager weaponManager;
    public Transform weaponHolder;
    public AimSystem aimSystem;
    public ShopManager shopManager;
    
    [Header("Gun Prefabs (Map gunName to Prefab)")]
    public List<GunPrefabMapping> gunPrefabMappings = new List<GunPrefabMapping>();

    [System.Serializable]
    public class GunPrefabMapping
    {
        public string gunName;  // Must match GunData.gunName exactly
        public GameObject gunPrefab;
        public Sprite gunIcon;  // Add this line
        public WeaponCustomization customization = new WeaponCustomization();
    }
    
    void Start()
    {
        LoadEquippedWeapons();
    }
    
    void LoadEquippedWeapons()
    {
        if (weaponManager == null)
        {
            weaponManager = GetComponent<WeaponManager>();
            if (weaponManager == null)
            {
                Debug.LogError("WeaponManager not found!");
                return;
            }
        }
        
        if (aimSystem == null)
            aimSystem = FindObjectOfType<AimSystem>();
        if (shopManager == null)
        shopManager = FindObjectOfType<ShopManager>();
        if (weaponHolder == null && weaponManager.weaponHolder != null)
            weaponHolder = weaponManager.weaponHolder;
        
        if (weaponHolder == null)
        {
            Debug.LogError("WeaponHolder not found!");
            return;
        }
        
        // Clear existing weapons
        foreach (Transform child in weaponHolder)
        {
            Destroy(child.gameObject);
        }
        
        // Clear weapon manager array
        for (int i = 0; i < weaponManager.weapons.Length; i++)
        {
            weaponManager.weapons[i] = null;
        }
        
        // Get equipped gun names from PlayerPrefs
        string[] equippedGunNames = new string[3];
        bool hasAnyWeapon = false;
        
        for (int i = 0; i < 3; i++)
        {
            equippedGunNames[i] = PlayerPrefs.GetString($"EquippedGun_{i}", "");
            if (!string.IsNullOrEmpty(equippedGunNames[i]))
                hasAnyWeapon = true;
        }
        
        // If no weapons equipped, use default pistol
        if (!hasAnyWeapon)
        {
            equippedGunNames[0] = "Pistol_Default";
        }
        
        // Create weapons for each slot
        for (int i = 0; i < 3; i++)
        {
            GameObject prefabToSpawn = null;
            WeaponCustomization customization = null;
            
            // Only create weapon if it's equipped
            if (string.IsNullOrEmpty(equippedGunNames[i]))
                continue;
            
            // Find the prefab mapping for this gun name
            GunPrefabMapping mapping = gunPrefabMappings.Find(m => m.gunName == equippedGunNames[i]);

            if (mapping != null)
            {
                prefabToSpawn = mapping.gunPrefab;
                customization = mapping.customization;
            }
            else
            {
                Debug.LogError($"No prefab mapping found for gun: {equippedGunNames[i]}");
                continue;
            }
            
            if (prefabToSpawn != null)
            {
                // Create the gun instance (parent with Gun script)
                GameObject gunObj = Instantiate(prefabToSpawn, weaponHolder);
                Gun gun = gunObj.GetComponent<Gun>();
                
                if (gun != null)
                {
                    // Apply custom position and rotation
                    gunObj.transform.localPosition = customization.position;
                    gunObj.transform.localRotation = Quaternion.Euler(customization.rotation);
                    
                    // Set the gun name to match what was saved
                    gun.gunName = equippedGunNames[i];
                    
                    // Set the gun name to match what was saved
                    gun.gunName = equippedGunNames[i];

                    // Get damage from PlayerPrefs (saved by ShopManager)
                    int upgradeLevel = PlayerPrefs.GetInt(gun.gunName + "_Upgrade", 0);
                    int baseDamage = PlayerPrefs.GetInt(gun.gunName + "_BaseDamage", 20); // Default 20 if not found

                    // Calculate final damage
                    int calculatedDamage = baseDamage + (upgradeLevel * 5);
                    gun.damage = calculatedDamage;

                    Debug.Log($"Loaded {gun.gunName} with damage: {calculatedDamage} (Base: {baseDamage}, Upgrade: {upgradeLevel})");

                    // Set important references
                    gun.aimSystem = aimSystem;
                    gun.playerCamera = aimSystem != null ? aimSystem.playerCamera : Camera.main;
                    
                    // FIND AMMO TEXT IN THE SCENE
                    if (gun.ammoText == null)
                    {
                        // Find the ammo text in the scene (adjust the name to match your UI)
                        gun.ammoText = GameObject.Find("AmmoText")?.GetComponent<TextMeshProUGUI>();
                        
                        // If not found by name, try finding any TextMeshProUGUI in the scene
                        if (gun.ammoText == null)
                            gun.ammoText = FindObjectOfType<TextMeshProUGUI>();
                        
                        if (gun.ammoText != null)
                            Debug.Log($"Found ammo text for {gun.gunName}");
                        else
                            Debug.LogWarning($"Could not find ammo text! Make sure there's a TextMeshProUGUI in the scene.");
                    }
                    
                    // ENABLE CHILD MODEL AND GET COMPONENTS
                    if (gunObj.transform.childCount > 0)
                    {
                        Transform childModel = gunObj.transform.GetChild(0);
                        childModel.gameObject.SetActive(true);
                        
                        if (gun.gunAnimator == null)
                            gun.gunAnimator = childModel.GetComponent<Animator>();
                        
                        if (gun.audioSource == null)
                            gun.audioSource = childModel.GetComponent<AudioSource>();
                    }
                    
                    // Setup gun with references
                    gun.SetupGun(gun.playerCamera, aimSystem);
                    
                    // Assign to weapon manager
                    weaponManager.weapons[i] = gun;
                    gunObj.SetActive(false); // Start inactive
                    
                    Debug.Log($"Loaded weapon: {gun.gunName} at slot {i}");
                }
                else
                {
                    Debug.LogError($"Gun component not found on prefab: {prefabToSpawn.name}");
                }
            }
            else
            {
                Debug.LogWarning($"No prefab assigned for slot {i}");
            }
        }
        
        // Initialize weapon manager with created weapons
        weaponManager.InitializeWeapons();
        weaponManager.UpdateWeaponUIVisibility();
        
        // Force update UI after initialization
        weaponManager.UpdateUI();
        
        Debug.Log("Equipped weapons loaded successfully!");
    }
}