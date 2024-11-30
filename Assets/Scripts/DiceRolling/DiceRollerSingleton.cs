using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class DiceRollerSingleton : MonoBehaviour
{
    private const float DICE_IDLE_ANIM_TIMER_INCREMENT = 1f;
    private const float DICE_IDLE_ANIM_CHANCE = 0.3f;
    private const float HIDE_TO_DORMANT_DELAY_INCREMENT = 0.2f;
    private const float PARTICLE_TO_STAT_ADDDITION_DELAY = 3f;

    public static DiceRollerSingleton Instance;
    public enum DiceRollingState { Dormant, ClickToRoll, Rolling, ReRollChoice };

    [SerializeField] private DiceRollingBehaviour[] _currentDice;
    [SerializeField] private LayerMask _diceLayerMask;

    private DiceRollingState _currentDiceRollState = DiceRollingState.Dormant;
    private float _currentDiceIdleAnimTimer = 0f;
    private int _diceFinishedRollingCount = 0;
    private int _rerollCount = 1;
    private Transform _currentHoveredDice;
    private bool _hasRerolledAlready = false;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        if (Instance != this)
            Destroy(this);

        //_currentDice = FindObjectsOfType<DiceRollingBehaviour>();
        SwitchToDiceState(DiceRollingState.Dormant);
        foreach(DiceRollingBehaviour die in _currentDice)
            die.gameObject.SetActive(false);
    }

    private void Update()
    {
        switch (_currentDiceRollState)
        {
            case DiceRollingState.Dormant:
                break;
            case DiceRollingState.ClickToRoll:
                if (_currentDiceIdleAnimTimer < Time.time)
                {
                    _currentDiceIdleAnimTimer = Time.time + DICE_IDLE_ANIM_TIMER_INCREMENT;
                    foreach(DiceRollingBehaviour diceRollingBehaviour in _currentDice)
                        if(UnityEngine.Random.Range(0f,1f) < DICE_IDLE_ANIM_CHANCE)
                            diceRollingBehaviour.OnPlayDiceIdleAnimation();
                }
                break;
            case DiceRollingState.Rolling:
                break;
            case DiceRollingState.ReRollChoice:
                HighlightHoveredDice();
                AddHoveredDiceToRerollSelectionOnClick();
                break;
        }
    }

    public void SwitchToDiceState(DiceRollingState newState)
    {
        if (newState == _currentDiceRollState)
            return;

        switch (newState)
        {
            case DiceRollingState.Dormant:
                float delay = 0f;
                foreach(DiceRollingBehaviour diceRollingBehaviour in _currentDice)
                {
                    diceRollingBehaviour.SetSelectionStatus(false);
                    diceRollingBehaviour.OnDissappearToDormant(delay);
                    delay += HIDE_TO_DORMANT_DELAY_INCREMENT;
                }
                CombatManagerSingleton.Instance.EnablePlayerControlOfCombat(delay + PARTICLE_TO_STAT_ADDDITION_DELAY);
                break;

            case DiceRollingState.ClickToRoll:

                List<Transform> diceToRollTransforms = new List<Transform>();
                foreach(DiceRollingBehaviour diceRollingTransform in _currentDice)
                    diceToRollTransforms.Add(diceRollingTransform.transform);
                DicePrerollPlacerSingleton.Instance.PlaceSelectedDiceInPattern(diceToRollTransforms);

                foreach(DiceRollingBehaviour diceRollingBehaviour in _currentDice)
                {
                    diceToRollTransforms.Add(diceRollingBehaviour.transform);
                    diceRollingBehaviour.gameObject.SetActive(true);
                    diceRollingBehaviour.OnSnapAndAppearFromDormant();
                }

                DiceBonusCalculatorSingleton.Instance.ResetRolledDiceBonuses();
                _rerollCount = _currentDice.Length / 2;
                UiDiceRerollSingleton.Instance.OnSetRerollCounter(_rerollCount);
                _diceFinishedRollingCount = 0;
                _hasRerolledAlready = false;
                
                UiDiceSummarySingleton.Instance.SetWindowVisibility(true);
                break;

            case DiceRollingState.Rolling:
                foreach(DiceRollingBehaviour diceRollingBehaviour in _currentDice)
                    if(diceRollingBehaviour.CurrentlyAllowsRolls)
                        diceRollingBehaviour.OnRollDice();
                
                UiDiceSummarySingleton.Instance.SetWindowVisibility(false);
                break;

            case DiceRollingState.ReRollChoice:
                _hasRerolledAlready = true;
                UiDiceSummarySingleton.Instance.SetWindowVisibility(true);
                break;
        }
        
        UiDiceRollSingleton.Instance.OnSetDiceRollState(newState);
        _currentDiceRollState = newState;
    }

    public void IncrementDiceFinishedRollingCount()
    {
        _diceFinishedRollingCount++;
        if (_diceFinishedRollingCount >= _currentDice.Length)
        {
            if (_rerollCount > 0 && !_hasRerolledAlready)
                SwitchToDiceState(DiceRollingState.ReRollChoice);
            else
                SwitchToDiceState(DiceRollingState.Dormant);
        }
    }

    public void OnSwitchToDiceRollState(int diceStateIndex)
    {
        SwitchToDiceState((DiceRollingState)diceStateIndex);
    }

    public void OnRackSelectedRerollDice()
    {
        List<Transform> diceToRollTransforms = new List<Transform>();
        foreach(DiceRollingBehaviour diceRollingTransform in _currentDice)
            if(diceRollingTransform.CurrentlyAllowsRolls)
                diceToRollTransforms.Add(diceRollingTransform.transform);

        DicePrerollPlacerSingleton.Instance.PlaceSelectedDiceInPattern(diceToRollTransforms);

        foreach(DiceRollingBehaviour die in _currentDice)
            die.GetComponent<DiceHighlightingBehaviour>().ClearStatus();

        SwitchToDiceState(DiceRollingState.Rolling);
    }

    private void HighlightHoveredDice()
    {
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit rayhit, 10f, _diceLayerMask))
        {
            if(_currentHoveredDice != rayhit.transform)
            {
                if(_currentHoveredDice != null)
                    _currentHoveredDice.GetComponent<DiceHighlightingBehaviour>().SetHighlightStatus(false);
                    
                _currentHoveredDice = rayhit.transform;
                _currentHoveredDice.GetComponent<DiceHighlightingBehaviour>().SetHighlightStatus(true);
            }
        }
        else if (_currentHoveredDice != null)
        {
            _currentHoveredDice.GetComponent<DiceHighlightingBehaviour>().SetHighlightStatus(false);
            _currentHoveredDice = null;
        }
    }

    private void AddHoveredDiceToRerollSelectionOnClick()
    {
        if(Input.GetMouseButtonDown(0) && _currentHoveredDice != null)
        {
            DiceRollingBehaviour diceRollingBehaviour = _currentHoveredDice.GetComponent<DiceRollingBehaviour>();

            if(!diceRollingBehaviour.SelectedForReroll && _rerollCount == 0)
            {
                UiDiceRerollSingleton.Instance.OnNotEnoughRerolls();
                return;
            }

            diceRollingBehaviour.SetSelectionStatus(!diceRollingBehaviour.SelectedForReroll);
            if(!diceRollingBehaviour.SelectedForReroll)
            {
                _diceFinishedRollingCount++;
                _rerollCount++;
            }
            else
            {
                _diceFinishedRollingCount--;
                _rerollCount--;
            }

            UiDiceRerollSingleton.Instance.OnSetRerollCounter(_rerollCount);
            UiDiceRollSingleton.Instance.OnSetPlayerActivelyRerolling(_diceFinishedRollingCount != _currentDice.Length);
        }
    }
}
