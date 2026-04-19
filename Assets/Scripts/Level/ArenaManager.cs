using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class ArenaManager : MonoBehaviour
{
    [Header("Arena Settings")]
    public int totalRounds = 20;
    public float roundBreakDuration = 5f;
    public float roundStartDelay = 2f;
    
    [Header("Round Progression Settings")]
    public int startingZombies = 10;
    public float startingSpawnInterval = 2f;
    public int startingZombieHealth = 50;
    
    [Header("Progression Multipliers")]
    public float zombieCountMultiplier = 1.5f;  // Each round: zombies * 1.5
    public float spawnIntervalDecrease = 0.1f;  // Each round: interval -0.1s
    public int zombieHealthIncrease = 50;       // Each round: health +50
    
    [Header("Reward Settings")]
    public int baseMoneyReward = 100;
    public int baseXPReward = 50;
    public float moneyMultiplierPerRound = 1.2f;  // Each round: money * 1.2
    public float xpMultiplierPerRound = 1.15f;    // Each round: XP * 1.15
    
    [Header("UI Elements")]
    public TextMeshProUGUI roundText;
    public TextMeshProUGUI zombiesToKillText;
    public TextMeshProUGUI roundBreakText;
    public GameObject roundBreakPanel;
    public GameObject arenaUIPanel;
    
    [Header("References")]
    public ZombieSpawner zombieSpawner;
    public LevelManager levelManager;
    
    private int currentRound = 0;
    private int zombiesRemainingInRound = 0;
    private int currentRoundZombiesToKill = 0;
    private float currentSpawnInterval = 0f;
    private int currentZombieHealth = 0;
    private bool isRoundBreak = false;
    private bool arenaCompleted = false;
    
    void Start()
    {
        if (levelManager == null)
            levelManager = FindObjectOfType<LevelManager>();
        
        if (zombieSpawner == null)
            zombieSpawner = FindObjectOfType<ZombieSpawner>();
        
        StartCoroutine(StartArena());
    }
    
    IEnumerator StartArena()
    {
        yield return new WaitForSeconds(roundStartDelay);
        StartRound(1);
    }
    
    void StartRound(int round)
    {
        currentRound = round;
        isRoundBreak = false;
        
        // Calculate round stats
        currentRoundZombiesToKill = Mathf.RoundToInt(startingZombies * Mathf.Pow(zombieCountMultiplier, currentRound - 1));
        currentSpawnInterval = Mathf.Max(0.3f, startingSpawnInterval - (spawnIntervalDecrease * (currentRound - 1)));
        currentZombieHealth = startingZombieHealth + (zombieHealthIncrease * (currentRound - 1));
        
        zombiesRemainingInRound = currentRoundZombiesToKill;
        
        // Update UI
        if (roundText != null)
            roundText.text = $"ROUND {currentRound}";
        
        if (zombiesToKillText != null)
            zombiesToKillText.text = $"Remaining Zombies: {zombiesRemainingInRound}";
        
        // Configure spawner for this round
        ConfigureSpawnerForRound();
        
        // Hide break panel
        if (roundBreakPanel != null)
            roundBreakPanel.SetActive(false);
        
        if (arenaUIPanel != null)
            arenaUIPanel.SetActive(true);
        
        // Start spawning
        zombieSpawner.StartSpawning();
        
        Debug.Log($"Round {currentRound} started! Need to kill: {currentRoundZombiesToKill}, Spawn Interval: {currentSpawnInterval}, Health: {currentZombieHealth}");
    }
    
    void ConfigureSpawnerForRound()
    {
        if (zombieSpawner != null)
        {
            zombieSpawner.maxZombies = Mathf.Min(currentRoundZombiesToKill, 30);
            zombieSpawner.spawnInterval = currentSpawnInterval;
            zombieSpawner.waveBasedSpawning = true;
            zombieSpawner.zombiesPerWave = Mathf.Min(5 + (currentRound / 2), 15);
            
            // Update zombie health for new spawns
            zombieSpawner.SetRoundHealth(currentZombieHealth);
        }
    }
    
    public void RegisterZombieKill()
    {
        if (arenaCompleted || isRoundBreak) return;
        
        zombiesRemainingInRound--;
        
        if (zombiesToKillText != null)
            zombiesToKillText.text = $"Zombies: {zombiesRemainingInRound}";
        
        if (zombiesRemainingInRound <= 0)
        {
            CompleteRound();
        }
    }
    
    void CompleteRound()
    {
        zombieSpawner.StopSpawning();
        
        // Calculate rewards for this round
        int moneyReward = Mathf.RoundToInt(baseMoneyReward * Mathf.Pow(moneyMultiplierPerRound, currentRound - 1));
        int xpReward = Mathf.RoundToInt(baseXPReward * Mathf.Pow(xpMultiplierPerRound, currentRound - 1));
        
        if (GameManager.instance != null)
        {
            GameManager.instance.AddMoney(moneyReward);
            GameManager.instance.AddXP(xpReward);
        }
        
        Debug.Log($"Round {currentRound} completed! Rewards: ${moneyReward}, {xpReward} XP");
        
        if (currentRound >= totalRounds)
        {
            CompleteArena();
        }
        else
        {
            StartCoroutine(RoundBreak());
        }
    }
    
    IEnumerator RoundBreak()
    {
        isRoundBreak = true;
        if (zombieSpawner != null)
        zombieSpawner.StopSpawning();

        if (roundBreakPanel != null)
        {
            roundBreakPanel.SetActive(true);
            if (roundBreakText != null)
            {
                int nextRound = currentRound + 1;
                int nextZombies = Mathf.RoundToInt(startingZombies * Mathf.Pow(zombieCountMultiplier, nextRound - 1));
                float nextInterval = Mathf.Max(0.3f, startingSpawnInterval - (spawnIntervalDecrease * (nextRound - 1)));
                int nextHealth = startingZombieHealth + (zombieHealthIncrease * (nextRound - 1));
                
                roundBreakText.text = $"ROUND {currentRound} COMPLETE!\n\n" +
                                     $"Next Round: {nextRound}\n" +
                                     $"Zombies: {nextZombies}\n" +
                                     $"Zombie Health: {nextHealth}\n" +
                                     $"Next round in {roundBreakDuration} seconds...";
            }
        }
        
        float timer = 0f;
        while (timer < roundBreakDuration)
        {
            timer += Time.deltaTime;
            yield return null;
        }
        
        if (roundBreakPanel != null)
            roundBreakPanel.SetActive(false);
        
        StartRound(currentRound + 1);
    }
    
    void CompleteArena()
    {
        arenaCompleted = true;
        zombieSpawner.StopSpawning();
        
        // Clear remaining zombies
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject enemy in enemies)
        {
            Destroy(enemy);
        }
        
        Debug.Log("ARENA COMPLETE! All 20 rounds finished!");
        
        if (levelManager != null)
        {
            // Call mission complete
            levelManager.CompleteMission(true);
        }
    }
    
    public int GetZombieHealthForRound()
    {
        return currentZombieHealth;
    }
    
    public bool IsArenaComplete()
    {
        return arenaCompleted;
    }
    public int GetCurrentRound()
    {
        return currentRound;
    }

    public int GetTotalRounds()
    {
        return totalRounds;
    }

    public int GetZombiesRemaining()
    {
        return zombiesRemainingInRound;
    }
}