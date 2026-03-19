using UnityEngine;
using UnityEngine.EventSystems;

public class LookPanel : MonoBehaviour, IDragHandler, IPointerDownHandler
{
    public PlayerLook playerLook;

    Vector2 lastPosition;

    public void OnPointerDown(PointerEventData eventData)
    {
        lastPosition = eventData.position;
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 delta = eventData.position - lastPosition;
        lastPosition = eventData.position;

        playerLook.Look(delta);
    }
}