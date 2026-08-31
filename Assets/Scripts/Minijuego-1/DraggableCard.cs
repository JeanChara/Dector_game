using UnityEngine;
using UnityEngine.EventSystems;

public class DraggableCard : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [HideInInspector] public bool bloqueada = false;
    [HideInInspector] public Transform parentAfterDrag;
    private Canvas canvas;
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Vector3 originalPosition;
    private Transform originalParent;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (bloqueada)
        {
            eventData.pointerDrag = null; // Cancel drag
            return;
        }

        originalParent = transform.parent;
        originalPosition = transform.position;
        parentAfterDrag = transform.parent;
        
        // Parent to Canvas so it renders above other UI elements
        if (canvas != null)
        {
            transform.SetParent(canvas.transform);
        }
        
        // Make sure it doesn't block raycasts so slot OnDrop triggers
        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = 0.6f; // Visual feedback for dragging
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (canvas != null)
        {
            rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
        }
        else
        {
            rectTransform.position = eventData.position;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        transform.SetParent(parentAfterDrag);
        transform.localPosition = Vector3.zero; // Snap to slot/layout center!
        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1.0f;

        // If it didn't find a new parent and stayed on canvas root, return to original position
        if (canvas != null && transform.parent == canvas.transform)
        {
            transform.SetParent(originalParent);
            transform.localPosition = Vector3.zero;
        }
    }
}
