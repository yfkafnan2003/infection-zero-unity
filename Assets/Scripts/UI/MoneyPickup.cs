using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;
public class MoneyPickup : MonoBehaviour
{
    public int moneyAmount = 10;
    
    public float rotateSpeed = 120f;
    public float bobSpeed = 2f;
    public float bobHeight = 0.2f;
    public float pickupRange = 2f;
    
    public AudioClip pickupSound;
    
    [Header("Glow Effect")]
    public Light glowLight;
    public Color glowColor = new Color(1f, 0.84f, 0f); // Gold color
    public float glowIntensity = 1f;
    
    [Header("UI Popup")]
    public GameObject pickupTextPrefab;
    public float popupDuration = 1.5f;
    
    private Vector3 startPosition;
    private float bobTimer = 0f;
    private Transform player;
    private static List<GameObject> activePopups = new List<GameObject>();
    private static float popupSpacing = 50f;
    void Start()
    {
        startPosition = transform.position;
        
        // Find player
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
        
        // Setup glow light
        if (glowLight == null)
            glowLight = GetComponentInChildren<Light>();
            
        if (glowLight != null)
        {
            glowLight.color = glowColor;
            glowLight.intensity = glowIntensity;
        }
        
        // Set money color (gold)
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = new Color(1f, 0.84f, 0f);
        }
    }
    
    void Update()
    {
        // Rotate
        transform.Rotate(Vector3.up * rotateSpeed * Time.deltaTime);
        
        // Bob up and down
        bobTimer += Time.deltaTime * bobSpeed;
        float bobY = Mathf.Sin(bobTimer) * bobHeight;
        transform.position = startPosition + new Vector3(0, bobY, 0);
        
        // Pulsing glow effect
        if (glowLight != null)
        {
            float pulse = Mathf.Sin(Time.time * 3f) * 0.5f + 0.5f;
            glowLight.intensity = glowIntensity * (0.5f + pulse * 0.5f);
        }
        
        // Check pickup range
        if (player != null)
        {
            float distance = Vector3.Distance(transform.position, player.position);
            if (distance <= pickupRange)
            {
                Pickup();
            }
        }
    }
    
    void Pickup()
    {
        // Add money to GameManager
        if (GameManager.instance != null)
        {
            GameManager.instance.AddMoney(moneyAmount);
        }
        
        // Show pickup popup
        ShowPickupPopup();
        
        // Play sound with volume control
        if (pickupSound != null)
            AudioSource.PlayClipAtPoint(pickupSound, transform.position, 0.5f); // 0.5 = 50% volume
        
        Destroy(gameObject);
    }
    void ShowPickupPopup()
    {
        if (pickupTextPrefab == null) return;
        
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null) return;
        
        // Remove any destroyed popups from list
        activePopups.RemoveAll(p => p == null);
        
        GameObject popup = Instantiate(pickupTextPrefab, canvas.transform);
        TextMeshProUGUI textComponent = popup.GetComponent<TextMeshProUGUI>();
        
        if (textComponent != null)
        {
            textComponent.text = $"+${moneyAmount} MONEY";
            textComponent.color = Color.green; // Changed to GREEN
            textComponent.fontSize = 24;
            textComponent.fontStyle = FontStyles.Bold;
            
            // Position popup based on existing popups
            float yPos = -50f - (activePopups.Count * popupSpacing);
            RectTransform rect = popup.GetComponent<RectTransform>();
            rect.anchoredPosition = new Vector2(0, yPos);
            
            // Add to active list
            activePopups.Add(popup);
            Destroy(popup, popupDuration);
            // Start animation and auto-destroy
            StartCoroutine(AnimatePopup(popup));
        }
    }
    IEnumerator AnimatePopup(GameObject popup)
    {
        TextMeshProUGUI text = popup.GetComponent<TextMeshProUGUI>();
        RectTransform rect = popup.GetComponent<RectTransform>();
        float elapsedTime = 0f;
        
        Vector2 startPos = rect.anchoredPosition;
        Vector2 endPos = startPos + new Vector2(0, 100);
        
        while (elapsedTime < popupDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / popupDuration;
            
            rect.anchoredPosition = Vector2.Lerp(startPos, endPos, t);
            
            if (text != null)
            {
                Color color = text.color;
                color.a = Mathf.Lerp(1f, 0f, t);
                text.color = color;
            }
            
            yield return null;
        }
        
        // Remove from list and destroy
        activePopups.Remove(popup);
        Destroy(popup);
        
        // Reposition remaining popups
        for (int i = 0; i < activePopups.Count; i++)
        {
            if (activePopups[i] != null)
            {
                float newYPos = -50f - (i * popupSpacing);
                activePopups[i].GetComponent<RectTransform>().anchoredPosition = new Vector2(0, newYPos);
            }
        }
    }
}