using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class DiceRollingBehaviour : MonoBehaviour
{
    private const float RANDOM_ROLL_FORCE_MULTIPLIER = 10f;
    private const float RANDOM_ROLL_FORCE_FLOOR = 0.3f;
    private const float RANDOM_ROLL_TORQUE_MULTIPLIER = 80f;
    private const float RANDOM_ROLL_TORQUE_FLOOR = 0.4f;
    private const string SPAWN_FROM_DORMANT_ANIM_STRING = "DiceAppearFromDormant";
    private const string DISSAPPEAR_TO_DORMANT_ANIM_STRING = "DiceDisappearToDormant";
    private const string IDLE_ANIM_STRING = "DiceIdleBeforeRoll_0";
    private const float DICE_FACE_MINIMUM_DOT_PRODUCT = 0.025f;
    private const float DICE_BOTTOM_OF_ROLLBOX_HEIGHT = -0.2f;
    private const int IDLE_ANIM_COUNT = 3;

    public bool SelectedForReroll { get { return _diceCurrentlySelected;}}
    public bool CurrentlyAllowsRolls { get { return _currentlyAllowsRolls;}}

    [SerializeField] private DiceFaceBehaviour[] _diceFaces;

    private Rigidbody _rigidbody;
    private Vector3 _startingScale;
    private Animation _animation;
    private DiceFaceBehaviour _rolledDiceFace;
    private bool _diceCurrentlySelected = false;
    private bool _currentlyAllowsRolls = true;

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

    public void OnRollDice()
    {
        _animation.Stop();
        _rolledDiceFace = null;
        transform.localScale = _startingScale;
        _rigidbody.isKinematic = false;
        _currentlyAllowsRolls = false;
        SetSelectionStatus(false);
        float upwardsForceMultiplier = transform.position.y < 0 ? 5 : 1;
        _rigidbody.AddForce(new Vector3(GetRandomFloatWithFloor(RANDOM_ROLL_FORCE_FLOOR) * RANDOM_ROLL_FORCE_MULTIPLIER, GetRandomFloatWithFloor(RANDOM_ROLL_FORCE_FLOOR) * RANDOM_ROLL_FORCE_MULTIPLIER, RANDOM_ROLL_FORCE_MULTIPLIER / -10 * upwardsForceMultiplier), ForceMode.Impulse);
        _rigidbody.AddTorque(new Vector3(GetRandomFloatWithFloor(RANDOM_ROLL_TORQUE_FLOOR) * RANDOM_ROLL_TORQUE_MULTIPLIER, GetRandomFloatWithFloor(RANDOM_ROLL_TORQUE_FLOOR) * RANDOM_ROLL_TORQUE_MULTIPLIER, GetRandomFloatWithFloor(RANDOM_ROLL_TORQUE_FLOOR) * RANDOM_ROLL_TORQUE_MULTIPLIER));
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

        yield return new WaitForSeconds(2f);
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
        GameObject spawnedParticle = Instantiate(_rolledDiceFace.MyDiceFaceData.PlayerPowerUpParticles, transform.position, Quaternion.identity);
        spawnedParticle.GetComponent<PlayerPowerUpParticleBehaviour>().InitializeBehaviour(CombatManagerSingleton.Instance.PlayerCharacterCombatBehaviour.transform);
        spawnedParticle.GetComponent<PlayerPowerUpParticleBehaviour>().OnParticleReachedDestination += CalculateRolledFaceBonus;
    }

    private void CalculateRolledFaceBonus()
    {
        DiceBonusCalculatorSingleton.Instance.CalculateBonusForRolledDiceFace(_rolledDiceFace.MyDiceFaceData.DiceFaceEnum);
    }
}