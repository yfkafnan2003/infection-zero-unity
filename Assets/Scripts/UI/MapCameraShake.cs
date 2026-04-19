using UnityEngine;

public class MapCameraShake : MonoBehaviour
{
    [Header("Shake Settings")]
    public float shakeIntensity = 0.05f;  // How much the camera moves
    public float shakeSpeed = 2f;         // How fast it moves
    public float rotationIntensity = 0.5f; // How much it rotates
    
    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private float shakeTimer = 0f;
    
    void Start()
    {
        originalPosition = transform.localPosition;
        originalRotation = transform.localRotation;
        
        // Start continuous subtle shake
        StartShake();
    }
    
    void Update()
    {
        if (shakeTimer > 0)
        {
            // Continuous gentle movement
            float offsetX = Mathf.Sin(Time.time * shakeSpeed) * shakeIntensity;
            float offsetY = Mathf.Cos(Time.time * shakeSpeed * 1.3f) * shakeIntensity * 0.5f;
            float offsetZ = Mathf.Sin(Time.time * shakeSpeed * 1.7f) * shakeIntensity * 0.3f;
            
            transform.localPosition = originalPosition + new Vector3(offsetX, offsetY, offsetZ);
            
            // Slight rotation for more organic feel
            float rotX = Mathf.Sin(Time.time * shakeSpeed * 0.8f) * rotationIntensity * 0.1f;
            float rotY = Mathf.Cos(Time.time * shakeSpeed * 1.2f) * rotationIntensity * 0.1f;
            float rotZ = Mathf.Sin(Time.time * shakeSpeed * 1.5f) * rotationIntensity * 0.05f;
            
            transform.localRotation = originalRotation * Quaternion.Euler(rotX, rotY, rotZ);
            
            shakeTimer -= Time.deltaTime;
        }
        else
        {
            // Return to original position when shake stops
            transform.localPosition = Vector3.Lerp(transform.localPosition, originalPosition, Time.deltaTime * 5f);
            transform.localRotation = Quaternion.Slerp(transform.localRotation, originalRotation, Time.deltaTime * 5f);
        }
    }
    
    public void StartShake()
    {
        shakeTimer = 999f; // Continuous shake
    }
    
    public void StopShake()
    {
        shakeTimer = 0f;
    }
    
    // Call this for a strong shake when clicking on POI
    public void StrongShake(float duration = 0.3f, float intensity = 0.15f)
    {
        StartCoroutine(StrongShakeCoroutine(duration, intensity));
    }
    
    System.Collections.IEnumerator StrongShakeCoroutine(float duration, float intensity)
    {
        float elapsed = 0f;
        Vector3 originalPos = transform.localPosition;
        Quaternion originalRot = transform.localRotation;
        
        while (elapsed < duration)
        {
            float x = Random.Range(-intensity, intensity);
            float y = Random.Range(-intensity * 0.5f, intensity * 0.5f);
            float z = Random.Range(-intensity * 0.3f, intensity * 0.3f);
            
            float rotX = Random.Range(-intensity * 2f, intensity * 2f);
            float rotY = Random.Range(-intensity, intensity);
            float rotZ = Random.Range(-intensity * 0.5f, intensity * 0.5f);
            
            transform.localPosition = originalPos + new Vector3(x, y, z);
            transform.localRotation = originalRot * Quaternion.Euler(rotX, rotY, rotZ);
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        // Return to original
        float returnTime = 0.2f;
        float returnElapsed = 0f;
        Vector3 startPos = transform.localPosition;
        Quaternion startRot = transform.localRotation;
        
        while (returnElapsed < returnTime)
        {
            returnElapsed += Time.deltaTime;
            float t = returnElapsed / returnTime;
            transform.localPosition = Vector3.Lerp(startPos, originalPos, t);
            transform.localRotation = Quaternion.Slerp(startRot, originalRot, t);
            yield return null;
        }
        
        transform.localPosition = originalPos;
        transform.localRotation = originalRot;
        
        // Restart continuous shake
        StartShake();
    }
}