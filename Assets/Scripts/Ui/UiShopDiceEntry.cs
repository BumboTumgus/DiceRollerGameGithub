using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UiShopDiceEntry : MonoBehaviour
{
    [SerializeField] private DiceFaceViewerController _diceFaceViewer;
    //[SerializeField] private TMP_Text _dieCountText;
    [SerializeField] private TMP_Text _dieCostText;
    [SerializeField] private GameObject _saleIcon;
    [SerializeField] private GameObject _soldOutBanner;

    private CanvasGroup _canvasGroup;
    private DiceRollingBehaviour _diceDataConnectedToButton;
    private int _dieCount = 1;
    private int _dieCost = 1;

    private void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
    }

    public void SetDieReadoutToDiceRollingBehaviour(DiceRollingBehaviour diceData, int count, bool onSale)
    {
        Debug.LogFormat("dice data was chosen with a face lsit of {0}, with a count of {1} and a sale flag of {2}", diceData.DiceFaces, count, onSale);
        gameObject.SetActive(true);

        _diceDataConnectedToButton = diceData;
        _dieCount = count;

        _dieCost = 0;
        foreach(DiceFaceBehaviour diceFaceBehaviour in diceData.DiceFaces)
            _dieCost += diceFaceBehaviour.MyDiceFaceData.DiceFaceCost;

        // Load the dice into the viewer
        _diceFaceViewer.LoadDiceIntoViewer(diceData);

        //_dieCountText.text = "x" + count;

        if (onSale)
        {
            _saleIcon.SetActive(true);
            _dieCost /= 2;
        }
        else
            _saleIcon.SetActive(false);

        _dieCostText.text = _dieCost.ToString();

        _soldOutBanner.SetActive(false);
        _canvasGroup.interactable = true;
    }

    public void SetDieReadoutToDiceRollingBehaviourDelayed(DiceRollingBehaviour diceData, int count, bool onSale)
    {
        gameObject.SetActive(true);
        StartCoroutine(AllowDieToInitializeBeforeLoadingIntoViewer(diceData, count, onSale));
    }

    private IEnumerator AllowDieToInitializeBeforeLoadingIntoViewer(DiceRollingBehaviour diceData, int count, bool onSale)
    {
        yield return null;
        SetDieReadoutToDiceRollingBehaviour(diceData, count, onSale);
    }

    public void HideDieReadout()
    {
        Debug.Log("Hiding FAce REadout");
        gameObject.SetActive(false);
    }

    public void AttemptToBuyDiceFromDiceFaceReadout()
    {
        if (_dieCount <= 0)
            return;
        if (PlayerInventorySingleton.Instance.CollectedGold < _dieCost)
        {
            ShopSingleton.Instance.ShowNotEnoughMoneyPopup();
            return;
        }
        if (DiceRollerSingleton.Instance.DieArsenalAtCapacity())
        {
            ShopSingleton.Instance.ShowDiceLimitReachedPopup();
            return;
        }

        DiceRollerSingleton.Instance.AddDieToArsenal(_diceDataConnectedToButton);
        PlayerInventorySingleton.Instance.UpdateGoldValue(PlayerInventorySingleton.Instance.CollectedGold - _dieCost);
        _diceFaceViewer.UnloadDiceFromViewer();
        _dieCount--;

        //_dieCountText.text = "x" + _dieCount;

        if (_dieCount == 0)
        {
            _soldOutBanner.SetActive(true);
            _canvasGroup.interactable = false;
        }
    }
}
