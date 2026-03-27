using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class MapManager : MonoBehaviour
{
    public GameObject currentPOIPanel;
    public POIPanelController panelController;
    
    [Header("POI Buttons")]
    public List<GameObject> poiButtons;
    public List<POIData> poiDataList;

    POIData currentPOI;

    void Start()
    {
        UpdatePOIVisibility();
    }

    void UpdatePOIVisibility()
    {
        if (GameManager.instance == null) return;
        
        for (int i = 0; i < poiButtons.Count && i < poiDataList.Count; i++)
        {
            POIData poi = poiDataList[i];
            
            // Check if POI is available (in current chain and not completed)
            bool isAvailable = GameManager.instance.IsPOIAvailable(poi);
            bool isCompleted = GameManager.instance.IsPOICompleted(poi.poiName);
            
            if (isCompleted || !isAvailable)
            {
                // Hide completed or locked POI button
                if (poiButtons[i] != null)
                    poiButtons[i].SetActive(false);
            }
            else
            {
                // Show available POI button
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

        Debug.Log("Selected POI: " + currentPOI.poiName);
        Debug.Log("POI Type: " + currentPOI.poiType);
        Debug.Log("POI Level Scene: " + currentPOI.levelScene);

        if(GameManager.instance == null)
        {
            Debug.LogError("GameManager instance not found!");
            return;
        }

        if(GameManager.instance.playerLevel < currentPOI.requiredPlayerLevel)
        {
            Debug.Log("Level too low! Required: " + currentPOI.requiredPlayerLevel + ", Player: " + GameManager.instance.playerLevel);
            return;
        }

        if(GameManager.instance.UseEnergy())
        {
            GameManager.instance.CurrentPOI = currentPOI;
            
            if(GameManager.instance.CurrentPOI != null)
            {
                Debug.Log("POI saved successfully: " + GameManager.instance.CurrentPOI.poiName);
                Debug.Log("Loading scene: " + currentPOI.levelScene);
                SceneManager.LoadScene(currentPOI.levelScene);
            }
            else
            {
                Debug.LogError("Failed to save POI data!");
            }
        }
        else
        {
            Debug.Log("Not enough energy! Current energy: " + GameManager.instance.currentEnergy);
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