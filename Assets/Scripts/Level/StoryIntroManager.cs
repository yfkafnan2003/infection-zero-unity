using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;
using System.Collections;

public class StoryIntroManager : MonoBehaviour
{
    public VideoPlayer videoPlayer;

    public GameObject skipButton;

    public string nextScene = "MapScene";

    private bool finished = false;

    [Header("Loading Screen")]
    public GameObject loadingPanel;
    public UnityEngine.UI.Slider loadingBar;
    public TMPro.TextMeshProUGUI loadingText;

    [Header("Loading Time")]
    public float minLoadTime = 2f;
    public float maxLoadTime = 4f;

    void Start()
    {
        if (loadingPanel != null)
        loadingPanel.SetActive(false);
        if(skipButton != null)
            skipButton.SetActive(false);

        videoPlayer.loopPointReached += VideoFinished;

        videoPlayer.Play();

        StartCoroutine(ShowSkip());
    }

    IEnumerator ShowSkip()
    {
        yield return new WaitForSeconds(1f);

        if(skipButton != null)
            skipButton.SetActive(true);
    }

    public void SkipVideo()
    {
        if(finished)
            return;

        finished = true;

        videoPlayer.Stop();

        StartCoroutine(FakeLoadingThenLoadScene());
    }

    void VideoFinished(VideoPlayer vp)
    {
        if(finished)
            return;

        finished = true;

        StartCoroutine(FakeLoadingThenLoadScene());
    }
    IEnumerator FakeLoadingThenLoadScene()
    {
        if (loadingPanel != null)
            loadingPanel.SetActive(true);

        if (videoPlayer != null)
            videoPlayer.gameObject.SetActive(false);

        if (skipButton != null)
            skipButton.SetActive(false);

        float loadDuration = Random.Range(minLoadTime, maxLoadTime);
        float elapsed = 0f;

        while (elapsed < loadDuration)
        {
            elapsed += Time.deltaTime;

            float progress = Mathf.Clamp01(elapsed / loadDuration);

            if (loadingBar != null)
                loadingBar.value = progress;

            if (loadingText != null)
            {
                int dots = Mathf.FloorToInt(Time.time * 2) % 4;
                loadingText.text = "Loading" + new string('.', dots);
            }

            yield return null;
        }

        if (loadingBar != null)
            loadingBar.value = 1;

        if (loadingText != null)
            loadingText.text = "Loading Complete!";

        yield return new WaitForSeconds(0.5f);

        SceneManager.LoadScene(nextScene);
    }
}