using UnityEngine;
using UnityEngine.SceneManagement;

public class UIFinder : MonoBehaviour
{
    public static UIFinder Instance;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    
    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Auto-find and register UI elements after scene loads
        UICustomizationManager uiManager = FindObjectOfType<UICustomizationManager>();
        if (uiManager != null)
        {
            uiManager.FindUIElementsInScene();
            uiManager.LoadPositions();
        }
    }
}