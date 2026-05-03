using UnityEngine;

public class PlayerLook : MonoBehaviour
{
    public Transform playerBody;
    public float sensitivity = 0.1f;
    public bool invertY = false;
    public float smoothTime = 0.1f; // Smoothing time for camera

    float xRotation = 0f;
    private Vector2 currentLookDelta;
    private Vector2 lookVelocity;
    private float currentXRotation;
    private float xRotationVelocity;

    void Start()
    {
        // Load saved sensitivity
        if (SettingsManager.Instance != null)
        {
            sensitivity = SettingsManager.Instance.GetNormalSensitivity();
            Debug.Log($"PlayerLook sensitivity loaded: {sensitivity}");
        }
        
    }

    public void Look(Vector2 delta)
    {
        // Apply sensitivity
        float mouseX = delta.x * sensitivity;
        float mouseY = delta.y * sensitivity;
        
        if (invertY)
            mouseY = -mouseY;

        // Smooth the look movement
        currentLookDelta = Vector2.SmoothDamp(currentLookDelta, new Vector2(mouseX, mouseY), ref lookVelocity, smoothTime);
        
        // Apply rotation - THIS IS THE FIX: limit total rotation
        xRotation -= currentLookDelta.y;
        xRotation = Mathf.Clamp(xRotation, -80f, 85f); // Changed upper limit to 85 to prevent over-rotation
        
        // Apply rotations
        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        playerBody.Rotate(Vector3.up * currentLookDelta.x);
    }
        
    public void SetSensitivity(float newSensitivity)
    {
        sensitivity = newSensitivity;
        Debug.Log($"Sensitivity updated to: {sensitivity}");
    }
}