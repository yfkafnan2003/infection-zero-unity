using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class MapUI : MonoBehaviour
{
    public TextMeshProUGUI energyText;
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI moneyText;
    public TextMeshProUGUI energyTimerText;

    public Slider xpSlider;
    public TextMeshProUGUI xpText;

    void Update()
    {
        GameManager gm = GameManager.instance;
        if (gm == null) return;

        energyText.text = gm.currentEnergy + "/" + gm.maxEnergy;
        levelText.text = "Level " + gm.playerLevel;
        moneyText.text = "$" + gm.playerMoney;

        xpSlider.maxValue = gm.xpToNextLevel;
        xpSlider.value = gm.currentXP;
        xpText.text = gm.currentXP + " / " + gm.xpToNextLevel + " XP";
        
        // Show energy regeneration timer if not at max energy
        if (gm.currentEnergy < gm.maxEnergy && energyTimerText != null)
        {
            float timeRemaining = gm.GetEnergyRegenTimeRemaining();
            int minutes = Mathf.FloorToInt(timeRemaining / 60);
            int seconds = Mathf.FloorToInt(timeRemaining % 60);
            energyTimerText.text = $"{minutes:00}:{seconds:00}";
            energyTimerText.gameObject.SetActive(true);
        }
        else if (energyTimerText != null)
        {
            energyTimerText.gameObject.SetActive(false);
        }
    }
}