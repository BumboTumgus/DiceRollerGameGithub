using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUiManagerSingleton : MonoBehaviour
{
    private const float UI_SLIDE_TARGET_HIDDEN = 650f;
    private const float UI_SLIDE_TARGET_BASE = 330f;
    private const float UI_SLIDE_TARGET_INCREMENT = 165f;
    private const float UI_SLIDE_SNAP_DISTANCE = 2f;
    private const float UI_SLIDE_SPEED = 0.1f;

    public static InventoryUiManagerSingleton Instance;

    public DiceFaceInventorySlot[] DiceFaceDataInventorySlots;

    [SerializeField] private RectTransform _inventorySlideParent;

    private bool _inventoryOpened = false;
    private float _inventorySlideTargetDistance = 0;
    private float _inventoryCurrentSlideDistance = 0;
    private float _inventoryColumnCount = 3;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        if (Instance != this)
            Destroy(this);
    }

    private void Start()
    {
        foreach (DiceFaceInventorySlot slot in DiceFaceDataInventorySlots)
            slot.WipeSlot();

        SetInventoryOpenStatus(false);
        _inventorySlideParent.localPosition = new Vector2(UI_SLIDE_TARGET_HIDDEN, _inventorySlideParent.localPosition.y);
        _inventoryCurrentSlideDistance = UI_SLIDE_TARGET_HIDDEN;
        _inventorySlideTargetDistance = UI_SLIDE_TARGET_HIDDEN;
    }

    private void Update()
    {
        SlideToTarget();
    }

    public void UpdateInventorySlot(int InventoryIndex)
    {
        // If we have an item, set the picture to that associated with the item ans show thbe image.
        DiceFaceInventorySlot slotToUpdate = DiceFaceDataInventorySlots[InventoryIndex];
        UpdateSlot(slotToUpdate, PlayerInventorySingleton.Instance.CollectedDiceFaces[InventoryIndex]);
    }

    private void UpdateSlot(DiceFaceInventorySlot slotToUpdate, DiceFaceData diceFaceData)
    {
        slotToUpdate.ConnectedDiceFaceDraggable.enabled = true;
        slotToUpdate.ConnectedDiceFaceDraggable.AttachedDiceFaceData = diceFaceData;
        slotToUpdate.SetImageToDiceFaceData(diceFaceData);
    }

    //public DiceFaceDraggable GetNextEmptySlot()
    //{
    //    // Return the next empty draggable item in the inventory.
    //    DiceFaceDraggable inventorySlot = null;
    //    foreach(DiceFaceInventorySlot slot in diceFaceDataInventorySlots)
    //    {
    //        // Check to see if the slot is null, just in case the item is unparented because it is being dragged around,
    //        if (slot.ConnectedDiceFaceDraggable == null)
    //        {
    //            inventorySlot = slot.ConnectedDiceFaceDraggable;
    //            break;
    //        }
    //    }

    //    return inventorySlot;
    //}

    #region Open Close Menu Logic
    public void UiButtonPress_OpenInventory()
    {
        SetInventoryOpenStatus(true);
    }

    public void UiButtonPress_CloseInventory()
    {
        SetInventoryOpenStatus(false);
    }

    private void SetInventoryOpenStatus(bool inventoryOpened)
    {
        if (_inventoryOpened == inventoryOpened)
            return;

        _inventoryOpened = inventoryOpened;
        if (_inventoryOpened)
            _inventorySlideTargetDistance = UI_SLIDE_TARGET_BASE - (_inventoryColumnCount - 1) * UI_SLIDE_TARGET_INCREMENT;
        else
            _inventorySlideTargetDistance = UI_SLIDE_TARGET_HIDDEN;
    }

    private void SlideToTarget()
    {
        if (_inventoryCurrentSlideDistance == _inventorySlideTargetDistance)
            return;

        _inventoryCurrentSlideDistance = Mathf.Lerp(_inventoryCurrentSlideDistance, _inventorySlideTargetDistance, UI_SLIDE_SPEED);

        if (Mathf.Abs(_inventoryCurrentSlideDistance - _inventorySlideTargetDistance) < UI_SLIDE_SNAP_DISTANCE)
            _inventoryCurrentSlideDistance = _inventorySlideTargetDistance;

        _inventorySlideParent.localPosition = new Vector2(_inventoryCurrentSlideDistance, _inventorySlideParent.localPosition.y);
    }

    public void SetInventoryUiBasedOnMaxSpace(int InventorySpaceMax)
    {
        //TODO HIDE SLOTS AND SET COLUMNS BASED ON OUR MAX
        for(int inventorySlotIndex = 0; inventorySlotIndex < DiceFaceDataInventorySlots)
    }
    #endregion
}
