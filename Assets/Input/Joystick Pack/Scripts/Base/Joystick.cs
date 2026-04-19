using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Joystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    public float Horizontal { get { return (snapX) ? SnapFloat(input.x, AxisOptions.Horizontal) : input.x; } }
    public float Vertical { get { return (snapY) ? SnapFloat(input.y, AxisOptions.Vertical) : input.y; } }
    public Vector2 Direction { get { return new Vector2(Horizontal, Vertical); } }

    public float HandleRange
    {
        get { return handleRange; }
        set { handleRange = Mathf.Abs(value); }
    }

    public float DeadZone
    {
        get { return deadZone; }
        set { deadZone = Mathf.Abs(value); }
    }

    public AxisOptions AxisOptions { get { return AxisOptions; } set { axisOptions = value; } }
    public bool SnapX { get { return snapX; } set { snapX = value; } }
    public bool SnapY { get { return snapY; } set { snapY = value; } }

    [SerializeField] private float handleRange = 1;
    [SerializeField] private float deadZone = 0;
    [SerializeField] private AxisOptions axisOptions = AxisOptions.Both;
    [SerializeField] private bool snapX = false;
    [SerializeField] private bool snapY = false;

    [SerializeField] protected RectTransform background = null;
    [SerializeField] private RectTransform handle = null;
    private RectTransform baseRect = null;

    [Header("Movement Area")]
    public RectTransform movementArea; // The panel where joystick can move
    private Vector2 joystickOriginalPosition;
    private bool isMovingJoystick = false;

    private Canvas canvas;
    private Camera cam;

    private Vector2 input = Vector2.zero;
    
    protected virtual void Start()
    {
        if (movementArea == null)
        {
            // Try to find by name
            GameObject area = GameObject.Find("JoystickMovementArea");
            if (area != null)
                movementArea = area.GetComponent<RectTransform>();
            
            // If still not found, create one
            if (movementArea == null && canvas != null)
            {
                CreateMovementArea();
            }
        }
        HandleRange = handleRange;
        DeadZone = deadZone;
        baseRect = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        if (canvas == null)
            Debug.LogError("The Joystick is not placed inside a canvas");

        Vector2 center = new Vector2(0.5f, 0.5f);
        background.pivot = center;
        handle.anchorMin = center;
        handle.anchorMax = center;
        handle.pivot = center;
        handle.anchoredPosition = Vector2.zero;
        
        joystickOriginalPosition = baseRect.anchoredPosition;
    }

    public virtual void OnPointerDown(PointerEventData eventData)
    {
        // Check if clicking within the movement area
        if (movementArea != null)
        {
            if (RectTransformUtility.RectangleContainsScreenPoint(movementArea, eventData.position, cam))
            {
                // Move entire joystick to click position within the area
                MoveJoystickToPosition(eventData.position);
                isMovingJoystick = true;
            }
        }
        
        // Start the joystick input
        OnDrag(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        cam = null;
        if (canvas.renderMode == RenderMode.ScreenSpaceCamera)
            cam = canvas.worldCamera;

        Vector2 position = RectTransformUtility.WorldToScreenPoint(cam, background.position);
        Vector2 radius = background.sizeDelta / 2;
        input = (eventData.position - position) / (radius * canvas.scaleFactor);
        FormatInput();
        HandleInput(input.magnitude, input.normalized, radius, cam);
        handle.anchoredPosition = input * radius * handleRange;
    }
    
    // Add this public method to your Joystick class
    public void MoveJoystickToPosition(Vector2 screenPosition)
    {
        if (canvas == null)
            canvas = GetComponentInParent<Canvas>();
        
        // Convert screen position to canvas position
        Vector2 localPoint;
        RectTransform canvasRect = canvas.GetComponent<RectTransform>();
        
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPosition, null, out localPoint))
        {
            // Get the movement area bounds (use the parent panel or screen edges)
            RectTransform parentRect = transform.parent.GetComponent<RectTransform>();
            
            if (parentRect != null)
            {
                Vector2 areaMin = parentRect.anchoredPosition - (parentRect.sizeDelta / 2);
                Vector2 areaMax = parentRect.anchoredPosition + (parentRect.sizeDelta / 2);
                
                float clampedX = Mathf.Clamp(localPoint.x, areaMin.x, areaMax.x);
                float clampedY = Mathf.Clamp(localPoint.y, areaMin.y, areaMax.y);
                
                baseRect.anchoredPosition = new Vector2(clampedX, clampedY);
            }
            else
            {
                // Just move to the clicked position
                baseRect.anchoredPosition = localPoint;
            }
            
            // Reset handle position
            handle.anchoredPosition = Vector2.zero;
            input = Vector2.zero;
        }
    }

    protected virtual void HandleInput(float magnitude, Vector2 normalised, Vector2 radius, Camera cam)
    {
        if (magnitude > deadZone)
        {
            if (magnitude > 1)
                input = normalised;
        }
        else
            input = Vector2.zero;
    }

    private void FormatInput()
    {
        if (axisOptions == AxisOptions.Horizontal)
            input = new Vector2(input.x, 0f);
        else if (axisOptions == AxisOptions.Vertical)
            input = new Vector2(0f, input.y);
    }
    void CreateMovementArea()
    {
        GameObject areaObj = new GameObject("JoystickMovementArea");
        areaObj.transform.SetParent(canvas.transform, false);
        
        movementArea = areaObj.AddComponent<RectTransform>();
        movementArea.anchorMin = new Vector2(0, 0.5f);
        movementArea.anchorMax = new Vector2(0.5f, 0.5f);
        movementArea.sizeDelta = new Vector2(Screen.width / 2, Screen.height);
        movementArea.anchoredPosition = new Vector2(movementArea.sizeDelta.x / 2, 0);
        
        // Add a transparent image to make it clickable
        UnityEngine.UI.Image image = areaObj.AddComponent<UnityEngine.UI.Image>();
        image.color = new Color(0, 0, 0, 0.01f); // Almost invisible but clickable
    }
    private float SnapFloat(float value, AxisOptions snapAxis)
    {
        if (value == 0)
            return value;

        if (axisOptions == AxisOptions.Both)
        {
            float angle = Vector2.Angle(input, Vector2.up);
            if (snapAxis == AxisOptions.Horizontal)
            {
                if (angle < 22.5f || angle > 157.5f)
                    return 0;
                else
                    return (value > 0) ? 1 : -1;
            }
            else if (snapAxis == AxisOptions.Vertical)
            {
                if (angle > 67.5f && angle < 112.5f)
                    return 0;
                else
                    return (value > 0) ? 1 : -1;
            }
            return value;
        }
        else
        {
            if (value > 0)
                return 1;
            if (value < 0)
                return -1;
        }
        return 0;
    }

    public virtual void OnPointerUp(PointerEventData eventData)
    {
        isMovingJoystick = false;
        input = Vector2.zero;
        handle.anchoredPosition = Vector2.zero;
    }

    protected Vector2 ScreenPointToAnchoredPosition(Vector2 screenPosition)
    {
        Vector2 localPoint = Vector2.zero;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(baseRect, screenPosition, cam, out localPoint))
        {
            Vector2 pivotOffset = baseRect.pivot * baseRect.sizeDelta;
            return localPoint - (background.anchorMax * baseRect.sizeDelta) + pivotOffset;
        }
        return Vector2.zero;
    }
    
    // Public method to reset joystick to original position
    public void ResetJoystickPosition()
    {
        baseRect.anchoredPosition = joystickOriginalPosition;
        handle.anchoredPosition = Vector2.zero;
        input = Vector2.zero;
    }
}

public enum AxisOptions { Both, Horizontal, Vertical }