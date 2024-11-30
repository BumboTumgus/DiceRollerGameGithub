using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ShopSingleton : MonoBehaviour
{
    private const int DICE_FACE_MIN = 3;
    private const int DICE_FACE_MAX = 6;
    private const float DICE_EXTRA_FACE_CHANCE = 0.33f;
    private const float DICE_FACE_SALE_CHANCE = 0.1f;
    private const int HEAL_HEAL_AMOUNT = 20;
    private const int HEAL_COST = 10;

    public static ShopSingleton Instance;

    [SerializeField] UiErrorPopup _errorPopup;
    [SerializeField] DiceFaceData[] _diceFaceDataShopBank;
    [SerializeField] UiShopDiceFaceEntry[] _diceFaceEntryShopBank;
    private List<DiceFaceData> _diceFacesForSale = new List<DiceFaceData>();
    private DiceRollingBehaviour _dicesForSale;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        if (Instance != this)
            Destroy(this);
    }

    private void Start()
    {
        RollShopContents();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.L))
            RollShopContents();
    }

    public void RollShopContents()
    {
        int diceFacesCount = Random.Range(DICE_FACE_MIN, DICE_FACE_MAX + 1);
        List<DiceFaceData> rollableDiceFacesInShop = _diceFaceDataShopBank.OfType<DiceFaceData>().ToList();
        _diceFacesForSale.Clear();

        Debug.Log("ROLLING DICE FACES IN SHOP, our total count is " + diceFacesCount);
        for(int diceFaceIndex = 0; diceFaceIndex < diceFacesCount; diceFaceIndex++)
        {
            DiceFaceData diceFaceData = rollableDiceFacesInShop[Random.Range(0, rollableDiceFacesInShop.Count)];
            _diceFacesForSale.Add(diceFaceData);
            rollableDiceFacesInShop.Remove(diceFaceData);
            Debug.Log("We rolled " + diceFaceData.DiceFaceEnum);
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

        for(int DiceFaceEntryIndex = DICE_FACE_MAX - 1; DiceFaceEntryIndex > _diceFacesForSale.Count - 1; DiceFaceEntryIndex--)
        {
            _diceFaceEntryShopBank[DiceFaceEntryIndex].HideDiceFaceReadout();
        }
    }

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
}
