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

    [SerializeField] private GameObject _combatRewardWindow;
    [SerializeField] private Button[] _dieFaceButtons;
    [SerializeField] private Button _takeAllButton;
    [SerializeField] private Button _discardButton;
    [SerializeField] private RectTransform _diceButtonContainer;
    [SerializeField] private TMP_Text _goldTextCount;

    private int _moneyEarned = 0;
    private List<DiceFaceData> _diceFacesEarned = new List<DiceFaceData>();
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
        }
        if(!ignoreAnimation)
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
        _moneyEarned = Random.Range((int) encounterRewards.GoldRange.x, (int)encounterRewards.GoldRange.y + 1);
        int _diceFaceCount = Random.Range((int) encounterRewards.DropCountRange.x, (int)encounterRewards.DropCountRange.y + 1);
        
        _diceFacesEarned.Clear();
        foreach(DiceFaceData garenteedDiceFace in encounterRewards.GarenteedDiceFaceDrops)
            _diceFacesEarned.Add(garenteedDiceFace);
        
        if(_diceFacesEarned.Count < _diceFaceCount)
        {
            int diceFacesToAdd = _diceFaceCount - _diceFacesEarned.Count;
            for(int index = 0; index < diceFacesToAdd; index++ )
            {
                DiceFaceData diceFaceToAdd = encounterRewards.PotentialDiceFaceDrops[Random.Range(0, encounterRewards.PotentialDiceFaceDrops.Length)];
                _diceFacesEarned.Add(diceFaceToAdd);
            }
        }

        _goldTextCount.text = "+" + _moneyEarned + " Gold";

        for(int index = 0; index < 3; index++)
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
        button.GetComponentInChildren<Image>().color = diceFaceData.DiceFaceUiColor;
        button.GetComponentInChildren<Image>().sprite = diceFaceData.DiceFaceUiSprite;
        // Subscribe an event that add this dice face to inventory then hides this button.
        // button.onClick += 
    }

    public void AddDiceFaceToInventoryAndHideButton(int diceFaceIndex)
    {
        Debug.Log("Add Dice Face of type: " + _diceFacesEarned[diceFaceIndex].DiceFaceEnum);
        PlayerInventorySingleton.Instance.AddDiceFaceToInventory(_diceFacesEarned[diceFaceIndex]);
    }

    public void AddAllLootToInventoryButton()
    {
        foreach(DiceFaceData diceFaceEarned in _diceFacesEarned)
        {
            Debug.Log("Add Dice Face of type: " + diceFaceEarned.DiceFaceEnum);
            PlayerInventorySingleton.Instance.AddDiceFaceToInventory(diceFaceEarned);
        }
        foreach(Button dieFaceButton in _dieFaceButtons)
        {
            dieFaceButton.gameObject.SetActive(false);  
        }
        SetWindowVisibility(false);
    }

    public void DiscardRemainingDiceFacesButton()
    {
        SetWindowVisibility(false);
    }

}
