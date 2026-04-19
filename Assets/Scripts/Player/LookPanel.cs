using UnityEngine;
using UnityEngine.EventSystems;

public class LookPanel : MonoBehaviour, IDragHandler, IPointerDownHandler
{
    public PlayerLook playerLook;

    Vector2 lastPosition;

    void Start()
    {
        // Find PlayerLook if not assigned
        if (playerLook == null)
        {
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
                playerLook = mainCamera.GetComponent<PlayerLook>();
        }
        
        Debug.Log($"LookPanel initialized - PlayerLook: {(playerLook != null ? "Found" : "Not Found")}");
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        lastPosition = eventData.position;
        Debug.Log($"Pointer down at: {lastPosition}");
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 delta = eventData.position - lastPosition;
        lastPosition = eventData.position;
        
        // Only log every 10 frames to avoid spam
        if (Time.frameCount % 30 == 0)
            Debug.Log($"Drag delta: {delta}, Sensitivity from PlayerLook: {playerLook?.sensitivity}");
        
        if (playerLook != null)
        {
            playerLook.Look(delta);
        }
    }
}