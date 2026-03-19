using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 100;
    public int currentHealth;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip damageSound;

    [Header("Camera Shake")]
    public Transform playerCamera;
    public float shakeDuration = 0.15f;
    public float shakeMagnitude = 0.2f;

    [Header("Damage Effect")]
    public Image damageOverlay;
    public float fadeSpeed = 2f;
    [Header("UI")]
    public Slider healthSlider;

    Vector3 originalCamPos;

    void Start()
    {
        currentHealth = maxHealth;

        if(healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }

        if(playerCamera != null)
            originalCamPos = playerCamera.localPosition;
    }
    void Update()
    {
        if(damageOverlay)
        {
            float healthPercent = (float)currentHealth / maxHealth;

            float targetAlpha = 0f;

            if(healthPercent < 0.3f)
                targetAlpha = 0.35f;

            Color c = damageOverlay.color;
            c.a = Mathf.Lerp(c.a, targetAlpha, Time.deltaTime * fadeSpeed);
            damageOverlay.color = c;
        }
    }
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;

        if(audioSource && damageSound)
            audioSource.PlayOneShot(damageSound);
        if(damageOverlay)
        {
            Color c = damageOverlay.color;
            c.a = 0.5f;
            damageOverlay.color = c;
        }
        if(playerCamera != null)
            StartCoroutine(ShakeCoroutine(shakeDuration, shakeMagnitude));

        UpdateHealthBar();

        if(currentHealth <= 0)
        {
            Die();
        }
    }

    IEnumerator ShakeCoroutine(float duration, float magnitude)
    {
        float timer = 0;

        while(timer < duration)
        {
            float x = Random.Range(-1f,1f) * magnitude;
            float y = Random.Range(-1f,1f) * magnitude;

            playerCamera.localPosition = new Vector3(
                originalCamPos.x + x,
                originalCamPos.y + y,
                originalCamPos.z
            );

            timer += Time.deltaTime;

            yield return null;
        }

        playerCamera.localPosition = originalCamPos;
    }
    void UpdateHealthBar()
    {
        if(healthSlider != null)
            StartCoroutine(SmoothHealth());
    }

    IEnumerator SmoothHealth()
    {
        float start = healthSlider.value;
        float target = currentHealth;
        float t = 0;

        while(t < 1)
        {
            t += Time.deltaTime * 5f;
            healthSlider.value = Mathf.Lerp(start, target, t);
            yield return null;
        }
    }
    void Die()
    {
        Debug.Log("Player Dead");
    }
}