using UnityEngine;
using UnityEngine.UI;

public class DynamicCrosshair : MonoBehaviour
{

    public Image crosshairDot;
    public Image crosshairL;
    public Image crosshairR;
    public Image crosshairU;
    public Image crosshairD;

    public Color normalColor = Color.white;
    public Color enemyColor = Color.red;
    public LayerMask enemyLayer;
    public Camera playerCamera;
    public float detectionDistance = 100f;
    public RectTransform top;
    public RectTransform bottom;
    public RectTransform left;
    public RectTransform right;

    public float defaultDistance = 20f;
    public float moveDistance = 40f;
    public float shootDistance = 60f;
    
    float currentDistance;
    float targetDistance;

    float shootTimer = 0f;
    public float shootExpandTime = 0.15f;

    void Start()
    {
        currentDistance = defaultDistance;
        targetDistance = defaultDistance;
        CheckEnemyTarget();
    }

    void Update()
    {
        CheckEnemyTarget();   // ← ADD THIS

        if (shootTimer > 0)
        {
            shootTimer -= Time.deltaTime;
            targetDistance = shootDistance;
        }

        currentDistance = Mathf.Lerp(currentDistance, targetDistance, Time.deltaTime * 8f);

        UpdateCrosshair();
    }
    void UpdateCrosshair()
    {
        top.anchoredPosition = new Vector2(0, currentDistance);
        bottom.anchoredPosition = new Vector2(0, -currentDistance);
        left.anchoredPosition = new Vector2(-currentDistance, 0);
        right.anchoredPosition = new Vector2(currentDistance, 0);
    }

    public void ShootExpand()
    {
        shootTimer = shootExpandTime;
    }

    public void MoveExpand()
    {
        if (shootTimer > 0) return; // shooting overrides movement

        targetDistance = moveDistance;
    }
    void CheckEnemyTarget()
    {
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;
        
        // Use single raycast instead of RaycastAll for better performance
        if (Physics.Raycast(ray, out hit, detectionDistance))
        {
            // Try to get ZombieHealth from the hit collider or its parent
            ZombieHealth enemy = hit.collider.GetComponent<ZombieHealth>();
            
            // If not found on the collider, try on parent
            if (enemy == null && hit.collider.transform.parent != null)
                enemy = hit.collider.transform.parent.GetComponent<ZombieHealth>();
            
            // If still not found, try on root
            if (enemy == null)
                enemy = hit.collider.GetComponentInParent<ZombieHealth>();
            
            if (enemy != null && !enemy.IsDead())
            {
                SetCrosshairColor(enemyColor);
                return;
            }
        }
        
        SetCrosshairColor(normalColor);
    }
    void SetCrosshairColor(Color c)
    {
        crosshairDot.color = c;
        crosshairL.color = c;
        crosshairR.color = c;
        crosshairU.color = c;
        crosshairD.color = c;
    }
    public void Idle()
    {
        if (shootTimer > 0) return;

        targetDistance = defaultDistance;
    }

    public void ShowCrosshair(bool show)
    {
        gameObject.SetActive(show);
    }
}