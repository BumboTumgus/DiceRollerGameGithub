﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DiceFaceDraggable : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler
{
    [HideInInspector] public Transform ParentToInteractWith = null;
    [HideInInspector] public Transform MyParent = null;
    [HideInInspector] public DiceFaceData AttachedDiceFaceData;

    private InventoryPopupTextManager popupManager;

    private void Start()
    {
        popupManager = InventoryPopupTextManager.Instance;
    }

    // When we click and start dragging this dude around, set our parent and our parent to return to.
    public void OnBeginDrag(PointerEventData eventData)
    {
        Debug.Log("INVENTORY On Begin Drag");

        ParentToInteractWith = null;
        MyParent = transform.parent;

        transform.SetParent(transform.parent.parent.parent);
        transform.SetAsLastSibling();

        //popupManager.HidePopups(false);
        //popupManager.AllowPopupsToAppear = false;

        GetComponent<CanvasGroup>().blocksRaycasts = false;
    }

    // While moving around, set our position to the mouse so we follow it.
    public void OnDrag(PointerEventData eventData)
    {
        transform.position = eventData.position;
        //popupManager.LockPointer = true;
        //inventoryUiManager.HighlightSlotType(attachedItem.GetComponent<Item>().itemType);
    }

    // When we end, set us back to our original parent unless we were dropped on a valid slot.
    public void OnEndDrag(PointerEventData eventData)
    {
        Debug.Log("INVENTORY On End Drag");
        //popupManager.LockPointer = false;
        //popupManager.AllowPopupsToAppear = true;

        if (ParentToInteractWith != null)
        {
            transform.SetParent(ParentToInteractWith);
            //audioManager.PlayAudio(1);
        }
        else
        {
            transform.SetParent(MyParent);
            //popupManager.HidePopups(true);
            //audioManager.PlayAudio(4);
        }
        transform.localPosition = Vector3.zero;
        ParentToInteractWith = null;

        MyParent = transform.parent;
        GetComponent<CanvasGroup>().blocksRaycasts = true;

        //inventoryUiManager.HighlightHideAll();
    }

    // Used when the mouse hovers over this item.
    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log("INVENTORY Pointer Enter");
        //if(!popupManager.LockPointer)
        //{
        //    transform.parent.SetAsLastSibling();
        //    //popupManager.moreInfoPopUp.transform.SetAsLastSibling();
        //    //popupManager.ShowPopup(transform, transform.parent.GetComponent<DiceFaceDropZone>().PopUpDirection);
        //    //audioManager.PlayAudio(0);

        //    //inventoryUiManager.HighlightSlotType(attachedItem.GetComponent<Item>().itemType);
        //}
    }

    // Used when the mouse is no longer hovering over this item.
    public void OnPointerExit(PointerEventData eventData)
    {
        Debug.Log("INVENTORY Pointer Exit");
        //popupManager.HidePopups(false);
        //inventoryUiManager.HighlightHideAll();
    }
}