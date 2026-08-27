using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance;
    
    [Header("Tutorial Steps")]
    public List<TutorialStep> tutorialSteps = new List<TutorialStep>();
    
    [Header("Typing Animation Settings")]
    public float typingSpeed = 0.05f; // Time between each character
    public float typingDelay = 0.5f; // Delay before next step after typing completes
    
    public GameObject tutorialPanel;
    public TextMeshProUGUI tutorialText;
    public Button nextButton;
    public int currentStep = 0;
    private bool isTutorialActive = false;
    public string currentTutorialID = "";
    public int currentChainIndexToWait = -1;
    
    private bool isTyping = false;
    private bool isWaitingForNext = false;
    private Coroutine typingCoroutine;
    private string currentFullMessage = "";
    
    [System.Serializable]
    public class TutorialStep
    {
        [TextArea(3, 5)]
        public string message;
        public int requiredChainIndex = -1; // Which POI chain index must be completed (-1 = no requirement)
    }
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        // Find tutorial UI in the scene
        FindTutorialUI();
        
        // Check if we should resume tutorial from where we left off
        LoadTutorialProgress();
    }
    
    void FindTutorialUI()
    {
        // Find canvas
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null) return;
        
        // Find tutorial panel
        Transform panel = canvas.transform.Find("TutorialPanel");
        if (panel != null)
        {
            tutorialPanel = panel.gameObject;
            tutorialText = panel.Find("Text")?.GetComponent<TextMeshProUGUI>();
            nextButton = panel.Find("NextButton")?.GetComponent<Button>();
            
            if (nextButton != null)
                nextButton.onClick.AddListener(OnNextButtonClick);
            
            // Hide initially
            tutorialPanel.SetActive(false);
        }
    }
    
    void OnNextButtonClick()
    {
        if (isTyping)
        {
            // Skip typing animation - show full text immediately
            StopTypingAnimation();
            tutorialText.text = currentFullMessage;
            isTyping = false;
            isWaitingForNext = true;
            
            // Keep next button visible but handle next step
            if (nextButton != null)
                nextButton.interactable = true;
        }
        else if (isWaitingForNext)
        {
            // Proceed to next step
            NextStep();
        }
    }
    
    void StopTypingAnimation()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }
    }
    
    IEnumerator TypeText(string message)
    {
        isTyping = true;
        isWaitingForNext = false;
        currentFullMessage = message;
        tutorialText.text = "";
        
        // Disable next button while typing
        if (nextButton != null)
            nextButton.interactable = false;
        
        // Type each character
        foreach (char c in message.ToCharArray())
        {
            tutorialText.text += c;
            yield return new WaitForSecondsRealtime(typingSpeed);
        }
        
        // Typing complete
        isTyping = false;
        isWaitingForNext = true;
        
        // Enable next button
        if (nextButton != null)
            nextButton.interactable = true;
        
        // Optional: Auto-advance after delay
        // yield return new WaitForSecondsRealtime(typingDelay);
        // if (isWaitingForNext)
        // {
        //     NextStep();
        // }
    }
    
    void LoadTutorialProgress()
    {
        // Load which step we were on
        currentStep = PlayerPrefs.GetInt("Tutorial_Step", 0);
        currentTutorialID = PlayerPrefs.GetString("Tutorial_ID", "");
        
        // If we have a saved tutorial and it's not complete, resume it
        if (!string.IsNullOrEmpty(currentTutorialID) && currentStep < tutorialSteps.Count)
        {
            // Check if we can resume (if the required chain is completed)
            TutorialStep step = tutorialSteps[currentStep];
            if (step.requiredChainIndex <= GetCurrentChainIndex())
            {
                StartTutorial(currentTutorialID, currentStep);
            }
        }
    }
    
    int GetCurrentChainIndex()
    {
        if (GameManager.instance != null)
            return GameManager.instance.currentChainIndex;
        return 0;
    }
    
    public void StartTutorial(string tutorialID = "initial", int startStep = 0)
    {
        // Check if this tutorial was already completed
        if (PlayerPrefs.GetInt("Tutorial_Completed_" + tutorialID, 0) == 1)
            return;
        
        // Find UI first
        FindTutorialUI();
        
        if (tutorialPanel == null)
        {
            Debug.LogWarning("TutorialPanel not found in scene!");
            return;
        }
        
        currentTutorialID = tutorialID;
        currentStep = startStep;
        isTutorialActive = true;
        
        // Save progress
        PlayerPrefs.SetString("Tutorial_ID", tutorialID);
        PlayerPrefs.SetInt("Tutorial_Step", currentStep);
        PlayerPrefs.Save();
        
        ShowStep(currentStep);
    }
    
    void ShowStep(int step)
    {
        if (step >= tutorialSteps.Count)
        {
            EndTutorial();
            return;
        }
        
        TutorialStep current = tutorialSteps[step];
        
        // Check if this step requires a certain chain index
        if (current.requiredChainIndex > GetCurrentChainIndex())
        {
            // Wait for chain to complete
            tutorialPanel.SetActive(false);
            StartCoroutine(WaitForChainIndex(current.requiredChainIndex));
            return;
        }
        
        // Show the tutorial
        tutorialPanel.SetActive(true);
        
        // Start typing animation
        if (tutorialText != null && !string.IsNullOrEmpty(current.message))
        {
            StopTypingAnimation();
            typingCoroutine = StartCoroutine(TypeText(current.message));
        }
        
        // Hide next button until typing completes (it will be shown in TypeText coroutine)
        if (nextButton != null)
        {
            nextButton.interactable = false;
            nextButton.gameObject.SetActive(true);
        }
        
        // Block game input
        Time.timeScale = 0f;
    }
    
    IEnumerator WaitForChainIndex(int requiredIndex)
    {
        while (GetCurrentChainIndex() < requiredIndex)
        {
            yield return new WaitForSecondsRealtime(0.5f);
        }
        
        // Chain completed, show the step
        if (isTutorialActive)
        {
            tutorialPanel.SetActive(true);
            ShowStep(currentStep);
        }
    }
    
    public void NextStep()
    {
        if (!isTutorialActive) return;
        if (isTyping) return; // Don't advance while typing
        
        currentStep++;
        
        // Save progress
        PlayerPrefs.SetInt("Tutorial_Step", currentStep);
        PlayerPrefs.Save();
        
        if (currentStep < tutorialSteps.Count)
        {
            ShowStep(currentStep);
        }
        else
        {
            EndTutorial();
        }
    }
    
    void EndTutorial()
    {
        isTutorialActive = false;
        isTyping = false;
        isWaitingForNext = false;
        
        StopTypingAnimation();
        
        if (tutorialPanel != null)
            tutorialPanel.SetActive(false);
        
        // Mark as completed
        PlayerPrefs.SetInt("Tutorial_Completed_" + currentTutorialID, 1);
        PlayerPrefs.DeleteKey("Tutorial_ID");
        PlayerPrefs.DeleteKey("Tutorial_Step");
        PlayerPrefs.Save();
        
        // Unblock game input
        Time.timeScale = 1f;
        
        Debug.Log("Tutorial completed!");
    }
    
    public void ResetTutorialProgress()
    {
        PlayerPrefs.DeleteKey("Tutorial_ID");
        PlayerPrefs.DeleteKey("Tutorial_Step");
        PlayerPrefs.DeleteKey("Tutorial_Completed_initial");
        
        PlayerPrefs.Save();
        currentStep = 0;
        isTutorialActive = false;
        isTyping = false;
        isWaitingForNext = false;
        StopTypingAnimation();
        Debug.Log("Tutorial progress reset!");
    }
    
    public bool IsTutorialActive()
    {
        return isTutorialActive;
    }
    
    public bool IsTyping()
    {
        return isTyping;
    }
}