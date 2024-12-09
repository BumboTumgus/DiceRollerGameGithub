using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RestingSingleton : MonoBehaviour
{
    private const string RESTING_SCREEN_APPEAR_ANIM_NAME = "RestingScreenOptions_Appear";
    private const string RESTING_SCREEN_DISAPPEAR_ANIM_NAME = "RestingScreenOptions_Disappear";

    public static RestingSingleton Instance;
    public List<DiceFaceData> DiceFacesUsed = new List<DiceFaceData>();

    [SerializeField] private UiAnimationPlayer _restingScreenOptionsAnimation;
    [SerializeField] private TMP_Text _restingScreenHealAmountText;
    [SerializeField] private CanvasGroup _restingScreenOptionsCanvasGroup;
    [SerializeField] private UiSlidingPanelController _restModifyDiceSlidingPanelController;
    [SerializeField] private UiSlidingPanelController _restForgeDieSlidingPanelController;
    [SerializeField] private DiceFaceViewerController _diceFaceViewerController;
    [SerializeField] private UiDiceFaceForgeExecutor[] _diceFaceForgeExecutors;
    [SerializeField] private GameObject _basicDieToInstantiate;
    [SerializeField] private UiErrorPopup _errorPopup;

    private bool _currentlyResting = false;
    private bool _restingScreenChoiceShowing = false;
    private int _currentTemperDieIndex = 0;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        if (Instance != this)
            Destroy(this);

        _restingScreenOptionsCanvasGroup.interactable = false;
        _restingScreenOptionsCanvasGroup.blocksRaycasts = false;
        _restingScreenOptionsCanvasGroup.alpha = 0f;
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Y))
            ShowRestScreen();
    }

    public void ShowRestScreenWithDelay(float delay)
    {
        Invoke(nameof(ShowRestScreen), delay);
    }

    public void ShowRestScreen()
    {
        _currentlyResting = true;
        DiceFacesUsed.Clear();
        _restingScreenChoiceShowing = true;
        _restingScreenOptionsAnimation.PlayAnimationByName(RESTING_SCREEN_APPEAR_ANIM_NAME);
        _restingScreenHealAmountText.text = "Heal Health (+" + CombatManagerSingleton.Instance.PlayerCharacterCombatBehaviour.CurrentMissingHealth() + "HP)";
    }

    #region Ui Button Handlers
    public void UiButtonPress_HealAllHealth()
    {
        CombatManagerSingleton.Instance.PlayerCharacterCombatBehaviour.HealHealth(99999);
        FinishedRestingNowProceedToMap();
    }

    public void UiButtonPress_ModifyDice()
    {
        _restingScreenOptionsAnimation.PlayAnimationByName(RESTING_SCREEN_DISAPPEAR_ANIM_NAME);
        _restingScreenChoiceShowing = false;
        _restModifyDiceSlidingPanelController.SetPanelOpenStatus(true);
        InventoryUiManagerSingleton.Instance.UiButtonPress_OpenInventory();

        _currentTemperDieIndex = 0;
        _diceFaceViewerController.SetDiceViewerVisibleStatus(true);
        _diceFaceViewerController.LoadDiceIntoViewer(DiceRollerSingleton.Instance.CurrentDice[_currentTemperDieIndex]);
    }

    public void UiButtonPress_NextDieToTemper()
    {
        _currentTemperDieIndex++;
        if (_currentTemperDieIndex == DiceRollerSingleton.Instance.CurrentDice.Count)
            _currentTemperDieIndex = 0;
        _diceFaceViewerController.LoadDiceIntoViewer(DiceRollerSingleton.Instance.CurrentDice[_currentTemperDieIndex]);
    }

    public void UiButtonPress_PreviousDieToTemper()
    {
        _currentTemperDieIndex--;
        if (_currentTemperDieIndex == -1)
            _currentTemperDieIndex = DiceRollerSingleton.Instance.CurrentDice.Count - 1;
        _diceFaceViewerController.LoadDiceIntoViewer(DiceRollerSingleton.Instance.CurrentDice[_currentTemperDieIndex]);
    }

    public void UiButtonPress_CancelTemper()
    {
        foreach (DiceFaceData diceFaceDataUsed in DiceFacesUsed)
            PlayerInventorySingleton.Instance.AddDiceFaceToInventory(diceFaceDataUsed);
        DiceFacesUsed.Clear();

        foreach (DiceRollingBehaviour diceRollingBehaviour in DiceRollerSingleton.Instance.CurrentDice)
        {
            foreach(DiceFaceBehaviour diceFaceBehaviour in diceRollingBehaviour.DiceFaces)
            {
                if (diceFaceBehaviour.MyTempDiceFaceData != null)
                    diceFaceBehaviour.RevertToOriginalDiceFace();
            }
        }

        _restingScreenOptionsAnimation.PlayAnimationByName(RESTING_SCREEN_APPEAR_ANIM_NAME);
        _restingScreenChoiceShowing = true;
        _restModifyDiceSlidingPanelController.SetPanelOpenStatus(false);
        _diceFaceViewerController.SetDiceViewerVisibleStatus(false, 0.5f);
        InventoryUiManagerSingleton.Instance.UiButtonPress_CloseInventoryNoCallbacks();
    }

    public void UiButtonPress_ComfirmTemper()
    {
        foreach (DiceRollingBehaviour diceRollingBehaviour in DiceRollerSingleton.Instance.CurrentDice)
        {
            foreach (DiceFaceBehaviour diceFaceBehaviour in diceRollingBehaviour.DiceFaces)
            {
                if (diceFaceBehaviour.MyTempDiceFaceData != null)
                    diceFaceBehaviour.SetTempAsNewOriginal();
            }
        }

        _restModifyDiceSlidingPanelController.SetPanelOpenStatus(false);
        _diceFaceViewerController.SetDiceViewerVisibleStatus(false, 0.5f);
        InventoryUiManagerSingleton.Instance.UiButtonPress_CloseInventory();
        DiceFacesUsed.Clear();
    }

    public void UiButtonPress_ForgeNewDie()
    {
        if (PlayerInventorySingleton.Instance.CollectedGold < 100)
        {
            ShowErrorPopup("Not Enough Gold To Forge");
            return;
        }

        _restingScreenOptionsAnimation.PlayAnimationByName(RESTING_SCREEN_DISAPPEAR_ANIM_NAME);
        _restingScreenChoiceShowing = false;
        _restForgeDieSlidingPanelController.SetPanelOpenStatus(true);
        InventoryUiManagerSingleton.Instance.UiButtonPress_OpenInventory();

        foreach(UiDiceFaceForgeExecutor executor in _diceFaceForgeExecutors)
            executor.ResetDiceFaceForgeExecutor();
    }

    public void UiButtonPress_ComfirmForge()
    {
        if (DiceFacesUsed.Count != 6)
        {
            ShowErrorPopup("Not Enough Dice Faces Selected To Forge");
            return;
        }

        DiceRollingBehaviour dieCreated = Instantiate(_basicDieToInstantiate, Vector3.one * 999, Quaternion.identity).GetComponent<DiceRollingBehaviour>();
        for(int dieFaceIndex = 0; dieFaceIndex < 6; dieFaceIndex++)
        {
            dieCreated.DiceFaces[dieFaceIndex].SetStartingDieFace(DiceFacesUsed[dieFaceIndex].DiceFaceEnum.ToString());
        }
        DiceRollerSingleton.Instance.AddDieToArsenal(dieCreated);

        DiceFacesUsed.Clear();
        _restForgeDieSlidingPanelController.SetPanelOpenStatus(false);
        InventoryUiManagerSingleton.Instance.UiButtonPress_CloseInventory();
    }

    public void UiButtonPress_CancelForge()
    {
        foreach (DiceFaceData diceFaceDataUsed in DiceFacesUsed)
            PlayerInventorySingleton.Instance.AddDiceFaceToInventory(diceFaceDataUsed);
        DiceFacesUsed.Clear();

        _restingScreenOptionsAnimation.PlayAnimationByName(RESTING_SCREEN_APPEAR_ANIM_NAME);
        _restingScreenChoiceShowing = true;
        _restForgeDieSlidingPanelController.SetPanelOpenStatus(false);
        InventoryUiManagerSingleton.Instance.UiButtonPress_CloseInventoryNoCallbacks();
    }

    public void UiButtonPress_RemoveCurses()
    {
        FinishedRestingNowProceedToMap();
    }
    #endregion

    public void FinishedRestingNowProceedToMap()
    {
        if (!_currentlyResting)
            return;
        _currentlyResting = false;

        _diceFaceViewerController.UnloadDiceFromViewer();

        if (_restingScreenChoiceShowing)
            _restingScreenOptionsAnimation.PlayAnimationByName(RESTING_SCREEN_DISAPPEAR_ANIM_NAME);
        _restingScreenChoiceShowing = false;

        _restModifyDiceSlidingPanelController.SetPanelOpenStatus(false);
        _restForgeDieSlidingPanelController.SetPanelOpenStatus(false);

        MapSingleton.Instance.SetMapShowStatus(true);
        MapSingleton.Instance.SetMapInteractibility(true);
    }

    private void ShowErrorPopup(string text)
    {
        _errorPopup.SetText(text);
        _errorPopup.ShowWarning();
    }
}
