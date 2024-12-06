using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DiceFaceDraggable : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    [HideInInspector] public Transform ParentToInteractWith = null;
    [HideInInspector] public Transform MyInventorySlotParent = null;
    [HideInInspector] public DiceFaceData AttachedDiceFaceData;


    // When we click and start dragging this dude around, set our parent and our parent to return to.
    public void OnBeginDrag(PointerEventData eventData)
    {
        ParentToInteractWith = null;
        MyInventorySlotParent = transform.parent;

        transform.SetParent(transform.parent.parent.parent);
        transform.SetAsLastSibling();

        InventoryUiManagerSingleton.Instance.SetGarbagePanelOpenStatus(true);

        GetComponent<CanvasGroup>().blocksRaycasts = false;
    }

    // While moving around, set our position to the mouse so we follow it.
    public void OnDrag(PointerEventData eventData)
    {
        transform.position = eventData.position;
    }

    // When we end, set us back to our original parent unless we were dropped on a valid slot.
    public void OnEndDrag(PointerEventData eventData)
    {

        if (ParentToInteractWith != null)
        {
            transform.SetParent(ParentToInteractWith);
        }
        else
        {
            transform.SetParent(MyInventorySlotParent);
        }
        transform.localPosition = Vector3.zero;
        ParentToInteractWith = null;

        MyInventorySlotParent = transform.parent;
        GetComponent<CanvasGroup>().blocksRaycasts = true;

        InventoryUiManagerSingleton.Instance.SetGarbagePanelOpenStatus(false);
    }
}
