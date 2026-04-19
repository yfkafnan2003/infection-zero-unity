    using UnityEngine;
    using UnityEngine.UI;
    using TMPro;

    public class PlayerUtilityController : MonoBehaviour
    {
        [Header("UI Buttons")]
        public Button healButton;
        public Button grenadeButton;
        
        [Header("Utility Prefabs")]
        public GameObject smallFirstAidPrefab;
        public GameObject mediumFirstAidPrefab;
        public GameObject largeFirstAidPrefab;
        public GameObject explosiveGrenadePrefab;
        public GameObject smokeGrenadePrefab;
        public GameObject fireGrenadePrefab;
        
        [Header("References")]
        public PlayerHealth playerHealth;
        public Transform throwPoint;
        
        [Header("Utility Display")]
        public Image utilityDisplayIcon;
        public TextMeshProUGUI utilityDisplayCount;
        public GameObject utilityDisplayPanel;
        
        // Store utility data from PlayerPrefs
        private string currentUtilityName = "";
        private UtilityType currentUtilityType;
        private int currentUtilityCount = 0;
        private Sprite currentUtilityIcon;
        private int equippedUtilityIndex = -1;
        
        void Start()
        {
            if (healButton != null)
                healButton.onClick.AddListener(UseHealItem);
            
            if (grenadeButton != null)
                grenadeButton.onClick.AddListener(UseGrenade);
        }
        
        void Update()
        {
            // Update utility display every frame
            UpdateCurrentUtility();
            
            // Update button visuals
            UpdateButtonVisuals();
        }
        
        void UpdateCurrentUtility()
        {
            // Get equipped utility index from PlayerPrefs (saved by EquipManager in Map Scene)
            int newEquippedIndex = PlayerPrefs.GetInt("EquippedUtility", -1);
            
            // Check if utility changed
            if (newEquippedIndex != equippedUtilityIndex)
            {
                equippedUtilityIndex = newEquippedIndex;
                LoadUtilityData();
            }
            
            // Update display if we have a utility
            if (equippedUtilityIndex >= 0 && !string.IsNullOrEmpty(currentUtilityName))
            {
                UpdateUtilityDisplayUI();
            }
            else
            {
                // Hide display when no utility equipped
                if (utilityDisplayPanel != null)
                    utilityDisplayPanel.SetActive(false);
            }
        }
        
        void LoadUtilityData()
        {
            if (equippedUtilityIndex < 0)
            {
                currentUtilityName = "";
                currentUtilityCount = 0;
                return;
            }
            
            // Match the exact names from your ShopManager
            switch (equippedUtilityIndex)
            {
                case 0:
                    currentUtilityName = "Bandage (Small)";
                    currentUtilityType = UtilityType.FirstAid;
                    break;
                case 1:
                    currentUtilityName = "Aid Pill (Medium)";
                    currentUtilityType = UtilityType.FirstAid;
                    break;
                case 2:
                    currentUtilityName = "Healthkit (Large)";
                    currentUtilityType = UtilityType.FirstAid;
                    break;
                case 3:
                    currentUtilityName = "Grenade";
                    currentUtilityType = UtilityType.Grenade;
                    break;
                case 4:
                    currentUtilityName = "Smoke";
                    currentUtilityType = UtilityType.Grenade;
                    break;
                case 5:
                    currentUtilityName = "Incendiary";
                    currentUtilityType = UtilityType.Grenade;
                    break;
                default:
                    currentUtilityName = "";
                    break;
            }
            
            // Load count from PlayerPrefs using the exact name
            currentUtilityCount = PlayerPrefs.GetInt(currentUtilityName + "_Count", 0);
            
            Debug.Log($"Loaded utility: {currentUtilityName}, Type: {currentUtilityType}, Count: {currentUtilityCount}");
            
            // Load icon
            LoadUtilityIcon();
        }
        void LoadUtilityIcon()
        {
            // Try to load icon from Resources folder with correct naming
            string iconName = "";
            switch (equippedUtilityIndex)
            {
                case 0: iconName = "Bandage(Small)"; break;
                case 1: iconName = "AidPill(Medium)"; break;
                case 2: iconName = "Healthkit(Large)"; break;
                case 3: iconName = "Grenade"; break;
                case 4: iconName = "Smoke"; break;
                case 5: iconName = "Incendiary"; break;
            }
            
            Sprite loadedIcon = Resources.Load<Sprite>($"UtilityIcons/{iconName}");
            
            if (loadedIcon != null)
            {
                currentUtilityIcon = loadedIcon;
            }
            else
            {
                // Try alternative naming
                loadedIcon = Resources.Load<Sprite>($"UtilityIcons/{currentUtilityName}");
                if (loadedIcon != null)
                {
                    currentUtilityIcon = loadedIcon;
                }
                else
                {
                    Debug.LogWarning($"Icon not found for {currentUtilityName}. Place icon in Resources/UtilityIcons/{iconName}.png");
                    // Don't create colored square - keep null so default image shows
                    currentUtilityIcon = null;
                }
            }
        }
        
        void UpdateUtilityDisplayUI()
        {
            if (string.IsNullOrEmpty(currentUtilityName)) return;
            
            // Show the display panel
            if (utilityDisplayPanel != null)
                utilityDisplayPanel.SetActive(true);
            
            // Update icon
            if (utilityDisplayIcon != null && currentUtilityIcon != null)
                utilityDisplayIcon.sprite = currentUtilityIcon;
            
            // Update count text
            if (utilityDisplayCount != null)
                utilityDisplayCount.text = currentUtilityCount.ToString();
        }
        
        void UpdateButtonVisuals()
        {
            if (!string.IsNullOrEmpty(currentUtilityName))
            {
                // Only show the button that matches the equipped utility type
                if (healButton != null)
                    healButton.gameObject.SetActive(currentUtilityType == UtilityType.FirstAid);
                
                if (grenadeButton != null)
                    grenadeButton.gameObject.SetActive(currentUtilityType == UtilityType.Grenade);
            }
            else
            {
                // Hide both buttons if no utility equipped
                if (healButton != null)
                    healButton.gameObject.SetActive(false);
                if (grenadeButton != null)
                    grenadeButton.gameObject.SetActive(false);
            }
        }
        
        void UseHealItem()
        {
            if (string.IsNullOrEmpty(currentUtilityName))
            {
                Debug.Log("No utility equipped!");
                return;
            }
            
            if (currentUtilityType != UtilityType.FirstAid)
            {
                Debug.Log("Equipped item is not a healing item!");
                return;
            }
            
            if (currentUtilityCount <= 0)
            {
                Debug.Log("No healing items left!");
                return;
            }
            
            // Determine which heal prefab to use based on name
            GameObject healPrefab = null;
            
            if (currentUtilityName.Contains("Bandage"))
                healPrefab = smallFirstAidPrefab;
            else if (currentUtilityName.Contains("Aid Pill"))
                healPrefab = mediumFirstAidPrefab;
            else if (currentUtilityName.Contains("Healthkit"))
                healPrefab = largeFirstAidPrefab;
            
            if (healPrefab != null)
            {
                GameObject healObj = Instantiate(healPrefab, transform.position, Quaternion.identity);
                FirstAidItem healItem = healObj.GetComponent<FirstAidItem>();
                
                if (healItem != null)
                {
                    healItem.Heal(playerHealth);
                    
                    // Reduce count
                    currentUtilityCount--;
                    
                    // Save to PlayerPrefs
                    PlayerPrefs.SetInt(currentUtilityName + "_Count", currentUtilityCount);
                    PlayerPrefs.Save();
                    
                    // Update display
                    if (utilityDisplayCount != null)
                        utilityDisplayCount.text = currentUtilityCount.ToString();
                    
                    Debug.Log($"Used {currentUtilityName}. Remaining: {currentUtilityCount}");
                }
            }
        }
        
        void UseGrenade()
        {
            if (string.IsNullOrEmpty(currentUtilityName))
            {
                Debug.Log("No utility equipped!");
                return;
            }
            
            if (currentUtilityType != UtilityType.Grenade)
            {
                Debug.Log("Equipped item is not a grenade!");
                return;
            }
            
            if (currentUtilityCount <= 0)
            {
                Debug.Log("No grenades left!");
                return;
            }
            
            // Determine which grenade prefab to use
            GameObject grenadePrefab = null;
            
            if (currentUtilityName.Contains("Grenade"))
                grenadePrefab = explosiveGrenadePrefab;
            else if (currentUtilityName.Contains("Smoke"))
                grenadePrefab = smokeGrenadePrefab;
            else if (currentUtilityName.Contains("Incendiary"))
                grenadePrefab = fireGrenadePrefab;
            
            if (grenadePrefab != null && throwPoint != null && Camera.main != null)
            {
                // Throw towards crosshair direction
                Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
                Vector3 throwDirection = ray.direction.normalized;
                
                GameObject grenade = Instantiate(grenadePrefab, throwPoint.position, Quaternion.LookRotation(throwDirection));
                Rigidbody rb = grenade.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.linearVelocity = throwDirection * 15f; // Adjust speed as needed
                }
                
                // Reduce count
                currentUtilityCount--;
                
                // Save to PlayerPrefs
                PlayerPrefs.SetInt(currentUtilityName + "_Count", currentUtilityCount);
                PlayerPrefs.Save();
                
                // Update display
                if (utilityDisplayCount != null)
                    utilityDisplayCount.text = currentUtilityCount.ToString();
                
                Debug.Log($"Used {currentUtilityName}. Remaining: {currentUtilityCount}");
            }
        }
    }