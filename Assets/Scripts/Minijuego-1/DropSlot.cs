using UnityEngine;
using UnityEngine.EventSystems;

public class DropSlot : MonoBehaviour, IDropHandler
{
    public enum SlotType
    {
        OrderSlot,   // One of the 1-5 slots
        SourcePanel  // The general Hechos panel
    }

    public SlotType tipoSlot;
    [Tooltip("El índice del slot (1 a 5). Solo relevante para OrderSlot.")]
    public int indiceSlot;

    public void OnDrop(PointerEventData eventData)
    {
        GameObject dropped = eventData.pointerDrag;
        if (dropped == null) return;

        DraggableCard draggableCard = dropped.GetComponent<DraggableCard>();
        if (draggableCard == null) return;

        if (tipoSlot == SlotType.OrderSlot)
        {
            // If this slot is already occupied, swap or return the current card to the source panel
            DraggableCard existingCard = GetComponentInChildren<DraggableCard>();
            if (existingCard != null && existingCard.gameObject != dropped)
            {
                // Return existing card to the source panel / original parent of the dragged card
                Transform draggedOriginalParent = draggableCard.parentAfterDrag;
                
                existingCard.parentAfterDrag = draggedOriginalParent;
                existingCard.transform.SetParent(draggedOriginalParent);
                existingCard.transform.localPosition = Vector3.zero;
            }

            draggableCard.parentAfterDrag = transform;
        }
        else if (tipoSlot == SlotType.SourcePanel)
        {
            // Just return it to the source panel
            draggableCard.parentAfterDrag = transform;
        }
    }
}
