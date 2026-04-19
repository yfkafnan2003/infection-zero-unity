using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class RetrieveBoxManager : MonoBehaviour
{
    [Header("Box Settings")]
    public int boxesToCollect = 3;
    public GameObject boxPrefab;
    public Transform exitPoint;
    public float boxCollectionRadius = 2f;
    
    [Header("UI")]
    public TextMeshProUGUI boxesRemainingText;
    
    [Header("Radar")]
    public GameObject radarDotPrefab;
    public Color boxRadarColor = Color.yellow;
    public Color exitRadarColor = Color.green;
    
    private int boxesCollected = 0;
    private List<GameObject> activeBoxes = new List<GameObject>();
    private List<GameObject> boxRadarDots = new List<GameObject>();
    private GameObject exitRadarDot;
    private bool isAtExit = false;
    private LevelManager levelManager;
    private MinimapRadar minimapRadar;
    
    void Start()
    {
        // Check if current mission is RetrieveBox
        if (GameManager.instance != null && GameManager.instance.CurrentPOI != null)
        {
            if (GameManager.instance.CurrentPOI.poiType != POIType.RetrieveBox)
            {
                // Destroy all boxes and this manager if not Retrieve Box mission
                DestroyAllBoxes();
                Destroy(gameObject);
                return;
            }
        }
        
        levelManager = FindObjectOfType<LevelManager>();
        minimapRadar = FindObjectOfType<MinimapRadar>();
        SpawnBoxes();
        UpdateUI();
    }
    
    void Update()
    {
        // Update box radar dot positions
        for (int i = 0; i < boxRadarDots.Count && i < activeBoxes.Count; i++)
        {
            if (boxRadarDots[i] != null && activeBoxes[i] != null)
            {
                if (minimapRadar != null)
                {
                    minimapRadar.UpdateCustomDotPosition(boxRadarDots[i], activeBoxes[i].transform.position);
                }
            }
        }
        
        // Update exit radar dot position
        if (exitRadarDot != null && exitPoint != null)
        {
            if (minimapRadar != null)
            {
                minimapRadar.UpdateCustomDotPosition(exitRadarDot, exitPoint.position);
            }
        }
        
        // Check if player is at exit with all boxes
        if (boxesCollected >= boxesToCollect && !isAtExit)
        {
            CheckExitProximity();
        }
    }
    
    void SpawnBoxes()
    {
        // Find all box spawn points in scene
        GameObject[] spawnPoints = GameObject.FindGameObjectsWithTag("BoxSpawnPoint");
        
        for (int i = 0; i < boxesToCollect && i < spawnPoints.Length; i++)
        {
            Vector3 spawnPos = spawnPoints[i].transform.position;
            GameObject box = Instantiate(boxPrefab, spawnPos, Quaternion.identity);
            activeBoxes.Add(box);
            
            // Setup box component
            CollectableBox boxScript = box.GetComponent<CollectableBox>();
            if (boxScript == null)
                boxScript = box.AddComponent<CollectableBox>();
            boxScript.Setup(this, i);
            
            // Add to radar
            AddBoxToRadar(box, spawnPos);
        }
    }
    void AddBoxToRadar(GameObject box, Vector3 position)
    {
        if (minimapRadar != null && radarDotPrefab != null)
        {
            // Create radar dot for box
            GameObject radarDot = Instantiate(radarDotPrefab, minimapRadar.radarPanel);
            UnityEngine.UI.Image dotImage = radarDot.GetComponent<UnityEngine.UI.Image>();
            if (dotImage != null)
                dotImage.color = boxRadarColor;
            
            // No need for alwaysShow - position is clamped to edge
            RectTransform rect = radarDot.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(minimapRadar.dotSize, minimapRadar.dotSize);
            
            // Set initial position
            minimapRadar.UpdateCustomDotPosition(radarDot, position);
            
            boxRadarDots.Add(radarDot);
        }
    }
    
    
    void AddExitToRadar()
    {
        if (minimapRadar != null && radarDotPrefab != null && exitPoint != null)
        {
            // Create exit radar dot
            exitRadarDot = Instantiate(radarDotPrefab, minimapRadar.radarPanel);
            UnityEngine.UI.Image dotImage = exitRadarDot.GetComponent<UnityEngine.UI.Image>();
            if (dotImage != null)
                dotImage.color = exitRadarColor;
            
            RectTransform rect = exitRadarDot.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(minimapRadar.dotSize, minimapRadar.dotSize);
            
            // Set initial position
            minimapRadar.UpdateCustomDotPosition(exitRadarDot, exitPoint.position);
        }
    }
    void DestroyAllBoxes()
    {
        // Destroy all active boxes
        foreach (GameObject box in activeBoxes)
        {
            if (box != null)
                Destroy(box);
        }
        activeBoxes.Clear();
        
        // Destroy all radar dots
        foreach (GameObject dot in boxRadarDots)
        {
            if (dot != null)
                Destroy(dot);
        }
        boxRadarDots.Clear();
        
        if (exitRadarDot != null)
            Destroy(exitRadarDot);
    }
    public void CollectBox(GameObject box)
    {
        if (activeBoxes.Contains(box))
        {
            activeBoxes.Remove(box);
            boxesCollected++;
            
            // Remove radar dot for this box
            if (boxRadarDots.Count > 0)
            {
                GameObject dot = boxRadarDots[0];
                boxRadarDots.RemoveAt(0);
                Destroy(dot);
            }
            
            UpdateUI();
            Debug.Log($"Box collected! {boxesCollected}/{boxesToCollect}");
            
            Destroy(box);
            
            // If all boxes collected, show exit on radar
            if (boxesCollected >= boxesToCollect)
            {
                AddExitToRadar();
            }
        }
    }
    
    void UpdateUI()
    {
        if (boxesRemainingText != null)
        {
            boxesRemainingText.text = $"Boxes: {boxesCollected}/{boxesToCollect}";
            
            if (boxesCollected >= boxesToCollect)
            {
                boxesRemainingText.text += "\nGo to Exit!";
                boxesRemainingText.color = Color.green;
            }
        }
    }
    
    void CheckExitProximity()
    {
        if (exitPoint == null) return;
        
        Transform player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (player == null) return;
        
        float distance = Vector3.Distance(player.position, exitPoint.position);
        
        if (distance <= boxCollectionRadius)
        {
            isAtExit = true;
            CompleteMission();
        }
    }
    
    void CompleteMission()
    {
        Debug.Log("All boxes collected and reached exit! Mission Complete!");
        
        // Remove exit radar dot
        if (exitRadarDot != null)
            Destroy(exitRadarDot);
        
        // Stop zombie spawner
        ZombieSpawner spawner = FindObjectOfType<ZombieSpawner>();
        if (spawner != null)
            spawner.StopSpawning();
        
        if (levelManager != null)
        {
            levelManager.CompleteMission(true);
        }
    }
    
    public int GetBoxesRemaining()
    {
        return boxesToCollect - boxesCollected;
    }
}