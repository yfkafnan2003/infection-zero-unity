using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class HitTextManager : MonoBehaviour
{
    public static HitTextManager Instance;
    
    [Header("UI Prefabs")]
    public GameObject hitTextPrefab;
    public Transform canvasTransform;
    
    [Header("Hit Text Settings")]
    public float floatSpeed = 1f;
    public float fadeDuration = 0.5f;
    public float lifetime = 1f;
    
    [Header("Headshot Combo")]
    private int headshotCombo = 0;
    private float lastHeadshotTime = 0f;
    public float comboTimeWindow = 5f;
    public int maxCombo = 10;
    
    private void Awake()
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
    
    public void ShowHitText(Vector3 worldPosition, int hitCount, bool isHeadshot)
    {
        if (hitTextPrefab == null || canvasTransform == null) return;
        
        Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPosition);
        GameObject hitTextObj = Instantiate(hitTextPrefab, canvasTransform);
        hitTextObj.transform.position = screenPos;
        
        TextMeshProUGUI textComponent = hitTextObj.GetComponent<TextMeshProUGUI>();
        
        if (textComponent != null)
        {
            if (isHeadshot)
            {
                UpdateHeadshotCombo();
                
                if (headshotCombo >= 2)
                {
                    textComponent.text = $"HEADSHOT {headshotCombo}x!";
                    textComponent.color = Color.red;
                    textComponent.fontSize = 28;
                }
                else
                {
                    textComponent.text = "HEADSHOT!";
                    textComponent.color = new Color(1f, 0.5f, 0f);
                    textComponent.fontSize = 24;
                }
            }
            else
            {
                textComponent.text = $"{hitCount}\nHITS";
                textComponent.color = Color.white;
                textComponent.fontSize = 20;
            }
        }
        
        StartCoroutine(AnimateHitText(hitTextObj));
    }
    public void ShowNoAmmoText()
    {
        if (hitTextPrefab == null || canvasTransform == null) return;
        
        // Get screen center position
        Vector3 screenPos = new Vector3(Screen.width / 2, Screen.height / 2, 0);
        GameObject hitTextObj = Instantiate(hitTextPrefab, canvasTransform);
        hitTextObj.transform.position = screenPos;
        
        TextMeshProUGUI textComponent = hitTextObj.GetComponent<TextMeshProUGUI>();
        
        if (textComponent != null)
        {
            textComponent.text = "OUT OF AMMO!";
            textComponent.color = Color.red;
            textComponent.fontSize = 24;
            textComponent.fontStyle = FontStyles.Bold;
        }
        
        StartCoroutine(AnimateNoAmmoText(hitTextObj));
    }

    IEnumerator AnimateNoAmmoText(GameObject hitTextObj)
    {
        TextMeshProUGUI text = hitTextObj.GetComponent<TextMeshProUGUI>();
        RectTransform rect = hitTextObj.GetComponent<RectTransform>();
        
        float elapsedTime = 0f;
        Vector3 startPos = rect.position;
        Color startColor = text.color;
        
        while (elapsedTime < lifetime)
        {
            elapsedTime += Time.deltaTime;
            
            float progress = elapsedTime / lifetime;
            rect.position = startPos + Vector3.up * (progress * floatSpeed * 50);
            
            if (progress > 0.5f)
            {
                float fadeProgress = (progress - 0.5f) / 0.5f;
                text.color = new Color(startColor.r, startColor.g, startColor.b, 1 - fadeProgress);
            }
            
            yield return null;
        }
        
        Destroy(hitTextObj);
    }
    // ADD THIS METHOD - For weakpoint hits
    public void ShowWeakpointHitText(Vector3 worldPosition, int damage)
    {
        if (hitTextPrefab == null || canvasTransform == null) return;
        
        Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPosition);
        GameObject hitTextObj = Instantiate(hitTextPrefab, canvasTransform);
        hitTextObj.transform.position = screenPos;
        
        TextMeshProUGUI textComponent = hitTextObj.GetComponent<TextMeshProUGUI>();
        
        if (textComponent != null)
        {
            textComponent.text = $"WEAKPOINT!\n{damage} DMG!";
            textComponent.color = new Color(1f, 0.8f, 0f); // Orange/Gold
            textComponent.fontSize = 26;
        }
        
        StartCoroutine(AnimateHitText(hitTextObj));
    }
    
    void UpdateHeadshotCombo()
    {
        float currentTime = Time.time;
        
        if (currentTime - lastHeadshotTime <= comboTimeWindow)
        {
            headshotCombo = Mathf.Min(headshotCombo + 1, maxCombo);
        }
        else
        {
            headshotCombo = 1;
        }
        
        lastHeadshotTime = currentTime;
        
        if (headshotCombo >= 3)
        {
            Debug.Log($"Headshot Combo: {headshotCombo}x!");
        }
    }
    
    public void ResetHeadshotCombo()
    {
        headshotCombo = 0;
        lastHeadshotTime = 0f;
    }
    
    IEnumerator AnimateHitText(GameObject hitTextObj)
    {
        TextMeshProUGUI text = hitTextObj.GetComponent<TextMeshProUGUI>();
        RectTransform rect = hitTextObj.GetComponent<RectTransform>();
        
        float elapsedTime = 0f;
        Vector3 startPos = rect.position;
        Color startColor = text.color;
        
        while (elapsedTime < lifetime)
        {
            elapsedTime += Time.deltaTime;
            
            float progress = elapsedTime / lifetime;
            rect.position = startPos + Vector3.up * (progress * floatSpeed * 100);
            
            if (progress > 0.5f)
            {
                float fadeProgress = (progress - 0.5f) / 0.5f;
                text.color = new Color(startColor.r, startColor.g, startColor.b, 1 - fadeProgress);
            }
            
            yield return null;
        }
        
        Destroy(hitTextObj);
    }
}