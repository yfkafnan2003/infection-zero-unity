using UnityEngine;

[CreateAssetMenu(fileName = "New POI", menuName = "Game/POI Data")]
public class POIData : ScriptableObject
{
    [Header("Basic Info")]
    public string poiName;
    public Sprite poiIcon;
    public string levelScene;
    public POIType poiType;

    [Header("Requirements (Developer Only)")]
    [Range(1, 100)]
    public int requiredPlayerLevel = 1;
    
    [Header("Difficulty Display (Player Info Only)")]
    [Tooltip("Just for display - does NOT affect gameplay")]
    [Range(1, 100)]
    public int difficultyLevel = 1;
    
    [Header("Zombie Customization")]
    [Tooltip("Base zombie health for this POI")]
    public int baseZombieHealth = 50;
    
    [Tooltip("Additional health multiplier")]
    [Range(0.5f, 5f)]
    public float healthMultiplier = 1f;
    
    [Tooltip("How much damage zombies deal to player")]
    [Range(5, 1000)]
    public int zombieDamage = 20;
    [Header("Retrieve Box Settings")]
    [Tooltip("Number of boxes to collect")]
    [Range(1, 10)]
    public int boxesToCollect = 3;
    [Tooltip("Zombie movement speed")]
    [Range(0.5f, 5f)]
    public float zombieSpeed = 3f;
    
    [Header("Spawn Customization")]
    [Tooltip("Maximum zombies active at once")]
    [Range(5, 1000)]
    public int maxZombies = 20;
    
    [Tooltip("Seconds between spawns")]
    [Range(0.5f, 10f)]
    public float spawnInterval = 3f;
    
    [Tooltip("Initial spawn delay")]
    [Range(0f, 5f)]
    public float initialSpawnDelay = 0f;
    
    [Header("Mission Settings")]
    [Tooltip("For KillZombies: total zombies to kill")]
    [Range(5, 1000)]
    public int zombieAmount = 10;
    
    [Tooltip("For wave-based spawning: zombies per wave")]
    [Range(1, 50)]
    public int zombiesToSpawn = 10;
    
    [Tooltip("For CountdownSurvive: seconds to survive")]
    [Range(30, 1000)]
    public int surviveTime = 60;
    
    [Header("Reward Customization")]
    [Tooltip("Base money reward")]
    [Range(50, 10000)]
    public int baseMoneyReward = 100;
    
    [Tooltip("Base XP reward")]
    [Range(25, 10000)]
    public int baseXPReward = 50;
    
    [Header("Protect Door Settings")]
    [Tooltip("Time to protect the door in seconds")]
    [Range(30, 300)]
    public int protectTime = 60;

    [Tooltip("Door initial health")]
    [Range(50, 500)]
    public int doorHealth = 100;

    [Tooltip("How much damage zombies deal to door")]
    [Range(1, 50)]
    public int zombieDoorDamage = 10;

    [Tooltip("How much health regen per second when player is near")]
    [Range(5, 50)]
    public int doorRegenRate = 10;
    
    [Tooltip("Distance to heal the door")]
    [Range(1, 10)]
    public float healDistance = 5f;
    
    [Header("Boss Fight Settings")]
    [Tooltip("Boss max health")]
    [Range(500, 5000)]
    public int bossMaxHealth = 1000;

    [Tooltip("Boss attack damage")]
    [Range(10, 100)]
    public int bossAttackDamage = 30;

    [Tooltip("Boss attack cooldown")]
    [Range(1, 5)]
    public float bossAttackCooldown = 3f;

    // Helper methods - difficultyLevel does NOT affect these anymore
    public int GetZombieHealth()
    {
        // Only use baseHealth and healthMultiplier, ignore difficultyLevel
        int finalHealth = Mathf.RoundToInt(baseZombieHealth * healthMultiplier);
        return Mathf.Clamp(finalHealth, 10, 1000);
    }
    
    public float GetSpawnInterval()
    {
        // Only use spawnInterval, ignore difficultyLevel
        return Mathf.Clamp(spawnInterval, 0.5f, 10f);
    }
    
    public int GetZombieDamage()
    {
        // Only use zombieDamage, ignore difficultyLevel
        return Mathf.Clamp(zombieDamage, 5, 100);
    }
    
    public int GetMoneyReward()
    {
        // Only use baseMoneyReward, ignore difficultyLevel
        return Mathf.Clamp(baseMoneyReward, 50, 500);
    }
    
    public int GetXPReward()
    {
        // Only use baseXPReward, ignore difficultyLevel
        return Mathf.Clamp(baseXPReward, 25, 250);
    }
}