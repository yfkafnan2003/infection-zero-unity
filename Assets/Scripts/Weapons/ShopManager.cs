using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
[System.Serializable]
public class GunData
{
    public string gunName;
    public GunType gunType;
    public int baseDamage;
    public float fireRate;
    public float reloadTime;
    public int price;
    public int requiredLevel = 1; // Add level requirement
    public int upgradeCost;
    public Sprite gunIcon;
    public GameObject gunPrefab;
    public bool isOwned;
    public int currentUpgradeLevel; // Max 5 upgrades
}

public class ShopManager : MonoBehaviour
{
    [Header("Shop UI")]
    public GameObject shopPanel;
    public Button leftArrowButton;
    public Button rightArrowButton;
    public Button buyUpgradeButton;
    public TextMeshProUGUI buttonText;
    
    [Header("Reset Buttons")]
    public Button resetStoreButton;
    public Button resetLevelButton;
    public TextMeshProUGUI warningMessageText;
    
    [Header("Audio")]
    public AudioSource buttonSoundSource;
    public AudioClip buttonClickSound;
    
    [Header("Gun Display")]
    public Image gunDisplayImage;
    public TextMeshProUGUI gunNameText;
    public TextMeshProUGUI gunTypeText;
    public TextMeshProUGUI damageText;
    public TextMeshProUGUI reloadTimeText;
    public TextMeshProUGUI fireRateText;
    public TextMeshProUGUI priceText;
    public TextMeshProUGUI requiredLevelText; // Add this
    
    [Header("Stat Sliders")]
    public Slider damageSlider;
    public Slider fireRateSlider;
    public Slider reloadTimeSlider;
    public TextMeshProUGUI damageSliderValue;
    public TextMeshProUGUI fireRateSliderValue;
    public TextMeshProUGUI reloadTimeSliderValue;

    [Header("Upgrade Preview")]
    public Slider upgradeSlider;
    public TextMeshProUGUI upgradePreviewText;

    [Header("Guns Database")]
    public List<GunData> allGuns = new List<GunData>();
    
    [Header("References")]
    public WeaponManager weaponManager;
    public TextMeshProUGUI playerMoneyText;
    public TextMeshProUGUI playerLevelText; // Add this
    
    [Header("Utilities")]
    public GameObject utilitiesPanel;
    public Button utilityLeftButton;
    public Button utilityRightButton;
    public Button buyUtilityButton;
    public TextMeshProUGUI utilityNameText;
    public TextMeshProUGUI utilityPriceText;
    public TextMeshProUGUI utilityRequiredLevelText; 
    public TextMeshProUGUI utilityCountText;
    public Image utilityIconImage;
    public List<UtilityData> allUtilities = new List<UtilityData>();

    private int currentUtilityIndex = 0;
    private UtilityData currentDisplayedUtility;
    private int currentGunIndex = 0;
    private GunData currentDisplayedGun;
    private float warningTimer = 0f;
    
    void Start()
    {
        LoadOwnedGuns();
        SetupShopUI();
        SetupUtilityUI();
        SetupResetButtons();
        UpdatePlayerMoneyDisplay();
        UpdatePlayerLevelDisplay();
    }
    
    void Update()
    {
        if (warningTimer > 0 && warningMessageText != null)
        {
            warningTimer -= Time.deltaTime;
            if (warningTimer <= 0)
                warningMessageText.gameObject.SetActive(false);
        }
    }
    
    void SetupResetButtons()
    {
        if (resetStoreButton != null)
        {
            resetStoreButton.onClick.RemoveAllListeners();
            resetStoreButton.onClick.AddListener(ResetStoreItems);
        }
        
        if (resetLevelButton != null)
        {
            resetLevelButton.onClick.RemoveAllListeners();
            resetLevelButton.onClick.AddListener(ResetLevelProgress);
        }
    }
    
    void ResetStoreItems()
    {
        PlayButtonSound();
        
        // Reset all guns except Glocky
        foreach (GunData gun in allGuns)
        {
            if (gun.gunName != "Glocky")
            {
                gun.isOwned = false;
                gun.currentUpgradeLevel = 0;
                
                // Clear PlayerPrefs for this gun
                PlayerPrefs.DeleteKey(gun.gunName + "_Owned");
                PlayerPrefs.DeleteKey(gun.gunName + "_Upgrade");
            }
            else
            {
                // Ensure Glocky is always owned
                gun.isOwned = true;
                PlayerPrefs.SetInt(gun.gunName + "_Owned", 1);
            }
        }
        
        // Reset utilities
        foreach (UtilityData utility in allUtilities)
        {
            utility.isUnlocked = false;
            utility.currentCount = 0;
            PlayerPrefs.DeleteKey(utility.utilityName + "_Unlocked");
            PlayerPrefs.DeleteKey(utility.utilityName + "_Count");
        }
        
        PlayerPrefs.Save();
        
        // Reload to refresh UI
        LoadOwnedGuns();
        LoadUtilities();
        DisplayGun(currentGunIndex);
        DisplayUtility(currentUtilityIndex);
        UpdatePlayerMoneyDisplay();
        
        ShowWarningMessage("Store items reset! (Glocky kept)");
    }
    
    void ResetLevelProgress()
    {
        PlayButtonSound();
        
        if (GameManager.instance != null)
        {
            GameManager.instance.ResetGameProgress();
            UpdatePlayerLevelDisplay();
            UpdatePlayerMoneyDisplay();
            ShowWarningMessage("Game progress reset! Level 1, $0");
        }
    }
    
    void ShowWarningMessage(string message)
    {
        if (warningMessageText != null)
        {
            warningMessageText.text = message;
            warningMessageText.gameObject.SetActive(true);
            warningTimer = 2f;
        }
    }
    
    void SetupShopUI()
    {
        if (leftArrowButton != null)
            leftArrowButton.onClick.AddListener(PreviousGun);
            
        if (rightArrowButton != null)
            rightArrowButton.onClick.AddListener(NextGun);
            
        if (buyUpgradeButton != null)
            buyUpgradeButton.onClick.AddListener(BuyOrUpgradeGun);
            
        if (allGuns.Count > 0)
        {
            currentGunIndex = 0;
            DisplayGun(currentGunIndex);
        }
    }
    
    void SetupUtilityUI()
    {
        if (utilityLeftButton != null)
            utilityLeftButton.onClick.AddListener(PreviousUtility);
        
        if (utilityRightButton != null)
            utilityRightButton.onClick.AddListener(NextUtility);
        
        if (buyUtilityButton != null)
            buyUtilityButton.onClick.AddListener(BuyUtility);
        
        LoadUtilities();
        
        if (allUtilities.Count > 0)
        {
            currentUtilityIndex = 0;
            DisplayUtility(currentUtilityIndex);
        }
    }

    void LoadUtilities()
    {
        foreach (UtilityData utility in allUtilities)
        {
            utility.isUnlocked = PlayerPrefs.GetInt(utility.utilityName + "_Unlocked", 0) == 1;
            utility.currentCount = PlayerPrefs.GetInt(utility.utilityName + "_Count", 0);
        }
    }

    public void SaveUtilities()
    {
        foreach (UtilityData utility in allUtilities)
        {
            PlayerPrefs.SetInt(utility.utilityName + "_Unlocked", utility.isUnlocked ? 1 : 0);
            PlayerPrefs.SetInt(utility.utilityName + "_Count", utility.currentCount);
        }
        PlayerPrefs.Save();
    }

    void PreviousUtility()
    {
        PlayButtonSound();
        currentUtilityIndex--;
        if (currentUtilityIndex < 0)
            currentUtilityIndex = allUtilities.Count - 1;
        DisplayUtility(currentUtilityIndex);
    }

    void NextUtility()
    {
        PlayButtonSound();
        currentUtilityIndex++;
        if (currentUtilityIndex >= allUtilities.Count)
            currentUtilityIndex = 0;
        DisplayUtility(currentUtilityIndex);
    }

    void DisplayUtility(int index)
    {
        if (index < 0 || index >= allUtilities.Count) return;
        
        currentDisplayedUtility = allUtilities[index];
        
        if (utilityIconImage != null && currentDisplayedUtility.utilityIcon != null)
            utilityIconImage.sprite = currentDisplayedUtility.utilityIcon;
        
        if (utilityNameText != null)
            utilityNameText.text = currentDisplayedUtility.utilityName;
        
        if (utilityPriceText != null)
            utilityPriceText.text = $"Price: ${currentDisplayedUtility.price}";
        
        if (utilityCountText != null)
            utilityCountText.text = $"Owned: {currentDisplayedUtility.currentCount}";
        
        // ADD THIS BLOCK - Show required level for utility
        if (utilityRequiredLevelText != null)
        {
            utilityRequiredLevelText.text = $"Required Level: {currentDisplayedUtility.requiredLevel}";
            
            // Change color based on if player meets requirement
            if (GameManager.instance != null)
            {
                if (GameManager.instance.playerLevel < currentDisplayedUtility.requiredLevel)
                    utilityRequiredLevelText.color = Color.red;
                else
                    utilityRequiredLevelText.color = Color.green;
            }
        }
    }

    void BuyUtility()
    {
        if (currentDisplayedUtility == null) return;
        PlayButtonSound();
        
        // Check level requirement (if any)
        if (GameManager.instance != null && GameManager.instance.playerLevel < currentDisplayedUtility.requiredLevel)
        {
            ShowWarningMessage($"Level {currentDisplayedUtility.requiredLevel} required to buy {currentDisplayedUtility.utilityName}!");
            return;
        }
        
        if (GameManager.instance.playerMoney >= currentDisplayedUtility.price)
        {
            if (currentDisplayedUtility.currentCount < currentDisplayedUtility.maxCount)
            {
                GameManager.instance.AddMoney(-currentDisplayedUtility.price);
                currentDisplayedUtility.currentCount++;
                currentDisplayedUtility.isUnlocked = true;
                SaveUtilities();
                DisplayUtility(currentUtilityIndex);
                UpdatePlayerMoneyDisplay();
                Debug.Log($"Bought {currentDisplayedUtility.utilityName}. Now have: {currentDisplayedUtility.currentCount}");
            }
            else
            {
                ShowWarningMessage($"Max count reached for {currentDisplayedUtility.utilityName}!");
            }
        }
        else
        {
            ShowWarningMessage("Not enough money!");
        }
    }

    void LoadOwnedGuns()
    {
        foreach (GunData gun in allGuns)
        {
            string key = gun.gunName + "_Owned";
            gun.isOwned = PlayerPrefs.GetInt(key, gun.gunName == "Glocky" ? 1 : 0) == 1;
            
            string upgradeKey = gun.gunName + "_Upgrade";
            gun.currentUpgradeLevel = PlayerPrefs.GetInt(upgradeKey, 0);
            
            PlayerPrefs.SetInt(gun.gunName + "_BaseDamage", gun.baseDamage);
        }
        
        PlayerPrefs.Save();
    }
    
    void SaveOwnedGuns()
    {
        foreach (GunData gun in allGuns)
        {
            string key = gun.gunName + "_Owned";
            PlayerPrefs.SetInt(key, gun.isOwned ? 1 : 0);
            
            string upgradeKey = gun.gunName + "_Upgrade";
            PlayerPrefs.SetInt(upgradeKey, gun.currentUpgradeLevel);
        }
        PlayerPrefs.Save();
    }
    
    void PreviousGun()
    {
        PlayButtonSound();
        currentGunIndex--;
        if (currentGunIndex < 0)
            currentGunIndex = allGuns.Count - 1;
        DisplayGun(currentGunIndex);
    }

    void NextGun()
    {
        PlayButtonSound();
        currentGunIndex++;
        if (currentGunIndex >= allGuns.Count)
            currentGunIndex = 0;
        DisplayGun(currentGunIndex);
    }
    
    void DisplayGun(int index)
    {
        if (index < 0 || index >= allGuns.Count) return;
        
        currentDisplayedGun = allGuns[index];
        
        if (gunDisplayImage != null && currentDisplayedGun.gunIcon != null)
            gunDisplayImage.sprite = currentDisplayedGun.gunIcon;
            
        if (gunNameText != null)
            gunNameText.text = currentDisplayedGun.gunName;
            
        if (gunTypeText != null)
            gunTypeText.text = currentDisplayedGun.gunType.ToString();
        
        // Show required level
        if (requiredLevelText != null)
        {
            requiredLevelText.text = $"Required Level: {currentDisplayedGun.requiredLevel}";
            if (GameManager.instance != null && GameManager.instance.playerLevel < currentDisplayedGun.requiredLevel)
                requiredLevelText.color = Color.red;
            else
                requiredLevelText.color = Color.green;
        }
        
        int currentDamage = currentDisplayedGun.baseDamage + (currentDisplayedGun.currentUpgradeLevel * 5);
        
        if (damageText != null)
            damageText.text = $"Damage: {currentDamage}";
            
        if (reloadTimeText != null)
            reloadTimeText.text = $"Reload: {currentDisplayedGun.reloadTime}s";
            
        if (fireRateText != null)
            fireRateText.text = $"Fire Rate: {currentDisplayedGun.fireRate}/s";
        
        if (damageSlider != null)
        {
            float minDmg = 1f;
            float maxDmg = 150f;
            float currentDmg = currentDisplayedGun.baseDamage * Mathf.Pow(1.25f, currentDisplayedGun.currentUpgradeLevel);
            
            damageSlider.minValue = minDmg;
            damageSlider.maxValue = maxDmg;
            damageSlider.value = currentDmg;
            
            if (damageSliderValue != null)
                damageSliderValue.text = $"{currentDmg}";
        }
        
        if (fireRateSlider != null)
        {
            float minFireRate = 0.05f;
            float maxFireRate = 3f;
            float normalizedValue = (maxFireRate - currentDisplayedGun.fireRate) / (maxFireRate - minFireRate);
            fireRateSlider.minValue = 0;
            fireRateSlider.maxValue = 1;
            fireRateSlider.value = normalizedValue;
            
            if (fireRateSliderValue != null)
                fireRateSliderValue.text = $"{currentDisplayedGun.fireRate}/s";
        }
        
        if (reloadTimeSlider != null)
        {
            float minReload = 0.01f;
            float maxReload = 4f;
            float normalizedValue = (maxReload - currentDisplayedGun.reloadTime) / (maxReload - minReload);
            reloadTimeSlider.minValue = 0;
            reloadTimeSlider.maxValue = 1;
            reloadTimeSlider.value = normalizedValue;
            
            if (reloadTimeSliderValue != null)
                reloadTimeSliderValue.text = $"{currentDisplayedGun.reloadTime}s";
        }
        
        if (upgradeSlider != null)
        {
            upgradeSlider.minValue = 1;
            upgradeSlider.maxValue = 150;
            
            if (currentDisplayedGun.isOwned && currentDisplayedGun.currentUpgradeLevel < 5)
            {
                float nextDamageValue = currentDisplayedGun.baseDamage * Mathf.Pow(1.25f, currentDisplayedGun.currentUpgradeLevel + 1);
                upgradeSlider.value = nextDamageValue;
            }
            else
            {
                upgradeSlider.value = upgradeSlider.minValue;
            }
        }
        
        if (upgradePreviewText != null)
        {
            if (currentDisplayedGun.isOwned && currentDisplayedGun.currentUpgradeLevel < 5)
            {
                int upgradeCost = currentDisplayedGun.upgradeCost * (currentDisplayedGun.currentUpgradeLevel + 1);
                int nextDamage = Mathf.RoundToInt(currentDisplayedGun.baseDamage * Mathf.Pow(1.25f, currentDisplayedGun.currentUpgradeLevel + 1));
                upgradePreviewText.text = $"Upgrade: {nextDamage} DMG (Cost: ${upgradeCost})";
            }
            else if (currentDisplayedGun.isOwned && currentDisplayedGun.currentUpgradeLevel >= 5)
            {
                upgradePreviewText.text = "MAX LEVEL REACHED!";
            }
            else
            {
                upgradePreviewText.text = $"Buy to unlock upgrades";
            }
        }
        
        // Check if player meets level requirement
        bool meetsLevelRequirement = GameManager.instance != null && 
                                     GameManager.instance.playerLevel >= currentDisplayedGun.requiredLevel;
        
        if (currentDisplayedGun.isOwned)
        {
            if (currentDisplayedGun.currentUpgradeLevel < 5)
            {
                int upgradeCost = currentDisplayedGun.upgradeCost * (currentDisplayedGun.currentUpgradeLevel + 1);
                if (priceText != null)
                    priceText.text = $"Upgrade Cost: ${upgradeCost}";
                if (buttonText != null)
                    buttonText.text = $"UPGRADE (Level {currentDisplayedGun.currentUpgradeLevel}/5)";
                if (buyUpgradeButton != null)
                    buyUpgradeButton.interactable = true;
            }
            else
            {
                if (priceText != null)
                    priceText.text = "MAX LEVEL";
                if (buttonText != null)
                    buttonText.text = "MAXED";
                if (buyUpgradeButton != null)
                    buyUpgradeButton.interactable = false;
            }
        }
        else
        {
            if (meetsLevelRequirement)
            {
                if (priceText != null)
                    priceText.text = $"Price: ${currentDisplayedGun.price}";
                if (buttonText != null)
                    buttonText.text = "BUY";
                if (buyUpgradeButton != null)
                    buyUpgradeButton.interactable = true;
            }
            else
            {
                if (priceText != null)
                    priceText.text = $"Price: ${currentDisplayedGun.price}";
                if (buttonText != null)
                    buttonText.text = $"REQUIRES LEVEL {currentDisplayedGun.requiredLevel}";
                if (buyUpgradeButton != null)
                    buyUpgradeButton.interactable = false;
            }
        }
    }
    
    void BuyOrUpgradeGun()
    {
        if (currentDisplayedGun == null) return;
        PlayButtonSound();
        
        // Check level requirement for buying
        if (!currentDisplayedGun.isOwned)
        {
            if (GameManager.instance != null && GameManager.instance.playerLevel < currentDisplayedGun.requiredLevel)
            {
                ShowWarningMessage($"Level {currentDisplayedGun.requiredLevel} required to buy {currentDisplayedGun.gunName}!");
                return;
            }
        }
        
        if (currentDisplayedGun.isOwned)
        {
            if (currentDisplayedGun.currentUpgradeLevel < 5)
            {
                int upgradeCost = currentDisplayedGun.upgradeCost * (currentDisplayedGun.currentUpgradeLevel + 1);
                if (GameManager.instance.playerMoney >= upgradeCost)
                {
                    GameManager.instance.AddMoney(-upgradeCost);
                    currentDisplayedGun.currentUpgradeLevel++;
                    SaveOwnedGuns();
                    
                    PlayerPrefs.SetInt(currentDisplayedGun.gunName + "_BaseDamage", currentDisplayedGun.baseDamage);
                    
                    DisplayGun(currentGunIndex);
                    UpdatePlayerMoneyDisplay();
                    Debug.Log($"Upgraded {currentDisplayedGun.gunName} to level {currentDisplayedGun.currentUpgradeLevel}");
                }
                else
                {
                    ShowWarningMessage("Not enough money for upgrade!");
                }
            }
        }
        else
        {
            if (GameManager.instance.playerMoney >= currentDisplayedGun.price)
            {
                GameManager.instance.AddMoney(-currentDisplayedGun.price);
                currentDisplayedGun.isOwned = true;
                SaveOwnedGuns();
                
                PlayerPrefs.SetInt(currentDisplayedGun.gunName + "_BaseDamage", currentDisplayedGun.baseDamage);
                
                DisplayGun(currentGunIndex);
                UpdatePlayerMoneyDisplay();
                Debug.Log($"Bought {currentDisplayedGun.gunName}");
            }
            else
            {
                ShowWarningMessage("Not enough money to buy this gun!");
            }
        }
    }
    
    void UpdatePlayerMoneyDisplay()
    {
        if (playerMoneyText != null && GameManager.instance != null)
            playerMoneyText.text = $"${GameManager.instance.playerMoney}";
    }
    
    void UpdatePlayerLevelDisplay()
    {
        if (playerLevelText != null && GameManager.instance != null)
            playerLevelText.text = $"Level: {GameManager.instance.playerLevel}";
    }
    
    public void OpenShop()
    {
        if (shopPanel != null)
        {
            shopPanel.SetActive(true);
            UpdatePlayerMoneyDisplay();
            UpdatePlayerLevelDisplay();
            DisplayGun(currentGunIndex);
            
            if (utilitiesPanel != null)
                utilitiesPanel.SetActive(true);
        }
    }
    
    public void CloseShop()
    {
        if (shopPanel != null)
            shopPanel.SetActive(false);
    }
    
    void PlayButtonSound()
    {
        if (buttonSoundSource != null && buttonClickSound != null)
            buttonSoundSource.PlayOneShot(buttonClickSound);
    }
    
    public int GetUtilityCount(UtilityType type)
    {
        foreach (UtilityData utility in allUtilities)
        {
            if (utility.utilityType == type)
                return utility.currentCount;
        }
        return 0;
    }

    public void UseUtility(UtilityType type)
    {
        foreach (UtilityData utility in allUtilities)
        {
            if (utility.utilityType == type && utility.currentCount > 0)
            {
                utility.currentCount--;
                SaveUtilities();
                Debug.Log($"Used {utility.utilityName}. Remaining: {utility.currentCount}");
                break;
            }
        }
    }
}