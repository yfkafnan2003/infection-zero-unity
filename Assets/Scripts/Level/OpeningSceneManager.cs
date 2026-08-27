using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;
using System.Collections;

public class OpeningSceneManager : MonoBehaviour
{
    [Header("Video Settings")]
    public VideoPlayer videoPlayer;
    public GameObject skipButton;
    
    [Header("Scene to Load")]
    public string mapSceneName = "IntroScene";
    
    private bool videoComplete = false;
    private bool skipPressed = false;
    
    void Start()
    {
        if (skipButton != null)
            skipButton.SetActive(false);
        
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
            SceneManager.LoadScene(mapSceneName);
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

        if(skipButton != null)
            skipButton.SetActive(false);

        SceneManager.LoadScene(mapSceneName);
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
    
}