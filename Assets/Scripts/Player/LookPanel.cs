using UnityEngine;
using UnityEngine.EventSystems;

public class LookPanel : MonoBehaviour, IDragHandler, IPointerDownHandler
{
    public PlayerLook playerLook;
    public float sensitivityMultiplier = 1f;
    public float maxDelta = 10f; // Limit maximum delta to prevent extreme jumps

    Vector2 lastPosition;
    Vector2 currentDelta;
    Vector2 deltaVelocity;

    void Start()
    {
        // Find PlayerLook if not assigned
        if (playerLook == null)
        {
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
                playerLook = mainCamera.GetComponent<PlayerLook>();
        }
        
        // Get sensitivity from settings
        if (SettingsManager.Instance != null)
        {
            sensitivityMultiplier = SettingsManager.Instance.GetNormalSensitivity();
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        lastPosition = eventData.position;
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 delta = eventData.position - lastPosition;
        lastPosition = eventData.position;
        
        // Clamp delta to prevent extreme jumps
        delta.x = Mathf.Clamp(delta.x, -maxDelta, maxDelta);
        delta.y = Mathf.Clamp(delta.y, -maxDelta, maxDelta);
        
        // Apply multiplier
        delta *= sensitivityMultiplier;
        
        if (playerLook != null)
        {
            playerLook.Look(delta);
        }
    }
}