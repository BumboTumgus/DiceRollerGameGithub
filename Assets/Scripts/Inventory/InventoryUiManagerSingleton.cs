using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUiManagerSingleton : MonoBehaviour
{
    public static InventoryUiManagerSingleton Instance;

    public DiceFaceInventorySlot[] DiceFaceDataInventorySlots;

    [SerializeField] private TMP_Text _goldReadout;
    [SerializeField] private UiSlidingPanelController _inventorySlidingPanelController;

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

    public IEnumerator SetInventoryShowStatusDelayed(bool showStatus, float delay)
    {
        yield return new WaitForSeconds(delay);
        _inventorySlidingPanelController.SetPanelOpenStatus(showStatus);
    }
    public void UiButtonPress_OpenInventory()
    {
        _inventorySlidingPanelController.SetPanelOpenStatus(true);
    }
    public void UiHideInventoryWithDelay(float delayInSeconds)
    {
        StartCoroutine(SetInventoryShowStatusDelayed(false, delayInSeconds));
    }
    public void UiShowInventoryWithDelay(float delayInSeconds)
    {
        StartCoroutine(SetInventoryShowStatusDelayed(true, delayInSeconds));
    }

    public void UiButtonPress_CloseInventory()
    {
        _inventorySlidingPanelController.SetPanelOpenStatus(false);
    }
    public void UiButtonPress_CloseInventoryNoCallbacks()
    {
        _inventorySlidingPanelController.SetPanelOpenStatus(false, false);
    }


    #endregion


    public void SetInventoryUiBasedOnMaxSpace(int inventorySpaceMax)
    {
        for (int inventorySlotIndex = inventorySpaceMax; inventorySlotIndex < DiceFaceDataInventorySlots.Length; inventorySlotIndex++)
        {
            DiceFaceDataInventorySlots[inventorySlotIndex].transform.parent.gameObject.SetActive(false);
        }
    }

    public void SetGoldReadoutValue(int value)
    {
        _goldReadout.text = value.ToString();
    }
}
