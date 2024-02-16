using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UiDiceRollSingleton : MonoBehaviour
{
    private const string DIVIDER_APPEAR_ANIM_CODE = "Ui_DividerCanvas_Appear";
    private const string DIVIDER_DISSAPPEAR_ANIM_CODE = "Ui_DividerCanvas_Disappear";

    public static UiDiceRollSingleton Instance;

    [SerializeField] private GameObject _devButton_Roll;
    [SerializeField] private GameObject _devButton_Reroll;
    [SerializeField] private GameObject _devButton_SkipReroll;
    [SerializeField] private GameObject _RerollCounter;
    [SerializeField] private Animation _dividerAnimation;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        if (Instance != this)
            Destroy(this);
    }

    public void OnSetDiceRollState(DiceRollerSingleton.DiceRollingState rollState)
    {
        switch(rollState)
        {
            case DiceRollerSingleton.DiceRollingState.Dormant:
            _devButton_Roll.SetActive(false);
            _devButton_Reroll.SetActive(false);
            _devButton_SkipReroll.SetActive(false);
            _RerollCounter.SetActive(false);
            _dividerAnimation.Play(DIVIDER_DISSAPPEAR_ANIM_CODE);
            break;

            case DiceRollerSingleton.DiceRollingState.ClickToRoll:
            _devButton_Roll.SetActive(true);
            _devButton_Reroll.SetActive(false);
            _devButton_SkipReroll.SetActive(false);
            _RerollCounter.SetActive(false);
            _dividerAnimation.Play(DIVIDER_APPEAR_ANIM_CODE);
            break;

            case DiceRollerSingleton.DiceRollingState.Rolling:
            _devButton_Roll.SetActive(false);
            _devButton_Reroll.SetActive(false);
            _devButton_SkipReroll.SetActive(false);
            _RerollCounter.SetActive(false);
            break;

            case DiceRollerSingleton.DiceRollingState.ReRollChoice:
            _devButton_Roll.SetActive(false);
            _devButton_Reroll.SetActive(false);
            _devButton_SkipReroll.SetActive(true);
            _RerollCounter.SetActive(true);
            break;
        }
    }

    public void OnSetPlayerActivelyRerolling(bool activelyRerolling)
    {
        _devButton_Reroll.SetActive(activelyRerolling);
    }
}
