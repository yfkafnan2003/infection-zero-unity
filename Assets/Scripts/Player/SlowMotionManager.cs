using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class SlowMotionManager : MonoBehaviour
{
    [Header("Slow Motion Settings")]
    public float slowMotionDuration = 10f;
    public float slowMotionScale = 0.25f;
    public float cooldownDuration = 60f;

    [Header("UI")]
    public Button slowMotionButton;
    public Image cooldownImage;
    public Slider durationSlider;
    public TextMeshProUGUI timerText;

    private bool isActive = false;
    private bool isOnCooldown = false;

    void Start()
    {
        Time.timeScale = 1f;

        if (cooldownImage != null)
        {
            cooldownImage.fillAmount = 0f;
        }

        if (durationSlider != null)
        {
            durationSlider.maxValue = slowMotionDuration;
            durationSlider.value = 0f;
        }

        if (timerText != null)
            timerText.gameObject.SetActive(false);
    }

    public void ActivateSlowMotion()
    {
        if (isActive || isOnCooldown)
            return;

        StartCoroutine(SlowMotionRoutine());
    }

    IEnumerator SlowMotionRoutine()
    {
        isActive = true;

        if (slowMotionButton != null)
            slowMotionButton.interactable = false;

        // Start slow motion
        Time.timeScale = slowMotionScale;

        float timer = 0f;

        if (timerText != null)
            timerText.gameObject.SetActive(true);

        // 10 second slow motion
        while (timer < slowMotionDuration)
        {
            // IMPORTANT:
            // unscaledDeltaTime is NOT affected by Time.timeScale
            timer += Time.unscaledDeltaTime;

            if (durationSlider != null)
                durationSlider.value = timer;

            if (timerText != null)
            {
                float remaining = slowMotionDuration - timer;
                timerText.text = Mathf.CeilToInt(remaining) + "s";
            }

            yield return null;
        }

        // Return game to normal speed
        Time.timeScale = 1f;

        if (durationSlider != null)
            durationSlider.value = 0f;

        if (timerText != null)
            timerText.gameObject.SetActive(false);

        isActive = false;

        // Start 1 minute cooldown
        StartCoroutine(CooldownRoutine());
    }

    IEnumerator CooldownRoutine()
    {
        isOnCooldown = true;

        float timer = 0f;

        if (cooldownImage != null)
        {
            cooldownImage.fillAmount = 1f;
        }

        while (timer < cooldownDuration)
        {
            timer += Time.unscaledDeltaTime;

            // 1 → 0 during cooldown
            float progress = timer / cooldownDuration;

            if (cooldownImage != null)
                cooldownImage.fillAmount = 1f - progress;

            yield return null;
        }

        if (cooldownImage != null)
            cooldownImage.fillAmount = 0f;

        isOnCooldown = false;

        if (slowMotionButton != null)
            slowMotionButton.interactable = true;
    }

    void OnDestroy()
    {
        Time.timeScale = 1f;
    }
}