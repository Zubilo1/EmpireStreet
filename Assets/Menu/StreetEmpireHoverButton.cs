using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(RectTransform))]
public class StreetEmpireHoverButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [Header("Hover animation")]
    public float scaleMultiplier = 1.045f;
    public float moveRight = 12f;
    public float animationSpeed = 12f;
    public Color hoverColor = new Color(1f, 0.22f, 0.12f, 1f);

    private RectTransform rectTransform;
    private Vector3 baseScale;
    private Vector2 basePosition;
    private Image image;
    private Color baseColor;
    private Coroutine animationRoutine;
    private bool hovering;
    private bool pressing;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        baseScale = rectTransform.localScale;
        basePosition = rectTransform.anchoredPosition;
        image = GetComponent<Image>();
        if (image != null) baseColor = image.color;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        hovering = true;
        AnimateToTarget();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        hovering = false;
        pressing = false;
        AnimateToTarget();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        pressing = true;
        AnimateToTarget();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        pressing = false;
        AnimateToTarget();
    }

    private void AnimateToTarget()
    {
        if (!isActiveAndEnabled) return;
        if (animationRoutine != null) StopCoroutine(animationRoutine);
        animationRoutine = StartCoroutine(Animate());
    }

    private IEnumerator Animate()
    {
        Vector3 targetScale = baseScale * (pressing ? scaleMultiplier * 0.98f : hovering ? scaleMultiplier : 1f);
        Vector2 targetPosition = basePosition + new Vector2(hovering ? moveRight : 0f, 0f);
        Color targetColor = hovering && image != null ? hoverColor : baseColor;

        while (Vector3.Distance(rectTransform.localScale, targetScale) > 0.001f ||
               Vector2.Distance(rectTransform.anchoredPosition, targetPosition) > 0.1f ||
               (image != null && ColorDistance(image.color, targetColor) > 0.01f))
        {
            float t = Time.unscaledDeltaTime * animationSpeed;
            rectTransform.localScale = Vector3.Lerp(rectTransform.localScale, targetScale, t);
            rectTransform.anchoredPosition = Vector2.Lerp(rectTransform.anchoredPosition, targetPosition, t);
            if (image != null) image.color = Color.Lerp(image.color, targetColor, t);
            yield return null;
        }

        rectTransform.localScale = targetScale;
        rectTransform.anchoredPosition = targetPosition;
        if (image != null) image.color = targetColor;
        animationRoutine = null;
    }

    private float ColorDistance(Color a, Color b)
    {
        return Mathf.Abs(a.r - b.r) + Mathf.Abs(a.g - b.g) + Mathf.Abs(a.b - b.b) + Mathf.Abs(a.a - b.a);
    }
}
