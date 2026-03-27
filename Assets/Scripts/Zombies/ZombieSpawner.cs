using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ZombieSpawner : MonoBehaviour
{
    [System.Serializable]
    public class ZombiePrefab
    {
        public GameObject prefab;
        public string zombieName;
        public int spawnWeight = 100;
    }

    [Header("Zombie Types")]
    public List<ZombiePrefab> zombiePrefabs = new List<ZombiePrefab>();
    
    [Header("Spawn Settings")]
    public int maxZombies = 20;
    public float spawnInterval = 3f;
    public bool useSpawnPoints = false;
    public List<Transform> spawnPoints = new List<Transform>(); // Add spawn points
    public float spawnRadius = 15f;
    public LayerMask groundLayer;
    public Transform playerTransform;
    
    [Header("Wave Settings")]
    public bool waveBasedSpawning = false;
    public int zombiesPerWave = 10;
    public float waveDelay = 5f;
    
    [Header("Debug")]
    public bool showSpawnRadius = true;
    
    private List<GameObject> activeZombies = new List<GameObject>();
    private bool isSpawning = true;
    private int currentWave = 0;
    private int zombiesSpawnedInWave = 0;
    private Coroutine spawnCoroutine;
    private LevelManager levelManager;
    
    void Start()
    {
        levelManager = FindObjectOfType<LevelManager>();
        StartSpawning();
    }
    
    void Update()
    {
        if (levelManager != null && levelManager.IsMissionComplete())
        {
            StopSpawning();
            return;
        }
        
        if (waveBasedSpawning)
        {
            if (activeZombies.Count == 0 && zombiesSpawnedInWave >= zombiesPerWave)
            {
                StartCoroutine(StartNextWave());
            }
        }
    }
    
    public void StartSpawning()
    {
        if (spawnCoroutine != null)
            StopCoroutine(spawnCoroutine);
        
        isSpawning = true;
        spawnCoroutine = StartCoroutine(SpawnRoutine());
    }
    
    public void StopSpawning()
    {
        isSpawning = false;
        if (spawnCoroutine != null)
            StopCoroutine(spawnCoroutine);
    }
    
    IEnumerator SpawnRoutine()
    {
        while (isSpawning)
        {
            if (activeZombies.Count < maxZombies)
            {
                if (waveBasedSpawning)
                {
                    if (zombiesSpawnedInWave < zombiesPerWave)
                    {
                        SpawnZombie();
                        zombiesSpawnedInWave++;
                        yield return new WaitForSeconds(spawnInterval);
                    }
                    else
                    {
                        yield return new WaitForSeconds(0.5f);
                    }
                }
                else
                {
                    SpawnZombie();
                    yield return new WaitForSeconds(spawnInterval);
                }
            }
            else
            {
                yield return new WaitForSeconds(0.5f);
            }
        }
    }
    
    IEnumerator StartNextWave()
    {
        currentWave++;
        zombiesSpawnedInWave = 0;
        
        Debug.Log($"Wave {currentWave} starting!");
        
        yield return new WaitForSeconds(waveDelay);
        
        if (isSpawning && (levelManager == null || !levelManager.IsMissionComplete()))
        {
            StartSpawning();
        }
    }
    
    void SpawnZombie()
    {
        if (playerTransform == null && !useSpawnPoints)
        {
            Debug.LogError("Player transform not assigned and no spawn points set!");
            return;
        }
        
        Vector3 spawnPosition = GetSpawnPosition();
        
        GameObject zombiePrefab = GetRandomZombiePrefab();
        
        if (zombiePrefab == null)
        {
            Debug.LogError("No zombie prefabs assigned!");
            return;
        }
        
        // Spawn zombie
        GameObject newZombie = Instantiate(zombiePrefab, spawnPosition, Quaternion.identity);
        
        // Ensure NavMeshAgent is enabled
        UnityEngine.AI.NavMeshAgent agent = newZombie.GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (agent != null)
        {
            agent.enabled = true;
            agent.Warp(spawnPosition); // Ensure proper position
        }
        
        // Ensure ZombieAI is enabled
        ZombieAI zombieAI = newZombie.GetComponent<ZombieAI>();
        if (zombieAI != null)
        {
            zombieAI.enabled = true;
            // Assign player reference if needed
            if (zombieAI.player == null && playerTransform != null)
                zombieAI.player = playerTransform;
        }
        
        // Ensure all other scripts are enabled
        MonoBehaviour[] scripts = newZombie.GetComponents<MonoBehaviour>();
        foreach (MonoBehaviour script in scripts)
        {
            script.enabled = true;
        }
        
        activeZombies.Add(newZombie);
        
        ZombieHealth zombieHealth = newZombie.GetComponent<ZombieHealth>();
        if (zombieHealth != null)
        {
            StartCoroutine(WaitForZombieDeath(newZombie));
        }
    }
    
    Vector3 GetSpawnPosition()
    {
        if (useSpawnPoints && spawnPoints.Count > 0)
        {
            // Use random spawn point
            Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Count)];
            return spawnPoint.position;
        }
        else
        {
            // Random position around player
            Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;
            Vector3 spawnPos = playerTransform.position + new Vector3(randomCircle.x, 0, randomCircle.y);
            
            RaycastHit hit;
            if (Physics.Raycast(spawnPos + Vector3.up * 10f, Vector3.down, out hit, 20f, groundLayer))
            {
                spawnPos.y = hit.point.y;
            }
            
            return spawnPos;
        }
    }
    
    GameObject GetRandomZombiePrefab()
    {
        if (zombiePrefabs.Count == 0)
            return null;
        
        int totalWeight = 0;
        foreach (var zombie in zombiePrefabs)
        {
            totalWeight += zombie.spawnWeight;
        }
        
        int randomWeight = Random.Range(0, totalWeight);
        int currentWeight = 0;
        
        foreach (var zombie in zombiePrefabs)
        {
            currentWeight += zombie.spawnWeight;
            if (randomWeight < currentWeight)
                return zombie.prefab;
        }
        
        return zombiePrefabs[0].prefab;
    }
    
    public void ResetSpawner()
    {
        foreach (GameObject zombie in activeZombies)
        {
            if (zombie != null)
                Destroy(zombie);
        }
        activeZombies.Clear();
        
        currentWave = 0;
        zombiesSpawnedInWave = 0;
        
        isSpawning = true;
        if (spawnCoroutine != null)
            StopCoroutine(spawnCoroutine);
        spawnCoroutine = StartCoroutine(SpawnRoutine());
    }
    
    public int GetActiveZombieCount()
    {
        activeZombies.RemoveAll(z => z == null);
        return activeZombies.Count;
    }
    
    IEnumerator WaitForZombieDeath(GameObject zombie)
    {
        ZombieHealth zombieHealth = zombie.GetComponent<ZombieHealth>();
        
        while (zombie != null && !zombieHealth.IsDead())
        {
            yield return new WaitForSeconds(0.1f);
        }
        
        if (activeZombies.Contains(zombie))
            activeZombies.Remove(zombie);
    }
    
    void OnDrawGizmosSelected()
    {
        if (showSpawnRadius && !useSpawnPoints && playerTransform != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(playerTransform.position, spawnRadius);
        }
        
        if (showSpawnRadius && useSpawnPoints && spawnPoints.Count > 0)
        {
            Gizmos.color = Color.green;
            foreach (Transform spawnPoint in spawnPoints)
            {
                if (spawnPoint != null)
                    Gizmos.DrawWireSphere(spawnPoint.position, 1f);
            }
        }
    }
}