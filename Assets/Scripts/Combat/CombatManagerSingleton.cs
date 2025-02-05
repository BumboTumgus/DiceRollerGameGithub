using RPGCharacterAnims.Actions;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class CombatManagerSingleton : MonoBehaviour
{
    private const float ATTACK_SLIDE_ANIM_LENGTH = 1.5f;
    private const float ATTACK_RETURN_TO_START_ANIM_LENGTH = 0.5f;
    private const float ENEMY_SPAWN_DELAY = 0.25f;
    private const float END_OF_ACTION_DELAY = 1.5f;
    private const string UI_DIVIDER_APPEAR_ANIM = "Ui_DividerCanvas_AttackAppear";
    private const string UI_DIVIDER_DISAPPEAR_ANIM = "Ui_DividerCanvas_AttackDisappear";

    public static CombatManagerSingleton Instance;

    public TMP_Text temporaryStateText;
    public PlayerCharacterCombatBehaviour PlayerCharacterCombatBehaviour { get { return _playerCharacterCombatBehaviour;}}
    public List<EnemyCombatBehaviour> EnemyCombatBehaviours { get { return _enemyCombatBehaviours;}}
    public enum CombatState { PlayerPickingTargets, PlayerExecutingAttacks, EnemyExecutingAttacks, Idle, NewCombatRound }  

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
    private List<EnemyCombatBehaviour> _targettedEnemies;
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
            case CombatState.PlayerPickingTargets:
                HighlightHoveredEnemy();
                AddHoveredEnemyAsAttackTarget();
                break;
            case CombatState.PlayerExecutingAttacks:
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
        Debug.Log("Payout combat rewards here and show the continue button");
        UiCombatRewardsSingleton.Instance.SetWindowVisibility(true);
        UiCombatRewardsSingleton.Instance.PopulateWindowWithRewards(_currentEncounter);
        //TODO: add a ui single call - show the ui for the rewards and populate the buttons
        // We get this rewards from our current chopsen encoutner.
    }

    private void GivePlayerControlOfTurn()
    {
        SwapToCombatState(CombatState.PlayerPickingTargets);
    }

    private void SwapToCombatState(CombatState state)
    {
        _currentCombatState = state;

        switch(state)
        {
            case CombatState.Idle:
                temporaryStateText.text = "COMBAT STATE: IDLE";
                break;

            case CombatState.PlayerPickingTargets:
                temporaryStateText.text = "COMBAT STATE: PICK TARGETS";
                _targettedEnemies = new List<EnemyCombatBehaviour>();
                break;

            case CombatState.PlayerExecutingAttacks:
                temporaryStateText.text = "COMBAT STATE: EXECUTING PLAYER ATTACK";
                foreach(EnemyCombatBehaviour enemyCombatBehaviour in _enemyCombatBehaviours)
                    enemyCombatBehaviour.GetComponent<PlayerToEnemyTargettingBehaviour>().SetHighlightStatus(false);
                    StartCoroutine(AllPlayerAttackRoutines());
                break;

            case CombatState.EnemyExecutingAttacks:
                temporaryStateText.text = "COMBAT STATE: EXECUTING ENEMY ATTACK";
                foreach (EnemyCombatBehaviour enemy in _enemyCombatBehaviours)
                {
                    enemy.NewTurnStatInitialization();
                }
                StartCoroutine(AllEnemyTurns());
                break;

            case CombatState.NewCombatRound:
                temporaryStateText.text = "COMBAT STATE: NEW COMBAT ROUND";
                foreach (EnemyCombatBehaviour enemy in _enemyCombatBehaviours)
                {
                    enemy.LoadNextAttack();
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
            _targettedEnemies.Add(enemyCombatBehaviour);
            enemyCombatBehaviour.UiCombatStats.AddPlayerAttackMarker();

            if (_targettedEnemies.Count == PlayerCharacterCombatBehaviour.AttackCountCurrent)
                SwapToCombatState(CombatState.PlayerExecutingAttacks);
        }
    }

    private IEnumerator AllPlayerAttackRoutines()
    {
        _dividerCanvasAnimation.Play(UI_DIVIDER_APPEAR_ANIM);


        foreach(EnemyCombatBehaviour target in _targettedEnemies)
        {
            yield return PlayerAttackRoutine(target);
        }

        EnemyCombatBehaviour lastTarget = _targettedEnemies[_targettedEnemies.Count - 1];
        _playerCharacterCombatBehaviour.transform.position = _playerAttackEndPoint.position;
        lastTarget.transform.position = _enemyAttackEndPoint.position;
        float currentTimer = 0;
        _dividerCanvasAnimation.Play(UI_DIVIDER_DISAPPEAR_ANIM);

        while(currentTimer < ATTACK_RETURN_TO_START_ANIM_LENGTH)
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

        if(_enemyCombatBehaviours.Count > 0)
            SwapToCombatState(CombatState.EnemyExecutingAttacks);
        else
        {
            SwapToCombatState(CombatState.Idle);
            PayoutCombatRewards();
        }
    }

    private IEnumerator PlayerAttackRoutine(EnemyCombatBehaviour target)
    {
        _playerCharacterCombatBehaviour.transform.position = _playerAttackStartPoint.position;
        target.transform.position = _enemyAttackStartPoint.position;
        float currentTimer = 0f;
        target.UiCombatStats.RemovePlayerAttackMarker();

        bool criticalStrike = _playerCharacterCombatBehaviour.IsAttackCritical();
        int damage = _playerCharacterCombatBehaviour.AttackDamageCurrent;
        if(criticalStrike)
            damage *= 2;

        _playerCharacterCombatBehaviour.CombatAnimationBehaviour.PlayAttackAnimation();
        if(target.DefenseCurrent < damage)
            target.CombatAnimationBehaviour.PlayHitAnimation();
        else
            target.CombatAnimationBehaviour.PlayDefenseAnimation();
            
        DamageNumberManagerSingleton.Instance.ShowEnemyDamageNumber(damage, damage <= target.DefenseCurrent, criticalStrike);
        if(damage > target.DefenseCurrent && PlayerCharacterCombatBehaviour.VamperismCurrent > 0)
            PlayerCharacterCombatBehaviour.HealHealth(PlayerCharacterCombatBehaviour.VamperismCurrent);

        target.TakeDamage(damage);
        while (currentTimer < ATTACK_SLIDE_ANIM_LENGTH)
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

    private IEnumerator AllEnemyTurns()
    {
        foreach(EnemyCombatBehaviour enemy in _enemyCombatBehaviours)
        {
            if(!enemy.IsAlive)
                continue;

            enemy.UiCombatStats.TriggerTurnIndicator(true);
            yield return new WaitForSeconds(END_OF_ACTION_DELAY);

            if(enemy.CurrentAttackSO.AttackTypeEnum == EnemyAttackScriptableObject.AttackType.Attack)
                yield return StartCoroutine(AllEnemyAttackRoutines(enemy));
            else if (enemy.CurrentAttackSO.AttackTypeEnum == EnemyAttackScriptableObject.AttackType.Defense)
                yield return StartCoroutine(EnemyDefenseRoutine(enemy));
            else if(enemy.CurrentAttackSO.DebuffToCastOnPlayer != null)
                yield return StartCoroutine(EnemyPlayerDebuffRoutine(enemy));
            else
                yield return StartCoroutine(EnemySelfBuffRoutine(enemy));

        }

        SwapToCombatState(CombatState.NewCombatRound);
        DiceRollerSingleton.Instance.SwitchToDiceState(DiceRollerSingleton.DiceRollingState.ClickToRoll);
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

        aggresor.transform.position = aggresor.OriginalPosition;
        _playerCharacterCombatBehaviour.CombatAnimationBehaviour.SetAnimSpeedToNormal();
        aggresor.CombatAnimationBehaviour.SetAnimSpeedToNormal();
    }

    private IEnumerator EnemyAttackRoutine(EnemyCombatBehaviour aggresor)
    {
        _playerCharacterCombatBehaviour.transform.position = _playerAttackStartPoint.position;
        aggresor.transform.position = _enemyAttackStartPoint.position;
        float currentTimer = 0f;

        bool criticalStrike = aggresor.AttacksAreCritical;
        int damage = aggresor.CurrentAttackSO.AttackDamage;
        if(criticalStrike)
            damage *= 2;

        aggresor.CombatAnimationBehaviour.PlayAttackAnimation();
        if(_playerCharacterCombatBehaviour.DefenseCurrent < damage)
            _playerCharacterCombatBehaviour.CombatAnimationBehaviour.PlayHitAnimation();
        else
            _playerCharacterCombatBehaviour.CombatAnimationBehaviour.PlayDefenseAnimation();
            
        DamageNumberManagerSingleton.Instance.ShowPlayerDamageNumber(damage, damage <= _playerCharacterCombatBehaviour.DefenseCurrent, criticalStrike);

        _playerCharacterCombatBehaviour.TakeDamage(damage);
        while (currentTimer < ATTACK_SLIDE_ANIM_LENGTH)
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
}
