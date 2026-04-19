using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EquipSlotUI : MonoBehaviour
{
    [Header("UI Elements")]
    public Image gunIcon;
    public TextMeshProUGUI gunNameText;
    public TextMeshProUGUI gunTypeText;
    public TextMeshProUGUI damageText;
    public Button selectButton;
    
    private GunData gunData;
    private EquipManager equipManager;
    
    public void Setup(GunData gun, EquipManager manager)
    {
        gunData = gun;
        equipManager = manager;
        
        if (gunIcon != null && gun.gunIcon != null)
            gunIcon.sprite = gun.gunIcon;
            
        if (gunNameText != null)
            gunNameText.text = gun.gunName;
            
        if (gunTypeText != null)
            gunTypeText.text = gun.gunType.ToString();
            
        // Show upgraded damage
        int currentDamage = gun.baseDamage + (gun.currentUpgradeLevel * 5);
        if (damageText != null)
            damageText.text = $"Damage: {currentDamage}";
        
        // Clear existing listeners
        if (selectButton != null)
        {
            selectButton.onClick.RemoveAllListeners();
            // Directly call OnGunButtonClick instead of SelectGunToEquip
            selectButton.onClick.AddListener(() => equipManager.OnGunButtonClick(gunData));
        }
        if (selectButton != null)
        {
            selectButton.onClick.RemoveAllListeners();
            selectButton.onClick.AddListener(() => {
                // Play sound from equip manager
                equipManager.PlayButtonSound();  // You'll need to make PlayButtonSound public
                equipManager.OnGunButtonClick(gunData);
            });
        }
    }
}