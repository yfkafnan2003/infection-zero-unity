using UnityEngine;
using System.Collections;

public class PlayerLook : MonoBehaviour
{
    [Header("Player")]
    public Transform playerBody;

    [Header("Look Settings")]
    public float sensitivity = 0.4f;
    public bool invertY = false;

    [Tooltip("Higher = faster camera response")]
    public float lookSmoothness = 20f;

    [Tooltip("Higher = faster input response")]
    public float inputSmoothness = 25f;

    [Header("Camera Rotation")]
    public float maxLookUp = 85f;
    public float maxLookDown = -80f;

    [Header("Aim Assist")]
    public bool aimAssist = true;
    public LayerMask zombieLayer;
    public float assistRange = 25f;
    public float assistRadius = 1.2f;

    [Range(0f, 1f)]
    public float slowdownMultiplier = 0.55f;

    public float horizontalAssistSpeed = 6f;

    private Camera cam;

    // Input
    private Vector2 targetLookInput;
    private Vector2 smoothLookInput;
    private Vector2 inputVelocity;

    // Rotation
    private float targetPitch;
    private float currentPitch;

    private float targetYaw;
    private float currentYaw;

    private float yawVelocity;
    private float pitchVelocity;

    void Awake()
    {
        cam = GetComponent<Camera>();

        if (playerBody != null)
        {
            currentYaw = playerBody.eulerAngles.y;
            targetYaw = currentYaw;
        }

        currentPitch = transform.localEulerAngles.x;

        if (currentPitch > 180f)
            currentPitch -= 360f;

        targetPitch = currentPitch;
    }

    void Start()
    {
        // Start with the Inspector sensitivity immediately.
        // This means PC testing works even if SettingsManager
        // isn't available yet.
        StartCoroutine(LoadSensitivity());
    }

    IEnumerator LoadSensitivity()
    {
        // Wait a maximum of 2 seconds for SettingsManager.
        float timer = 0f;

        while (SettingsManager.Instance == null && timer < 2f)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        // If SettingsManager exists, use saved sensitivity.
        if (SettingsManager.Instance != null)
        {
            sensitivity =
                SettingsManager.Instance.GetNormalSensitivity();

            Debug.Log(
                "PlayerLook sensitivity loaded: " +
                sensitivity
            );
        }
        else
        {
            Debug.Log(
                "SettingsManager not found. " +
                "Using Inspector sensitivity: " +
                sensitivity
            );
        }
    }

    public void Look(Vector2 delta)
    {
        // Clamp input
        delta.x = Mathf.Clamp(
            delta.x,
            -10f,
            10f
        );

        delta.y = Mathf.Clamp(
            delta.y,
            -10f,
            10f
        );

        float mouseX =
            delta.x * sensitivity;

        float mouseY =
            delta.y * sensitivity;

        if (invertY)
            mouseY = -mouseY;

        // Aim slowdown
        if (aimAssist && cam != null)
        {
            Ray ray =
                cam.ViewportPointToRay(
                    new Vector3(0.5f, 0.5f)
                );

            if (Physics.SphereCast(
                ray,
                assistRadius,
                out RaycastHit hit,
                assistRange,
                zombieLayer,
                QueryTriggerInteraction.Ignore))
            {
                ZombieHealth zombie =
                    hit.collider.GetComponentInParent<ZombieHealth>();

                if (zombie != null && !zombie.IsDead())
                {
                    mouseX *= slowdownMultiplier;
                    mouseY *= slowdownMultiplier;
                }
            }
        }

        // Add the movement instead of replacing it.
        //
        // This is important for mouse movement because
        // several pointer events can happen before LateUpdate.
        targetLookInput += new Vector2(
            mouseX,
            mouseY
        );

        // Prevent accumulated input from becoming huge.
        targetLookInput.x =
            Mathf.Clamp(
                targetLookInput.x,
                -20f,
                20f
            );

        targetLookInput.y =
            Mathf.Clamp(
                targetLookInput.y,
                -20f,
                20f
            );
    }

    void LateUpdate()
    {
        // Smooth input
        smoothLookInput =
            Vector2.SmoothDamp(
                smoothLookInput,
                targetLookInput,
                ref inputVelocity,
                1f / inputSmoothness
            );

        // Consume input gradually
        targetLookInput =
            Vector2.Lerp(
                targetLookInput,
                Vector2.zero,
                Time.deltaTime * inputSmoothness
            );

        // -----------------------------
        // YAW
        // -----------------------------

        targetYaw += smoothLookInput.x;

        // -----------------------------
        // PITCH
        // -----------------------------

        targetPitch -= smoothLookInput.y;

        targetPitch =
            Mathf.Clamp(
                targetPitch,
                maxLookDown,
                maxLookUp
            );

        // -----------------------------
        // AIM ASSIST
        // -----------------------------

        if (aimAssist && cam != null)
        {
            float assist =
                GetHorizontalAimAssist();

            targetYaw +=
                assist * Time.deltaTime;
        }

        // -----------------------------
        // SMOOTH ROTATION
        // -----------------------------

        currentYaw =
            Mathf.SmoothDampAngle(
                currentYaw,
                targetYaw,
                ref yawVelocity,
                1f / lookSmoothness
            );

        currentPitch =
            Mathf.SmoothDampAngle(
                currentPitch,
                targetPitch,
                ref pitchVelocity,
                1f / lookSmoothness
            );

        // -----------------------------
        // CAMERA
        // -----------------------------

        transform.localRotation =
            Quaternion.Euler(
                currentPitch,
                0f,
                0f
            );

        // -----------------------------
        // PLAYER BODY
        // -----------------------------

        if (playerBody != null)
        {
            playerBody.rotation =
                Quaternion.Euler(
                    0f,
                    currentYaw,
                    0f
                );
        }
    }

    float GetHorizontalAimAssist()
    {
        if (cam == null)
            return 0f;

        Ray ray =
            cam.ViewportPointToRay(
                new Vector3(0.5f, 0.5f)
            );

        if (Physics.SphereCast(
            ray,
            assistRadius,
            out RaycastHit hit,
            assistRange,
            zombieLayer,
            QueryTriggerInteraction.Ignore))
        {
            ZombieHealth zombie =
                hit.collider.GetComponentInParent<ZombieHealth>();

            if (zombie != null && !zombie.IsDead())
            {
                Vector3 targetDir =
                    (
                        hit.collider.bounds.center -
                        cam.transform.position
                    ).normalized;

                float angle =
                    Vector3.SignedAngle(
                        cam.transform.forward,
                        targetDir,
                        Vector3.up
                    );

                return Mathf.Clamp(
                    angle,
                    -1f,
                    1f
                ) * horizontalAssistSpeed;
            }
        }

        return 0f;
    }

    public void SetSensitivity(
        float newSensitivity)
    {
        sensitivity = newSensitivity;

        Debug.Log(
            "Sensitivity updated to: " +
            sensitivity
        );
    }
}