using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class ProtectDoorManager : MonoBehaviour
{
    [Header("Door References")]
    public DoorHealth[] doors;
    
    [Header("Mission Settings")]
    public float protectTime = 60f;
    private float timeRemaining;
    private bool missionActive = true;
    
    [Header("UI")]
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI doorsStatusText;
    public GameObject missionCompletePanel;
    public DoorUIManager doorUIManager;
    
    [Header("Radar")]
    public GameObject radarDotPrefab;
    public Color doorRadarColor = Color.green;
    
    [Header("Spawner")]
    public ZombieSpawner zombieSpawner;
    
    private LevelManager levelManager;
    private int doorsAlive;
    private List<GameObject> doorRadarDots = new List<GameObject>();
    private MinimapRadar minimapRadar;
    
    void Start()
    {
        if (GameManager.instance?.CurrentPOI == null || 
        GameManager.instance.CurrentPOI.poiType != POIType.ProtectDoor)
        {
            // Destroy this manager if not protect door mission
            Destroy(gameObject);
            return;
        }
        levelManager = FindObjectOfType<LevelManager>();
        minimapRadar = FindObjectOfType<MinimapRadar>();
        
        POIData poi = GameManager.instance?.CurrentPOI;
        if (poi != null)
        {
            protectTime = poi.protectTime;
            
            // Setup each door
            foreach (DoorHealth door in doors)
            {
                door.Setup(poi.doorHealth, poi.doorRegenRate, poi.healDistance);
                door.SetDoorIndex(System.Array.IndexOf(doors, door));
                door.SetDoorManager(this);
                AddDoorToRadar(door);
            }
        }
        
        timeRemaining = protectTime;
        doorsAlive = doors.Length;
        
        // Initialize Door UI
        if (doorUIManager != null)
            doorUIManager.InitializeDoors(doors);
        
        // Start spawner
        if (zombieSpawner != null)
            zombieSpawner.StartSpawning();
        
        UpdateUI();
        StartCoroutine(TimerRoutine());
    }
    
    public void UpdateDoorUI(int doorIndex, int currentHealth, int maxHealth)
    {
        if (doorUIManager != null)
            doorUIManager.UpdateDoorHealth(doorIndex, currentHealth, maxHealth);
    }
    
    void AddDoorToRadar(DoorHealth door)
    {
        if (minimapRadar != null && radarDotPrefab != null && door != null)
        {
            GameObject radarDot = Instantiate(radarDotPrefab, minimapRadar.radarPanel);
            UnityEngine.UI.Image dotImage = radarDot.GetComponent<UnityEngine.UI.Image>();
            if (dotImage != null)
                dotImage.color = doorRadarColor;
            
            RadarDot dotScript = radarDot.AddComponent<RadarDot>();
            dotScript.target = door.transform;
            dotScript.alwaysShow = true;
            
            RectTransform rect = radarDot.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(minimapRadar.dotSize, minimapRadar.dotSize);
            
            if (minimapRadar != null)
                minimapRadar.UpdateCustomDotPosition(radarDot, door.transform.position);
            
            doorRadarDots.Add(radarDot);
        }
    }
    
    void Update()
    {
        if (!missionActive) return;
        
        // Update radar dot positions
        for (int i = 0; i < doorRadarDots.Count && i < doors.Length; i++)
        {
            if (doorRadarDots[i] != null && doors[i] != null && doors[i].currentHealth > 0)
            {
                if (minimapRadar != null)
                {
                    minimapRadar.UpdateCustomDotPosition(doorRadarDots[i], doors[i].transform.position);
                }
            }
            else if (doorRadarDots[i] != null && (doors[i] == null || doors[i].currentHealth <= 0))
            {
                Destroy(doorRadarDots[i]);
                doorRadarDots.RemoveAt(i);
                i--;
            }
        }
        
        UpdateUI();
    }
    
    IEnumerator TimerRoutine()
    {
        while (timeRemaining > 0 && missionActive && doorsAlive > 0)
        {
            yield return new WaitForSeconds(1f);
            timeRemaining--;
            UpdateUI();
        }
        
        if (timeRemaining <= 0 && doorsAlive > 0 && missionActive)
        {
            MissionComplete();
        }
    }
    
    void UpdateUI()
    {
        if (timerText != null)
        {
            int minutes = Mathf.FloorToInt(timeRemaining / 60);
            int seconds = Mathf.FloorToInt(timeRemaining % 60);
            timerText.text = $"Time Left: {minutes:00}:{seconds:00}";
        }
        
        if (doorsStatusText != null)
            doorsStatusText.text = $"Doors Standing: {doorsAlive}";
    }
    
    void MissionComplete()
    {
        missionActive = false;
        
        if (zombieSpawner != null)
            zombieSpawner.StopSpawning();
        
        foreach (GameObject dot in doorRadarDots)
        {
            if (dot != null) Destroy(dot);
        }
        doorRadarDots.Clear();
        
        if (doorUIManager != null)
            doorUIManager.HideUI();
        
        Debug.Log("Protect Door Mission Complete!");
        
        if (levelManager != null)
            levelManager.CompleteMission(true);
    }
    public void ShowDoorRepairing(int doorIndex)
    {
        if (doorUIManager != null)
            doorUIManager.ShowDoorRepairing(doorIndex);
    }
    void MissionFailed()
    {
        missionActive = false;
        
        if (zombieSpawner != null)
            zombieSpawner.StopSpawning();
        
        foreach (GameObject dot in doorRadarDots)
        {
            if (dot != null) Destroy(dot);
        }
        doorRadarDots.Clear();
        
        if (doorUIManager != null)
            doorUIManager.HideUI();
        
        Debug.Log("Protect Door Mission Failed - Door destroyed!");
        
        if (levelManager != null)
        {
            // Call ShowMissionFailed instead of CompleteMission
            levelManager.ShowMissionFailed();
        }
    }
    
    public void OnDoorDestroyed()
    {
        doorsAlive--;
        
        // Fail mission immediately when ANY door is destroyed
        if (missionActive)
        {
            MissionFailed();
        }
    }
    
    public bool IsMissionActive()
    {
        return missionActive;
    }
}