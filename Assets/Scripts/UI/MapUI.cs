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
    public Button energyAdButton;

    void Start()
    {
        if (energyAdButton != null)
            energyAdButton.onClick.AddListener(OnEnergyAdButtonClicked);

        UpdateEnergyUI();
    }

    void OnEnergyAdButtonClicked()
    {
        // Don't allow rewarded energy ads if Infinite Energy is active
        if (GameManager.instance != null &&
            GameManager.instance.infiniteStamina)
        {
            return;
        }

        if (EnergyAdManager.Instance != null)
        {
            EnergyAdManager.Instance.ShowConfirmationPanel();
        }
    }

    void Update()
    {
        GameManager gm = GameManager.instance;

        if (gm == null)
            return;

        // =========================
        // ENERGY
        // =========================

        UpdateEnergyUI();

        // =========================
        // LEVEL
        // =========================

        levelText.text = "Level " + gm.playerLevel;

        // =========================
        // MONEY
        // =========================

        moneyText.text = "$" + gm.playerMoney;

        // =========================
        // XP
        // =========================

        xpSlider.maxValue = gm.xpToNextLevel;
        xpSlider.value = gm.currentXP;

        xpText.text =
            gm.currentXP + " / " +
            gm.xpToNextLevel + " XP";
    }

    void UpdateEnergyUI()
    {
        GameManager gm = GameManager.instance;

        if (gm == null)
            return;

        // =========================
        // INFINITE ENERGY
        // =========================

        if (gm.infiniteStamina)
        {
            // Show infinity symbol
            energyText.text = "∞";

            // Hide regeneration timer
            if (energyTimerText != null)
                energyTimerText.gameObject.SetActive(false);

            // Optional:
            // Hide the "watch ad for energy" button
            if (energyAdButton != null)
                energyAdButton.gameObject.SetActive(false);

            return;
        }

        // =========================
        // NORMAL ENERGY
        // =========================

        energyText.text =
            gm.currentEnergy + "/" +
            gm.maxEnergy;

        // Show regeneration timer
        if (gm.currentEnergy < gm.maxEnergy &&
            energyTimerText != null)
        {
            float timeRemaining =
                gm.GetEnergyRegenTimeRemaining();

            int minutes =
                Mathf.FloorToInt(timeRemaining / 60);

            int seconds =
                Mathf.FloorToInt(timeRemaining % 60);

            energyTimerText.text =
                $"{minutes:00}:{seconds:00}";

            energyTimerText.gameObject.SetActive(true);
        }
        else if (energyTimerText != null)
        {
            energyTimerText.gameObject.SetActive(false);
        }

        // Show ad button again
        if (energyAdButton != null)
            energyAdButton.gameObject.SetActive(true);
    }
}