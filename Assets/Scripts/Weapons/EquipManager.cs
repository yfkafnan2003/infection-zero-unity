using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

public class EquipManager : MonoBehaviour
{
    [Header("Equip UI")]
    public GameObject equipPanel;
    public Transform ownedGunsContainer;
    public GameObject equipSlotPrefab;
    [Header("Audio")]
    public AudioSource buttonSoundSource;
    public AudioClip buttonClickSound;
    [Header("UI Elements")]
    public TextMeshProUGUI[] slotTypeTexts;
    public Image[] slotIcons;
    public Button[] slotButtons;
    public TextMeshProUGUI warningText;
    [Header("Utility Slots")]
    public Image utilityIcon;
    public TextMeshProUGUI utilityCountText;
    public Button utilityButton;
    public Button utilityLeftButton;   // Add this
    public Button utilityRightButton;  // Add this
    private int equippedUtility = -1;
    private int selectedUtilityIndex = 0;  // Add this for cycling

    [Header("References")]
    public ShopManager shopManager;
    public WeaponInventory weaponInventory;
    
    private float warningTimer = 0f;
    private string[] equippedGunNames = new string[3];
    
    void Start()
    {
        if (weaponInventory == null)
            weaponInventory = FindObjectOfType<WeaponInventory>();
            
        if (shopManager == null)
            shopManager = FindObjectOfType<ShopManager>();
        for (int i = 0; i < slotIcons.Length; i++)
        {
            if (slotIcons[i] != null)
                slotIcons[i].gameObject.SetActive(false);
        }
        LoadEquippedGuns();
        ValidateEquippedGuns();
        UpdateEquipUI();
        RefreshOwnedGunsList();
        LoadEquippedUtilities();
        SetupUtilityNavigation();  // Add this
        UpdateUtilityDisplay();  
    }
    
    void Update()
    {
        if (warningTimer > 0)
        {
            warningTimer -= Time.deltaTime;
            if (warningTimer <= 0 && warningText != null)
                warningText.gameObject.SetActive(false);
        }
    }
    void LoadEquippedUtilities()
    {
        equippedUtility = PlayerPrefs.GetInt("EquippedUtility", -1);
    }
    public int GetEquippedUtilityIndex() 
    { 
        return equippedUtility; 
    }
    void SaveEquippedUtilities()
    {
        PlayerPrefs.SetInt("EquippedUtility", equippedUtility);
        PlayerPrefs.Save();
    }
    void SetupUtilityNavigation()
    {
        if (utilityLeftButton != null)
        {
            utilityLeftButton.onClick.RemoveAllListeners();
            utilityLeftButton.onClick.AddListener(PreviousUtility);
        }
        
        if (utilityRightButton != null)
        {
            utilityRightButton.onClick.RemoveAllListeners();
            utilityRightButton.onClick.AddListener(NextUtility);
        }
    }
    void PreviousUtility()
    {
        if (shopManager == null || shopManager.allUtilities.Count == 0) return;
        
        PlayButtonSound();
        selectedUtilityIndex--;
        if (selectedUtilityIndex < 0)
            selectedUtilityIndex = shopManager.allUtilities.Count - 1;
        
        UpdateUtilityDisplay();
    }

    void NextUtility()
    {
        if (shopManager == null || shopManager.allUtilities.Count == 0) return;
        
        PlayButtonSound();
        selectedUtilityIndex++;
        if (selectedUtilityIndex >= shopManager.allUtilities.Count)
            selectedUtilityIndex = 0;
        
        UpdateUtilityDisplay();
    }

    void UpdateUtilityDisplay()
    {
        if (shopManager == null || shopManager.allUtilities.Count == 0) return;
        
        UtilityData utility = shopManager.allUtilities[selectedUtilityIndex];
        
        if (utilityIcon != null && utility.utilityIcon != null)
            utilityIcon.sprite = utility.utilityIcon;
        
        if (utilityCountText != null)
            utilityCountText.text = $"{utility.utilityName}\nOwned: {utility.currentCount}";
        
        // Update equip button text based on whether this utility is equipped
        if (utilityButton != null)
        {
            if (equippedUtility == selectedUtilityIndex)
            {
                var buttonText = utilityButton.GetComponentInChildren<TextMeshProUGUI>();
                if (buttonText != null) buttonText.text = "EQUIPPED";
                ColorBlock colors = utilityButton.colors;
                colors.normalColor = new Color(0.5f, 0.5f, 0.5f);
                utilityButton.colors = colors;
            }
            else if (utility.isUnlocked && utility.currentCount > 0)
            {
                var buttonText = utilityButton.GetComponentInChildren<TextMeshProUGUI>();
                if (buttonText != null) buttonText.text = "EQUIP";
                ColorBlock colors = utilityButton.colors;
                colors.normalColor = Color.white;
                utilityButton.colors = colors;
            }
            else
            {
                var buttonText = utilityButton.GetComponentInChildren<TextMeshProUGUI>();
                if (buttonText != null) buttonText.text = "LOCKED";
                ColorBlock colors = utilityButton.colors;
                colors.normalColor = new Color(0.3f, 0.3f, 0.3f);
                utilityButton.colors = colors;
            }
        }
    }
    void UpdateUtilityUI()
    {
        if (shopManager == null) return;
        
        // Find first owned utility to display if nothing equipped
        if (equippedUtility == -1)
        {
            foreach (UtilityData utility in shopManager.allUtilities)
            {
                if (utility.isUnlocked && utility.currentCount > 0)
                {
                    // Auto-select the first owned utility
                    equippedUtility = GetUtilityIndex(utility.utilityType);
                    SaveEquippedUtilities();
                    break;
                }
            }
        }
        
        if (utilityIcon != null)
        {
            if (equippedUtility >= 0 && equippedUtility < shopManager.allUtilities.Count)
            {
                UtilityData utility = shopManager.allUtilities[equippedUtility];
                utilityIcon.sprite = utility.utilityIcon;
                utilityIcon.gameObject.SetActive(true);
                
                if (utilityCountText != null)
                    utilityCountText.text = $"{utility.currentCount}";
            }
            else
            {
                utilityIcon.gameObject.SetActive(false);
                if (utilityCountText != null)
                    utilityCountText.text = "";
            }
        }
        
        if (utilityButton != null)
        {
            utilityButton.onClick.RemoveAllListeners();
            utilityButton.onClick.AddListener(() => OnUtilitySlotClick());
        }
    }
    public void OnUtilitySlotClick()
    {
        if (shopManager == null) return;
        
        UtilityData utility = shopManager.allUtilities[selectedUtilityIndex];
        
        if (equippedUtility == selectedUtilityIndex)
        {
            // Unequip
            equippedUtility = -1;
            SaveEquippedUtilities();
            ShowWarning($"Unequipped {utility.utilityName}");
        }
        else if (utility.isUnlocked && utility.currentCount > 0)
        {
            // Equip
            equippedUtility = selectedUtilityIndex;
            SaveEquippedUtilities();
            ShowWarning($"Equipped {utility.utilityName}");
        }
        else
        {
            ShowWarning($"Cannot equip {utility.utilityName}. Not owned or no count!");
            return;
        }
        
        UpdateUtilityDisplay();
    }

    int GetUtilityIndex(UtilityType type)
    {
        for (int i = 0; i < shopManager.allUtilities.Count; i++)
        {
            if (shopManager.allUtilities[i].utilityType == type)
                return i;
        }
        return -1;
    }
    void LoadEquippedGuns()
    {
        for (int i = 0; i < 3; i++)
        {
            equippedGunNames[i] = PlayerPrefs.GetString($"EquippedGun_{i}", "");
        }
        
        // Ensure slot 0 has a pistol if empty
        if (string.IsNullOrEmpty(equippedGunNames[0]) && shopManager != null)
        {
            foreach (GunData gun in shopManager.allGuns)
            {
                if (gun.gunType == GunType.Pistol && gun.isOwned)
                {
                    equippedGunNames[0] = gun.gunName;
                    SaveEquippedGuns();
                    break;
                }
            }
        }
    }
    
    void SaveEquippedGuns()
    {
        for (int i = 0; i < 3; i++)
        {
            PlayerPrefs.SetString($"EquippedGun_{i}", equippedGunNames[i]);
        }
        PlayerPrefs.Save();
        
        if (weaponInventory != null)
        {
            weaponInventory.equippedGunNames = equippedGunNames;
            weaponInventory.SaveInventory();
        }
    }
    
    void ValidateEquippedGuns()
    {
        // Check each slot has correct weapon type
        for (int i = 0; i < 3; i++)
        {
            if (!string.IsNullOrEmpty(equippedGunNames[i]))
            {
                GunData gun = FindGunByName(equippedGunNames[i]);
                if (gun != null)
                {
                    bool isValid = false;
                    switch (i)
                    {
                        case 0: isValid = (gun.gunType == GunType.Pistol); break;
                        case 1: isValid = (gun.gunType == GunType.Shotgun); break;
                        case 2: isValid = (gun.gunType == GunType.Machinegun); break;
                    }
                    
                    if (!isValid)
                    {
                        equippedGunNames[i] = "";
                    }
                }
                else
                {
                    equippedGunNames[i] = "";
                }
            }
        }
        
        // Ensure at least one weapon is equipped
        bool hasAnyWeapon = false;
        for (int i = 0; i < 3; i++)
        {
            if (!string.IsNullOrEmpty(equippedGunNames[i]))
            {
                hasAnyWeapon = true;
                break;
            }
        }
        
        if (!hasAnyWeapon)
        {
            foreach (GunData gun in shopManager.allGuns)
            {
                if (gun.gunType == GunType.Pistol && gun.isOwned)
                {
                    equippedGunNames[0] = gun.gunName;
                    break;
                }
            }
        }
        
        SaveEquippedGuns();
    }
    
    void UpdateEquipUI()
    {
        string[] slotNames = { "PISTOL", "SHOTGUN", "MACHINEGUN" };
        int equippedCount = GetEquippedWeaponCount();
        
        for (int i = 0; i < 3 && i < slotTypeTexts.Length; i++)
        {
            // Update slot text
            if (slotTypeTexts[i] != null)
            {
                if (!string.IsNullOrEmpty(equippedGunNames[i]))
                {
                    GunData gun = FindGunByName(equippedGunNames[i]);
                    if (gun != null)
                    {
                        slotTypeTexts[i].text = $"{slotNames[i]}\n{GetShortName(gun.gunName)}";
                    }
                }
                else
                {
                    slotTypeTexts[i].text = $"{slotNames[i]}\nEMPTY";
                }
            }
            
            // Update slot icon
            if (slotIcons[i] != null)
            {
                if (!string.IsNullOrEmpty(equippedGunNames[i]) && shopManager != null)
                {
                    GunData gunData = FindGunByName(equippedGunNames[i]);
                    if (gunData != null && gunData.gunIcon != null)
                    {
                        slotIcons[i].sprite = gunData.gunIcon;
                        slotIcons[i].gameObject.SetActive(true);  // Enable when weapon equipped
                    }
                    else
                    {
                        slotIcons[i].gameObject.SetActive(false); // Disable if no icon
                    }
                }
                else
                {
                    slotIcons[i].sprite = null;
                    slotIcons[i].gameObject.SetActive(false); // Disable when slot is empty
                }
            }
            
            // Update slot button color based on whether it has a weapon and if it's the only one
            if (slotButtons[i] != null)
            {
                ColorBlock colors = slotButtons[i].colors;
                
                if (!string.IsNullOrEmpty(equippedGunNames[i]))
                {
                    // Slot has a weapon
                    if (equippedCount <= 1)
                    {
                        // Only weapon - can't unequip, show as disabled/ash color
                        colors.normalColor = new Color(0.5f, 0.5f, 0.5f);
                        colors.disabledColor = new Color(0.5f, 0.5f, 0.5f);
                        slotButtons[i].interactable = false;
                        
                        // Change text to "Equipped"
                        var buttonText = slotButtons[i].GetComponentInChildren<TextMeshProUGUI>();
                        if (buttonText != null)
                        {
                            buttonText.text = "EQUIPPED";
                            buttonText.fontSize = 14;
                        }
                    }
                    else
                    {
                        // Can be unequipped
                        colors.normalColor = Color.white;
                        slotButtons[i].interactable = true;
                        
                        var buttonText = slotButtons[i].GetComponentInChildren<TextMeshProUGUI>();
                        if (buttonText != null)
                        {
                            buttonText.text = "UNEQUIP";
                            buttonText.fontSize = 14;
                        }
                    }
                }
                else
                {
                    // Empty slot
                    colors.normalColor = Color.white;
                    slotButtons[i].interactable = true;
                    
                    var buttonText = slotButtons[i].GetComponentInChildren<TextMeshProUGUI>();
                    if (buttonText != null)
                    {
                        buttonText.text = "EMPTY";
                        buttonText.fontSize = 14;
                    }
                }
                slotButtons[i].colors = colors;
            }
        }
    }
    
    int GetEquippedWeaponCount()
    {
        int count = 0;
        for (int i = 0; i < equippedGunNames.Length; i++)
        {
            if (!string.IsNullOrEmpty(equippedGunNames[i]))
                count++;
        }
        return count;
    }
    
    string GetShortName(string fullName)
    {
        if (fullName.Contains("_"))
        {
            string[] parts = fullName.Split('_');
            if (parts.Length > 1)
                return parts[1].Replace("_", " ");
        }
        return fullName;
    }
    
    GunData FindGunByName(string gunName)
    {
        if (shopManager == null) return null;
        
        foreach (GunData gun in shopManager.allGuns)
        {
            if (gun.gunName == gunName)
                return gun;
        }
        return null;
    }
    
    public void OpenEquipPanel()
    {
        if (equipPanel != null)
        {
            equipPanel.SetActive(true);
            RefreshOwnedGunsList();
            UpdateEquipUI();
            UpdateUtilityDisplay();  // Add this
        }
    }
    
    public void CloseEquipPanel()
    {
        if (equipPanel != null)
            equipPanel.SetActive(false);
    }
    
    void RefreshOwnedGunsList()
    {
        if (ownedGunsContainer != null)
        {
            foreach (Transform child in ownedGunsContainer)
            {
                Destroy(child.gameObject);
            }
        }
        
        if (shopManager == null) return;
        
        // Create buttons for all owned guns
        foreach (GunData gun in shopManager.allGuns)
        {
            if (gun.isOwned)
            {
                GameObject slotObj = Instantiate(equipSlotPrefab, ownedGunsContainer);
                EquipSlotUI slotUI = slotObj.GetComponent<EquipSlotUI>();
                if (slotUI != null)
                {
                    slotUI.Setup(gun, this);
                }
                else
                {
                    slotUI = slotObj.AddComponent<EquipSlotUI>();
                    slotUI.Setup(gun, this);
                }
            }
        }
    }
    
    bool IsGunEquipped(string gunName)
    {
        for (int i = 0; i < equippedGunNames.Length; i++)
        {
            if (equippedGunNames[i] == gunName)
                return true;
        }
        return false;
    }
    public void PlayButtonSound()
    {
        if (buttonSoundSource != null && buttonClickSound != null)
            buttonSoundSource.PlayOneShot(buttonClickSound);
    }
    // Called when clicking on a gun in the owned guns list
    public void OnGunButtonClick(GunData gun)
    {
        PlayButtonSound();
        // Check if gun is already equipped
        if (IsGunEquipped(gun.gunName))
        {
            ShowWarning($"{gun.gunName} is already equipped!");
            return;
        }
        
        // Find the correct slot for this gun type
        int targetSlot = -1;
        switch (gun.gunType)
        {
            case GunType.Pistol: targetSlot = 0; break;
            case GunType.Shotgun: targetSlot = 1; break;
            case GunType.Machinegun: targetSlot = 2; break;
        }
        
        if (targetSlot == -1)
        {
            ShowWarning("Invalid gun type!");
            return;
        }
        
        // Check if target slot already has a weapon (swap)
        if (!string.IsNullOrEmpty(equippedGunNames[targetSlot]))
        {
            string swappedGun = equippedGunNames[targetSlot];
            equippedGunNames[targetSlot] = gun.gunName;
            ShowWarning($"Swapped {gun.gunName} with {swappedGun}");
        }
        else
        {
            // Empty slot, just equip
            equippedGunNames[targetSlot] = gun.gunName;
            ShowWarning($"{gun.gunName} equipped to slot {targetSlot + 1}");
        }
        
        // Save and refresh UI
        SaveEquippedGuns();
        UpdateEquipUI();
        RefreshOwnedGunsList();
    }
    
    // Called when clicking on an equipped slot button (UNEQUIP)
    public void OnSlotButtonClick(int slotIndex)
    {
        PlayButtonSound();
        // Check if slot has a weapon
        if (string.IsNullOrEmpty(equippedGunNames[slotIndex]))
        {
            ShowWarning("This slot is already empty!");
            return;
        }
        
        // Check if this is the only weapon
        int equippedCount = GetEquippedWeaponCount();
        if (equippedCount <= 1)
        {
            ShowWarning("You must have at least one weapon equipped!");
            return;
        }
        
        // Unequip the weapon
        string unequippedGun = equippedGunNames[slotIndex];
        equippedGunNames[slotIndex] = "";
        SaveEquippedGuns();
        UpdateEquipUI();
        RefreshOwnedGunsList();
        ShowWarning($"Unequipped {unequippedGun}");
    }
    
    void ShowWarning(string message)
    {
        if (warningText != null)
        {
            warningText.text = message;
            warningText.gameObject.SetActive(true);
            warningTimer = 2f;
        }
        Debug.Log(message);
    }
    
    public string[] GetEquippedGunNames()
    {
        return equippedGunNames;
    }
}