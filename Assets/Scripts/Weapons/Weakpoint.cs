using UnityEngine;

public class Weakpoint : MonoBehaviour
{
    public BossHealth bossHealth;
    public float damageMultiplier = 3f;
    
    void Start()
    {
        if (bossHealth == null)
            bossHealth = GetComponentInParent<BossHealth>();
    }
    
    public void OnHit(int damage)
    {
        if (bossHealth != null)
        {
            int finalDamage = Mathf.RoundToInt(damage * damageMultiplier);
            bossHealth.TakeDamage(finalDamage, true);
            
            // ADD THIS - Show weakpoint hit text
            if (HitTextManager.Instance != null)
            {
                HitTextManager.Instance.ShowWeakpointHitText(transform.position, finalDamage);
            }
        }
    }
}