using UnityEngine;
using UnityEngine.EventSystems;

public class LookPanel : MonoBehaviour,
    IDragHandler,
    IPointerDownHandler,
    IPointerUpHandler
{
    public PlayerLook playerLook;

    [Header("Touch / Mouse Settings")]
    public float sensitivityMultiplier = 0.3f;

    public float maxDelta = 10f;

    public static bool IsDragging = false;

    void Start()
    {
        if (playerLook == null)
        {
            Camera mainCamera = Camera.main;

            if (mainCamera != null)
            {
                playerLook =
                    mainCamera.GetComponent<PlayerLook>();
            }
        }
    }

    public void OnPointerDown(
        PointerEventData eventData)
    {
        IsDragging = true;
    }

    public void OnDrag(
        PointerEventData eventData)
    {
        IsDragging = true;

        Vector2 delta =
            eventData.delta * sensitivityMultiplier;

        delta.x = Mathf.Clamp(
            delta.x,
            -maxDelta,
            maxDelta
        );

        delta.y = Mathf.Clamp(
            delta.y,
            -maxDelta,
            maxDelta
        );

        if (playerLook != null)
        {
            playerLook.Look(delta);
        }
    }

    public void OnPointerUp(
        PointerEventData eventData)
    {
        IsDragging = false;
    }
}