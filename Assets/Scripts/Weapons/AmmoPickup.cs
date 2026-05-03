using UnityEngine;
using TMPro;
using System.Collections.Generic;
public class AmmoPickup : MonoBehaviour
{
    public AmmoType ammoType; // This is now just for visual appearance
    // No longer used for specific ammo amount

    public float rotateSpeed = 120f;
    public float bobSpeed = 2f;
    public float bobHeight = 0.2f;
    
    public AudioClip pickupSound;
    
    [Header("Glow Effect")]
    public Light glowLight;
    public Color glowColor = Color.yellow;
    public float glowIntensity = 1f;
    private static List<GameObject> activePopups = new List<GameObject>();
    private static float popupSpacing = 50f;
    [Header("UI Popup")]
    public GameObject pickupTextPrefab; // Assign a TextMeshPro prefab in Inspector
    public float popupDuration = 1.5f;
    public Color popupColor = Color.green;
    
    private Vector3 startPosition;
    private float bobTimer = 0f;
    
    // Ammo range for each weapon type
    private Vector2 pistolRange = new Vector2(20f, 40f);
    private Vector2 shotgunRange = new Vector2(6f, 15f);
    private Vector2 machinegunRange = new Vector2(30f, 60f);
    
    void Start()
    {
        startPosition = transform.position;
        
        // Setup glow light
        if (glowLight == null)
            glowLight = GetComponentInChildren<Light>();
            
        if (glowLight != null)
        {
            glowLight.color = glowColor;
            glowLight.intensity = glowIntensity;
        }
        
        // Set ammo color based on type
        SetAmmoColor();
    }
    
    void Update()
    {
        // Rotate the ammo
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
    }
    
    void SetAmmoColor()
    {
        // Change the material color based on ammo type
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            switch (ammoType)
            {
                case AmmoType.Pistol:
                    renderer.material.color = Color.yellow;
                    glowColor = Color.yellow;
                    break;
                case AmmoType.Shotgun:
                    renderer.material.color = new Color(1f, 0.5f, 0f); // Orange
                    glowColor = new Color(1f, 0.5f, 0f);
                    break;
                case AmmoType.Machinegun:
                    renderer.material.color = Color.cyan;
                    glowColor = Color.cyan;
                    break;
            }
            
            if (glowLight != null)
                glowLight.color = glowColor;
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        Gun gun = other.GetComponentInChildren<Gun>();
        
        if (gun != null)
        {
            // Generate random ammo amounts for each weapon type
            int pistolAmmo = Random.Range((int)pistolRange.x, (int)pistolRange.y + 1);
            int shotgunAmmo = Random.Range((int)shotgunRange.x, (int)shotgunRange.y + 1);
            int machinegunAmmo = Random.Range((int)machinegunRange.x, (int)machinegunRange.y + 1);
            
            // Add ammo to ALL weapon types
            gun.AddAmmo(pistolAmmo, AmmoType.Pistol);
            gun.AddAmmo(shotgunAmmo, AmmoType.Shotgun);
            gun.AddAmmo(machinegunAmmo, AmmoType.Machinegun);
            
            // Show pickup popup (without showing amounts)
            ShowPickupPopup();
            
            // Play sound with volume control
            if (pickupSound != null)
                AudioSource.PlayClipAtPoint(pickupSound, transform.position, 0.07f);
            
            Destroy(gameObject);
        }
    }
    
    void ShowPickupPopup()
    {
        if (pickupTextPrefab == null) return;
        
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null) return;
        
        // Remove any destroyed popups from list
        activePopups.RemoveAll(p => p == null);
        
        // Create popup text
        GameObject popup = Instantiate(pickupTextPrefab, canvas.transform);
        TextMeshProUGUI textComponent = popup.GetComponent<TextMeshProUGUI>();
        
        if (textComponent != null)
        {
            // Simple text that just says ammo was increased
            textComponent.text = "AMMO INCREASED!";
            textComponent.fontSize = 28;
            textComponent.fontStyle = FontStyles.Bold;
            textComponent.color = popupColor;
            
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
        
    System.Collections.IEnumerator AnimatePopup(GameObject popup)
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