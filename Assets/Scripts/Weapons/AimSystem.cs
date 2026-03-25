using UnityEngine;

public class AimSystem : MonoBehaviour
{
    public Camera playerCamera;
    public DynamicCrosshair crosshair;
    public float normalFOV = 60f;
    public float aimFOV = 35f;
    public float aimSpeed = 10f;

    public Transform gun;
    public Vector3 normalGunPosition;
    public Vector3 aimGunPosition;
    public float gunMoveSpeed = 10f;

    public PlayerMovement playerMovement;
    public float normalSpeed = 5f;
    public float aimSpeedMovement = 2.5f;
    [Header("Gun Rotation")]
    public Vector3 normalGunRotation;
    public Vector3 aimGunRotation;
    bool aiming = false;
    [Header("Weapon Bob")]
    public float bobSpeed = 6f; 
    public float bobAmount = 0.03f;

float bobTimer = 0f;

    void Update()
    {
        // Camera zoom
        float targetFOV = aiming ? aimFOV : normalFOV;

        playerCamera.fieldOfView = Mathf.Lerp(
            playerCamera.fieldOfView,
            targetFOV,
            Time.deltaTime * aimSpeed
        );

        // Gun position
        Vector3 targetPos = aiming ? aimGunPosition : normalGunPosition;

        Vector3 recoilPos = Vector3.zero;
        float recoilRot = 0f;

        Gun gunScript = gun.GetComponent<Gun>();

        if(gunScript != null)
        {
            recoilPos = gunScript.GetRecoilPosition();
            recoilRot = gunScript.GetRecoilRotation();
        }

        Vector3 finalPos = targetPos + recoilPos;

        gun.localPosition = Vector3.Lerp(
            gun.localPosition,
            finalPos,
            Time.deltaTime * gunMoveSpeed
        );

        Vector3 baseRot = aiming ? aimGunRotation : normalGunRotation;

        Quaternion targetRot = Quaternion.Euler(
            baseRot.x + recoilRot,
            baseRot.y,
            baseRot.z
        );

        gun.localRotation = Quaternion.Lerp(
            gun.localRotation,
            targetRot,
            Time.deltaTime * gunMoveSpeed
        );
        WeaponBob();
    }
    public void ToggleAim()
    {
        aiming = !aiming;

        if (aiming)
        {
            playerMovement.speed = aimSpeedMovement;

            if(crosshair != null)
                crosshair.ShowCrosshair(false);
        }
        else
        {
            playerMovement.speed = normalSpeed;

            if(crosshair != null)
                crosshair.ShowCrosshair(true);
        }
    }
    public bool IsAiming()
    {
        return aiming;
    }

    public void StopAim()
    {
        aiming = false;
        playerMovement.speed = normalSpeed;

        if(crosshair != null)
            crosshair.ShowCrosshair(true);
    }

    public void ForceAim()
    {   
        aiming = true;
        playerMovement.speed = aimSpeedMovement;

        if(crosshair != null)
            crosshair.ShowCrosshair(false);
    }
    void WeaponBob()
    {
        if(playerMovement == null || gun == null) return;

        float move = Mathf.Abs(playerMovement.joystick.Horizontal + playerMovement.keyboardInput.x) +
                    Mathf.Abs(playerMovement.joystick.Vertical + playerMovement.keyboardInput.y);

        if(move > 0.1f && !aiming)
        {
            bobTimer += Time.deltaTime * bobSpeed;

            float bobX = Mathf.Cos(bobTimer) * bobAmount;
            float bobY = Mathf.Sin(bobTimer * 2) * bobAmount;

            gun.localPosition += new Vector3(bobX, bobY, 0) * Time.deltaTime;
        }
    }
}