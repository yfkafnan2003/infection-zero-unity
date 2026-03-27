using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    [Header("POI Progress")]
    public List<string> completedPOIs = new List<string>();

    [Header("Player Progress")]
    public int playerLevel = 1;
    public int playerMoney = 0;

    [System.Serializable]
    public class POIChain
    {
        public string chainName;
        public List<POIData> poiList; // Order matters - will unlock in sequence
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
    
    [Header("XP System")]
    public int currentXP = 0;
    public int xpToNextLevel = 100;

    [Header("Energy")]
    public int maxEnergy = 5;
    public int currentEnergy = 5;
    public float energyRegenTime = 300f; // 5 minutes in seconds
    private float energyRegenTimer = 0f;

    void Awake()
    {
        if(instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("GameManager initialized and persists across scenes");
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Debug to check if POI persists
        if(_currentPOI != null)
        {
            Debug.Log("GameManager has POI: " + _currentPOI.poiName);
        }
        else
        {
            Debug.Log("GameManager has no POI set");
        }
        
        // Start energy regen timer
        energyRegenTimer = energyRegenTime;
    }

    void Update()
    {
        // Energy regeneration
        if (currentEnergy < maxEnergy)
        {
            energyRegenTimer -= Time.deltaTime;
            
            if (energyRegenTimer <= 0)
            {
                currentEnergy = Mathf.Min(currentEnergy + 1, maxEnergy);
                energyRegenTimer = energyRegenTime;
                Debug.Log($"Energy regenerated! Current energy: {currentEnergy}/{maxEnergy}");
            }
        }
        else
        {
            // Reset timer when at max energy
            energyRegenTimer = energyRegenTime;
        }
    }

    public bool UseEnergy()
    {
        if(currentEnergy <= 0)
            return false;

        currentEnergy--;
        // Reset regen timer when energy is used
        energyRegenTimer = energyRegenTime;
        return true;
    }

    public void AddMoney(int amount)
    {
        playerMoney += amount;
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
    }
    
    public void ResetGame()
    {
        // Reset player progress
        playerLevel = 1;
        playerMoney = 0;
        currentXP = 0;
        xpToNextLevel = 100;
        
        // Reset energy
        currentEnergy = maxEnergy;
        energyRegenTimer = energyRegenTime;
        
        // Reset POI progress
        ResetPOIProgress();
        
        Debug.Log("Game has been reset! All progress cleared.");
    }
    
    public void MarkPOIAsCompleted(string poiName)
    {
        if (!completedPOIs.Contains(poiName))
        {
            completedPOIs.Add(poiName);
            Debug.Log($"POI '{poiName}' marked as completed!");
            
            // Check if current chain is fully completed
            CheckChainCompletion();
        }
    }
    
    void CheckChainCompletion()
    {
        if (currentChainIndex >= poiChains.Count)
            return;
            
        POIChain currentChain = poiChains[currentChainIndex];
        bool allCompleted = true;
        
        // Check if all POIs in current chain are completed
        foreach (POIData poi in currentChain.poiList)
        {
            if (!completedPOIs.Contains(poi.poiName))
            {
                allCompleted = false;
                break;
            }
        }
        
        // If all POIs in current chain are completed, move to next chain
        if (allCompleted && currentChainIndex + 1 < poiChains.Count)
        {
            currentChainIndex++;
            Debug.Log($"Chain '{currentChain.chainName}' completed! Moved to next chain: {poiChains[currentChainIndex].chainName}");
        }
    }
    
    public void ResetPOIProgress()
    {
        completedPOIs.Clear();
        currentChainIndex = 0;
        
        // First chain's first POI is automatically available
        // No need to mark anything as completed
        Debug.Log("POI progress reset!");
    }
    
    public bool IsPOICompleted(string poiName)
    {
        return completedPOIs.Contains(poiName);
    }
    // Add this property to GameManager class to expose the timer:
    public float GetEnergyRegenTimeRemaining()
    {
        return energyRegenTimer;
    }
    // Check if a POI is available (not completed and in current chain)
    public bool IsPOIAvailable(POIData poi)
    {
        // If already completed, not available
        if (completedPOIs.Contains(poi.poiName))
            return false;
            
        // Check if POI belongs to current chain
        if (currentChainIndex < poiChains.Count)
        {
            POIChain currentChain = poiChains[currentChainIndex];
            return currentChain.poiList.Contains(poi);
        }
        
        return false;
    }

    // Add this method to get the next available POI
    public POIData GetNextPOIInChain()
    {
        if (poiChains.Count == 0 || currentChainIndex >= poiChains.Count)
            return null;
        
        POIChain currentChain = poiChains[currentChainIndex];
        
        // Find next uncompleted POI in current chain
        for (int i = 0; i < currentChain.poiList.Count; i++)
        {
            if (!completedPOIs.Contains(currentChain.poiList[i].poiName))
            {
                return currentChain.poiList[i];
            }
        }
        
        // If all POIs in current chain are completed, move to next chain
        if (currentChainIndex + 1 < poiChains.Count)
        {
            currentChainIndex++;
            // Return first POI of next chain
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
        Debug.Log($"Level Up! Now level {playerLevel}. Next level needs {xpToNextLevel} XP");
    }
}