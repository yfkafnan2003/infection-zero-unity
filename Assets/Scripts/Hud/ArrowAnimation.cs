using UnityEngine;

public class ArrowAnimation : MonoBehaviour
{
    public float floatAmount = 15f;
    public float floatSpeed = 3f;

    public float scaleAmount = 0.15f;
    public float scaleSpeed = 3f;

    RectTransform rect;
    Vector2 basePos;
    Vector3 baseScale;

    void OnEnable()
    {
        rect = GetComponent<RectTransform>();
        basePos = rect.anchoredPosition;
        baseScale = rect.localScale;
    }

    void Update()
    {
        rect.anchoredPosition =
            basePos +
            Vector2.up * Mathf.Sin(Time.unscaledTime * floatSpeed) * floatAmount;

        float scale =
            1f + Mathf.Sin(Time.unscaledTime * scaleSpeed) * scaleAmount;

        rect.localScale = baseScale * scale;
    }
}