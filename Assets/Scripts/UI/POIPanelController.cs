using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class POIPanelController : MonoBehaviour
{
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI typeText;
    public TextMeshProUGUI difficultyText;
    public TextMeshProUGUI missionDetailsText;
    public TextMeshProUGUI levelRequirementText; // Add this for level requirement display
    public Image poiIconImage;
    
    public Slider difficultySlider;
    
    private POIData poi;

    public void SetupPOI(POIData data)
    {
        poi = data;
        
        // Set the POI icon
        if (poiIconImage != null && data.poiIcon != null)
        {
            poiIconImage.sprite = data.poiIcon;
            poiIconImage.color = Color.white;
            poiIconImage.gameObject.SetActive(true);
        }
        else if (poiIconImage != null)
        {
            poiIconImage.gameObject.SetActive(false);
            Debug.LogWarning($"No icon assigned for POI: {data.poiName}");
        }
        
        // Basic info
        nameText.text = data.poiName;
        typeText.text = GetTypeText(data.poiType);
        difficultyText.text = GetDifficultyText(data.difficultyLevel);
        
        // Show level requirement with color
        UpdateLevelRequirementDisplay();
        
        // Show mission details based on type
        UpdateMissionDetails();
        
        // If you want to show difficulty on slider (read-only)
        if (difficultySlider != null)
        {
            difficultySlider.maxValue = 10;
            difficultySlider.minValue = 1;
            difficultySlider.value = data.difficultyLevel;
            difficultySlider.interactable = false;
        }
    }
    
    void UpdateLevelRequirementDisplay()
    {
        if (levelRequirementText == null) return;
        
        int playerLevel = GameManager.instance != null ? GameManager.instance.playerLevel : 1;
        int requiredLevel = poi.requiredPlayerLevel;
        
        if (playerLevel >= requiredLevel)
        {
            // Player meets requirement - Green text
            levelRequirementText.text = $"START";
            levelRequirementText.color = Color.green;
        }
        else
        {
            // Player doesn't meet requirement - Red text
            levelRequirementText.text = $"Level Required {requiredLevel}";
            levelRequirementText.color = Color.red;
        }
    }
    
    void UpdateMissionDetails()
    {
        if (missionDetailsText == null) return;
        
        switch (poi.poiType)
        {
            case POIType.KillZombies:
                int finalHealth = poi.GetZombieHealth();
                int finalDamage = poi.GetZombieDamage();
                missionDetailsText.text = $"Kill Zombies: {poi.zombieAmount}\n" +
                                         $"Reward: ${poi.GetMoneyReward()} / {poi.GetXPReward()} XP";
                break;
                
            case POIType.CountdownSurvive:
                missionDetailsText.text = $"Survive: {poi.surviveTime} seconds\n" +
                                         $"Reward: ${poi.GetMoneyReward()} / {poi.GetXPReward()} XP";
                break;
                
            case POIType.BossFight:
                missionDetailsText.text = $"DEFEAT THE INFECTION ZERO \n" +
                                        $"KILL THE SOURCE OF VIRUS";
                break;
                
            case POIType.Arena:
                missionDetailsText.text = $"Rounds: 20\n" +
                                         $"Rewards increase each round\n" +
                                         $"3 Guns should equipped";
                break;
            case POIType.ProtectDoor:
                missionDetailsText.text = $"Protect Door for {poi.protectTime} seconds\n" +
                                        $"Reward: ${poi.GetMoneyReward()} / {poi.GetXPReward()} XP";
                break; 

            default:
                missionDetailsText.text = $"Difficulty: {GetDifficultyText(poi.difficultyLevel)}\n" +
                                         $"Reward: ${poi.GetMoneyReward()} / {poi.GetXPReward()} XP";
                break;
        }
    }
    
    string GetTypeText(POIType type)
    {
        switch(type)
        {
            case POIType.CountdownSurvive: return "SURVIVE";
            case POIType.KillZombies: return "ELIMINATION";
            case POIType.RetrieveBox: return "RETRIEVAL";
            case POIType.ReachDestination: return "EXPLORATION";
            case POIType.BossFight: return "BOSS BATTLE";
            case POIType.ProtectDoor: return "PROTECT";
            case POIType.Arena: return "ARENA";
            default: return "MISSION";
        }
    }
    
    string GetDifficultyText(int level)
    {
        if(level <= 1) return "Easy";
        if(level <= 3) return "Normal";
        if(level <= 5) return "Hard";
        if(level <= 7) return "Extreme";
        if(level <= 9) return "Nightmare";
        return "INSANE";
    }
    
    public void StartLevel()
    {
        // Check if player meets level requirement before starting
        if (GameManager.instance != null && GameManager.instance.playerLevel < poi.requiredPlayerLevel)
        {
            Debug.Log($"Cannot start {poi.poiName}. Need level {poi.requiredPlayerLevel}, you are level {GameManager.instance.playerLevel}");
            return;
        }
        
        if (MapManager.Instance != null)
        {
            MapManager.Instance.StartLevel();
        }
    }
}