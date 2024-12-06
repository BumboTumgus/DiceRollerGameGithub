using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUiManagerSingleton : MonoBehaviour
{
    private const float UI_SLIDE_TARGET_BASE = 330f;
    private const float UI_SLIDE_TARGET_INCREMENT = 160f;

    public static InventoryUiManagerSingleton Instance;

    public DiceFaceInventorySlot[] DiceFaceDataInventorySlots;

    [SerializeField] private TMP_Text _goldReadout;
    [SerializeField] private UiSlidingPanelController _inventorySlidingPanelController;
    [SerializeField] private UiSlidingPanelController _garbageSlidingPanelController;

    private int _inventoryColumnCount = 3;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        if (Instance != this)
            Destroy(this);

        _inventorySlidingPanelController.OnPanelSuccessfullyClosed_Callback.AddListener(() => ShopSingleton.Instance.UiButtonPress_CloseShop());
        _inventorySlidingPanelController.OnPanelSuccessfullyClosed_Callback.AddListener(() => RestingSingleton.Instance.FinishedRestingNowProceedToMap());
    }

    private void Start()
    {
        DiceFaceDataInventorySlots = GetComponentsInChildren<DiceFaceInventorySlot>();

        foreach (DiceFaceInventorySlot slot in DiceFaceDataInventorySlots)
            slot.WipeSlot();
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
        _inventorySlidingPanelController.SetPanelOpenStatus(true);
    }

    public void UiButtonPress_CloseInventory()
    {
        _inventorySlidingPanelController.SetPanelOpenStatus(false);
    }

    public void SetGarbagePanelOpenStatus(bool open)
    {
        _garbageSlidingPanelController.SetPanelOpenStatus(open);
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

        _inventorySlidingPanelController.SlideTargetShown = UI_SLIDE_TARGET_BASE - (_inventoryColumnCount - 1) * UI_SLIDE_TARGET_INCREMENT;
    }

    public void SetGoldReadoutValue(int value)
    {
        _goldReadout.text = value.ToString();
    }
}
