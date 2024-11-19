using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using static UnityEditor.Progress;

public class DiceFaceDropZone : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
{
    public enum DropZoneType { Inventory, Dice, Discard }
    public DropZoneType slotType;
    public InventoryPopupTextManager.PopUpDirection PopUpDirection;
    public int SlotIndex;

    private InventoryPopupTextManager popupManager;

    private void Start()
    {
        popupManager = transform.parent.GetComponent<InventoryPopupTextManager>();
    }

    // Used when the pointer enters the dropzone, we check to see if there is an atatched drag item to it.
    public void OnPointerEnter(PointerEventData eventData)
    {
        // If we have an object connected and this isa new slot we are hovering over, start the stat checker.
        if (eventData.pointerDrag != null)
        {
            DiceFaceDraggable movedDiceFace = eventData.pointerDrag.GetComponent<DiceFaceDraggable>();

            //if (movedDiceFace.myParent != gameObject.transform)
            //{
            //    //TODO: THIS IS HARDCODED AND A NO GO
            //    Transform myPanel = transform.Find("ItemPanel");
            //    DiceFaceDraggable dropzoneDiceFace = myPanel.GetComponent<DiceFaceDraggable>();
            //}

            popupManager.ShowPopup(popupManager.itemPopUp.transform.parent, PopUpDirection);
        }
    }

    // USed when a pointer exits the dropzone.
    public void OnPointerExit(PointerEventData eventData)
    {

    }

    // This is used to see if the item was dropped on an appropriate slot.
    public void OnDrop(PointerEventData eventData)
    {
        //Debug.Log("item was dropped on " + gameObject.name);
         
        DiceFaceDraggable movedDiceFaceDraggable = eventData.pointerDrag.GetComponent<DiceFaceDraggable>();
        if(movedDiceFaceDraggable != null && movedDiceFaceDraggable.MyParent != gameObject.transform)
        {

            Transform myPanel = transform.Find("ItemPanel");
            DiceFaceDraggable dropZoneDiceFaceDraggable = null;

            if(slotType != DropZoneType.Discard)
                dropZoneDiceFaceDraggable = myPanel.GetComponent<DiceFaceDraggable>();


            // If we dropped an item on the dropitem type slot, return the item and wipe the slot then drop the item.
            if(slotType == DropZoneType.Discard && movedDiceFaceDraggable.MyParent.GetComponent<DiceFaceDropZone>().slotType == DropZoneType.Inventory)
            {
                Debug.Log("we should discard the diceface here");
                popupManager.LockPointer = false;
                movedDiceFaceDraggable.transform.SetParent(movedDiceFaceDraggable.MyParent);
                popupManager.HidePopups(true);
                popupManager.AllowPopupsToAppear = true;

                movedDiceFaceDraggable.transform.localPosition = Vector3.zero;
                movedDiceFaceDraggable.ParentToInteractWith = null;
                movedDiceFaceDraggable.MyParent = transform.parent;
                movedDiceFaceDraggable.GetComponent<CanvasGroup>().blocksRaycasts = true;

                //TODO: DELETE THE DICE FACE HERE
                //transform.parent.GetComponent<InventoryUiManager>().playerInventory.DropItem(movedDiceFace.attachedDiceFaceData.GetComponent<Item>().inventoryIndex);
            }

            // WE HAVE A VALID TARGET BEGIN SHIFTING IT OVER BELOW
            // make the items switch their indexes (case only works for two items)
            if (dropZoneDiceFaceDraggable.AttachedDiceFaceData != null)
            {
                int dropZoneItemIndex = dropZoneDiceFaceDraggable.AttachedDiceFaceData.InventoryIndex;
                dropZoneDiceFaceDraggable.AttachedDiceFaceData.InventoryIndex = movedDiceFaceDraggable.AttachedDiceFaceData.InventoryIndex;
                movedDiceFaceDraggable.AttachedDiceFaceData.InventoryIndex = dropZoneItemIndex;
            }
            // The logic for when there is no itemn on this panel when we slide an item over.
            else
                movedDiceFaceDraggable.AttachedDiceFaceData.InventoryIndex = SlotIndex;

            // Set the object at the target (here) to the parent of the object the player is moving.
            movedDiceFaceDraggable.ParentToInteractWith = myPanel.parent;

            myPanel.SetParent(movedDiceFaceDraggable.MyParent);
            myPanel.localPosition = Vector3.zero;

            popupManager.HidePopups(true);
        }
    }
}
