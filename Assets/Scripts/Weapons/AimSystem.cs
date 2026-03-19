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

    bool aiming = false;

    void Update()
    {
        // Camera zoom
        float targetFOV = aiming ? aimFOV : normalFOV;
        playerCamera.fieldOfView = Mathf.Lerp(
            playerCamera.fieldOfView,
            targetFOV,
            Time.deltaTime * aimSpeed
        );

        // Gun position change
        Vector3 targetPos = aiming ? aimGunPosition : normalGunPosition;
        gun.localPosition = Vector3.Lerp(
            gun.localPosition,
            targetPos,
            Time.deltaTime * gunMoveSpeed
        );
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
}