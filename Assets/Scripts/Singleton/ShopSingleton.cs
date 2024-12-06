using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class ShopSingleton : MonoBehaviour
{
    private const int DICE_FACE_MIN = 3;
    private const int DICE_FACE_MAX = 6;
    private const float DICE_EXTRA_FACE_CHANCE = 0.33f;
    private const float DICE_FACE_SALE_CHANCE = 0.1f;

    private const int DICE_MIN = 1;
    private const int DICE_MAX = 2;
    private const float DICE_SALE_CHANCE = 0.1f;
    private const float DICE_RANDOMIZE_FACES_CHANCE = 0.2f;

    private const int HEAL_HEAL_AMOUNT = 20;
    private const int HEAL_COST = 10;

    public static ShopSingleton Instance;

    [SerializeField] TMP_Text _healCostText;
    [SerializeField] TMP_Text _healAmountText;
    [SerializeField] UiSlidingPanelController _slidingPanelController;

    [SerializeField] UiErrorPopup _errorPopup;
    [SerializeField] DiceFaceData[] _diceFaceDataShopBank;
    [SerializeField] UiShopDiceFaceEntry[] _diceFaceEntryShopBank;
    private List<DiceFaceData> _diceFacesForSale = new List<DiceFaceData>();

    [SerializeField] GameObject[] _diceShopBank;
    [SerializeField] UiShopDiceEntry[] _diceEntryWithViewerShopBank;
    private List<DiceRollingBehaviour> _diceForSale = new List<DiceRollingBehaviour>();

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        if (Instance != this)
            Destroy(this);

        _healCostText.text = HEAL_COST.ToString();
        _healAmountText.text = "+" + HEAL_HEAL_AMOUNT + " HP";

        _slidingPanelController.OnPanelSuccessfullyClosed_Callback.AddListener(() => MapSingleton.Instance.UiShowMapWithDelay(1f));
        _slidingPanelController.OnPanelSuccessfullyClosed_Callback.AddListener(() => MapSingleton.Instance.SetMapInteractibility(true));
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {
            RollShopContents();
            UiButtonPress_OpenShop();
        }
    }

    public void RollShopContents()
    {
        // Creating Dice faces to sell
        int diceFacesCount = Random.Range(DICE_FACE_MIN, DICE_FACE_MAX + 1);
        List<DiceFaceData> rollableDiceFacesInShop = _diceFaceDataShopBank.OfType<DiceFaceData>().ToList();
        _diceFacesForSale.Clear();

        for(int diceFaceIndex = 0; diceFaceIndex < diceFacesCount; diceFaceIndex++)
        {
            DiceFaceData diceFaceData = rollableDiceFacesInShop[Random.Range(0, rollableDiceFacesInShop.Count)];
            _diceFacesForSale.Add(diceFaceData);
            rollableDiceFacesInShop.Remove(diceFaceData);
        }

        for(int diceFaceDataIndex = 0; diceFaceDataIndex < _diceFacesForSale.Count; diceFaceDataIndex++)
        {
            bool onSale = Random.Range(0f, 1f) <= DICE_FACE_SALE_CHANCE;

            int diceFaceCount = 1;
            bool attemptingAddingToCount = true;
            while(attemptingAddingToCount)
            {
                if (Random.Range(0f, 1f) <= DICE_EXTRA_FACE_CHANCE)
                    diceFaceCount++;
                else
                    attemptingAddingToCount = false;
            }

            _diceFaceEntryShopBank[diceFaceDataIndex].SetDiceFaceReadoutToDiceFaceData(_diceFacesForSale[diceFaceDataIndex], diceFaceCount, onSale);
        }

        for(int diceFaceEntryIndex = DICE_FACE_MAX - 1; diceFaceEntryIndex > _diceFacesForSale.Count - 1; diceFaceEntryIndex--)
        {
            _diceFaceEntryShopBank[diceFaceEntryIndex].HideDiceFaceReadout();
        }

        // Creating Dice to sell
        int diceCount = Random.Range(DICE_MIN, DICE_MAX + 1);
        _diceForSale.Clear();

        for (int diceIndex = 0; diceIndex < diceCount; diceIndex++)
        {
            DiceRollingBehaviour dieRollingData = Instantiate(_diceShopBank[Random.Range(0, _diceShopBank.Length)], Vector3.one * 999, Quaternion.identity).GetComponent<DiceRollingBehaviour>();
            _diceForSale.Add(dieRollingData);

            if (Random.Range(0f, 1f) < DICE_RANDOMIZE_FACES_CHANCE)
                dieRollingData.RandomizeDiceFaces();
        }

        for (int diceDataIndex = 0; diceDataIndex < _diceForSale.Count; diceDataIndex++)
        {
            bool onSale = Random.Range(0f, 1f) <= DICE_SALE_CHANCE;
            _diceEntryWithViewerShopBank[diceDataIndex].SetDieReadoutToDiceRollingBehaviourDelayed(_diceForSale[diceDataIndex], 1, onSale);
        }

        for (int dieEntryIndex = DICE_MAX - 1; dieEntryIndex > _diceForSale.Count - 1; dieEntryIndex--)
        {
            _diceEntryWithViewerShopBank[dieEntryIndex].HideDieReadout();
        }
    }

    #region Error Popups
    public void ShowNotEnoughMoneyPopup()
    {
        _errorPopup.SetText("Not Enough Gold");
        _errorPopup.ShowWarning();
    }

    public void ShowNotEnoughInventorySpacePopup()
    {
        _errorPopup.SetText("Not Enough Inventory Space");
        _errorPopup.ShowWarning();
    }

    public void ShowAlreadyFullHealthPopup()
    {
        _errorPopup.SetText("Already FUll Health");
        _errorPopup.ShowWarning();
    }

    public void ShowDiceLimitReachedPopup()
    {
        _errorPopup.SetText("Dice Maximum Reached");
        _errorPopup.ShowWarning();
    }
    #endregion


    #region Open Close Menu Logic
    public void UiButtonPress_OpenShop()
    {
        _slidingPanelController.SetPanelOpenStatus(true);
        InventoryUiManagerSingleton.Instance.UiButtonPress_OpenInventory();
    }

    public void UiButtonPress_CloseShop()
    {
        _slidingPanelController.SetPanelOpenStatus(false);
    }

    public void UiButtonPress_AttemptHealPlayer()
    {
        if(CombatManagerSingleton.Instance.PlayerCharacterCombatBehaviour.IsAtMaxHealth())
        {
            ShowAlreadyFullHealthPopup();
            return;
        }
        if(PlayerInventorySingleton.Instance.CollectedGold < HEAL_COST)
        {
            ShowNotEnoughMoneyPopup();
            return;
        }

        CombatManagerSingleton.Instance.PlayerCharacterCombatBehaviour.HealHealth(HEAL_HEAL_AMOUNT);
        PlayerInventorySingleton.Instance.UpdateGoldValue(PlayerInventorySingleton.Instance.CollectedGold - HEAL_COST);
    }

    public void OpenShopWithDelay(float delay)
    {
        Invoke(nameof(UiButtonPress_OpenShop), delay);
    }

    #endregion
}
