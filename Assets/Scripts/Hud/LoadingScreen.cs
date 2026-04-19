using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;
using UnityEngine.SceneManagement;

public class LoadingScreen : MonoBehaviour
{
    public static LoadingScreen Instance;
    
    [Header("UI Elements")]
    public GameObject loadingPanel;
    public Slider progressBar;
    public TextMeshProUGUI progressText;
    public TextMeshProUGUI loadingTipText;
    public Image loadingImage;
    
    [Header("Loading Tips")]
    public string[] loadingTips;
    
    [Header("Loading Images")]
    public Sprite[] loadingImages;
    public bool randomizeImage = true;
    
    [Header("Loading Settings")]
    public float minimumLoadTime = 2f; // Minimum time to show loading screen
    public float fakeProgressSpeed = 0.3f; // Speed of fake progress animation
    
    private bool isLoading = false;
    private float fakeProgress = 0f;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("LoadingScreen initialized");
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        if (loadingPanel != null)
            loadingPanel.SetActive(false);
    }
    
    public void LoadScene(string sceneName)
    {
        if (isLoading)
        {
            Debug.LogWarning("Already loading a scene!");
            return;
        }
        
        StartCoroutine(LoadSceneAsync(sceneName));
    }
    
    IEnumerator LoadSceneAsync(string sceneName)
    {
        isLoading = true;
        fakeProgress = 0f;
        
        // Show loading screen
        if (loadingPanel != null)
        {
            loadingPanel.SetActive(true);
            Debug.Log("Loading panel activated");
        }
        
        // Reset progress bar
        if (progressBar != null)
            progressBar.value = 0f;
        
        // Show random tip
        if (loadingTipText != null && loadingTips != null && loadingTips.Length > 0)
        {
            int randomTip = Random.Range(0, loadingTips.Length);
            loadingTipText.text = loadingTips[randomTip];
        }
        
        // Show random image
        if (loadingImage != null && loadingImages != null && loadingImages.Length > 0 && randomizeImage)
        {
            int randomImage = Random.Range(0, loadingImages.Length);
            loadingImage.sprite = loadingImages[randomImage];
            loadingImage.color = Color.white;
            loadingImage.preserveAspect = true;
            Debug.Log($"Loading image set to index {randomImage}");
        }
        
        // Start async loading
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
        
        if (operation == null)
        {
            Debug.LogError($"Failed to load scene: {sceneName}");
            isLoading = false;
            yield break;
        }
        
        operation.allowSceneActivation = false;
        
        float startTime = Time.unscaledTime;
        bool hasReachedReady = false;
        float timeWhenReady = 0f;
        
        // Loading loop
        while (!operation.isDone)
        {
            // Real progress from Unity (0 to 0.9)
            float realProgress = Mathf.Clamp01(operation.progress / 0.9f);
            
            // Smoothly increase fake progress towards real progress
            fakeProgress = Mathf.MoveTowards(fakeProgress, realProgress, Time.unscaledDeltaTime * fakeProgressSpeed);
            
            // Display fake progress (looks smoother)
            float displayProgress = fakeProgress;
            
            if (progressBar != null)
                progressBar.value = displayProgress;
            
            if (progressText != null)
                progressText.text = $"Loading... {Mathf.RoundToInt(displayProgress * 100)}%";
            
            // Check if loading is complete (reached 90%)
            if (operation.progress >= 0.9f && !hasReachedReady)
            {
                hasReachedReady = true;
                timeWhenReady = Time.unscaledTime;
                Debug.Log("Loading ready, waiting for minimum time...");
            }
            
            // If loading is ready and minimum time has passed, activate scene
            if (hasReachedReady && (Time.unscaledTime - timeWhenReady) >= minimumLoadTime)
            {
                // Fill progress bar to 100% smoothly
                while (fakeProgress < 1f)
                {
                    fakeProgress = Mathf.MoveTowards(fakeProgress, 1f, Time.unscaledDeltaTime * fakeProgressSpeed * 2);
                    
                    if (progressBar != null)
                        progressBar.value = fakeProgress;
                    
                    if (progressText != null)
                        progressText.text = $"Loading... {Mathf.RoundToInt(fakeProgress * 100)}%";
                    
                    yield return null;
                }
                
                Debug.Log("Loading complete, activating scene...");
                operation.allowSceneActivation = true;
            }
            
            yield return null;
        }
        
        // Small delay to ensure scene is ready
        yield return null;
        
        // Hide loading screen
        if (loadingPanel != null)
            loadingPanel.SetActive(false);
        
        isLoading = false;
        Debug.Log($"Scene {sceneName} loaded successfully");
    }
    
    public void HideLoadingScreen()
    {
        if (loadingPanel != null)
            loadingPanel.SetActive(false);
        isLoading = false;
    }
}