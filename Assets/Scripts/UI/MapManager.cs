using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
public class MapManager : MonoBehaviour
{
    public static MapManager Instance;
    
    public GameObject currentPOIPanel;
    public POIPanelController panelController;
    
    [Header("POI Buttons")]
    public List<GameObject> poiButtons;
    public List<POIData> poiDataList;
    
    [Header("POI Button Components")]
    public List<Image> poiButtonIcons; // References to icon images on buttons
    public List<TextMeshProUGUI> poiButtonTexts; // References to text components

    private POIData currentPOI;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    
    void Start()
    {
        UpdatePOIVisibility();
        SetupPOIButtons();
    }
    
    void SetupPOIButtons()
    {
        // Setup icons and text for each POI button
        for (int i = 0; i < poiButtons.Count && i < poiDataList.Count; i++)
        {
            POIData poi = poiDataList[i];
            
            // Set icon if available
            if (i < poiButtonIcons.Count && poiButtonIcons[i] != null && poi.poiIcon != null)
            {
                poiButtonIcons[i].sprite = poi.poiIcon;
                poiButtonIcons[i].color = Color.white;
            }
            
            // Set button text
            if (i < poiButtonTexts.Count && poiButtonTexts[i] != null)
            {
                poiButtonTexts[i].text = poi.poiName;
            }
            
            // Add click listener to button
            if (poiButtons[i] != null)
            {
                int index = i; // Capture for closure
                Button button = poiButtons[i].GetComponent<Button>();
                if (button != null)
                {
                    button.onClick.RemoveAllListeners();
                    button.onClick.AddListener(() => OpenPOI(poiDataList[index]));
                }
            }
        }
    }
    
    void UpdatePOIVisibility()
    {
        if (GameManager.instance == null) return;
        
        for (int i = 0; i < poiButtons.Count && i < poiDataList.Count; i++)
        {
            POIData poi = poiDataList[i];
            
            bool isAvailable = GameManager.instance.IsPOIAvailable(poi);
            bool isCompleted = GameManager.instance.IsPOICompleted(poi.poiName);
            
            // Hide if NOT available OR completed
            if (!isAvailable || isCompleted)
            {
                if (poiButtons[i] != null)
                    poiButtons[i].SetActive(false);
            }
            else
            {
                if (poiButtons[i] != null)
                    poiButtons[i].SetActive(true);
            }
        }
    }

    public void OpenPOI(POIData poi)
    {
        if (GameManager.instance != null && !GameManager.instance.IsPOIAvailable(poi))
        {
            Debug.Log("This POI is not available yet!");
            return;
        }
        
        currentPOI = poi;
        currentPOIPanel.SetActive(true);
        panelController.SetupPOI(currentPOI);
    }

    public void ClosePOI()
    {
        currentPOIPanel.SetActive(false);
    }

    public void StartLevel()
    {
        if(currentPOI == null) 
        {
            Debug.LogError("No POI selected!");
            return;
        }

        if(GameManager.instance == null)
        {
            Debug.LogError("GameManager instance not found!");
            return;
        }

        if(GameManager.instance.playerLevel < currentPOI.requiredPlayerLevel)
        {
            Debug.Log("Level too low! Required: " + currentPOI.requiredPlayerLevel);
            return;
        }

        if(GameManager.instance.UseEnergy())
        {
            GameManager.instance.CurrentPOI = currentPOI;
            
            // Check if LoadingScreen exists
            if (LoadingScreen.Instance != null)
            {
                Debug.Log("Using LoadingScreen to load: " + currentPOI.levelScene);
                LoadingScreen.Instance.LoadScene(currentPOI.levelScene);
            }
            else
            {
                Debug.LogWarning("LoadingScreen.Instance is null! Make sure LoadingScreen exists in the scene.");
                // Fallback direct load
                SceneManager.LoadScene(currentPOI.levelScene);
            }
        }
    }
    
    void OnEnable()
    {
        if (GameManager.instance != null)
        {
            UpdatePOIVisibility();
        }
    }
}