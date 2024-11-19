using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUiManager : MonoBehaviour
{
    public DiceFaceInventorySlot[] diceFaceDataInventorySlots;

    private void Start()
    {
        foreach (DiceFaceInventorySlot slot in diceFaceDataInventorySlots)
            slot.WipeSlot();
    }

    public void UpdateInventorySlot(DiceFaceData diceFaceData)
    {
        // If we have an item, set the picture to that associated with the item ans show thbe image.
        DiceFaceInventorySlot slotToUpdate = diceFaceDataInventorySlots[diceFaceData.InventoryIndex];
        UpdateSlot(slotToUpdate, diceFaceData);
    }

    private void UpdateSlot(DiceFaceInventorySlot slotToUpdate, DiceFaceData diceFaceData)
    {
        slotToUpdate.ConnectedDiceFaceDraggable.enabled = true;
        slotToUpdate.ConnectedDiceFaceDraggable.AttachedDiceFaceData = diceFaceData;
        slotToUpdate.SetImageToDiceFaceData(diceFaceData);
    }

    public DiceFaceDraggable GetNextEmptySlot()
    {
        // Return the next empty draggable item in the inventory.
        DiceFaceDraggable inventorySlot = null;
        foreach(DiceFaceInventorySlot slot in diceFaceDataInventorySlots)
        {
            // Check to see if the slot is null, just in case the item is unparented because it is being dragged around,
            if (slot.ConnectedDiceFaceDraggable == null)
            {
                inventorySlot = slot.ConnectedDiceFaceDraggable;
                break;
            }
        }

        return inventorySlot;
    }
}
