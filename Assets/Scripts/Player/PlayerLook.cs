using UnityEngine;

public class PlayerLook : MonoBehaviour
{
    public Transform playerBody;
    public float sensitivity = 0.1f;
    public bool invertY = false;

    float xRotation = 0f;

    void Start()
    {
        // Load saved sensitivity
        if (SettingsManager.Instance != null)
        {
            sensitivity = SettingsManager.Instance.GetNormalSensitivity();
            Debug.Log($"PlayerLook sensitivity loaded: {sensitivity}");
        }
    }
    void Update()
    {
        // For testing - remove after confirming it works
        if (Input.GetMouseButton(0)) // Left click to test
        {
            float testX = Input.GetAxis("Mouse X") * sensitivity;
            float testY = Input.GetAxis("Mouse Y") * sensitivity;
            
            xRotation -= testY;
            xRotation = Mathf.Clamp(xRotation, -80f, 80f);
            
            transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
            playerBody.Rotate(Vector3.up * testX);
            
            Debug.Log($"Mouse test - Sensitivity: {sensitivity}, X: {testX}, Y: {testY}");
        }
    }
    public void Look(Vector2 delta)
    {
        // Remove the * 0.1f multiplier
        float mouseX = delta.x * sensitivity;
        float mouseY = delta.y * sensitivity;
        
        if (invertY)
            mouseY = -mouseY;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -80f, 80f);

        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        playerBody.Rotate(Vector3.up * mouseX);
    }
    
    public void SetSensitivity(float newSensitivity)
    {
        sensitivity = newSensitivity;
        Debug.Log($"Sensitivity updated to: {sensitivity}");
    }
}