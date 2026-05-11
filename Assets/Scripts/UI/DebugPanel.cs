using UnityEngine;
using UnityEngine.UI;

public class DebugPanel : MonoBehaviour
{
    [Header("Debug Buttons")]
    public Button infiniteEnergyButton;
    public Button addMoneyButton;
    public Button addXPButton;
    
    [Header("Settings")]
    public int moneyAmount = 50000;
    public int xpAmount = 10;
    
    private bool isInfiniteEnergy = false;
    private int originalEnergy;
    
    void Start()
    {
        // Setup button listeners
        if (infiniteEnergyButton != null)
            infiniteEnergyButton.onClick.AddListener(ToggleInfiniteEnergy);
        
        if (addMoneyButton != null)
            addMoneyButton.onClick.AddListener(AddMoney);
        
        if (addXPButton != null)
            addXPButton.onClick.AddListener(AddXP);
        
        // Optional: Hide debug panel in release builds
        #if !UNITY_EDITOR
        gameObject.SetActive(false);
        #endif
    }
    
    void ToggleInfiniteEnergy()
    {
        if (GameManager.instance == null) return;
        
        isInfiniteEnergy = !isInfiniteEnergy;
        
        if (isInfiniteEnergy)
        {
            // Store original energy
            originalEnergy = GameManager.instance.currentEnergy;
            // Set to max
            GameManager.instance.currentEnergy = GameManager.instance.maxEnergy;
            // Disable energy regen
            GameManager.instance.energyRegenTime = 999999f;
            
            Debug.Log("<color=green>INFINITE ENERGY ACTIVATED!</color>");
            
            // Change button color to indicate active
            if (infiniteEnergyButton != null)
            {
                ColorBlock colors = infiniteEnergyButton.colors;
                colors.normalColor = Color.green;
                infiniteEnergyButton.colors = colors;
            }
        }
        else
        {
            // Restore original energy
            GameManager.instance.currentEnergy = originalEnergy;
            // Restore energy regen
            GameManager.instance.energyRegenTime = 300f;
            
            Debug.Log("<color=red>INFINITE ENERGY DEACTIVATED!</color>");
            
            // Reset button color
            if (infiniteEnergyButton != null)
            {
                ColorBlock colors = infiniteEnergyButton.colors;
                colors.normalColor = Color.white;
                infiniteEnergyButton.colors = colors;
            }
        }
        
        GameManager.instance.SaveAllData();
    }
    
    void AddMoney()
    {
        if (GameManager.instance == null) return;
        
        GameManager.instance.AddMoney(moneyAmount);
        Debug.Log($"<color=yellow>Added ${moneyAmount} money! Total: ${GameManager.instance.playerMoney}</color>");
    }
    
    void AddXP()
    {
        if (GameManager.instance == null) return;
        
        GameManager.instance.AddXP(xpAmount);
        Debug.Log($"<color=cyan>Added {xpAmount} XP! Total XP: {GameManager.instance.currentXP}</color>");
    }

}