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
    
    private POIType currentMissionType;
    private bool missionCompleted = false;
    private bool missionFailed = false;
    
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
    }
    
    public void PlayerDied()
    {
        if (!missionCompleted && !missionFailed)
        {
            CompleteMission(false);
        }
    }
    
   void CompleteMission(bool success)
    {
        if (missionCompleted || missionFailed)
            return;
            
        missionCompleted = success;
        
        if (success)
        {
            Debug.Log("Mission Complete!");
            
            // Stop zombie spawner
            ZombieSpawner spawner = FindObjectOfType<ZombieSpawner>();
            if (spawner != null)
                spawner.StopSpawning();
            
            // Freeze remaining zombies (they won't attack)
            GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
            foreach (GameObject enemy in enemies)
            {
                // Disable NavMeshAgent to stop movement
                UnityEngine.AI.NavMeshAgent agent = enemy.GetComponent<UnityEngine.AI.NavMeshAgent>();
                if (agent != null) agent.enabled = false;
                
                // Disable enemy AI
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
            Debug.Log("Mission Failed!");
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

    IEnumerator ReturnToMapAfterDelay(float delay)
    {
        yield return new WaitForSecondsRealtime(delay); // Use real time since game is frozen
        Time.timeScale = 1f; // Reset time scale before loading
        SceneManager.LoadScene("MapScene");
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
        
        SceneManager.LoadScene("MapScene");
    }

    int CalculateReward()
    {
        if (GameManager.instance == null || GameManager.instance.CurrentPOI == null)
            return 50;
            
        POIData poi = GameManager.instance.CurrentPOI;
        int baseReward = 50;
        float difficultyMultiplier = 1 + (poi.difficultyLevel - 1) * 0.2f;
        float typeBonus = currentMissionType == POIType.CountdownSurvive ? 1.2f : 1f;
        
        return Mathf.RoundToInt(baseReward * difficultyMultiplier * typeBonus);
    }
    
    int CalculateXP()
    {
        if (GameManager.instance == null || GameManager.instance.CurrentPOI == null)
            return 25;
            
        POIData poi = GameManager.instance.CurrentPOI;
        int baseXP = 25;
        float difficultyMultiplier = 1 + (poi.difficultyLevel - 1) * 0.15f;
        
        return Mathf.RoundToInt(baseXP * difficultyMultiplier);
    }
}