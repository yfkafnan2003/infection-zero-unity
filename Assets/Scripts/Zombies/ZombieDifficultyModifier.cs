using UnityEngine;

[RequireComponent(typeof(ZombieHealth))]
public class ZombieDifficultyModifier : MonoBehaviour
{
    void Start()
    {
        // Apply difficulty settings from LevelManager
        LevelManager levelManager = FindObjectOfType<LevelManager>();
        if (levelManager != null && levelManager.currentPOIData != null)
        {
            ZombieHealth health = GetComponent<ZombieHealth>();
            if (health != null)
            {
                health.health = levelManager.currentPOIData.GetZombieHealth();
            }
        }
    }
}