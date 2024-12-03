using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UiShopDiceFaceEntry : MonoBehaviour
{
    [SerializeField] private Image _diceFaceImage;
    [SerializeField] private TMP_Text _diceFaceCountText;
    [SerializeField] private TMP_Text _diceFaceCostText;
    [SerializeField] private GameObject _saleIcon;
    [SerializeField] private GameObject _soldOutBanner;

    private CanvasGroup _canvasGroup;
    private DiceFaceData _diceFaceDataConnectedToButton;
    private int _diceFaceCount = 1;
    private int _diceFaceCost = 1;

    private void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
    }

    public void SetDiceFaceReadoutToDiceFaceData(DiceFaceData diceFaceData, int count, bool onSale)
    {
        Debug.LogFormat("dice face data was chosen of type {0}, with a count of {1} and a sale flag of {2}", diceFaceData.DiceFaceEnum, count, onSale);
        gameObject.SetActive(true);

        _diceFaceDataConnectedToButton = diceFaceData;
        _diceFaceCount = count;
        _diceFaceCost = diceFaceData.DiceFaceCost;

        _diceFaceImage.sprite = diceFaceData.DiceFaceUiSprite;
        _diceFaceImage.color = diceFaceData.DiceFaceUiColor;

        _diceFaceCountText.text = "x" + count;

        if(onSale) 
        { 
            _saleIcon.SetActive(true);
            _diceFaceCost /= 2;
        }
        else
            _saleIcon.SetActive(false);

        _diceFaceCostText.text = _diceFaceCost.ToString();

        _soldOutBanner.SetActive(false);
        _canvasGroup.interactable = true;
    }

    public void HideDiceFaceReadout()
    {
        Debug.Log("Hiding FAce REadout");
        gameObject.SetActive(false);
    }

    public void AttemptToBuyDiceFromDiceFaceReadout()
    {
        if (_diceFaceCount <= 0)
            return;
        if (PlayerInventorySingleton.Instance.CollectedGold < _diceFaceCost)
        {
            ShopSingleton.Instance.ShowNotEnoughMoneyPopup();
            return;
        }
        if (!PlayerInventorySingleton.Instance.RoomInInventory())
        {
            ShopSingleton.Instance.ShowNotEnoughInventorySpacePopup();
            return;
        }

        PlayerInventorySingleton.Instance.AddDiceFaceToInventory(_diceFaceDataConnectedToButton);
        PlayerInventorySingleton.Instance.UpdateGoldValue(PlayerInventorySingleton.Instance.CollectedGold - _diceFaceCost);
        _diceFaceCount--;

        _diceFaceCountText.text = "x" + _diceFaceCount;

        if (_diceFaceCount == 0) 
        {
            _soldOutBanner.SetActive(true);
            _canvasGroup.interactable = false;
        }
    }
}
