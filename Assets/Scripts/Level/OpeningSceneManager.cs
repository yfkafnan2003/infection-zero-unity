using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;
using System.Collections;

public class OpeningSceneManager : MonoBehaviour
{
    [Header("Video Settings")]
    public VideoPlayer videoPlayer;
    public GameObject skipButton;
    
    [Header("Loading Screen")]
    public GameObject loadingPanel;
    public UnityEngine.UI.Slider loadingBar;
    public TMPro.TextMeshProUGUI loadingText;
    
    [Header("Scene to Load")]
    public string mapSceneName = "MapScene";
    
    [Header("Fake Loading Settings")]
    public float minLoadTime = 2f;
    public float maxLoadTime = 4f;
    
    private bool videoComplete = false;
    private bool skipPressed = false;
    
    void Start()
    {
        if (skipButton != null)
            skipButton.SetActive(false);
        
        if (loadingPanel != null)
            loadingPanel.SetActive(false);
        
        // Setup video player
        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached += OnVideoFinished;
            videoPlayer.Play();
            
            // Show skip button after 1 second
            StartCoroutine(ShowSkipButtonAfterDelay(1f));
        }
        else
        {
            // No video, go directly to loading
            StartCoroutine(FakeLoadingThenLoadScene());
        }
    }
    
    IEnumerator ShowSkipButtonAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (skipButton != null && !videoComplete)
            skipButton.SetActive(true);
    }
    
    void OnVideoFinished(VideoPlayer vp)
    {
        videoComplete = true;
        if (skipButton != null)
            skipButton.SetActive(false);
        
        StartCoroutine(FakeLoadingThenLoadScene());
    }
    
    public void SkipVideo()
    {
        if (videoPlayer != null && videoPlayer.isPlaying)
        {
            skipPressed = true;
            videoPlayer.Stop();
            OnVideoFinished(videoPlayer);
        }
    }
    
    IEnumerator FakeLoadingThenLoadScene()
    {
        // Show loading panel
        if (loadingPanel != null)
            loadingPanel.SetActive(true);
        
        // Random load time (creates "fake" loading feel)
        float loadDuration = Random.Range(minLoadTime, maxLoadTime);
        float elapsedTime = 0f;
        
        while (elapsedTime < loadDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / loadDuration;
            
            // Update loading bar
            if (loadingBar != null)
                loadingBar.value = progress;
            
            // Update loading text with dots animation
            if (loadingText != null)
            {
                int dots = Mathf.FloorToInt(Time.time * 2) % 4;
                string dotText = new string('.', dots);
                loadingText.text = $"Loading{dotText}";
            }
            
            yield return null;
        }
        
        // Ensure loading bar is full
        if (loadingBar != null)
            loadingBar.value = 1f;
        
        if (loadingText != null)
            loadingText.text = "Loading complete!";
        
        yield return new WaitForSeconds(0.5f);
        
        // Load the map scene
        SceneManager.LoadScene(mapSceneName);
    }
}