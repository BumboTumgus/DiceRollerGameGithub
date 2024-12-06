using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RestingSingleton : MonoBehaviour
{
    private const string RESTING_SCREEN_APPEAR_ANIM_NAME = "RestingScreenOptions_Appear";
    private const string RESTING_SCREEN_DISAPPEAR_ANIM_NAME = "RestingScreenOptions_Disappear";

    public static RestingSingleton Instance;
    public List<DiceFaceData> DiceFacesUsed = new List<DiceFaceData>();

    [SerializeField] private UiAnimationPlayer _restingScreenOptionsAnimation;
    [SerializeField] private CanvasGroup _restingScreenOptionsCanvasGroup;
    [SerializeField] private UiSlidingPanelController _restModifyDiceSlidingPanelController;
    [SerializeField] private DiceFaceViewerController _diceFaceViewerController;

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

    public void ShowRestScreen()
    {
        _currentlyResting = true;
        DiceFacesUsed = new List<DiceFaceData>();
        _restingScreenChoiceShowing = true;
        _restingScreenOptionsAnimation.PlayAnimationByName(RESTING_SCREEN_APPEAR_ANIM_NAME);
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

        foreach(DiceRollingBehaviour diceRollingBehaviour in DiceRollerSingleton.Instance.CurrentDice)
        {
            foreach(DiceFaceBehaviour diceFaceBehaviour in diceRollingBehaviour.DiceFaces)
            {
                if (diceFaceBehaviour.MyTempDiceFaceData != null)
                    diceFaceBehaviour.RevertToOriginalDiceFace();
            }
        }

        _restingScreenOptionsAnimation.PlayAnimationByName(RESTING_SCREEN_APPEAR_ANIM_NAME);
        _restModifyDiceSlidingPanelController.SetPanelOpenStatus(false);
        InventoryUiManagerSingleton.Instance.UiButtonPress_CloseInventoryNoCallbacks();
    }

    public void UiButtonPress_ComfirmTemper()
    {
        DiceFacesUsed = new List<DiceFaceData>();

        foreach (DiceRollingBehaviour diceRollingBehaviour in DiceRollerSingleton.Instance.CurrentDice)
        {
            foreach (DiceFaceBehaviour diceFaceBehaviour in diceRollingBehaviour.DiceFaces)
            {
                if (diceFaceBehaviour.MyTempDiceFaceData != null)
                    diceFaceBehaviour.SetTempAsNewOriginal();
            }
        }

        _restModifyDiceSlidingPanelController.SetPanelOpenStatus(false);
        InventoryUiManagerSingleton.Instance.UiButtonPress_CloseInventory();
    }

    public void UiButtonPress_ForgeNewDie()
    {
    }

    public void UiButtonPress_RemoveCurses()
    {

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

        MapSingleton.Instance.SetMapShowStatus(true);
        MapSingleton.Instance.SetMapInteractibility(true);
    }
}
