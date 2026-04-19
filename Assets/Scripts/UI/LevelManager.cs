using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class LevelManager : MonoBehaviour
{
    [Header("Mission UI")]
    public TextMeshProUGUI missionTitleText;
    public TextMeshProUGUI missionObjectiveText;
    public TextMeshProUGUI missionProgressText;
    public Image missionIcon;
    
    [Header("Mission Icons")]
    public Sprite killZombiesIcon;
    public Sprite countdownIcon;
    public Sprite retrieveBoxIcon;
    public Sprite protectDoorIcon;
    private bool isGameFrozen = false;
    
    [Header("Kill Zombies Settings")]
    public int zombiesToKill = 10;
    public int currentZombieKills = 0;
    
    [Header("Countdown Settings")]
    public float timeRemaining = 60f;
    public bool isCountdownActive = false;
    
    [Header("Mission Completion")]
    public GameObject completionPanel;
    public TextMeshProUGUI completionText;
    public Button continueButton;
    public CanvasGroup completionPanelCanvasGroup; // Add this for fade effect
    public float slowMotionTimeScale = 0.2f; // Slow motion speed
    public float fadeInDuration = 1.5f; // How long fade takes
    private ArenaManager arenaManager;

    [Header("Death Panel")]
    public GameObject deathPanel;
    public TextMeshProUGUI deathText;
    public AudioClip deathSound;
    public AudioClip winsound;
    public float deathSceneDelay = 3f;
    public AudioSource audioSource;

    [Header("References")]
    public PlayerHealth playerHealth;
    public GameObject enemySpawner;
    [HideInInspector] public POIData currentPOIData;
    private POIType currentMissionType;
    private bool missionCompleted = false;
    private bool missionFailed = false;
    private ProtectDoorManager protectDoorManager;

    private BossHealth bossHealth;
    void Start()
    {
        // Setup completion panel fade
        if (completionPanelCanvasGroup == null && completionPanel != null)
            completionPanelCanvasGroup = completionPanel.GetComponent<CanvasGroup>();
        
        if (completionPanelCanvasGroup != null)
        {
            completionPanelCanvasGroup.alpha = 0f;
        }
        
        // Debug: Check if GameManager exists
        if (GameManager.instance == null)
        {
            Debug.LogError("GameManager instance not found! Make sure GameManager exists in the scene.");
            missionTitleText.text = "ERROR";
            missionObjectiveText.text = "GameManager not found!";
            return;
        }
        if (GameManager.instance != null && GameManager.instance.CurrentPOI != null)
        {
            currentPOIData = GameManager.instance.CurrentPOI;
            
            // Configure zombie spawner
            ZombieSpawner spawner = FindObjectOfType<ZombieSpawner>();
            if (spawner != null)
            {
                spawner.ConfigureFromPOI(currentPOIData);
            }
            
            // Setup mission based on POI type
            SetupMissionFromPOI(currentPOIData);
        }
        // Debug: Check current POI
        if (GameManager.instance.CurrentPOI == null)
        {
            Debug.LogError("No POI data found in GameManager! Make sure MapManager saved it correctly.");
            
            // Try to find POI data in the scene as fallback
            POIData scenePOI = FindObjectOfType<POIData>();
            if (scenePOI != null)
            {
                Debug.Log("Found POI data in scene as fallback: " + scenePOI.poiName);
                GameManager.instance.CurrentPOI = scenePOI;
            }
            else
            {
                missionTitleText.text = "ERROR";
                missionObjectiveText.text = "No mission data found!";
                return;
            }
        }
        
        POIData currentPOI = GameManager.instance.CurrentPOI;
        Debug.Log("Loading mission from POI: " + currentPOI.poiName);
        Debug.Log("POI Type: " + currentPOI.poiType);
        
        currentMissionType = currentPOI.poiType;
        
        // Setup mission based on type
        switch (currentMissionType)
        {
            case POIType.KillZombies:
                SetupKillZombiesMission(currentPOI.zombieAmount);
                break;
                
            case POIType.CountdownSurvive:
                SetupCountdownMission(currentPOI.surviveTime);
                break;
            case POIType.Arena:  // Add this case
                SetupArenaMission();
                break;
            case POIType.RetrieveBox:
                SetupRetrieveBoxMission();
                break;   
            case POIType.ProtectDoor:
                SetupProtectDoorMission();
                break;
            case POIType.BossFight:
                SetupBossFightMission();
                break;
            default:
                Debug.LogWarning("Unsupported mission type: " + currentMissionType);
                missionTitleText.text = "UNSUPPORTED MISSION";
                missionObjectiveText.text = "This mission type is not implemented yet!";
                break;
        }
        
        if (continueButton != null)
            continueButton.onClick.AddListener(OnContinueButtonClicked);
            
        // Hide panels initially
        if (completionPanel != null)
            completionPanel.SetActive(false);
            
        if (deathPanel != null)
            deathPanel.SetActive(false);
    }
    public bool IsMissionComplete()
    {
        return missionCompleted || missionFailed;
    }
    void SetupKillZombiesMission(int zombieAmount)
    {
        zombiesToKill = zombieAmount;
        currentZombieKills = 0;
        
        // Update UI
        if (missionTitleText != null)
            missionTitleText.text = "KILL ZOMBIES";
            
        if (missionObjectiveText != null)
            missionObjectiveText.text = $"Eliminate all zombies in the area";
            
        if (missionIcon != null && killZombiesIcon != null)
            missionIcon.sprite = killZombiesIcon;
            
        UpdateMissionProgress();
        
        Debug.Log($"Kill Zombies Mission Started. Need to kill: {zombiesToKill} zombies");
    }
    void SetupMissionFromPOI(POIData poi)
    {
        switch (poi.poiType)
        {
            case POIType.KillZombies:
                SetupKillZombiesMission(poi.zombieAmount);
                break;
                
            case POIType.CountdownSurvive:
                SetupCountdownMission(poi.surviveTime);
                break;
        }
    }
    void SetupArenaMission()
    {
        if (missionTitleText != null)
            missionTitleText.text = "ARENA MODE";
        
        if (missionObjectiveText != null)
            missionObjectiveText.text = "Survive all 20 rounds!";
        
        if (missionIcon != null)
            missionIcon.sprite = killZombiesIcon; // Or use arena icon
        
        arenaManager = FindObjectOfType<ArenaManager>();
        if (arenaManager == null)
        {
            Debug.LogError("ArenaManager not found in scene!");
        }
        
        UpdateMissionProgress();
    }
    void SetupBossFightMission()
    {
        if (missionTitleText != null)
            missionTitleText.text = "BOSS FIGHT";
        
        if (missionObjectiveText != null)
            missionObjectiveText.text = "Defeat the Boss!";
        
        if (missionIcon != null)
            missionIcon.sprite = killZombiesIcon;
        
        bossHealth = FindObjectOfType<BossHealth>();
        if (bossHealth == null)
        {
            Debug.LogError("BossHealth not found in scene!");
        }
        
        UpdateMissionProgress();
    }

    void UpdateBossFightMission()
    {
        if (bossHealth != null && missionProgressText != null)
        {
            missionProgressText.text = $"Boss Health: {bossHealth.currentHealth}/{bossHealth.maxHealth}";
        }
    }
    void SlowMotionAndFadeIn()
    {
        // Set slow motion
        Time.timeScale = slowMotionTimeScale;
        
        // Show completion panel
        if (completionPanel != null)
        {
            completionPanel.SetActive(true);
            StartCoroutine(FadeInPanel());
        }
    }
    
    IEnumerator FadeInPanel()
    {
        if (completionPanelCanvasGroup == null)
            yield break;
            
        float elapsedTime = 0f;
        
        while (elapsedTime < fadeInDuration)
        {
            elapsedTime += Time.unscaledDeltaTime; // Use unscaled time since time is slowed
            float alpha = Mathf.Clamp01(elapsedTime / fadeInDuration);
            completionPanelCanvasGroup.alpha = alpha;
            yield return null;
        }
        
        completionPanelCanvasGroup.alpha = 1f;
    }
    
    void SetupCountdownMission(int surviveTime)
    {
        timeRemaining = surviveTime;
        isCountdownActive = true;
        
        // Update UI
        if (missionTitleText != null)
            missionTitleText.text = "SURVIVE";
            
        if (missionObjectiveText != null)
            missionObjectiveText.text = "Survive until time runs out";
            
        if (missionIcon != null && countdownIcon != null)
            missionIcon.sprite = countdownIcon;
            
        UpdateMissionProgress();
        
        Debug.Log($"Countdown Mission Started. Need to survive: {surviveTime} seconds");
    }
        
    void Update()
    {
        if (missionCompleted || missionFailed)
            return;
        
        // Update mission progress based on type
        switch (currentMissionType)
        {
            case POIType.KillZombies:
                UpdateKillZombiesMission();
                break;
                
            case POIType.CountdownSurvive:
                UpdateCountdownMission();
                break;
                
            case POIType.Arena:
                UpdateArenaMission();
                break;
            case POIType.ProtectDoor:
                UpdateProtectDoorMission();
                break;
            case POIType.RetrieveBox:
                UpdateRetrieveBoxMission();
                break;
            case POIType.BossFight:
                UpdateBossFightMission();
                break;
        }
    }
    public void InstantMissionComplete()
    {
        if (missionCompleted || missionFailed) return;
        CompleteMission(true);
    }
    void UpdateArenaMission()
    {
        if (arenaManager != null && missionProgressText != null)
        {
            // Show round info in mission progress
            missionProgressText.text = $"Round: {arenaManager.GetCurrentRound()}/{arenaManager.GetTotalRounds()}\n" +
                                    $"Zombies Left: {arenaManager.GetZombiesRemaining()}";
        }
    }
    
    void UpdateKillZombiesMission()
    {
        if (!missionCompleted && currentZombieKills >= zombiesToKill)
        {
            CompleteMission(true);
        }
        else
        {
            UpdateMissionProgress();
        }
    }
    private RetrieveBoxManager retrieveBoxManager;

    void SetupRetrieveBoxMission()
    {
        if (missionTitleText != null)
            missionTitleText.text = "RETRIEVE BOXES";
        
        if (missionObjectiveText != null)
            missionObjectiveText.text = $"Find {currentPOIData.boxesToCollect} boxes and reach the exit";
        
        if (missionIcon != null && retrieveBoxIcon != null) // Use retrieveBoxIcon instead
            missionIcon.sprite = retrieveBoxIcon;
        
        retrieveBoxManager = FindObjectOfType<RetrieveBoxManager>();
        if (retrieveBoxManager == null)
        {
            Debug.LogError("RetrieveBoxManager not found in scene!");
        }
        
        UpdateMissionProgress();
    }

    void UpdateRetrieveBoxMission()
    {
        if (retrieveBoxManager != null && missionProgressText != null)
        {
            missionProgressText.text = $"Boxes Collected: {currentPOIData.boxesToCollect - retrieveBoxManager.GetBoxesRemaining()}/{currentPOIData.boxesToCollect}";
        }
    }
    void UpdateCountdownMission()
    {
        if (isCountdownActive && !missionCompleted)
        {
            timeRemaining -= Time.deltaTime;
            UpdateMissionProgress();
            
            if (timeRemaining <= 0)
            {
                timeRemaining = 0;
                isCountdownActive = false;
                CompleteMission(true);
            }
        }
    }
    
    void UpdateMissionProgress()
    {
        if (missionProgressText == null)
            return;
            
        switch (currentMissionType)
        {
            case POIType.KillZombies:
                missionProgressText.text = $"Zombies Remaining: {zombiesToKill - currentZombieKills} / {zombiesToKill}";
                break;
                
            case POIType.CountdownSurvive:
                int minutes = Mathf.FloorToInt(timeRemaining / 60);
                int seconds = Mathf.FloorToInt(timeRemaining % 60);
                missionProgressText.text = $"Time Remaining: {minutes:00}:{seconds:00}";
                break;
        }
    }
    
    public void RegisterZombieKill()
    {
        if (currentMissionType == POIType.KillZombies && !missionCompleted)
        {
            currentZombieKills++;
            UpdateMissionProgress();
            Debug.Log($"Zombie killed! Progress: {currentZombieKills}/{zombiesToKill}");
        }
        else if (currentMissionType == POIType.Arena && !missionCompleted)
        {
            if (arenaManager != null)
            {
                arenaManager.RegisterZombieKill();
            }
        }
    }
    void SetupProtectDoorMission()
    {
        if (missionTitleText != null)
            missionTitleText.text = "PROTECT THE DOOR";
        
        if (missionObjectiveText != null)
            missionObjectiveText.text = $"Protect the door for {currentPOIData.protectTime} seconds";
        
        if (missionIcon != null && protectDoorIcon != null)
            missionIcon.sprite = protectDoorIcon;
        
        protectDoorManager = FindObjectOfType<ProtectDoorManager>();
        if (protectDoorManager == null)
        {
            Debug.LogError("ProtectDoorManager not found in scene!");
        }
        
        UpdateMissionProgress();
    }

    void UpdateProtectDoorMission()
    {
        if (protectDoorManager != null && missionProgressText != null)
        {
        }
    }
    public void PlayerDied()
    {
        if (!missionCompleted && !missionFailed)
        {
            CompleteMission(false);
        }
    }
    
    public void CompleteMission(bool success)
    {
        if (missionCompleted || missionFailed)
            return;
        
        if (success)
        {
            missionCompleted = true;
            Debug.Log("Mission Complete!");
            
            // Stop zombie spawner
            ZombieSpawner spawner = FindObjectOfType<ZombieSpawner>();
            if (spawner != null)
                spawner.StopSpawning();
            
            // Freeze remaining zombies (they won't attack)
            GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
            foreach (GameObject enemy in enemies)
            {
                UnityEngine.AI.NavMeshAgent agent = enemy.GetComponent<UnityEngine.AI.NavMeshAgent>();
                if (agent != null) agent.enabled = false;
                
                MonoBehaviour[] scripts = enemy.GetComponents<MonoBehaviour>();
                foreach (MonoBehaviour script in scripts)
                {
                    script.enabled = false;
                }
            }
            
            // Play win sound
            if (audioSource != null && winsound != null)
            {
                audioSource.PlayOneShot(winsound);
            }
            
            int moneyReward = CalculateReward();
            int xpReward = CalculateXP();
            
            if (GameManager.instance != null)
            {
                GameManager.instance.AddMoney(moneyReward);
                GameManager.instance.AddXP(xpReward);
            }
            
            if (completionText != null)
            {
                completionText.text = $"MISSION COMPLETE!\n\nRewards:\n${moneyReward}\n{xpReward} XP";
            }
            
            // Apply slow motion and fade in effect
            SlowMotionAndFadeIn();
        }
        else
        {
            // Player died
            missionFailed = true;
            Debug.Log("Player Died - Mission Failed!");
            
            // Stop zombie spawner
            ZombieSpawner spawner = FindObjectOfType<ZombieSpawner>();
            if (spawner != null)
                spawner.StopSpawning();
            
            FreezeGame();
            ShowDeathPanel();
        }
    }
    
    void FreezeGame()
    {
        isGameFrozen = true;
        Time.timeScale = 0f;
        
        // Find and disable all enemies (make them unable to attack)
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject enemy in enemies)
        {
            // Disable NavMeshAgent to stop movement
            UnityEngine.AI.NavMeshAgent agent = enemy.GetComponent<UnityEngine.AI.NavMeshAgent>();
            if (agent != null) agent.enabled = false;
            
            // Disable enemy AI scripts
            MonoBehaviour[] scripts = enemy.GetComponents<MonoBehaviour>();
            foreach (MonoBehaviour script in scripts)
            {
                script.enabled = false;
            }
            
            // Also disable colliders to prevent any remaining damage
            Collider[] colliders = enemy.GetComponents<Collider>();
            foreach (Collider col in colliders)
            {
                col.enabled = false;
            }
        }
        
        // Disable player movement
        PlayerMovement playerMovement = FindObjectOfType<PlayerMovement>();
        if (playerMovement != null) playerMovement.enabled = false;
        
        // Disable player shooting
        Gun gun = FindObjectOfType<Gun>();
        if (gun != null) gun.enabled = false;
    }


    void UnfreezeGame()
    {
        Time.timeScale = 1f;
    }

    void ShowDeathPanel()
    {
        if (deathPanel != null)
        {
            deathPanel.SetActive(true);
            if (deathText != null)
            {
                deathText.text = "YOU DIED";
            }
            
            if (audioSource != null && deathSound != null)
            {
                audioSource.PlayOneShot(deathSound);
            }
            
            StartCoroutine(ReturnToMapAfterDelay(deathSceneDelay));
        }
    }
    public void ShowMissionFailed()
    {
        if (missionCompleted || missionFailed)
            return;
        
        missionFailed = true;
        
        Debug.Log("Mission Failed!");
        
        // Stop zombie spawner
        ZombieSpawner spawner = FindObjectOfType<ZombieSpawner>();
        if (spawner != null)
            spawner.StopSpawning();
        
        FreezeGame();
        
        // Show death panel but with different text
        if (deathPanel != null)
        {
            deathPanel.SetActive(true);
            if (deathText != null)
            {
                deathText.text = "MISSION FAILED!";
            }
            
            if (audioSource != null && deathSound != null)
            {
                audioSource.PlayOneShot(deathSound);
            }
            
            StartCoroutine(ReturnToMapAfterDelay(deathSceneDelay));
        }
    }
    IEnumerator ReturnToMapAfterDelay(float delay)
    {
        yield return new WaitForSecondsRealtime(delay); // Use real time since game is frozen
        // Use loading screen
        if (LoadingScreen.Instance != null)
        {
            LoadingScreen.Instance.LoadScene("MapScene");
        }
        else
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene("MapScene");
        }
    }

    void OnContinueButtonClicked()
    {
        Time.timeScale = 1f; // Reset time scale before loading
        UnfreezeGame();
        
        if (GameManager.instance != null && GameManager.instance.CurrentPOI != null)
        {
            GameManager.instance.MarkPOIAsCompleted(GameManager.instance.CurrentPOI.poiName);
            
            POIData nextPOI = GameManager.instance.GetNextPOIInChain();
            
            if (nextPOI != null)
            {
                Debug.Log($"Next POI in chain: {nextPOI.poiName}");
                GameManager.instance.CurrentPOI = nextPOI;
            }
            else
            {
                Debug.Log("All POIs completed!");
            }
        }
        
        // Use loading screen (REMOVED the duplicate SceneManager.LoadScene)
        if (LoadingScreen.Instance != null)
        {
            LoadingScreen.Instance.LoadScene("MapScene");
        }
        else
        {
            SceneManager.LoadScene("MapScene");
        }
    }

    int CalculateReward()
    {
        if (GameManager.instance == null || GameManager.instance.CurrentPOI == null)
            return 50;
            
        POIData poi = GameManager.instance.CurrentPOI;
        return poi.GetMoneyReward();
    }

    int CalculateXP()
    {
        if (GameManager.instance == null || GameManager.instance.CurrentPOI == null)
            return 25;
            
        POIData poi = GameManager.instance.CurrentPOI;
        return poi.GetXPReward();
    }
}