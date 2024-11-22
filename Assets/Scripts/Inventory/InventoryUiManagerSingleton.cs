using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUiManagerSingleton : MonoBehaviour
{
    private const float UI_SLIDE_TARGET_HIDDEN = 650f;
    private const float UI_SLIDE_TARGET_BASE = 330f;
    private const float UI_SLIDE_TARGET_INCREMENT = 160f;
    private const float UI_SLIDE_SNAP_DISTANCE = 2f;
    private const float UI_SLIDE_SPEED = 0.1f;

    private const float UI_GARBAGE_SLIDE_TARGET_HIDDEN = 92f;
    private const float UI_GARBAGE_SLIDE_TARGET_SHOWN = -65f;

    public static InventoryUiManagerSingleton Instance;

    public DiceFaceInventorySlot[] DiceFaceDataInventorySlots;

    [SerializeField] private RectTransform _inventorySlideParent;
    [SerializeField] private RectTransform _garbageSlideParent;
    [SerializeField] private TMP_Text _goldReadout;

    private bool _inventoryOpened = false;
    private float _inventorySlideTargetDistance = 0;
    private float _inventoryCurrentSlideDistance = 0;
    private bool _garbageOpened = false;
    private float _garbageSlideTargetDistance = 0;
    private float _garbageCurrentSlideDistance = 0;
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
        DiceFaceDataInventorySlots = GetComponentsInChildren<DiceFaceInventorySlot>();

        foreach (DiceFaceInventorySlot slot in DiceFaceDataInventorySlots)
            slot.WipeSlot();

        SetInventoryOpenStatus(false);
        _inventorySlideParent.localPosition = new Vector2(UI_SLIDE_TARGET_HIDDEN, _inventorySlideParent.localPosition.y);
        _inventoryCurrentSlideDistance = UI_SLIDE_TARGET_HIDDEN;
        _inventorySlideTargetDistance = UI_SLIDE_TARGET_HIDDEN;

        SetGarbageOpenCloseStatus(false);
        _garbageSlideParent.localPosition = new Vector2(UI_GARBAGE_SLIDE_TARGET_HIDDEN, _garbageSlideParent.localPosition.y);
        _garbageCurrentSlideDistance = UI_GARBAGE_SLIDE_TARGET_HIDDEN;
        _garbageSlideTargetDistance = UI_GARBAGE_SLIDE_TARGET_HIDDEN;
    }

    private void Update()
    {
        MenuSlideToTarget();
        GarbageSlideToTarget();
    }

    public void UpdateInventorySlot(int InventoryIndex)
    {
        // If we have an item, set the picture to that associated with the item ans show thbe image.
        DiceFaceInventorySlot slotToUpdate = DiceFaceDataInventorySlots[InventoryIndex];
        slotToUpdate.UpdateSlot(PlayerInventorySingleton.Instance.CollectedDiceFaces[InventoryIndex]);
    }

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

    private void MenuSlideToTarget()
    {
        if (_inventoryCurrentSlideDistance == _inventorySlideTargetDistance)
            return;

        _inventoryCurrentSlideDistance = Mathf.Lerp(_inventoryCurrentSlideDistance, _inventorySlideTargetDistance, UI_SLIDE_SPEED);

        if (Mathf.Abs(_inventoryCurrentSlideDistance - _inventorySlideTargetDistance) < UI_SLIDE_SNAP_DISTANCE)
            _inventoryCurrentSlideDistance = _inventorySlideTargetDistance;

        _inventorySlideParent.localPosition = new Vector2(_inventoryCurrentSlideDistance, _inventorySlideParent.localPosition.y);
    }

    public void SetGarbageOpenCloseStatus(bool garbageOpened)
    {
        if (_garbageOpened == garbageOpened)
            return;

        _garbageOpened = garbageOpened;
        if (_garbageOpened)
            _garbageSlideTargetDistance = UI_GARBAGE_SLIDE_TARGET_SHOWN;
        else
            _garbageSlideTargetDistance = UI_GARBAGE_SLIDE_TARGET_HIDDEN;
    }

    private void GarbageSlideToTarget()
    {
        if (_garbageCurrentSlideDistance == _garbageSlideTargetDistance)
            return;

        _garbageCurrentSlideDistance = Mathf.Lerp(_garbageCurrentSlideDistance, _garbageSlideTargetDistance, UI_SLIDE_SPEED);

        if (Mathf.Abs(_garbageCurrentSlideDistance - _garbageSlideTargetDistance) < UI_SLIDE_SNAP_DISTANCE)
            _garbageCurrentSlideDistance = _garbageSlideTargetDistance;

        _garbageSlideParent.localPosition = new Vector2(_garbageCurrentSlideDistance, _garbageSlideParent.localPosition.y);
    }
    #endregion


    public void SetInventoryUiBasedOnMaxSpace(int inventorySpaceMax)
    {
        for (int inventorySlotIndex = inventorySpaceMax; inventorySlotIndex < DiceFaceDataInventorySlots.Length; inventorySlotIndex++)
        {
            DiceFaceDataInventorySlots[inventorySlotIndex].transform.parent.gameObject.SetActive(false);
        }

        if (inventorySpaceMax < 6)
            _inventoryColumnCount = 1;
        else if (inventorySpaceMax < 11)
            _inventoryColumnCount = 2;
        else if (inventorySpaceMax < 16)
            _inventoryColumnCount = 3;
    }

    public void SetGoldReadoutValue(int value)
    {
        _goldReadout.text = value.ToString();
    }
}
