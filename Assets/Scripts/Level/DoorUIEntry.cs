using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class DoorUIEntry : MonoBehaviour
{
    private Slider healthSlider;
    private DoorHealth doorReference;
    public TextMeshProUGUI repairingText;
    private Coroutine hideRepairingCoroutine;
    public void Setup(DoorHealth door, Slider slider)
    {
        doorReference = door;
        healthSlider = slider;
        healthSlider.maxValue = door.maxHealth;
        healthSlider.value = door.currentHealth;
    }
    
    public void UpdateHealth(int currentHealth, int maxHealth)
    {
        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }
    }
    public void ShowRepairing()
    {
        if (repairingText != null)
        {
            // Stop existing coroutine
            if (hideRepairingCoroutine != null)
                StopCoroutine(hideRepairingCoroutine);
            
            repairingText.text = "REPAIRING...";
            repairingText.gameObject.SetActive(true);
            
            // Start coroutine to hide after 1 second
            hideRepairingCoroutine = StartCoroutine(HideRepairingAfterDelay(1f));
        }
    }
    private System.Collections.IEnumerator HideRepairingAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (repairingText != null)
            repairingText.gameObject.SetActive(false);
    }
}