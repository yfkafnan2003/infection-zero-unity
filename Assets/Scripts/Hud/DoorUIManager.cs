using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class DoorUIManager : MonoBehaviour
{
    [Header("UI Panel")]
    public GameObject doorUIPanel;
    public GameObject doorEntryPrefab;
    
    private List<GameObject> doorUIEntries = new List<GameObject>();
    private DoorHealth[] doors;
    

    public void InitializeDoors(DoorHealth[] doorArray)
    {
        doors = doorArray;
        
        // Clear existing entries
        foreach (GameObject entry in doorUIEntries)
        {
            Destroy(entry);
        }
        doorUIEntries.Clear();
        
        // Create UI for each door
        for (int i = 0; i < doors.Length; i++)
        {
            if (doorEntryPrefab != null && doorUIPanel != null)
            {
                GameObject entry = Instantiate(doorEntryPrefab, doorUIPanel.transform);
                
                // Set door name
                TextMeshProUGUI nameText = entry.GetComponentInChildren<TextMeshProUGUI>();
                if (nameText != null)
                    nameText.text = $"DOOR {i + 1}";
                
                // Position the entry - stack vertically
                RectTransform rect = entry.GetComponent<RectTransform>();
                if (rect != null)
                {
                    float yOffset = -i * 60f;
                    rect.anchoredPosition = new Vector2(0, yOffset);
                }
                
                // Get slider
                Slider healthSlider = entry.GetComponentInChildren<Slider>();
                if (healthSlider != null)
                {
                    healthSlider.maxValue = doors[i].maxHealth;
                    healthSlider.value = doors[i].currentHealth;
                }
                
                // Store reference
                DoorUIEntry uiEntry = entry.GetComponent<DoorUIEntry>();
                if (uiEntry == null)
                    uiEntry = entry.AddComponent<DoorUIEntry>();
                
                uiEntry.Setup(doors[i], healthSlider);
                doorUIEntries.Add(entry);
            }
        }  // <-- This brace was misplaced - make sure it's here
        
        // Adjust panel height based on number of doors
        if (doorUIPanel != null && doorUIEntries.Count > 0)
        {
            RectTransform panelRect = doorUIPanel.GetComponent<RectTransform>();
            if (panelRect != null)
            {
                float panelHeight = doorUIEntries.Count * 70f;
                panelRect.sizeDelta = new Vector2(panelRect.sizeDelta.x, panelHeight);
            }
            
            doorUIPanel.SetActive(true);
        }
    }
    
    public void HideUI()
    {
        if (doorUIPanel != null)
            doorUIPanel.SetActive(false);
    }
    
    public void UpdateDoorHealth(int doorIndex, int currentHealth, int maxHealth)
    {
        if (doorIndex < doorUIEntries.Count)
        {
            DoorUIEntry entry = doorUIEntries[doorIndex].GetComponent<DoorUIEntry>();
            if (entry != null)
                entry.UpdateHealth(currentHealth, maxHealth);
        }
    }
    public void ShowDoorRepairing(int doorIndex)
    {
        if (doorIndex < doorUIEntries.Count)
        {
            DoorUIEntry entry = doorUIEntries[doorIndex].GetComponent<DoorUIEntry>();
            if (entry != null)
                entry.ShowRepairing();
        }
    }
}
