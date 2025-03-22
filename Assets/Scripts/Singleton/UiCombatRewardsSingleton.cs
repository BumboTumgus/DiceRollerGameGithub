using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UiCombatRewardsSingleton : MonoBehaviour
{
    public static UiCombatRewardsSingleton Instance;

    private const string WINDOW_APPEAR_ANIM_CODE = "Ui_RewardScreen_Appear";
    private const string WINDOW_DISSAPEAR_ANIM_CODE = "Ui_RewardScreen_Disappear";
    private const float INVENTORY_APPEAR_DELAY = 2f;

    [SerializeField] private GameObject _combatRewardWindow;
    [SerializeField] private Button[] _dieFaceButtons;
    [SerializeField] private Button _goldRewardButton;
    [SerializeField] private Button _takeAllButton;
    [SerializeField] private Button _discardButton;
    [SerializeField] private RectTransform _diceButtonContainer;
    [SerializeField] private TMP_Text _goldTextCount;

    private bool _goldTaken = false;
    private int _goldEarned = 0;
    private List<DiceFaceData> _diceFacesEarned = new List<DiceFaceData>();
    private bool[] _diceFacesTaken;
    private Animation _windowAnimation;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        if (Instance != this)
            Destroy(this);
        
        _windowAnimation = GetComponent<Animation>();
        SetWindowVisibility(false, true);
    }

    public void SetWindowVisibility(bool visibility, bool ignoreAnimation = false)
    {
        if(visibility)
        {
            _takeAllButton.gameObject.SetActive(true);
            _discardButton.gameObject.SetActive(true);
            InventoryUiManagerSingleton.Instance.UiShowInventoryWithDelay(INVENTORY_APPEAR_DELAY);
        }
        if (!ignoreAnimation)
        {
            _windowAnimation.Play(visibility ? WINDOW_APPEAR_ANIM_CODE : WINDOW_DISSAPEAR_ANIM_CODE);
            if (visibility)
                _combatRewardWindow.SetActive(visibility);
            else
                Invoke(nameof(HideWindow), 0.75f);
        }
        else
            _combatRewardWindow.SetActive(visibility);
    }

    public void PopulateWindowWithRewards(EncounterScriptableObject encounterRewards)
    {
        _goldEarned = Random.Range((int) encounterRewards.GoldRange.x, (int)encounterRewards.GoldRange.y + 1);
        int _diceFaceCount = Random.Range((int) encounterRewards.DropCountRange.x, (int)encounterRewards.DropCountRange.y + 1);

        _goldTaken = false;

        _diceFacesEarned.Clear();
        foreach(DiceFaceData garenteedDiceFace in encounterRewards.GarenteedDiceFaceDrops)
            _diceFacesEarned.Add(garenteedDiceFace);

        _diceFacesTaken = new bool[_diceFaceCount];
        for(int index = 0; index < _diceFacesTaken.Length; index++)
            _diceFacesTaken[index] = false;
        
        if(_diceFacesEarned.Count < _diceFaceCount)
        {
            int diceFacesToAdd = _diceFaceCount - _diceFacesEarned.Count;
            for(int index = 0; index < diceFacesToAdd; index++ )
            {
                DiceFaceData diceFaceToAdd = encounterRewards.PotentialDiceFaceDrops[Random.Range(0, encounterRewards.PotentialDiceFaceDrops.Length)];
                _diceFacesEarned.Add(diceFaceToAdd);
            }
        }

        _goldTextCount.text = _goldEarned + " Gold";

        for(int index = 0; index < _dieFaceButtons.Length; index++)
        {
            if(index < _diceFacesEarned.Count)
            {
                _dieFaceButtons[index].gameObject.SetActive(true);
                PopulateButtonWithData(_dieFaceButtons[index], _diceFacesEarned[index]);
            }
            else
            {
                _dieFaceButtons[index].gameObject.SetActive(false);
            }
        }
        
        _diceButtonContainer.sizeDelta = new Vector2(_diceButtonContainer.sizeDelta.x, _diceButtonContainer.GetComponent<VerticalLayoutGroup>().preferredHeight);
    }

    private void HideWindow()
    {
        _combatRewardWindow.SetActive(false);
    }

    private void PopulateButtonWithData(Button button, DiceFaceData diceFaceData)
    {
        button.GetComponentInChildren<TMP_Text>().text = diceFaceData.DiceFaceEnum.ToString();
        button.GetComponentInChildren<Image>().sprite = diceFaceData.DiceFaceUiSprite;
    }

    public void AddGoldToInventoryAndHideButton()
    {
        _goldTaken = true;
        PlayerInventorySingleton.Instance.UpdateGoldValue(PlayerInventorySingleton.Instance.CollectedGold + _goldEarned);
    }

    public void AddDiceFaceToInventoryAndHideButton(int diceFaceIndex)
    {
        Debug.Log("Add Dice Face of type: " + _diceFacesEarned[diceFaceIndex].DiceFaceEnum);
        PlayerInventorySingleton.Instance.AddDiceFaceToInventory(_diceFacesEarned[diceFaceIndex]);
        _diceFacesTaken[diceFaceIndex] = true;
    }

    public void AddAllLootToInventoryButton()
    {
        if (!_goldTaken)
            PlayerInventorySingleton.Instance.UpdateGoldValue(PlayerInventorySingleton.Instance.CollectedGold + _goldEarned);

        for(int diceFaceEarnedIndex = 0; diceFaceEarnedIndex < _diceFacesEarned.Count; diceFaceEarnedIndex++) 
        {
            if (_diceFacesTaken[diceFaceEarnedIndex])
                continue;
            PlayerInventorySingleton.Instance.AddDiceFaceToInventory(_diceFacesEarned[diceFaceEarnedIndex]);
        }
        foreach(Button dieFaceButton in _dieFaceButtons)
        {
            dieFaceButton.gameObject.SetActive(false);  
        }

        PlayerInventorySingleton.Instance.UpdateGoldValue(PlayerInventorySingleton.Instance.CollectedGold + _goldEarned);
        InventoryUiManagerSingleton.Instance.UiButtonPress_CloseInventory();
        MapSingleton.Instance.UiShowMapButtonPress();
        MapSingleton.Instance.SetMapInteractibility(true);
        SetWindowVisibility(false);
    }

    public void DiscardRemainingDiceFacesButton()
    {
        InventoryUiManagerSingleton.Instance.UiButtonPress_CloseInventory();
        MapSingleton.Instance.UiShowMapButtonPress();
        MapSingleton.Instance.SetMapInteractibility(true);
        SetWindowVisibility(false);
    }

}
