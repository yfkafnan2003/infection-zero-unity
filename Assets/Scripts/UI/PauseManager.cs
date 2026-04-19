using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    public static PauseManager Instance;
    
    [Header("Panels")]
    public GameObject pausePanel;
    public GameObject settingsPanel;
    public GameObject termsPanel;
    
    [Header("Buttons")]
    public GameObject pauseButton;
    
    [Header("UI Sounds")]
    public AudioSource uiAudioSource;
    public AudioClip buttonClickSound;
    
    private bool isPaused = false;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }
    
    void Start()
    {
        ResumeGame();
        if (pausePanel != null) pausePanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (termsPanel != null) termsPanel.SetActive(false);
    }
    
    
    public void PlayButtonSound()
    {
        if (uiAudioSource != null && buttonClickSound != null)
        {
            uiAudioSource.PlayOneShot(buttonClickSound);
        }
    }
    
    public void PauseGame()
    {
        PlayButtonSound();
        isPaused = true;
        Time.timeScale = 0f;
        
        if (pausePanel != null) pausePanel.SetActive(true);
        if (pauseButton != null) pauseButton.SetActive(false);
        
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    
    public void ResumeGame()
    {
        PlayButtonSound();
        isPaused = false;
        Time.timeScale = 1f;
        
        if (pausePanel != null) pausePanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (termsPanel != null) termsPanel.SetActive(false);
        if (pauseButton != null) pauseButton.SetActive(true);
        
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

    }
    
    public void OpenSettings()
    {
        PlayButtonSound();
        if (pausePanel != null) pausePanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(true);
    }
    
    public void CloseSettings()
    {
        PlayButtonSound();
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (pausePanel != null) pausePanel.SetActive(true);
        
        if (SettingsManager.Instance != null)
            SettingsManager.Instance.ApplySettings();
    }
    
    public void OpenTerms()
    {
        PlayButtonSound();
        if (pausePanel != null) pausePanel.SetActive(false);
        if (termsPanel != null) termsPanel.SetActive(true);
    }
    
    public void CloseTerms()
    {
        PlayButtonSound();
        if (termsPanel != null) termsPanel.SetActive(false);
        if (pausePanel != null) pausePanel.SetActive(true);
    }
    
    public void ExitToMainMenu()
    {
        PlayButtonSound();
        Time.timeScale = 1f;
        if (LoadingScreen.Instance != null)
        {
            LoadingScreen.Instance.LoadScene("MapScene");
        }
        else
        {
            SceneManager.LoadScene("MapScene");
        }
    }
}