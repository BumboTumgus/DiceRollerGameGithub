using RPGCharacterAnims.Actions;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class CombatManagerSingleton : MonoBehaviour
{
    private const float ATTACK_SLIDE_ANIM_LENGTH = 1.5f;
    private const float ACTION_SPEED_INCREASE_PERCENTAGE = 1.15f;
    private const float ATTACK_RETURN_TO_START_ANIM_LENGTH = 0.5f;
    private const float STUN_OSCILLATION_COUNT = 14;
    private const float STUN_OSCILLATION_DISTANCE = 0.2f;
    private const float STUN_TWEEN_DURATION = 1f;
    private const float ENEMY_SPAWN_DELAY = 0.25f;
    private const float END_OF_ACTION_DELAY = 1.5f;
    private const string UI_DIVIDER_APPEAR_ANIM = "Ui_DividerCanvas_AttackAppear";
    private const string UI_DIVIDER_DISAPPEAR_ANIM = "Ui_DividerCanvas_AttackDisappear";

    public static CombatManagerSingleton Instance;

    public TMP_Text temporaryStateText;
    public PlayerCharacterCombatBehaviour PlayerCharacterCombatBehaviour { get { return _playerCharacterCombatBehaviour;}}
    public List<EnemyCombatBehaviour> EnemyCombatBehaviours { get { return _enemyCombatBehaviours;}}
    public enum CombatState { PlayerPickingAttackTargets, PlayerExecutingAllActions, EnemyExecutingAttacks, Idle, NewCombatRound, PlayerPickingDebuffTargets }  

    [SerializeField] private PlayerCharacterCombatBehaviour _playerCharacterCombatBehaviour;
    [SerializeField] private List<EnemyCombatBehaviour> _enemyCombatBehaviours;
    [SerializeField] private LayerMask _enemyLayerMask;
    [SerializeField] private Transform _playerAttackStartPoint;
    [SerializeField] private Transform _playerAttackEndPoint;
    [SerializeField] private Transform _enemyAttackStartPoint;
    [SerializeField] private Transform _enemyAttackEndPoint;
    [SerializeField] private Animation _dividerCanvasAnimation;
    [SerializeField] private EncounterScriptableObject _currentEncounter;
    [SerializeField] private Transform[] _enemySpawns;

    private CombatState _currentCombatState = CombatState.Idle;
    private Transform _currentHoveredEnemy;
    private List<EnemyCombatBehaviour> _attackTargettedEnemies;
    private List<EnemyCombatBehaviour> _debuffTargettedEnemies;
    private List<BuffScriptableObject> _debuffsToInflict;
    private List<int> _debuffsToInflictCount;
    private float _currentActionSpeedMultiplier = 1f;

    private WaitForEndOfFrame _waitForEndOFrame = new WaitForEndOfFrame();

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        if (Instance != this)
            Destroy(this);
    }

    private void Update()
    {
        switch(_currentCombatState)
        {
            case CombatState.Idle:
                break;
            case CombatState.PlayerPickingAttackTargets:
                HighlightHoveredEnemy();
                AddHoveredEnemyAsAttackTarget();
                break;
            case CombatState.PlayerPickingDebuffTargets:
                HighlightHoveredEnemy();
                AddHoveredEnemyAsDebuffTarget();
                break;
            case CombatState.PlayerExecutingAllActions:
                break;
            case CombatState.EnemyExecutingAttacks:
                break;
        }
    }

    public void EnablePlayerControlOfCombat(float delayInSeconds)
    {
        Invoke(nameof(GivePlayerControlOfTurn), delayInSeconds);
    }

    public void StartCombat(EncounterScriptableObject encounter)
    {
        _currentEncounter = encounter;
        StartCoroutine(SetUpEncounter());
    }
        
    private IEnumerator SetUpEncounter()
    {
        for(int enemyPrefabIndex = 0; enemyPrefabIndex < _currentEncounter.EnemyPrefabs.Length; enemyPrefabIndex++)
        {
            _enemyCombatBehaviours.Add(Instantiate(_currentEncounter.EnemyPrefabs[enemyPrefabIndex], _enemySpawns[enemyPrefabIndex].position, _enemySpawns[enemyPrefabIndex].rotation).GetComponent<EnemyCombatBehaviour>());
            _enemyCombatBehaviours[enemyPrefabIndex].UiCombatStats.transform.rotation = Quaternion.Euler(0, 10 * (3 - enemyPrefabIndex), 0);
            yield return new WaitForSeconds(enemyPrefabIndex * ENEMY_SPAWN_DELAY);
        }
        SwapToCombatState(CombatState.Idle);
        yield return new WaitForSeconds(1f);
        DiceRollerSingleton.Instance.SwitchToDiceState(DiceRollerSingleton.DiceRollingState.ClickToRoll);
    }

    private void PayoutCombatRewards()
    {
        UiCombatRewardsSingleton.Instance.SetWindowVisibility(true);
        UiCombatRewardsSingleton.Instance.PopulateWindowWithRewards(_currentEncounter);
    }


    private void GivePlayerControlOfTurn()
    {
        if (PlayerCharacterCombatBehaviour.BuffManager.IsBuffActive(BuffScriptableObject.BuffType.Regen))
            PlayerCharacterCombatBehaviour.HealHealth(PlayerCharacterCombatBehaviour.BuffManager.GetBuffStackCount(BuffScriptableObject.BuffType.Regen));
        if (PlayerCharacterCombatBehaviour.BuffManager.IsBuffActive(BuffScriptableObject.BuffType.Tenacity))
            PlayerCharacterCombatBehaviour.AddDefense(PlayerCharacterCombatBehaviour.BuffManager.GetBuffStackCount(BuffScriptableObject.BuffType.Tenacity));

        if (PlayerCharacterCombatBehaviour.BuffManager.IsBuffActive(BuffScriptableObject.BuffType.Stun))
            StartCoroutine(PlayerStunnedRoutine());
        else
            SwapToCombatState(CombatState.PlayerPickingDebuffTargets);

        PlayerCharacterCombatBehaviour.BuffManager.DecrementAllBuffs();
    }

    private void SwapToCombatState(CombatState state)
    {
        _currentCombatState = state;
        Debug.Log("Swapped to combat state " + state);

        switch(state)
        {
            case CombatState.Idle:
                temporaryStateText.text = "COMBAT STATE: IDLE";
                PlayerCharacterCombatBehaviour.NewTurnStatInitialization();
                break;

            case CombatState.PlayerPickingAttackTargets:
                temporaryStateText.text = "COMBAT STATE: PICK ATTACK TARGETS";
                Debug.Log("CLEARED");
                _attackTargettedEnemies = new List<EnemyCombatBehaviour>();
                break;

            case CombatState.PlayerPickingDebuffTargets:
                temporaryStateText.text = "COMBAT STATE: PICK DEBUFF TARGETS";
                _debuffTargettedEnemies = new List<EnemyCombatBehaviour>();
                _debuffsToInflict = PlayerCharacterCombatBehaviour.DebuffInflictionManager.DebuffsToInflict;
                _debuffsToInflictCount = PlayerCharacterCombatBehaviour.DebuffInflictionManager.DebuffsToInflictCount;
                if (PlayerCharacterCombatBehaviour.DebuffInflictionManager.DebuffsToInflict.Count >= 1)
                {
                    UiCombatDebuffPickTargetTooltipSingleton.Instance.ShowTooltip(PlayerCharacterCombatBehaviour.DebuffInflictionManager.DebuffsToInflict[0], PlayerCharacterCombatBehaviour.DebuffInflictionManager.DebuffsToInflictCount[0]);
                }
                else
                {
                    SwapToCombatState(CombatState.PlayerPickingAttackTargets);
                    UiCombatDebuffPickTargetTooltipSingleton.Instance.HideTooltip();
                }
                break;

            case CombatState.PlayerExecutingAllActions:
                temporaryStateText.text = "COMBAT STATE: EXECUTING PLAYER ATTACK";
                foreach(EnemyCombatBehaviour enemyCombatBehaviour in _enemyCombatBehaviours)
                    enemyCombatBehaviour.GetComponent<PlayerToEnemyTargettingBehaviour>().SetHighlightStatus(false);
                _currentActionSpeedMultiplier = 1f;
                StartCoroutine(AllPlayerActionsRoutine());
                break;

            case CombatState.EnemyExecutingAttacks:
                temporaryStateText.text = "COMBAT STATE: EXECUTING ENEMY ATTACK"; 
                PlayerCharacterCombatBehaviour.DebuffInflictionManager.DebuffsToInflict.Clear();
                PlayerCharacterCombatBehaviour.DebuffInflictionManager.DebuffsToInflictCount.Clear();
                for (int enemyIndex = 0; enemyIndex < _enemyCombatBehaviours.Count; enemyIndex++)
                {
                    _enemyCombatBehaviours[enemyIndex].NewTurnStatInitialization();
                    if (!_enemyCombatBehaviours[enemyIndex].IsAlive)
                    {
                        _enemyCombatBehaviours[enemyIndex].CharacterDeath();
                        _enemyCombatBehaviours.Remove(_enemyCombatBehaviours[enemyIndex]);
                        enemyIndex--;
                    }
                }

                if (IsAnyEnemyIsAlive())
                    StartCoroutine(AllEnemyTurns());
                break;

            case CombatState.NewCombatRound:
                temporaryStateText.text = "COMBAT STATE: NEW COMBAT ROUND";
                foreach (EnemyCombatBehaviour enemy in _enemyCombatBehaviours)
                {
                    enemy.LoadNextAction();
                }
                SwapToCombatState(CombatState.Idle);
                break;
        }
    }

    private void HighlightHoveredEnemy()
    {
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit rayhit, 10f, _enemyLayerMask))
        {
            if(_currentHoveredEnemy != rayhit.transform)
            {
                if(_currentHoveredEnemy != null)
                    _currentHoveredEnemy.GetComponent<PlayerToEnemyTargettingBehaviour>().SetHighlightStatus(false);
                    
                _currentHoveredEnemy = rayhit.transform;
                _currentHoveredEnemy.GetComponent<PlayerToEnemyTargettingBehaviour>().SetHighlightStatus(true);
            }
        }
        else if (_currentHoveredEnemy != null)
        {
            _currentHoveredEnemy.GetComponent<PlayerToEnemyTargettingBehaviour>().SetHighlightStatus(false);
            _currentHoveredEnemy = null;
        }
    }

    private void AddHoveredEnemyAsAttackTarget()
    {
        if(Input.GetMouseButtonDown(0) && _currentHoveredEnemy != null)
        {
            EnemyCombatBehaviour enemyCombatBehaviour = _currentHoveredEnemy.GetComponent<EnemyCombatBehaviour>();
            _attackTargettedEnemies.Add(enemyCombatBehaviour);
            enemyCombatBehaviour.UiCombatStats.AddPlayerActionMarker();

            if (_attackTargettedEnemies.Count == PlayerCharacterCombatBehaviour.GetCurrentAttackCount())
                SwapToCombatState(CombatState.PlayerExecutingAllActions);
        }
    }

    private void AddHoveredEnemyAsDebuffTarget()
    {
        if (Input.GetMouseButtonDown(0) && _currentHoveredEnemy != null)
        {
            EnemyCombatBehaviour enemyCombatBehaviour = _currentHoveredEnemy.GetComponent<EnemyCombatBehaviour>();
            enemyCombatBehaviour.UiCombatStats.AddPlayerActionMarker(_debuffsToInflict[_debuffTargettedEnemies.Count]);
            _debuffTargettedEnemies.Add(enemyCombatBehaviour);

            if (_debuffTargettedEnemies.Count == PlayerCharacterCombatBehaviour.DebuffInflictionManager.DebuffsToInflict.Count)
            {
                SwapToCombatState(CombatState.PlayerPickingAttackTargets);
                UiCombatDebuffPickTargetTooltipSingleton.Instance.HideTooltip();
            }
            else
            {
                UiCombatDebuffPickTargetTooltipSingleton.Instance.ShowTooltip(PlayerCharacterCombatBehaviour.DebuffInflictionManager.DebuffsToInflict[_debuffTargettedEnemies.Count]
                , PlayerCharacterCombatBehaviour.DebuffInflictionManager.DebuffsToInflictCount[_debuffTargettedEnemies.Count]);
            }

        }
    }

    private IEnumerator AllPlayerActionsRoutine()
    {
        _dividerCanvasAnimation.Play(UI_DIVIDER_APPEAR_ANIM);

        Debug.LogFormat("our count for debuffs on ememies is {0} and our count for attacks targttted on enemies is {1} ", _debuffTargettedEnemies.Count, _attackTargettedEnemies.Count);
        for(int index = 0; index < _debuffTargettedEnemies.Count; index++)
        {
            yield return PlayerDebuffEnemyRoutine(_debuffTargettedEnemies[index], _debuffsToInflict[index], _debuffsToInflictCount[index]);
        }

        foreach (EnemyCombatBehaviour target in _attackTargettedEnemies)
        {
            yield return PlayerAttackRoutine(target);
        }
        Debug.Log("The enenmy attackl count is now " + _attackTargettedEnemies.Count);
        EnemyCombatBehaviour lastTarget = _attackTargettedEnemies[_attackTargettedEnemies.Count - 1];
        _playerCharacterCombatBehaviour.transform.position = _playerAttackEndPoint.position;
        lastTarget.transform.position = _enemyAttackEndPoint.position;
        float currentTimer = 0;
        _dividerCanvasAnimation.Play(UI_DIVIDER_DISAPPEAR_ANIM);

        while (currentTimer < ATTACK_RETURN_TO_START_ANIM_LENGTH)
        {
            currentTimer += Time.deltaTime;
            _playerCharacterCombatBehaviour.transform.position = Vector3.Lerp(_playerAttackEndPoint.position, _playerCharacterCombatBehaviour.OriginalPosition, currentTimer / ATTACK_RETURN_TO_START_ANIM_LENGTH);
            lastTarget.transform.position = Vector3.Lerp(_enemyAttackEndPoint.position, lastTarget.OriginalPosition, currentTimer / ATTACK_RETURN_TO_START_ANIM_LENGTH);
            yield return _waitForEndOFrame;
        }

        for (int enemyIndex = 0; enemyIndex < _enemyCombatBehaviours.Count; enemyIndex++)
        {
            if (!_enemyCombatBehaviours[enemyIndex].IsAlive)
            {
                _enemyCombatBehaviours[enemyIndex].CharacterDeath();
                _enemyCombatBehaviours.Remove(_enemyCombatBehaviours[enemyIndex]);
                enemyIndex--;
            }
        }

        yield return new WaitForSeconds(END_OF_ACTION_DELAY);

        if (IsAnyEnemyIsAlive())
            SwapToCombatState(CombatState.EnemyExecutingAttacks);
    }

    private IEnumerator PlayerStunnedRoutine()
    {
        float currentTimer = 0;
        int oscilllationCount = 0;
        Vector3 leftPos = _playerCharacterCombatBehaviour.OriginalPosition + Vector3.left * STUN_OSCILLATION_DISTANCE;
        Vector3 rightPos = _playerCharacterCombatBehaviour.OriginalPosition + Vector3.right * STUN_OSCILLATION_DISTANCE;
        Vector3 currentTarget;

        while (oscilllationCount < STUN_OSCILLATION_COUNT)
        {
            if (oscilllationCount % 2 > 0)
                currentTarget = leftPos;
            else
                currentTarget = rightPos;

            while (currentTimer < STUN_TWEEN_DURATION / STUN_OSCILLATION_COUNT)
            {
                currentTimer += Time.deltaTime;
                _playerCharacterCombatBehaviour.transform.position = Vector3.Lerp(_playerCharacterCombatBehaviour.transform.position, currentTarget, currentTimer / STUN_TWEEN_DURATION);
                yield return _waitForEndOFrame;
            }
            currentTimer = 0;
            oscilllationCount++;
        }

        _playerCharacterCombatBehaviour.transform.position = _playerCharacterCombatBehaviour.OriginalPosition;

        yield return new WaitForSeconds(END_OF_ACTION_DELAY);

        if (IsAnyEnemyIsAlive())
            SwapToCombatState(CombatState.EnemyExecutingAttacks);
    }

    private bool IsAnyEnemyIsAlive()
    {
        if (_enemyCombatBehaviours.Count <= 0)
        {
            SwapToCombatState(CombatState.Idle);
            PayoutCombatRewards();
            DiceRollerSingleton.Instance.PostCombatDiceCleanup();
        }

        return _enemyCombatBehaviours.Count > 0;
    }

    private IEnumerator PlayerAttackRoutine(EnemyCombatBehaviour target)
    {
        _playerCharacterCombatBehaviour.transform.position = _playerAttackStartPoint.position;
        target.transform.position = _enemyAttackStartPoint.position;
        float currentTimer = 0f;
        _currentActionSpeedMultiplier *= ACTION_SPEED_INCREASE_PERCENTAGE;
        target.UiCombatStats.RemovePlayerActionMarker();

        bool criticalStrike = _playerCharacterCombatBehaviour.IsAttackCritical();
        int damage = _playerCharacterCombatBehaviour.AttackDamageCurrent  + _playerCharacterCombatBehaviour.BuffManager.GetBuffStackCount(BuffScriptableObject.BuffType.Strength);
        if (_playerCharacterCombatBehaviour.BuffManager.IsBuffActive(BuffScriptableObject.BuffType.Weaken))
            damage /= 2;
        if (criticalStrike)
            damage *= 2;
        if (_playerCharacterCombatBehaviour.BuffManager.IsBuffActive(BuffScriptableObject.BuffType.Whisper))
            damage = 1;

        _playerCharacterCombatBehaviour.CombatAnimationBehaviour.PlayAttackAnimation();
        if(target.DefenseCurrent < damage)
            target.CombatAnimationBehaviour.PlayHitAnimation();
        else
            target.CombatAnimationBehaviour.PlayDefenseAnimation();
            
        DamageNumberManagerSingleton.Instance.ShowEnemyDamageNumber(damage, damage <= target.DefenseCurrent, criticalStrike);
        if(damage > target.DefenseCurrent && PlayerCharacterCombatBehaviour.BuffManager.IsBuffActive(BuffScriptableObject.BuffType.Vamperism))
            PlayerCharacterCombatBehaviour.HealHealth(PlayerCharacterCombatBehaviour.BuffManager.GetBuffStackCount(BuffScriptableObject.BuffType.Vamperism));

        target.TakeDamage(damage);
        while (currentTimer < ATTACK_SLIDE_ANIM_LENGTH / _currentActionSpeedMultiplier)
        {
            currentTimer += Time.deltaTime;
            _playerCharacterCombatBehaviour.transform.position = Vector3.Lerp(_playerAttackStartPoint.position, _playerAttackEndPoint.position, currentTimer / ATTACK_SLIDE_ANIM_LENGTH);
            target.transform.position = Vector3.Lerp(_enemyAttackStartPoint.position, _enemyAttackEndPoint.position, currentTimer / ATTACK_SLIDE_ANIM_LENGTH);
            yield return _waitForEndOFrame;
        }
        
        target.transform.position = target.OriginalPosition;
        _playerCharacterCombatBehaviour.CombatAnimationBehaviour.SetAnimSpeedToNormal();
        target.CombatAnimationBehaviour.SetAnimSpeedToNormal();
    }

    private IEnumerator PlayerDebuffEnemyRoutine(EnemyCombatBehaviour target, BuffScriptableObject debuff, int debuffCount)
    {
        _dividerCanvasAnimation.Play(UI_DIVIDER_APPEAR_ANIM);

        _playerCharacterCombatBehaviour.transform.position = _playerAttackStartPoint.position;
        target.transform.position = _enemyAttackStartPoint.position;
        float currentTimer = 0f;
        _currentActionSpeedMultiplier *= ACTION_SPEED_INCREASE_PERCENTAGE;
        target.UiCombatStats.RemovePlayerActionMarker(debuff);

        target.CombatAnimationBehaviour.PlayDebuffAnimation();
        _playerCharacterCombatBehaviour.CombatAnimationBehaviour.PlayAttackAnimation();

        DamageNumberManagerSingleton.Instance.ShowEnemyBuff(debuff.BuffIcon);

        target.BuffManager.AddBuff(debuff, debuffCount);
        while (currentTimer < ATTACK_SLIDE_ANIM_LENGTH / _currentActionSpeedMultiplier)
        {
            currentTimer += Time.deltaTime;
            _playerCharacterCombatBehaviour.transform.position = Vector3.Lerp(_playerAttackStartPoint.position, _playerAttackEndPoint.position, currentTimer / ATTACK_SLIDE_ANIM_LENGTH);
            target.transform.position = Vector3.Lerp(_enemyAttackStartPoint.position, _enemyAttackEndPoint.position, currentTimer / ATTACK_SLIDE_ANIM_LENGTH);
            yield return _waitForEndOFrame;
        }

        target.transform.position = target.OriginalPosition;
        _playerCharacterCombatBehaviour.CombatAnimationBehaviour.SetAnimSpeedToNormal();
        target.CombatAnimationBehaviour.SetAnimSpeedToNormal();

        //target.transform.position = _enemyAttackEndPoint.position;
        //_playerCharacterCombatBehaviour.CombatAnimationBehaviour.SetAnimSpeedToNormal();
        //target.CombatAnimationBehaviour.SetAnimSpeedToNormal();
        //_playerCharacterCombatBehaviour.transform.position = _playerAttackEndPoint.position;

        //currentTimer = 0;
        //_dividerCanvasAnimation.Play(UI_DIVIDER_DISAPPEAR_ANIM);

        //while (currentTimer < ATTACK_RETURN_TO_START_ANIM_LENGTH)
        //{
        //    currentTimer += Time.deltaTime;
        //    target.transform.position = Vector3.Lerp(_enemyAttackEndPoint.position, target.OriginalPosition, currentTimer / ATTACK_RETURN_TO_START_ANIM_LENGTH);
        //    _playerCharacterCombatBehaviour.transform.position = Vector3.Lerp(_playerAttackEndPoint.position, _playerCharacterCombatBehaviour.OriginalPosition, currentTimer / ATTACK_RETURN_TO_START_ANIM_LENGTH);

        //    yield return _waitForEndOFrame;
        //}

        //yield return new WaitForSeconds(END_OF_ACTION_DELAY / 3);

        //TODO MAKE THIS NOT REMOVE THE DEBUFF FROM THE LISTZ AND RETURN TO TARGETTING, ALL TARGETTING SHALL BE DOEN BEFORE HAND AND REMOVAL AFTER.
        // WE KNOW OUR CURRENT DEBUFF TO APPLY BASED ON HOPW MANY TARGETS ARE IN OUR LIST.
        //PlayerCharacterCombatBehaviour.DebuffInflictionManager.RemoveFirstDebuffFromList();
    }

    private IEnumerator AllEnemyTurns()
    {
        for(int enemyIndex = 0; enemyIndex < _enemyCombatBehaviours.Count; enemyIndex++)
        {
            if (!_enemyCombatBehaviours[enemyIndex].IsAlive)
                continue;

            _currentActionSpeedMultiplier = 1f;

            if (_enemyCombatBehaviours[enemyIndex].BuffManager.IsBuffActive(BuffScriptableObject.BuffType.Regen))
                _enemyCombatBehaviours[enemyIndex].HealHealth(_enemyCombatBehaviours[enemyIndex].BuffManager.GetBuffStackCount(BuffScriptableObject.BuffType.Regen));
            if (_enemyCombatBehaviours[enemyIndex].BuffManager.IsBuffActive(BuffScriptableObject.BuffType.Tenacity))
                _enemyCombatBehaviours[enemyIndex].AddDefense(_enemyCombatBehaviours[enemyIndex].BuffManager.GetBuffStackCount(BuffScriptableObject.BuffType.Tenacity));
            if (_enemyCombatBehaviours[enemyIndex].BuffManager.IsBuffActive(BuffScriptableObject.BuffType.Bleed))
                _enemyCombatBehaviours[enemyIndex].TakeDamage(_enemyCombatBehaviours[enemyIndex].BuffManager.GetBuffStackCount(BuffScriptableObject.BuffType.Bleed));

            _enemyCombatBehaviours[enemyIndex].UiCombatStats.TriggerTurnIndicator(true);

            yield return new WaitForSeconds(END_OF_ACTION_DELAY);
            if (_enemyCombatBehaviours[enemyIndex].IsAlive)
            {
                if (_enemyCombatBehaviours[enemyIndex].BuffManager.IsBuffActive(BuffScriptableObject.BuffType.Stun))
                    yield return StartCoroutine(EnemyStunnedRoutine(_enemyCombatBehaviours[enemyIndex]));
                else if (_enemyCombatBehaviours[enemyIndex].CurrentAttackSO.AttackTypeEnum == EnemyAttackScriptableObject.AttackType.Attack)
                    yield return StartCoroutine(AllEnemyAttackRoutines(_enemyCombatBehaviours[enemyIndex]));
                else if (_enemyCombatBehaviours[enemyIndex].CurrentAttackSO.AttackTypeEnum == EnemyAttackScriptableObject.AttackType.Defense)
                    yield return StartCoroutine(EnemyDefenseRoutine(_enemyCombatBehaviours[enemyIndex]));
                else if (_enemyCombatBehaviours[enemyIndex].CurrentAttackSO.DebuffToCastOnPlayer != null)
                    yield return StartCoroutine(EnemyPlayerDebuffRoutine(_enemyCombatBehaviours[enemyIndex]));
                else
                    yield return StartCoroutine(EnemySelfBuffRoutine(_enemyCombatBehaviours[enemyIndex]));
            }

            if (!_enemyCombatBehaviours[enemyIndex].IsAlive)
            {
                _enemyCombatBehaviours[enemyIndex].CharacterDeath();
                _enemyCombatBehaviours.Remove(_enemyCombatBehaviours[enemyIndex]);
                enemyIndex--;
            }
            else
                _enemyCombatBehaviours[enemyIndex].BuffManager.DecrementAllBuffs();
        }

        if (IsAnyEnemyIsAlive())
        {
            SwapToCombatState(CombatState.NewCombatRound);
            DiceRollerSingleton.Instance.SwitchToDiceState(DiceRollerSingleton.DiceRollingState.ClickToRoll);
        }
    }

    private IEnumerator AllEnemyAttackRoutines(EnemyCombatBehaviour enemy)
    {
        _dividerCanvasAnimation.Play(UI_DIVIDER_APPEAR_ANIM);

        for(int index = 0; index < enemy.CurrentAttackSO.AttackCount; index++)
            yield return EnemyAttackRoutine(enemy);

        _playerCharacterCombatBehaviour.transform.position = _playerAttackEndPoint.position;
        enemy.transform.position = _enemyAttackEndPoint.position;
        float currentTimer = 0;
        _dividerCanvasAnimation.Play(UI_DIVIDER_DISAPPEAR_ANIM);

        while(currentTimer < ATTACK_RETURN_TO_START_ANIM_LENGTH)
        {
            currentTimer += Time.deltaTime;
            _playerCharacterCombatBehaviour.transform.position = Vector3.Lerp(_playerAttackEndPoint.position, _playerCharacterCombatBehaviour.OriginalPosition, currentTimer / ATTACK_RETURN_TO_START_ANIM_LENGTH);
            enemy.transform.position = Vector3.Lerp(_enemyAttackEndPoint.position, enemy.OriginalPosition, currentTimer / ATTACK_RETURN_TO_START_ANIM_LENGTH);
            yield return _waitForEndOFrame;
        }

        enemy.UiCombatStats.TriggerTurnIndicator(false);
        enemy.HideAttackUi();
        yield return new WaitForSeconds(END_OF_ACTION_DELAY);
    }

    private IEnumerator EnemyDefenseRoutine(EnemyCombatBehaviour enemy)
    {
        _dividerCanvasAnimation.Play(UI_DIVIDER_APPEAR_ANIM);

        enemy.transform.position = _enemyAttackStartPoint.position;
        float currentTimer = 0f;

        enemy.CombatAnimationBehaviour.PlayDefenseAnimation();

        DamageNumberManagerSingleton.Instance.ShowEnemyBuff(enemy.CurrentAttackSO.AttackIcon);

        enemy.AddDefense(enemy.CurrentAttackSO.AttackDamage);
        while (currentTimer < ATTACK_SLIDE_ANIM_LENGTH)
        {
            currentTimer += Time.deltaTime;
            enemy.transform.position = Vector3.Lerp(_enemyAttackStartPoint.position, _enemyAttackEndPoint.position, currentTimer / ATTACK_SLIDE_ANIM_LENGTH);
            yield return _waitForEndOFrame;
        }

        enemy.CombatAnimationBehaviour.SetAnimSpeedToNormal();
        enemy.transform.position = _enemyAttackEndPoint.position;
         currentTimer = 0;
        _dividerCanvasAnimation.Play(UI_DIVIDER_DISAPPEAR_ANIM);

        while (currentTimer < ATTACK_RETURN_TO_START_ANIM_LENGTH)
        {
            currentTimer += Time.deltaTime;
            enemy.transform.position = Vector3.Lerp(_enemyAttackEndPoint.position, enemy.OriginalPosition, currentTimer / ATTACK_RETURN_TO_START_ANIM_LENGTH);
            yield return _waitForEndOFrame;
        }

        enemy.UiCombatStats.TriggerTurnIndicator(false);
        enemy.HideAttackUi();
        yield return new WaitForSeconds(END_OF_ACTION_DELAY);
    }

    private IEnumerator EnemySelfBuffRoutine(EnemyCombatBehaviour enemy)
    {
        _dividerCanvasAnimation.Play(UI_DIVIDER_APPEAR_ANIM);

        enemy.transform.position = _enemyAttackStartPoint.position;
        float currentTimer = 0f;

        enemy.CombatAnimationBehaviour.PlayBuffAnimation(enemy.CurrentAttackSO.AttackCount);

        DamageNumberManagerSingleton.Instance.ShowEnemyBuff(enemy.CurrentAttackSO.AttackIcon);

        enemy.BuffManager.AddBuff(enemy.CurrentAttackSO.BuffToCastOnSelf, enemy.CurrentAttackSO.BuffCount);
        while (currentTimer < ATTACK_SLIDE_ANIM_LENGTH)
        {
            currentTimer += Time.deltaTime;
            enemy.transform.position = Vector3.Lerp(_enemyAttackStartPoint.position, _enemyAttackEndPoint.position, currentTimer / ATTACK_SLIDE_ANIM_LENGTH);
            yield return _waitForEndOFrame;
        }

        enemy.CombatAnimationBehaviour.SetAnimSpeedToNormal();
        enemy.transform.position = _enemyAttackEndPoint.position;
        currentTimer = 0;
        _dividerCanvasAnimation.Play(UI_DIVIDER_DISAPPEAR_ANIM);

        while (currentTimer < ATTACK_RETURN_TO_START_ANIM_LENGTH)
        {
            currentTimer += Time.deltaTime;
            enemy.transform.position = Vector3.Lerp(_enemyAttackEndPoint.position, enemy.OriginalPosition, currentTimer / ATTACK_RETURN_TO_START_ANIM_LENGTH);
            yield return _waitForEndOFrame;
        }

        enemy.UiCombatStats.TriggerTurnIndicator(false);
        enemy.HideAttackUi();
        yield return new WaitForSeconds(END_OF_ACTION_DELAY);
    }

    private IEnumerator EnemyPlayerDebuffRoutine(EnemyCombatBehaviour aggresor)
    {
        _dividerCanvasAnimation.Play(UI_DIVIDER_APPEAR_ANIM);

        _playerCharacterCombatBehaviour.transform.position = _playerAttackStartPoint.position;
        aggresor.transform.position = _enemyAttackStartPoint.position;
        float currentTimer = 0f;

        aggresor.CombatAnimationBehaviour.PlayBuffAnimation(aggresor.CurrentAttackSO.AttackCount);
        _playerCharacterCombatBehaviour.CombatAnimationBehaviour.PlayDebuffAnimation();

        DamageNumberManagerSingleton.Instance.ShowPlayerBuff(aggresor.CurrentAttackSO.DebuffToCastOnPlayer.BuffIcon);

        _playerCharacterCombatBehaviour.BuffManager.AddBuff(aggresor.CurrentAttackSO.DebuffToCastOnPlayer, aggresor.CurrentAttackSO.BuffCount);
        while (currentTimer < ATTACK_SLIDE_ANIM_LENGTH)
        {
            currentTimer += Time.deltaTime;
            _playerCharacterCombatBehaviour.transform.position = Vector3.Lerp(_playerAttackStartPoint.position, _playerAttackEndPoint.position, currentTimer / ATTACK_SLIDE_ANIM_LENGTH);
            aggresor.transform.position = Vector3.Lerp(_enemyAttackStartPoint.position, _enemyAttackEndPoint.position, currentTimer / ATTACK_SLIDE_ANIM_LENGTH);
            yield return _waitForEndOFrame;
        }

        aggresor.transform.position = _enemyAttackEndPoint.position;
        _playerCharacterCombatBehaviour.CombatAnimationBehaviour.SetAnimSpeedToNormal();
        aggresor.CombatAnimationBehaviour.SetAnimSpeedToNormal();
        _playerCharacterCombatBehaviour.transform.position = _playerAttackEndPoint.position;

        currentTimer = 0;
        _dividerCanvasAnimation.Play(UI_DIVIDER_DISAPPEAR_ANIM);

        while (currentTimer < ATTACK_RETURN_TO_START_ANIM_LENGTH)
        {
            currentTimer += Time.deltaTime;
            aggresor.transform.position = Vector3.Lerp(_enemyAttackEndPoint.position, aggresor.OriginalPosition, currentTimer / ATTACK_RETURN_TO_START_ANIM_LENGTH);
            _playerCharacterCombatBehaviour.transform.position = Vector3.Lerp(_playerAttackEndPoint.position, _playerCharacterCombatBehaviour.OriginalPosition, currentTimer / ATTACK_RETURN_TO_START_ANIM_LENGTH);

            yield return _waitForEndOFrame;
        }

        aggresor.UiCombatStats.TriggerTurnIndicator(false);
        aggresor.HideAttackUi();
        yield return new WaitForSeconds(END_OF_ACTION_DELAY);
    }

    private IEnumerator EnemyAttackRoutine(EnemyCombatBehaviour aggresor)
    {
        _playerCharacterCombatBehaviour.transform.position = _playerAttackStartPoint.position;
        aggresor.transform.position = _enemyAttackStartPoint.position;
        float currentTimer = 0f;
        _currentActionSpeedMultiplier *= ACTION_SPEED_INCREASE_PERCENTAGE;
        bool attackEvaded = false;

        bool criticalStrike = aggresor.AttacksAreCritical;
        int damage = aggresor.CurrentAttackSO.AttackDamage + aggresor.BuffManager.GetBuffStackCount(BuffScriptableObject.BuffType.Strength);
        if (aggresor.BuffManager.IsBuffActive(BuffScriptableObject.BuffType.Weaken))
            damage /= 2;
        if(criticalStrike)
            damage *= 2;

        aggresor.CombatAnimationBehaviour.PlayAttackAnimation();
        if (_playerCharacterCombatBehaviour.DefenseCurrent >= damage)
            _playerCharacterCombatBehaviour.CombatAnimationBehaviour.PlayDefenseAnimation();
        else if (_playerCharacterCombatBehaviour.BuffManager.IsBuffActive(BuffScriptableObject.BuffType.Evade))
        {
            _playerCharacterCombatBehaviour.CombatAnimationBehaviour.PlayDefenseAnimation();
            _playerCharacterCombatBehaviour.BuffManager.DecrementBuff(BuffScriptableObject.BuffType.Evade);
            damage = 0;
            attackEvaded = true;
        }
        else
            _playerCharacterCombatBehaviour.CombatAnimationBehaviour.PlayHitAnimation();
        
        if (!attackEvaded)
            DamageNumberManagerSingleton.Instance.ShowPlayerDamageNumber(damage, damage <= _playerCharacterCombatBehaviour.DefenseCurrent, criticalStrike);

        _playerCharacterCombatBehaviour.TakeDamage(damage);

        if(_playerCharacterCombatBehaviour.BuffManager.IsBuffActive(BuffScriptableObject.BuffType.Thorns))
        {
            int thornDamage = _playerCharacterCombatBehaviour.BuffManager.GetBuffStackCount(BuffScriptableObject.BuffType.Thorns);
            aggresor.TakeDamage(thornDamage);
            DamageNumberManagerSingleton.Instance.ShowEnemyDamageNumber(thornDamage, thornDamage <= aggresor.DefenseCurrent, false);
        }

        while (currentTimer < ATTACK_SLIDE_ANIM_LENGTH / _currentActionSpeedMultiplier)
        {
            currentTimer += Time.deltaTime;
            _playerCharacterCombatBehaviour.transform.position = Vector3.Lerp(_playerAttackStartPoint.position, _playerAttackEndPoint.position, currentTimer / ATTACK_SLIDE_ANIM_LENGTH);
            aggresor.transform.position = Vector3.Lerp(_enemyAttackStartPoint.position, _enemyAttackEndPoint.position, currentTimer / ATTACK_SLIDE_ANIM_LENGTH);
            yield return _waitForEndOFrame;
        }

        aggresor.transform.position = aggresor.OriginalPosition;
        _playerCharacterCombatBehaviour.CombatAnimationBehaviour.SetAnimSpeedToNormal();
        aggresor.CombatAnimationBehaviour.SetAnimSpeedToNormal();
    }

    private IEnumerator EnemyStunnedRoutine(EnemyCombatBehaviour stunnedUnit)
    {
        float currentTimer = 0;
        int oscilllationCount = 0;
        Vector3 leftPos = stunnedUnit.OriginalPosition + Vector3.left * STUN_OSCILLATION_DISTANCE;
        Vector3 rightPos = stunnedUnit.OriginalPosition + Vector3.right * STUN_OSCILLATION_DISTANCE;
        Vector3 currentTarget;

        while (oscilllationCount < STUN_OSCILLATION_COUNT)
        {
            if (oscilllationCount % 2 > 0)
                currentTarget = leftPos;
            else
                currentTarget = rightPos;

            while (currentTimer < STUN_TWEEN_DURATION / STUN_OSCILLATION_COUNT)
            {
                currentTimer += Time.deltaTime;
                stunnedUnit.transform.position = Vector3.Lerp(stunnedUnit.transform.position, currentTarget, currentTimer / STUN_TWEEN_DURATION);
                yield return _waitForEndOFrame;
            }
            currentTimer = 0;
            oscilllationCount++;
        }

        stunnedUnit.transform.position = stunnedUnit.OriginalPosition;
        stunnedUnit.UiCombatStats.TriggerTurnIndicator(false);
        stunnedUnit.HideAttackUi();
        yield return new WaitForSeconds(END_OF_ACTION_DELAY);
    }
}
