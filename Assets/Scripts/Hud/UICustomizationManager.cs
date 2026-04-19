using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems; 
using System.Collections.Generic;

public class UICustomizationManager : MonoBehaviour
{
    public static UICustomizationManager Instance;
    
    [Header("UI Elements to Customize")]
    public List<RectTransform> customizableUIElements;
    
    [Header("Customization Mode")]
    public bool isCustomizing = false;
    public GameObject customizationPanel;
    public Button saveButton;
    public Button cancelButton;
    
    private Dictionary<RectTransform, Vector2> originalPositions = new Dictionary<RectTransform, Vector2>();
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        // Auto-find references in current scene
        FindCustomizationUIReferences();
        
        if (saveButton != null)
            saveButton.onClick.AddListener(SavePositions);
        if (cancelButton != null)
            cancelButton.onClick.AddListener(CancelCustomization);
        
        LoadPositions();
    }
    
    void FindCustomizationUIReferences()
    {
        // Find Customization Panel by name or component
        if (customizationPanel == null)
        {
            // Try to find by name
            GameObject panel = GameObject.Find("CustomizationPanel");
            if (panel == null)
            {
                // Try to find by tag or create it
                Canvas canvas = FindObjectOfType<Canvas>();
                if (canvas != null)
                {
                    panel = CreateCustomizationPanel(canvas.transform);
                }
            }
            
            if (panel != null)
            {
                customizationPanel = panel;
                Debug.Log("Customization panel found/created automatically");
            }
        }
        
        // Find Save Button
        if (saveButton == null && customizationPanel != null)
        {
            Button button = customizationPanel.GetComponentInChildren<Button>();
            while (button != null)
            {
                if (button.name.ToLower().Contains("save"))
                {
                    saveButton = button;
                    break;
                }
                button = button.GetComponentInChildren<Button>();
            }
            
            if (saveButton == null)
            {
                saveButton = CreateButton(customizationPanel.transform, "SaveButton", "SAVE");
            }
        }
        
        // Find Cancel Button
        if (cancelButton == null && customizationPanel != null)
        {
            Button[] buttons = customizationPanel.GetComponentsInChildren<Button>();
            foreach (Button btn in buttons)
            {
                if (btn.name.ToLower().Contains("cancel") || btn.name.ToLower().Contains("back"))
                {
                    cancelButton = btn;
                    break;
                }
            }
            
            if (cancelButton == null)
            {
                cancelButton = CreateButton(customizationPanel.transform, "CancelButton", "CANCEL");
            }
        }
        
        // Initially hide the panel
        if (customizationPanel != null)
            customizationPanel.SetActive(false);
    }
    
    GameObject CreateCustomizationPanel(Transform parent)
    {
        // Create panel GameObject
        GameObject panel = new GameObject("CustomizationPanel");
        panel.transform.SetParent(parent, false);
        
        // Add RectTransform
        RectTransform rect = panel.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(400, 200);
        
        // Add CanvasRenderer
        panel.AddComponent<CanvasRenderer>();
        
        // Add Image for background
        Image image = panel.AddComponent<Image>();
        image.color = new Color(0, 0, 0, 0.8f);
        
        // Add a vertical layout group
        VerticalLayoutGroup layout = panel.AddComponent<VerticalLayoutGroup>();
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.spacing = 20;
        layout.padding = new RectOffset(20, 20, 20, 20);
        
        // Add Title Text
        GameObject titleObj = new GameObject("TitleText");
        titleObj.transform.SetParent(panel.transform, false);
        Text titleText = titleObj.AddComponent<Text>();
        titleText.text = "CUSTOMIZE UI";
        titleText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        titleText.fontSize = 24;
        titleText.alignment = TextAnchor.MiddleCenter;
        titleText.color = Color.white;
        
        RectTransform titleRect = titleObj.GetComponent<RectTransform>();
        titleRect.sizeDelta = new Vector2(300, 40);
        
        // Create buttons container
        GameObject buttonsContainer = new GameObject("ButtonsContainer");
        buttonsContainer.transform.SetParent(panel.transform, false);
        HorizontalLayoutGroup buttonLayout = buttonsContainer.AddComponent<HorizontalLayoutGroup>();
        buttonLayout.childAlignment = TextAnchor.MiddleCenter;
        buttonLayout.spacing = 20;
        
        RectTransform containerRect = buttonsContainer.GetComponent<RectTransform>();
        containerRect.sizeDelta = new Vector2(300, 50);
        
        return panel;
    }
    
    Button CreateButton(Transform parent, string name, string buttonText)
    {
        GameObject buttonObj = new GameObject(name);
        buttonObj.transform.SetParent(parent, false);
        
        // Find the buttons container or use parent
        Transform buttonsContainer = parent.Find("ButtonsContainer");
        if (buttonsContainer != null)
            buttonObj.transform.SetParent(buttonsContainer, false);
        
        RectTransform rect = buttonObj.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(120, 50);
        
        Image image = buttonObj.AddComponent<Image>();
        image.color = new Color(0.2f, 0.6f, 0.2f);
        
        Button button = buttonObj.AddComponent<Button>();
        
        // Add text
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform, false);
        Text text = textObj.AddComponent<Text>();
        text.text = buttonText;
        text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        text.fontSize = 20;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.white;
        
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
        
        return button;
    }
    
    public void StartCustomization()
    {
        // Refresh UI elements before starting
        FindUIElementsInScene();
        
        isCustomizing = true;
        if (customizationPanel != null)
            customizationPanel.SetActive(true);
        
        // Make all UI elements draggable
        foreach (RectTransform element in customizableUIElements)
        {
            if (element != null)
            {
                AddDragHandler(element);
                // Store original position
                if (!originalPositions.ContainsKey(element))
                    originalPositions[element] = element.anchoredPosition;
            }
        }
    }
    
    public void StopCustomization()
    {
        isCustomizing = false;
        if (customizationPanel != null)
            customizationPanel.SetActive(false);
        
        // Remove drag handlers
        foreach (RectTransform element in customizableUIElements)
        {
            if (element != null)
            {
                RemoveDragHandler(element);
            }
        }
    }
    
    void AddDragHandler(RectTransform element)
    {
        DraggableUI draggable = element.GetComponent<DraggableUI>();
        if (draggable == null)
            draggable = element.gameObject.AddComponent<DraggableUI>();
        
        draggable.Initialize(this);
    }
    
    void RemoveDragHandler(RectTransform element)
    {
        DraggableUI draggable = element.GetComponent<DraggableUI>();
        if (draggable != null)
            Destroy(draggable);
    }
    
    public void OnElementDrag(RectTransform element, Vector2 delta)
    {
        if (isCustomizing)
        {
            element.anchoredPosition += delta;
        }
    }
    
    void SavePositions()
    {
        foreach (RectTransform element in customizableUIElements)
        {
            if (element != null)
            {
                string key = GetElementKey(element);
                PlayerPrefs.SetFloat(key + "_X", element.anchoredPosition.x);
                PlayerPrefs.SetFloat(key + "_Y", element.anchoredPosition.y);
            }
        }
        PlayerPrefs.Save();
        
        Debug.Log("UI positions saved!");
        StopCustomization();
    }
    
    void CancelCustomization()
    {
        foreach (var kvp in originalPositions)
        {
            if (kvp.Key != null)
                kvp.Key.anchoredPosition = kvp.Value;
        }
        StopCustomization();
    }
    
    public void LoadPositions()
    {
        foreach (RectTransform element in customizableUIElements)
        {
            if (element != null)
            {
                string key = GetElementKey(element);
                if (PlayerPrefs.HasKey(key + "_X"))
                {
                    float x = PlayerPrefs.GetFloat(key + "_X");
                    float y = PlayerPrefs.GetFloat(key + "_Y");
                    element.anchoredPosition = new Vector2(x, y);
                }
            }
        }
    }
    
    string GetElementKey(RectTransform element)
    {
        return "UI_" + element.name;
    }
    
    public void FindUIElementsInScene()
    {
        customizableUIElements.Clear();
        
        // Find joystick
        Joystick joystick = FindObjectOfType<Joystick>();
        if (joystick != null)
            customizableUIElements.Add(joystick.GetComponent<RectTransform>());
        
        // Find all buttons with specific names
        Button[] buttons = FindObjectsOfType<Button>();
        foreach (Button button in buttons)
        {
            string name = button.name.ToLower();
            if (name.Contains("shoot") || name.Contains("reload") || 
                name.Contains("pause") || name.Contains("settings"))
            {
                customizableUIElements.Add(button.GetComponent<RectTransform>());
            }
        }
        
        Debug.Log($"Found {customizableUIElements.Count} UI elements to customize");
    }
}

public class DraggableUI : MonoBehaviour, IDragHandler
{
    private UICustomizationManager manager;
    private RectTransform rectTransform;
    
    public void Initialize(UICustomizationManager mgr)
    {
        manager = mgr;
        rectTransform = GetComponent<RectTransform>();
    }
    
    public void OnDrag(PointerEventData eventData)
    {
        if (manager != null && manager.isCustomizing)
        {
            rectTransform.anchoredPosition += eventData.delta;
        }
    }
}