using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;
using UnityEngine.UI;

public class CreditSceneManager : MonoBehaviour
{
    [Header("Fade Settings")]
    public Image fadeImage;
    public float fadeDuration = 1.5f;
    
    [Header("Scrolling Credits")]
    public TextMeshProUGUI creditText;
    public RectTransform creditContainer; // The container that holds the credit text
    public float scrollSpeed = 30f; // Speed at which text scrolls up
    public float startDelay = 1f; // Delay before scrolling starts
    public float endDelay = 2f; // Delay after scrolling finishes before fading out
    
    [Header("Scene")]
    public string mapSceneName = "MapScene";
    
    private string fullCreditText = "Afnan Studio Present\n\n" +
                                    "INFECTION ZERO\n\n" +
                                    "Developed by: Kazi Afnan Alam\n\n" +
                                    "Sound by:\n" +
                                    "Savfk - The Age Of Wood\n" +
                                    "Makai Symphony - The Army of Minotaur\n Savfk - Reloaded\n\n\n" +
                                    "Special Thanks to:\n" +
                                    "Kabungus\n" +
                                    "Fries and Seagull\n" +
                                    "VenCreations\n" +
                                    "Simon Mercuzot\n\n\n" +
                                    "Thanks For Playing!\n\n" +
                                    "© 2026 Infection Zero";
    
    void Start()
    {
        StartCoroutine(PlayCredits());
    }
    
    IEnumerator PlayCredits()
    {
        // Fade in from black
        yield return StartCoroutine(Fade(0f, 1f, fadeDuration));
        
        // Set initial position (below the screen)
        if (creditContainer != null)
        {
            creditContainer.anchoredPosition = new Vector2(0, -Screen.height);
        }
        
        // Set the text
        if (creditText != null)
        {
            creditText.text = fullCreditText;
        }
        
        // Wait before scrolling starts
        yield return new WaitForSeconds(startDelay);
        
        // Start scrolling
        yield return StartCoroutine(ScrollCredits());
        
        // Wait after scrolling finishes
        yield return new WaitForSeconds(endDelay);
        
        // Fade out to black
        yield return StartCoroutine(Fade(1f, 0f, fadeDuration));
        
        // Load map scene
        if (LoadingScreen.Instance != null)
        {
            LoadingScreen.Instance.LoadScene(mapSceneName);
        }
        else
        {
            SceneManager.LoadScene(mapSceneName);
        }
    }
    
    IEnumerator ScrollCredits()
    {
        if (creditContainer == null) yield break;
        
        // Get the height of the text
        float textHeight = creditText.preferredHeight;
        float startY = -Screen.height;
        float endY = textHeight + Screen.height;
        
        // Scroll until the text completely leaves the screen
        while (creditContainer.anchoredPosition.y < endY)
        {
            float newY = creditContainer.anchoredPosition.y + scrollSpeed * Time.deltaTime;
            creditContainer.anchoredPosition = new Vector2(0, newY);
            yield return null;
        }
    }
    
    IEnumerator Fade(float startAlpha, float endAlpha, float duration)
    {
        if (fadeImage == null) yield break;
        
        float elapsed = 0f;
        Color color = fadeImage.color;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(startAlpha, endAlpha, elapsed / duration);
            color.a = alpha;
            fadeImage.color = color;
            yield return null;
        }
        
        color.a = endAlpha;
        fadeImage.color = color;
    }
}