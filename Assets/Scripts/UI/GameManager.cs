using System.Collections.Generic;
using UnityEngine;
using System.Collections;
public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    [Header("Special POIs (Always Available)")]
    public List<POIData> specialPOIs = new List<POIData>();
    [Header("POI Progress")]
    public List<string> completedPOIs = new List<string>();

    [Header("Player Progress")]
    public int playerLevel = 1;
    public int playerMoney = 0;
    public int currentXP = 0;
    public int xpToNextLevel = 100;

    [System.Serializable]
    public class POIChain
    {
        public string chainName;
        public List<POIData> poiList;
    }

    [Header("POI Chain System")]
    public List<POIChain> poiChains = new List<POIChain>();
    public int currentChainIndex = 0;
    
    [Header("Current Mission")]
    private POIData _currentPOI;
    public POIData CurrentPOI
    {
        get { return _currentPOI; }
        set { _currentPOI = value; }
    }
    
    [Header("Energy")]
    public int maxEnergy = 5;
    public int currentEnergy = 5;
    public float energyRegenTime = 600f;
    private string lastEnergyUpdateKey = "LastEnergyUpdate";
    private float realTimeRegenTimer = 0f; // Real-time countdown timer
    [Header("Premium")]
    public bool infiniteStamina = false;
        
    void Awake()
    {
        if(instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            LoadAllData();
            UpdateEnergyOffline(); // Calculate energy gained while app was closed
            Debug.Log("GameManager initialized and persists across scenes");
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        if(_currentPOI != null)
        {
            Debug.Log("GameManager has POI: " + _currentPOI.poiName);
        }
        else
        {
            Debug.Log("GameManager has no POI set");
        }
    }

    void Update()
    {
        // Real-time energy regeneration while app is running
        if (currentEnergy < maxEnergy)
        {
            realTimeRegenTimer += Time.deltaTime;
            
            if (realTimeRegenTimer >= energyRegenTime)
            {
                currentEnergy = Mathf.Min(currentEnergy + 1, maxEnergy);
                realTimeRegenTimer = 0f;
                SaveAllData();
                Debug.Log($"Energy regenerated! Current energy: {currentEnergy}/{maxEnergy}");
            }
        }
        else
        {
            realTimeRegenTimer = 0f;
        }
    }
    public void AddEnergy(int amount)
    {
        currentEnergy = Mathf.Min(currentEnergy + amount, maxEnergy);
        SaveAllData();
        Debug.Log($"Added {amount} energy! Current energy: {currentEnergy}/{maxEnergy}");
    }
    void UpdateEnergyOffline()
    {
        float lastUpdate = PlayerPrefs.GetFloat(lastEnergyUpdateKey, (float)GetCurrentTimestamp());
        float currentTime = (float)GetCurrentTimestamp();
        float timePassed = currentTime - lastUpdate;
        
        // Add offline time to realTimeRegenTimer
        realTimeRegenTimer += timePassed;
        
        if (realTimeRegenTimer >= energyRegenTime && currentEnergy < maxEnergy)
        {
            int energyToAdd = Mathf.FloorToInt(realTimeRegenTimer / energyRegenTime);
            currentEnergy = Mathf.Min(currentEnergy + energyToAdd, maxEnergy);
            realTimeRegenTimer = realTimeRegenTimer % energyRegenTime;
            Debug.Log($"Offline energy regeneration: Added {energyToAdd} energy. Now at {currentEnergy}/{maxEnergy}");
        }
        
        // Update last update time
        PlayerPrefs.SetFloat(lastEnergyUpdateKey, currentTime);
        PlayerPrefs.Save();
    }
    public void SyncAllGunData(List<GunData> allGuns)
    {
        foreach (GunData gun in allGuns)
        {
            gun.isOwned = PlayerPrefs.GetInt(gun.gunName + "_Owned", gun.gunName == "Glocky" ? 1 : 0) == 1;
            gun.currentUpgradeLevel = PlayerPrefs.GetInt(gun.gunName + "_Upgrade", 0);
        }
    }
    public float GetEnergyRegenTimeRemaining()
    {
        if (currentEnergy >= maxEnergy)
            return 0f;
        
        return Mathf.Max(0, energyRegenTime - realTimeRegenTimer);
    }
    double GetCurrentTimestamp()
    {
        return System.DateTime.UtcNow.Subtract(new System.DateTime(1970, 1, 1)).TotalSeconds;
    }

    public void SaveAllData()
    {
        PlayerPrefs.SetInt("PlayerLevel", playerLevel);
        PlayerPrefs.SetInt("PlayerMoney", playerMoney);
        PlayerPrefs.SetInt("CurrentXP", currentXP);
        PlayerPrefs.SetInt("XPToNextLevel", xpToNextLevel);
        PlayerPrefs.SetInt("CurrentEnergy", currentEnergy);
        PlayerPrefs.SetInt("CurrentChainIndex", currentChainIndex);
        PlayerPrefs.SetInt("InfiniteStamina", infiniteStamina ? 1 : 0);
        // Save real-time timer progress
        PlayerPrefs.SetFloat("RealTimeRegenTimer", realTimeRegenTimer);
        
        // Save last energy update time for offline
        PlayerPrefs.SetFloat(lastEnergyUpdateKey, (float)GetCurrentTimestamp());
        
        // Save completed POIs
        PlayerPrefs.SetInt("CompletedPOICount", completedPOIs.Count);
        for (int i = 0; i < completedPOIs.Count; i++)
        {
            PlayerPrefs.SetString($"CompletedPOI_{i}", completedPOIs[i]);
        }
        
        PlayerPrefs.Save();
        Debug.Log("Game data saved!");
    }

    public void LoadAllData()
    {
        playerLevel = PlayerPrefs.GetInt("PlayerLevel", 1);
        playerMoney = PlayerPrefs.GetInt("PlayerMoney", 0);
        currentXP = PlayerPrefs.GetInt("CurrentXP", 0);
        xpToNextLevel = PlayerPrefs.GetInt("XPToNextLevel", 100);
        currentEnergy = PlayerPrefs.GetInt("CurrentEnergy", maxEnergy);
        currentChainIndex = PlayerPrefs.GetInt("CurrentChainIndex", 0);
        infiniteStamina = PlayerPrefs.GetInt("InfiniteStamina",0) == 1;
        // Load real-time timer
        realTimeRegenTimer = PlayerPrefs.GetFloat("RealTimeRegenTimer", 0f);
        
        // Load completed POIs
        completedPOIs.Clear();
        int completedCount = PlayerPrefs.GetInt("CompletedPOICount", 0);
        for (int i = 0; i < completedCount; i++)
        {
            string poiName = PlayerPrefs.GetString($"CompletedPOI_{i}", "");
            if (!string.IsNullOrEmpty(poiName))
                completedPOIs.Add(poiName);
        }
        
        // Start tutorial ONLY on fresh game (level 1, no money, no XP, no completed POIs)
        bool isFreshStart = (playerLevel == 1 && playerMoney == 0 && currentXP == 0 && completedPOIs.Count == 0);
        
        if (isFreshStart)
        {
            Debug.Log("Fresh start detected - starting tutorial");
            // Delay to ensure UI is loaded
            StartCoroutine(StartTutorialAfterDelay(0.5f));
        }
        
        Debug.Log($"Game data loaded! Level: {playerLevel}, Money: ${playerMoney}, Energy: {currentEnergy}");
    }

    IEnumerator StartTutorialAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        
        if (TutorialManager.Instance != null)
        {
            TutorialManager.Instance.StartTutorial("initial");
        }
    }
    // Rest of your existing methods...
    public void ResetAllStoreItemsExceptGlocky()
    {
        SaveAllData();
        Debug.Log("Store items reset (except Glocky)");
    }
    
    public void ResetGameProgress()
    {
        playerLevel = 1;
        playerMoney = 0;
        currentXP = 0;
        xpToNextLevel = 100;
        currentEnergy = maxEnergy;
        currentChainIndex = 0;
        completedPOIs.Clear();
        
        // Reset tutorial progress
        if (TutorialManager.Instance != null)
        {
            TutorialManager.Instance.ResetTutorialProgress();
        }
        
        SaveAllData();
        Debug.Log("Game progress reset! Level and money reset to default.");
    }
    
    public bool UseEnergy()
    {
        if (infiniteStamina)
            return true;

        if(currentEnergy <= 0)
            return false;

        currentEnergy--;
        SaveAllData();
        return true;
    }

    public void AddMoney(int amount)
    {
        playerMoney += amount;
        SaveAllData();
        Debug.Log($"Added ${amount}. Total money: ${playerMoney}");
    }

    public void AddXP(int amount)
    {
        currentXP += amount;
        Debug.Log($"Added {amount} XP. Total XP: {currentXP}");

        if(currentXP >= xpToNextLevel)
        {
            currentXP -= xpToNextLevel;
            LevelUp();
        }
        SaveAllData();
    }
    
    public void MarkPOIAsCompleted(string poiName)
    {
        if (!completedPOIs.Contains(poiName))
        {
            completedPOIs.Add(poiName);
            SaveAllData();
            Debug.Log($"POI '{poiName}' marked as completed!");
            CheckChainCompletion();
        }
    }
    
    void CheckChainCompletion()
    {
        if (currentChainIndex >= poiChains.Count)
            return;
            
        POIChain currentChain = poiChains[currentChainIndex];
        bool allCompleted = true;
        
        foreach (POIData poi in currentChain.poiList)
        {
            if (!completedPOIs.Contains(poi.poiName))
            {
                allCompleted = false;
                break;
            }
        }
        
        if (allCompleted && currentChainIndex + 1 < poiChains.Count)
        {
            currentChainIndex++;
            SaveAllData();
            Debug.Log($"Chain '{currentChain.chainName}' completed! Moved to next chain: {poiChains[currentChainIndex].chainName}");
        }
    }
    
    public void ResetPOIProgress()
    {
        completedPOIs.Clear();
        currentChainIndex = 0;
        SaveAllData();
        Debug.Log("POI progress reset!");
    }
    
    public bool IsPOICompleted(string poiName)
    {
        return completedPOIs.Contains(poiName);
    }
    
    public bool IsPOIAvailable(POIData poi)
    {
        // Special POIs are always available
        if (specialPOIs.Contains(poi))
        {
            return true;
        }
        
        // Only check current chain
        if (currentChainIndex < poiChains.Count)
        {
            POIChain currentChain = poiChains[currentChainIndex];
            // POI is available if it exists in current chain (regardless of completion status)
            return currentChain.poiList.Contains(poi);
        }
        
        return false;
    }

    public POIData GetNextPOIInChain()
    {
        if (poiChains.Count == 0 || currentChainIndex >= poiChains.Count)
            return null;
        
        POIChain currentChain = poiChains[currentChainIndex];
        
        for (int i = 0; i < currentChain.poiList.Count; i++)
        {
            if (!completedPOIs.Contains(currentChain.poiList[i].poiName))
            {
                return currentChain.poiList[i];
            }
        }
        
        if (currentChainIndex + 1 < poiChains.Count)
        {
            currentChainIndex++;
            SaveAllData();
            if (poiChains[currentChainIndex].poiList.Count > 0)
            {
                return poiChains[currentChainIndex].poiList[0];
            }
        }
        
        return null;
    }
    void LevelUp()
    {
        playerLevel++;
        xpToNextLevel += 50;
        SaveAllData();
        Debug.Log($"Level Up! Now level {playerLevel}. Next level needs {xpToNextLevel} XP");
    }
}