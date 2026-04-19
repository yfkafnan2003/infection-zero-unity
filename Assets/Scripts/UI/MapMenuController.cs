using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class MapMenuController : MonoBehaviour
{
    [Header("Main Menu Panel")]
    public GameObject mainMenuPanel; // The main menu panel that contains all buttons
    
    [Header("Individual Panels")]
    public GameObject settingsPanel; // Your existing settings panel
    public GameObject shopPanel;
    public GameObject equipPanel;
    public GameObject savePanel;
    
    [Header("References")]
    public SettingsManager settingsManager; // Reference to your existing SettingsManager
    public Button menuButton; // The button that opens the menu
    // Add these variables to your MapMenuController
    [Header("Shop and Equip")]
    public ShopManager shopManager;
    public EquipManager equipManager;

    [Header("Animation Settings")]
    public float panelAnimationDuration = 0.3f;
    public AnimationCurve panelAnimationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    private bool isMenuOpen = false;
    private CanvasGroup mainMenuCanvasGroup;
    private RectTransform mainMenuRectTransform;
    
    void Start()
    {
        // Setup canvas group for fade animations
        if (mainMenuPanel != null)
        {
            mainMenuCanvasGroup = mainMenuPanel.GetComponent<CanvasGroup>();
            if (mainMenuCanvasGroup == null)
            {
                mainMenuCanvasGroup = mainMenuPanel.AddComponent<CanvasGroup>();
            }
            
            mainMenuRectTransform = mainMenuPanel.GetComponent<RectTransform>();
            
            // Ensure menu starts closed
            mainMenuPanel.SetActive(false);
            mainMenuCanvasGroup.alpha = 0;
        }
        
        // Setup menu button listener
        if (menuButton != null)
        {
            menuButton.onClick.AddListener(ToggleMenu);
        }
        else
        {
            Debug.LogError("Menu button not assigned in MapMenuController!");
        }
        
        // Ensure all panels are closed initially
        CloseAllPanels();
    }
    
    void ToggleMenu()
    {
        if (isMenuOpen)
        {
            CloseMenu();
        }
        else
        {
            OpenMenu();
        }
    }
    
    void OpenMenu()
    {
        if (mainMenuPanel == null) return;
        
        isMenuOpen = true;
        mainMenuPanel.SetActive(true);
        
        // Optional: Play sound effect if you have one
        if (settingsManager != null)
        {
            settingsManager.PlayButtonSound();
        }
        
        // Start fade in animation
        StartCoroutine(AnimatePanel(true));
    }
    
    void CloseMenu()
    {
        if (mainMenuPanel == null) return;
        
        isMenuOpen = false;
        
        // Close any open sub-panels when closing main menu
        CloseAllPanels();
        
        // Start fade out animation
        StartCoroutine(AnimatePanel(false));
    }
    
    IEnumerator AnimatePanel(bool open)
    {
        if (mainMenuCanvasGroup == null) yield break;
        
        float startAlpha = open ? 0 : 1;
        float endAlpha = open ? 1 : 0;
        float elapsedTime = 0;
        
        if (open)
        {
            mainMenuPanel.SetActive(true);
        }
        
        while (elapsedTime < panelAnimationDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / panelAnimationDuration;
            float curveValue = panelAnimationCurve.Evaluate(t);
            mainMenuCanvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, curveValue);
            yield return null;
        }
        
        mainMenuCanvasGroup.alpha = endAlpha;
        
        if (!open)
        {
            mainMenuPanel.SetActive(false);
        }
    }
    
    public void OnSettingsButtonClick()
    {
        Debug.Log("Settings button clicked");
        
        // Play sound
        if (settingsManager != null)
        {
            settingsManager.PlayButtonSound();
        }
        
        // Close main menu and open settings panel
        if (settingsPanel != null)
        {
            CloseMenu();
            settingsPanel.SetActive(true);
            
            // Refresh settings UI if needed
            if (settingsManager != null)
            {
                settingsManager.LoadSettings();  // Load saved settings
                settingsManager.ApplySettings(); // Apply to current scene
            }
        }
        else
        {
            Debug.LogWarning("Settings panel not assigned!");
        }
    }
    
    public void OnShopButtonClick()
    {
        Debug.Log("Shop button clicked");
        
        if (settingsManager != null)
            settingsManager.PlayButtonSound();
        
        CloseMenu();
        
        if (shopManager != null)
            shopManager.OpenShop();
        else
            Debug.LogWarning("ShopManager not assigned!");
    }

    public void OnEquipButtonClick()
    {
        Debug.Log("Equip button clicked");
        
        if (settingsManager != null)
            settingsManager.PlayButtonSound();
        
        CloseMenu();
        
        if (equipManager != null)
            equipManager.OpenEquipPanel();
        else
            Debug.LogWarning("EquipManager not assigned!");
    }
    public void OnSaveButtonClick()
    {
        Debug.Log("Save button clicked - Structure created");
        
        // Play sound
        if (settingsManager != null)
        {
            settingsManager.PlayButtonSound();
        }
        
        // Close main menu and open save panel
        if (savePanel != null)
        {
            CloseMenu();
            savePanel.SetActive(true);
        }
        else
        {
            Debug.LogWarning("Save panel not assigned! Will implement save logic later.");
        }
    }
    
    public void OnExitButtonClick()
    {
        Debug.Log("Exit button clicked - Exiting game");
        
        // Play sound
        if (settingsManager != null)
        {
            settingsManager.PlayButtonSound();
        }
        
        // Exit the game
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
    
    public void CloseAllPanels()
    {
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (shopPanel != null) shopPanel.SetActive(false);
        if (equipPanel != null) equipPanel.SetActive(false);
        if (savePanel != null) savePanel.SetActive(false);
    }

    // Method to check if menu is open
    public bool IsMenuOpen()
    {
        return isMenuOpen;
    }
    
    // Optional: Method to close menu from outside (e.g., when clicking outside)
    public void CloseMenuFromOutside()
    {
        if (isMenuOpen)
        {
            CloseMenu();
        }
    }
}