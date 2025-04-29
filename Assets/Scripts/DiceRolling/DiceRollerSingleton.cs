using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class DiceRollerSingleton : MonoBehaviour
{
    private const int MAXIMUM_DICE_CAPACITY = 15;
    private const float DICE_IDLE_ANIM_TIMER_INCREMENT = 1f;
    private const float DICE_IDLE_ANIM_CHANCE = 0.3f;
    private const float HIDE_TO_DORMANT_DELAY_INCREMENT = 0.2f;
    private const float PARTICLE_TO_STAT_ADDDITION_DELAY = 3f;
    private const float CHAIN_SPAWN_HEIGHT = -0.15f;

    public static DiceRollerSingleton Instance;
    public enum DiceRollingState { Dormant, ClickToRoll, Rolling, ReRollChoice };
    public List<DiceRollingBehaviour> CurrentDice { get => _currentDice; }
    public bool RerollsLockedByCurse { get => _rerollsLockedByCurse; }
    public int DiesRemovedFromCombat { get => _diesRemovedFromCombat; set => _diesRemovedFromCombat = value; }

    [SerializeField] private List<DiceRollingBehaviour> _currentDice;
    [SerializeField] private LayerMask _diceLayerMask;
    [SerializeField] private GameObject _rerollLockChainsPrefab;

    private DiceRollingState _currentDiceRollState = DiceRollingState.Dormant;
    private float _currentDiceIdleAnimTimer = 0f;
    private int _diceFinishedRollingCount = 0;
    private int _rerollCount = 1;
    private Transform _currentHoveredDice;
    private bool _hasRerolledAlready = false;
    private bool _rerollsLockedByCurse = false;
    private int _diesRemovedFromCombat = 0;

    private List<GameObject> _rerollLockChains = new List<GameObject>();


    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        if (Instance != this)
            Destroy(this);

        SwitchToDiceState(DiceRollingState.Dormant);
        foreach(DiceRollingBehaviour die in _currentDice)
            die.gameObject.transform.position = Vector3.one * 999;
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
                if (RerollsLockedByCurse)
                    break;
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
                ClearRerollLockChainFx();
                float delay = 0f;
                foreach(DiceRollingBehaviour diceRollingBehaviour in _currentDice)
                {
                    diceRollingBehaviour.SetSelectionStatus(false);
                    diceRollingBehaviour.OnDissappearToDormant(delay);
                    diceRollingBehaviour.DiceRerollCount = 0;
                    delay += HIDE_TO_DORMANT_DELAY_INCREMENT;
                }
                CombatManagerSingleton.Instance.EnablePlayerControlOfCombat(delay + PARTICLE_TO_STAT_ADDDITION_DELAY);
                break;

            case DiceRollingState.ClickToRoll:

                List<Transform> diceToRollTransforms = new List<Transform>();
                foreach(DiceRollingBehaviour diceRollingTransform in _currentDice)
                    if(!diceRollingTransform.RemovedFromActiveCombat)
                        diceToRollTransforms.Add(diceRollingTransform.transform);
                DicePrerollPlacerSingleton.Instance.PlaceSelectedDiceInPattern(diceToRollTransforms);

                foreach(DiceRollingBehaviour diceRollingBehaviour in _currentDice)
                {
                    if (diceRollingBehaviour.RemovedFromActiveCombat) continue;

                    diceToRollTransforms.Add(diceRollingBehaviour.transform);
                    diceRollingBehaviour.gameObject.SetActive(true);
                    diceRollingBehaviour.OnSnapAndAppearFromDormant();
                }

                DiceBonusCalculatorSingleton.Instance.ResetRolledDiceBonuses();
                _rerollCount = _currentDice.Count / 2;
                UiDiceRerollSingleton.Instance.OnSetRerollCounter(_rerollCount);
                _diceFinishedRollingCount = 0;
                _hasRerolledAlready = false;
                
                UiDiceSummarySingleton.Instance.SetWindowVisibility(true);
                break;

            case DiceRollingState.Rolling:
                foreach(DiceRollingBehaviour diceRollingBehaviour in _currentDice)
                    if(diceRollingBehaviour.CurrentlyAllowsRolls && !diceRollingBehaviour.RemovedFromActiveCombat)
                        diceRollingBehaviour.OnRollDice(diceRollingBehaviour.SelectedForReroll);
                
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
        if (_diceFinishedRollingCount >= _currentDice.Count - DiesRemovedFromCombat)
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
            if(diceRollingTransform.CurrentlyAllowsRolls && !diceRollingTransform.RemovedFromActiveCombat)
                diceToRollTransforms.Add(diceRollingTransform.transform);

        DicePrerollPlacerSingleton.Instance.PlaceSelectedDiceInPattern(diceToRollTransforms);

        foreach(DiceRollingBehaviour die in _currentDice)
            die.GetComponent<DiceHighlightingBehaviour>().ClearStatus();

        SwitchToDiceState(DiceRollingState.Rolling);
    }

    public void RackAndRerollSelectDice(DiceRollingBehaviour diceToForceReroll)
    {
        DicePrerollPlacerSingleton.Instance.PlaceSelectedDiceInPattern(new List<Transform> { diceToForceReroll.transform });
        diceToForceReroll.OnRollDice(true);
    }

    public void AddDieToArsenal(DiceRollingBehaviour dieToAdd)
    {
        _currentDice.Add(dieToAdd);
    }

    public bool DieArsenalAtCapacity()
    {
        return _currentDice.Count >= MAXIMUM_DICE_CAPACITY;
    }

    public bool DieArsenalContainsDie(DiceRollingBehaviour dieToCheck)
    {
        return _currentDice.Contains(dieToCheck);
    }

    public int DieFacesThatMatch(DiceFaceData diceFace)
    {
        int faceMatchCount = 0;

        foreach (DiceRollingBehaviour diceRollingBehaviour in _currentDice)
        {
            foreach (DiceFaceBehaviour diceFaceBehaviour in diceRollingBehaviour.DiceFaces)
            {
                if (diceFaceBehaviour.MyDiceFaceData.DiceFaceEnum == diceFace.DiceFaceEnum)
                    faceMatchCount++;
            }
        }

        return faceMatchCount;
    }

    public bool DiceContainsRequiredDiceFaces(List<DiceFaceData> diceFacesRequired)
    {
        if (diceFacesRequired == null || diceFacesRequired.Count == 0)
            return true;

        List<DiceFaceData> diceFacesUsedForComparison = new List<DiceFaceData>();
        bool diceFaceFoundInDie = false;

        foreach(DiceFaceData diceFaceRequired in diceFacesRequired)
        {
            foreach (DiceRollingBehaviour diceRollingBehaviour in _currentDice)
            {
                foreach (DiceFaceBehaviour diceFaceBehaviour in diceRollingBehaviour.DiceFaces)
                {
                    if (diceFaceBehaviour.MyDiceFaceData.DiceFaceEnum != diceFaceRequired.DiceFaceEnum)
                        continue;
                    if (diceFacesUsedForComparison.Contains(diceFaceBehaviour.MyDiceFaceData))
                        continue;

                    diceFacesUsedForComparison.Add(diceFaceBehaviour.MyDiceFaceData);
                    diceFaceFoundInDie = true;
                    break;
                }

                if (diceFaceFoundInDie)
                    break;
            }

            if (!diceFaceFoundInDie)
                return false;
            diceFaceFoundInDie = false;
        }

        return true;
    }

    public void PostCombatDiceCleanup()
    {
        foreach(DiceRollingBehaviour diceRollingBehaviour in _currentDice)
        {
            diceRollingBehaviour.ResetDieToBaseState();
        }
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
            UiDiceRollSingleton.Instance.OnSetPlayerActivelyRerolling(_diceFinishedRollingCount != _currentDice.Count);
        }
    }

    public void AddDiceRerolls(int v)
    {
        _rerollCount += v;
    }

    public void LockRerollsByCurse()
    {
        if (_rerollsLockedByCurse)
            return;

        _rerollsLockedByCurse = true;

        foreach (DiceRollingBehaviour diceBehaviour in CurrentDice)
            if (diceBehaviour.DiceFinishedRolling)
                SpawnRerollLockChains(diceBehaviour.transform);
    }

    public void UnlockRerollsByCurse()
    {
        if (!_rerollsLockedByCurse)
            return;

        _rerollsLockedByCurse = false;
        ClearRerollLockChainFx();
    }

    public void ClearRerollLockChainFx()
    {

        foreach (GameObject chainFx in _rerollLockChains)
        {
            var emissionModule = chainFx.GetComponent<ParticleSystem>().emission;
            emissionModule.rateOverTime = 0;
            Destroy(chainFx, 1f);
        }
        _rerollLockChains.Clear();
    }

    public void SpawnRerollLockChains(Transform targetTransform)
    {
        _rerollLockChains.Add(Instantiate(_rerollLockChainsPrefab, targetTransform.position + Vector3.forward * CHAIN_SPAWN_HEIGHT, Quaternion.Euler(0, 0, UnityEngine.Random.Range(0, 360))));
    }
}
