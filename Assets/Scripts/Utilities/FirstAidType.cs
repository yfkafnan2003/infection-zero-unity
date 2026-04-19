using UnityEngine;

public enum FirstAidType
{
    Small,   // Heals 25 HP
    Medium,  // Heals 50 HP
    Large    // Heals 100 HP
}

public class FirstAidItem : MonoBehaviour
{
    public FirstAidType aidType;
    public int healAmount;
    
    [Header("Visuals")]
    public AudioClip healSound;
    
    void Start()
    {
        // Set heal amount based on type
        switch (aidType)
        {
            case FirstAidType.Small:
                healAmount = 20;
                break;
            case FirstAidType.Medium:
                healAmount = 50;
                break;
            case FirstAidType.Large:
                healAmount = 100;
                break;
        }
    }
    
    public void Heal(PlayerHealth playerHealth)
    {
        if (playerHealth != null)
        {
            playerHealth.Heal(healAmount);
            
            // Play sound
            if (healSound != null)
                AudioSource.PlayClipAtPoint(healSound, playerHealth.transform.position);
        }
        
        Destroy(gameObject);
    }
}