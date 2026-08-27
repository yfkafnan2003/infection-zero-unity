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

    public WeaponManager weaponManager;
    [Header("Auto Shoot")]
    public bool autoShoot = true;
    public float autoShootDistance = 20f;
    private bool isAutoShooting = false;
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
        Gun gun = weaponManager.GetCurrentWeapon();

        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, detectionDistance))
        {
            ZombieHealth zombie = hit.collider.GetComponentInParent<ZombieHealth>();
            BossHealth boss = hit.collider.GetComponentInParent<BossHealth>();

            bool validTarget =
                (zombie != null && !zombie.IsDead()) ||
                (boss != null && !boss.IsDead());

            if (validTarget)
            {
                SetCrosshairColor(enemyColor);

                float distance = hit.distance;

                if (autoShoot &&
                    gun != null &&
                    !gun.IsReloading() &&
                    distance <= autoShootDistance)
                {
                    if (!isAutoShooting)
                    {
                        gun.StartShooting();
                        isAutoShooting = true;
                    }
                }
                else
                {
                    if (isAutoShooting)
                    {
                        gun.StopShooting();
                        isAutoShooting = false;
                    }
                }

                return;
            }
        }

        SetCrosshairColor(normalColor);

        if(gun != null && isAutoShooting)
        {
            gun.StopShooting();
            isAutoShooting = false;
        }
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
    public void SetVisible(bool visible)
    {
        float alpha = visible ? 1f : 0f;

        SetImageAlpha(crosshairDot, alpha);
        SetImageAlpha(crosshairL, alpha);
        SetImageAlpha(crosshairR, alpha);
        SetImageAlpha(crosshairU, alpha);
        SetImageAlpha(crosshairD, alpha);
    }

    void SetImageAlpha(Image img, float alpha)
    {
        Color c = img.color;
        c.a = alpha;
        img.color = c;
    }
}