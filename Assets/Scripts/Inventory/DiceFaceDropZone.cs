﻿using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using static UnityEditor.Progress;

public class DiceFaceDropZone : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
{
    private const string DICE_FACE_DRAGGABLE_NAME = "Ui_DiceFaceDraggable";
    private const string DICE_FACE_INVENTORY_SLOT_NAME = "Ui_DiceFaceInventorySlot";

    public enum DropZoneType { Inventory, DiceTemper, DiceForge, Discard }
    public DropZoneType slotType;
    public int SlotIndex;

    // Used when the pointer enters the dropzone, we check to see if there is an atatched drag item to it.
    public void OnPointerEnter(PointerEventData eventData)
    {
        // If we have an object connected and this isa new slot we are hovering over, start the stat checker.
        if (eventData.pointerDrag != null)
        {
            DiceFaceDraggable movedDiceFace = eventData.pointerDrag.GetComponent<DiceFaceDraggable>();
        }
    }

    // USed when a pointer exits the dropzone.
    public void OnPointerExit(PointerEventData eventData)
    {

    }

    // This is used to see if the item was dropped on an appropriate slot.
    public void OnDrop(PointerEventData eventData)
    {
        DiceFaceDraggable movedDiceFaceDraggable = eventData.pointerDrag.GetComponent<DiceFaceDraggable>();
        if (movedDiceFaceDraggable != null && movedDiceFaceDraggable.MyInventorySlotParent.parent != gameObject.transform)
        {
            // If we dropped an item on the dropitem type slot, return the item and wipe the slot then drop the item.
            if (slotType == DropZoneType.Discard)
            {
                movedDiceFaceDraggable.transform.SetParent(movedDiceFaceDraggable.MyInventorySlotParent);
                movedDiceFaceDraggable.transform.localPosition = Vector3.zero;
                movedDiceFaceDraggable.ParentToInteractWith = null;
                movedDiceFaceDraggable.GetComponent<CanvasGroup>().blocksRaycasts = true;

                PlayerInventorySingleton.Instance.RemoveDiceFaceAtIndex(movedDiceFaceDraggable.MyInventorySlotParent.parent.GetComponent<DiceFaceDropZone>().SlotIndex);
                return;    
            }

            // make the items switch their indexes (case only works for two items)
            if (slotType == DropZoneType.Inventory)
            {
                Transform myDiceFaceDraggable = transform.Find(DICE_FACE_INVENTORY_SLOT_NAME).Find(DICE_FACE_DRAGGABLE_NAME);
                PlayerInventorySingleton.Instance.SwapDiceFacesInventoryIndexes(SlotIndex, movedDiceFaceDraggable.MyInventorySlotParent.parent.GetComponent<DiceFaceDropZone>().SlotIndex);

                movedDiceFaceDraggable.ParentToInteractWith = myDiceFaceDraggable.parent;

                myDiceFaceDraggable.SetParent(movedDiceFaceDraggable.MyInventorySlotParent);
                myDiceFaceDraggable.localPosition = Vector3.zero;
            }

            // assign the dice face this slot is assigned to a new temporary face.
            if(slotType == DropZoneType.DiceTemper)
            {
                Debug.Log("DROPPED ON A DICE< DELETE THIS FACE AND ADD IT TO THE CONNECTED DICE FACE");

                // execute the logic of applying this face to the dice
                GetComponent<UiDiceFaceTemperExecutor>().UpdateDiceFaceAtIndex(SlotIndex, movedDiceFaceDraggable.AttachedDiceFaceData);

                movedDiceFaceDraggable.transform.SetParent(movedDiceFaceDraggable.MyInventorySlotParent);
                movedDiceFaceDraggable.transform.localPosition = Vector3.zero;
                movedDiceFaceDraggable.ParentToInteractWith = null;
                movedDiceFaceDraggable.GetComponent<CanvasGroup>().blocksRaycasts = true;

                PlayerInventorySingleton.Instance.RemoveDiceFaceAtIndex(movedDiceFaceDraggable.MyInventorySlotParent.parent.GetComponent<DiceFaceDropZone>().SlotIndex);
            }

            // add this diceface to the list of dice face to add onto our new forged dice if we confirm the forging.
            if (slotType == DropZoneType.DiceForge)
            {
                Debug.Log("DROPPED ON THE FORGE< LETS DO SOME SHIIII");
                GetComponent<UiDiceFaceForgeExecutor>().AddDiceFaceToForgeList(movedDiceFaceDraggable.AttachedDiceFaceData);

                movedDiceFaceDraggable.transform.SetParent(movedDiceFaceDraggable.MyInventorySlotParent);
                movedDiceFaceDraggable.transform.localPosition = Vector3.zero;
                movedDiceFaceDraggable.ParentToInteractWith = null;
                movedDiceFaceDraggable.GetComponent<CanvasGroup>().blocksRaycasts = true;

                PlayerInventorySingleton.Instance.RemoveDiceFaceAtIndex(movedDiceFaceDraggable.MyInventorySlotParent.parent.GetComponent<DiceFaceDropZone>().SlotIndex);
            }
        }
    }
}
