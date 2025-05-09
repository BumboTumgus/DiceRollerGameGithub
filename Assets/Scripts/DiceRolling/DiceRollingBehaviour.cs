using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class DiceRollingBehaviour : MonoBehaviour
{
    private const float RANDOM_ROLL_FORCE_MULTIPLIER = 10f;
    private const float RANDOM_ROLL_FORCE_FLOOR = 0.3f;
    private const float RANDOM_ROLL_TORQUE_MULTIPLIER = 1000000;
    private const float RANDOM_ROLL_TORQUE_FLOOR = 0.4f;
    private const string SPAWN_FROM_DORMANT_ANIM_STRING = "DiceAppearFromDormant";
    private const string DISSAPPEAR_TO_DORMANT_ANIM_STRING = "DiceDisappearToDormant";
    private const string IDLE_ANIM_STRING = "DiceIdleBeforeRoll_0";
    private const float DICE_FACE_MINIMUM_DOT_PRODUCT = 0.025f;
    private const float DICE_BOTTOM_OF_ROLLBOX_HEIGHT = -0.2f;
    private const int IDLE_ANIM_COUNT = 3;
    private const int MAXIMUM_REROLL_COUNT = 3;

    public bool SelectedForReroll { get { return _diceCurrentlySelected;}}
    public bool CurrentlyAllowsRolls { get { return _currentlyAllowsRolls;}}
    public DiceFaceBehaviour[] DiceFaces { get { return _diceFaces;}}
    public int DiceRerollCount { get => _diceRerollCount; set => _diceRerollCount = value; }
    public bool RemovedFromActiveCombat { get => _removedFromActiveCombat; }
    public bool TemporaryDie { get => _temporaryDie; set => _temporaryDie = value; }
    public bool DiceFinishedRolling { get => _diceFinishedRolling; }

    [SerializeField] private DiceFaceBehaviour[] _diceFaces;
    [SerializeField] private GameObject _diceSacrificeParticleFx;

    private Rigidbody _rigidbody;
    private Vector3 _startingScale;
    private Animation _animation;
    private DiceFaceBehaviour _rolledDiceFace;
    private bool _diceCurrentlySelected = false;
    private bool _currentlyAllowsRolls = true;
    private int _diceRerollCount = 0;
    private bool _removedFromActiveCombat = false;
    private bool _temporaryDie = false;
    private bool _diceFinishedRolling = false;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _animation = GetComponent<Animation>();
        _startingScale = transform.localScale;
    }

    public void OnSnapAndAppearFromDormant()
    {
        _rigidbody.isKinematic = true;
        _currentlyAllowsRolls = true;
        transform.rotation = Quaternion.identity;
        _animation.Play(SPAWN_FROM_DORMANT_ANIM_STRING);
    }

    public void OnDissappearToDormant(float delay)
    {
        Invoke(nameof(DissapearToDormant), delay);
    }

    public void OnRollDice(bool diceRerolled)
    {
        if(diceRerolled)
        {
            if(CombatManagerSingleton.Instance.PlayerCharacterCombatBehaviour.BuffManager.IsBuffActive(BuffScriptableObject.BuffType.RerollAttack))
                CombatManagerSingleton.Instance.PlayerCharacterCombatBehaviour.AddAttackDamage(CombatManagerSingleton.Instance.PlayerCharacterCombatBehaviour.BuffManager.GetBuffStackCount(BuffScriptableObject.BuffType.RerollAttack));
            if (CombatManagerSingleton.Instance.PlayerCharacterCombatBehaviour.BuffManager.IsBuffActive(BuffScriptableObject.BuffType.RerollDefense))
                CombatManagerSingleton.Instance.PlayerCharacterCombatBehaviour.AddDefense(CombatManagerSingleton.Instance.PlayerCharacterCombatBehaviour.BuffManager.GetBuffStackCount(BuffScriptableObject.BuffType.RerollDefense));
        }

        _animation.Stop();
        _rolledDiceFace = null;
        _diceFinishedRolling = false;
        transform.localScale = _startingScale;
        _rigidbody.isKinematic = false;
        _currentlyAllowsRolls = false;
        SetSelectionStatus(false);
        float upwardsForceMultiplier = transform.position.y < 0 ? 5 : 1;
        _rigidbody.AddForce(new Vector3(GetRandomFloatWithFloor(RANDOM_ROLL_FORCE_FLOOR) * RANDOM_ROLL_FORCE_MULTIPLIER, GetRandomFloatWithFloor(RANDOM_ROLL_FORCE_FLOOR) * RANDOM_ROLL_FORCE_MULTIPLIER, RANDOM_ROLL_FORCE_MULTIPLIER / -10 * upwardsForceMultiplier), ForceMode.Impulse);
        _rigidbody.AddTorque(new Vector3(GetRandomFloatWithFloor(RANDOM_ROLL_TORQUE_FLOOR) * RANDOM_ROLL_TORQUE_MULTIPLIER, GetRandomFloatWithFloor(RANDOM_ROLL_TORQUE_FLOOR) * RANDOM_ROLL_TORQUE_MULTIPLIER, GetRandomFloatWithFloor(RANDOM_ROLL_TORQUE_FLOOR) * RANDOM_ROLL_TORQUE_MULTIPLIER), ForceMode.Impulse);
        StartCoroutine(WaitForDiceToStopRolling());
    }

    public void OnPlayDiceIdleAnimation()
    {
        if (!_animation.isPlaying)
            _animation.Play(IDLE_ANIM_STRING + Random.Range(1, IDLE_ANIM_COUNT + 1));
    }

    public void OnMakeDiceDormant()
    {
        gameObject.SetActive(false);
    }

    public void SetSelectionStatus(bool diceSelected)
    {
        _diceCurrentlySelected = diceSelected;
        GetComponent<DiceHighlightingBehaviour>().SetSelectionStatus(_diceCurrentlySelected);
        _currentlyAllowsRolls = diceSelected;
    }

    public void RandomizeDiceFaces()
    {
        StartCoroutine(RandomizeDiceFacesAfterSingleFrameDelay());
    }

    private IEnumerator RandomizeDiceFacesAfterSingleFrameDelay()
    {
        yield return null;
        foreach (DiceFaceBehaviour diceFaceBehaviour in _diceFaces)
            diceFaceBehaviour.SwitchDiceFace(DiceFaceDataSingleton.Instance.GetRandomDiceFaceDataIdentifier());
    }

    private float GetRandomFloatWithFloor(float floor)
    {
        float randomFloat = Random.Range(-1f,1f);
        if(randomFloat < 0)
        {
            if(randomFloat > -floor)
                randomFloat = -floor;
        }
        else
        {
            if(randomFloat < floor)
                randomFloat = floor;
        }
        return randomFloat;
    }

    private IEnumerator WaitForDiceToStopRolling()
    {
        bool successfulRoll = false;
        int rollCount = 0;
        while (!successfulRoll)
        {
            yield return new WaitUntil(() => _rigidbody.IsSleeping());

            if (CheckForSingleValidUpwardFacingDiceFace() && transform.position.z > DICE_BOTTOM_OF_ROLLBOX_HEIGHT)
                successfulRoll = true;
            else
            { 
                if(rollCount >= 3)
                    transform.position = Vector3.zero;

                rollCount++;
                float upwardsForceMultiplier = transform.position.y < 0 ? 5 : 1;
                _rigidbody.AddForce(new Vector3(GetRandomFloatWithFloor(RANDOM_ROLL_FORCE_FLOOR) * RANDOM_ROLL_FORCE_MULTIPLIER, GetRandomFloatWithFloor(RANDOM_ROLL_FORCE_FLOOR) * RANDOM_ROLL_FORCE_MULTIPLIER, RANDOM_ROLL_FORCE_MULTIPLIER / -10 * upwardsForceMultiplier), ForceMode.Impulse);
                _rigidbody.AddTorque(new Vector3(GetRandomFloatWithFloor(RANDOM_ROLL_TORQUE_FLOOR) * RANDOM_ROLL_TORQUE_MULTIPLIER, GetRandomFloatWithFloor(RANDOM_ROLL_TORQUE_FLOOR) * RANDOM_ROLL_TORQUE_MULTIPLIER, GetRandomFloatWithFloor(RANDOM_ROLL_TORQUE_FLOOR) * RANDOM_ROLL_TORQUE_MULTIPLIER));
            }
        }
        _rolledDiceFace.PopupDiceFace(0.1f);
        _rigidbody.isKinematic = true;
        _diceFinishedRolling = true;

        yield return new WaitForSeconds(1f);

        if (DiceRollerSingleton.Instance.RerollsLockedByCurse)
            DiceRollerSingleton.Instance.SpawnRerollLockChains(transform);

        yield return new WaitForSeconds(1f);



        if (_rolledDiceFace.MyTempDiceFaceData == null && _rolledDiceFace.MyDiceFaceData == DiceFaceDataSingleton.Instance.GetDiceFaceDataByType(DiceFaceData.DiceFace.Ward))
        {
            Debug.Log("WARDED");
            CurseManagerSingleton.Instance.WardedFromCurses = true;
            DiceRollerSingleton.Instance.UnlockRerollsByCurse();
        }
        if (_rolledDiceFace.MyTempDiceFaceData != null && _rolledDiceFace.MyTempDiceFaceData == DiceFaceDataSingleton.Instance.GetDiceFaceDataByType(DiceFaceData.DiceFace.Lock) && !CurseManagerSingleton.Instance.WardedFromCurses)
        {
            Debug.Log("LOCKED");
            DiceRollerSingleton.Instance.LockRerollsByCurse();
        }

        if (_rolledDiceFace.MyTempDiceFaceData == null && _rolledDiceFace.MyDiceFaceData == DiceFaceDataSingleton.Instance.GetDiceFaceDataByType(DiceFaceData.DiceFace.Reroll) && _diceRerollCount < MAXIMUM_REROLL_COUNT && !DiceRollerSingleton.Instance.RerollsLockedByCurse)
        {
            _currentlyAllowsRolls = true;
            DiceRollerSingleton.Instance.AddDiceRerolls(1);
            DiceRollerSingleton.Instance.RackAndRerollSelectDice(this);
            _diceRerollCount++;
        }
        else
            DiceRollerSingleton.Instance.IncrementDiceFinishedRollingCount();
    }

    private bool CheckForSingleValidUpwardFacingDiceFace()
    {
        foreach (DiceFaceBehaviour diceFace in _diceFaces)
        {
            if (Vector3.Dot(diceFace.transform.forward, Vector3.forward) > 1 - DICE_FACE_MINIMUM_DOT_PRODUCT)
            {
                _rolledDiceFace = diceFace;
                return true;
            }
        }
        return false;
    }

    private void DissapearToDormant()
    {
        _animation.Play(DISSAPPEAR_TO_DORMANT_ANIM_STRING);
        GameObject spawnedParticle = null;
        if (_rolledDiceFace.MyTempDiceFaceData != null)
            if(!CurseManagerSingleton.Instance.WardedFromCurses && _rolledDiceFace.MyTempDiceFaceData == DiceFaceDataSingleton.Instance.GetDiceFaceDataByType(DiceFaceData.DiceFace.Sacrifice))
            {
                _removedFromActiveCombat = true;
                DiceRollerSingleton.Instance.DiesRemovedFromCombat++;
                Instantiate(_diceSacrificeParticleFx, transform.position, Quaternion.Euler(-90,0,0));
                return;
            }
            else
                spawnedParticle = Instantiate(_rolledDiceFace.MyTempDiceFaceData.PlayerPowerUpParticles, transform.position, Quaternion.identity);
        else
            spawnedParticle = Instantiate(_rolledDiceFace.MyDiceFaceData.PlayerPowerUpParticles, transform.position, Quaternion.identity);

        spawnedParticle.GetComponent<PlayerPowerUpParticleBehaviour>().InitializeBehaviour(CombatManagerSingleton.Instance.PlayerCharacterCombatBehaviour.transform);
        spawnedParticle.GetComponent<PlayerPowerUpParticleBehaviour>().OnParticleReachedDestination += CalculateRolledFaceBonus;
    }

    private void CalculateRolledFaceBonus()
    {
        if(_rolledDiceFace.MyTempDiceFaceData != null)
            DiceBonusCalculatorSingleton.Instance.CalculateBonusForRolledDiceFace(_rolledDiceFace.MyTempDiceFaceData.DiceFaceEnum);
        else
            DiceBonusCalculatorSingleton.Instance.CalculateBonusForRolledDiceFace(_rolledDiceFace.MyDiceFaceData.DiceFaceEnum);
    }

    public void ResetDieToBaseState()
    {
        if(TemporaryDie)
        {
            StartCoroutine(DestroyDieAfterFrameDelay());
            return;
        }

        _removedFromActiveCombat = false;

        foreach(DiceFaceBehaviour diceFace in _diceFaces)
        {
            if (diceFace.TempFaceRemovedAfterCombat)
                diceFace.RevertToOriginalDiceFace();
        }
    }

    private IEnumerator DestroyDieAfterFrameDelay()
    {
        yield return new WaitForEndOfFrame();
        DiceRollerSingleton.Instance.CurrentDice.Remove(this);
        Destroy(gameObject);
    }

    public void RemoveFromActiveCombat()
    {
        _removedFromActiveCombat = true;
    }
}
