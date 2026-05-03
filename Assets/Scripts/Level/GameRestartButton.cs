using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;

public class GameRestartButton : MonoBehaviour
{
    [Header("Restart UI")]
    public Button restartButton;
    public GameObject confirmationPanel;
    public Button confirmYesButton;
    public Button confirmNoButton;
    public TextMeshProUGUI warningText;
    
    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip buttonClickSound;
    
    private void Start()
    {
        if (restartButton != null)
            restartButton.onClick.AddListener(ShowConfirmationPanel);
        
        if (confirmYesButton != null)
            confirmYesButton.onClick.AddListener(ConfirmRestart);
        
        if (confirmNoButton != null)
            confirmNoButton.onClick.AddListener(HideConfirmationPanel);
        
        // Initially hide confirmation panel
        if (confirmationPanel != null)
            confirmationPanel.SetActive(false);
    }
    
    void ShowConfirmationPanel()
    {
        PlaySound();
        if (confirmationPanel != null)
            confirmationPanel.SetActive(true);
    }
    
    void HideConfirmationPanel()
    {
        PlaySound();
        if (confirmationPanel != null)
            confirmationPanel.SetActive(false);
    }
    
    void ConfirmRestart()
    {
        PlaySound();
        
        if (warningText != null)
            warningText.text = "Restarting game...";
        
        // Start restart coroutine
        StartCoroutine(RestartGame());
    }
    
    IEnumerator RestartGame()
    {
        // Show loading indicator
        if (warningText != null)
            warningText.gameObject.SetActive(true);
        
        // 1. Reset GameManager progress (keep Glocky)
        if (GameManager.instance != null)
        {
            // Reset player progress
            GameManager.instance.playerLevel = 1;
            GameManager.instance.playerMoney = 0;
            GameManager.instance.currentXP = 0;
            GameManager.instance.xpToNextLevel = 100;
            GameManager.instance.currentEnergy = GameManager.instance.maxEnergy;
            GameManager.instance.currentChainIndex = 0;
            GameManager.instance.completedPOIs.Clear();
            
            // Reset tutorial progress
            if (TutorialManager.Instance != null)
            {
                TutorialManager.Instance.ResetTutorialProgress();
            }
            
            GameManager.instance.SaveAllData();
            Debug.Log("GameManager reset complete");
        }
        
        // 2. Reset ShopManager - remove all guns except Glocky
        ShopManager shopManager = FindObjectOfType<ShopManager>();
        if (shopManager != null)
        {
            // Reset all guns in shop
            foreach (GunData gun in shopManager.allGuns)
            {
                if (gun.gunName != "Glocky")
                {
                    gun.isOwned = false;
                    gun.currentUpgradeLevel = 0;
                    PlayerPrefs.DeleteKey(gun.gunName + "_Owned");
                    PlayerPrefs.DeleteKey(gun.gunName + "_Upgrade");
                }
                else
                {
                    gun.isOwned = true;
                    gun.currentUpgradeLevel = 0;
                    PlayerPrefs.SetInt(gun.gunName + "_Owned", 1);
                    PlayerPrefs.SetInt(gun.gunName + "_Upgrade", 0);
                }
            }
            
            // Reset utilities
            foreach (UtilityData utility in shopManager.allUtilities)
            {
                utility.isUnlocked = false;
                utility.currentCount = 0;
                PlayerPrefs.DeleteKey(utility.utilityName + "_Unlocked");
                PlayerPrefs.DeleteKey(utility.utilityName + "_Count");
            }
            
            PlayerPrefs.Save();
            Debug.Log("ShopManager reset complete");
        }
        
        // 3. Reset EquipManager - only Glocky equipped
        EquipManager equipManager = FindObjectOfType<EquipManager>();
        if (equipManager != null)
        {
            // Find Glocky
            string glockyName = "Glocky";
            
            // Reset equipped guns - only Glocky in pistol slot
            PlayerPrefs.SetString("EquippedGun_0", glockyName);
            PlayerPrefs.SetString("EquippedGun_1", "");
            PlayerPrefs.SetString("EquippedGun_2", "");
            
            // Reset equipped utility
            PlayerPrefs.SetInt("EquippedUtility", -1);
            
            PlayerPrefs.Save();
            Debug.Log("EquipManager reset complete");
        }
        
        // 4. Reset WeaponInventory if exists
        WeaponInventory weaponInventory = FindObjectOfType<WeaponInventory>();
        if (weaponInventory != null)
        {
            weaponInventory.SaveInventory();
            Debug.Log("WeaponInventory reset complete");
        }
        
        // 5. Clear all PlayerPrefs except audio/settings
        // Save settings before clearing
        float normalSens = PlayerPrefs.GetFloat("NormalSensitivity", 1.5f);
        float aimSens = PlayerPrefs.GetFloat("AimSensitivity", 1f);
        float masterVol = PlayerPrefs.GetFloat("MasterVolume", 0.75f);
        float musicVol = PlayerPrefs.GetFloat("MusicVolume", 0.75f);
        float sfxVol = PlayerPrefs.GetFloat("SFXVolume", 0.75f);
        
        // Clear all PlayerPrefs
        PlayerPrefs.DeleteAll();
        
        // Restore settings
        PlayerPrefs.SetFloat("NormalSensitivity", normalSens);
        PlayerPrefs.SetFloat("AimSensitivity", aimSens);
        PlayerPrefs.SetFloat("MasterVolume", masterVol);
        PlayerPrefs.SetFloat("MusicVolume", musicVol);
        PlayerPrefs.SetFloat("SFXVolume", sfxVol);
        
        // Restore Glocky ownership
        PlayerPrefs.SetInt("Glocky_Owned", 1);
        PlayerPrefs.SetInt("Glocky_Upgrade", 0);
        
        // Restore equipped guns
        PlayerPrefs.SetString("EquippedGun_0", "Glocky");
        PlayerPrefs.SetString("EquippedGun_1", "");
        PlayerPrefs.SetString("EquippedGun_2", "");
        
        PlayerPrefs.Save();
        
        yield return new WaitForSeconds(0.5f);
        
        // 6. Reload the entire application
        // Method 1: Reload current scene (if that's your main menu)
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        
        // Alternative Method 2: Load a specific scene (uncomment if you have a main menu scene)
        // SceneManager.LoadScene("MainMenu");
        
        // Alternative Method 3: Full application restart (only works in built game, not editor)
        // UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }
    
    void PlaySound()
    {
        if (audioSource != null && buttonClickSound != null)
            audioSource.PlayOneShot(buttonClickSound);
    }
}