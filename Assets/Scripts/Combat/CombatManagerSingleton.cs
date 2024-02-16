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
        StartCoroutine(SetUpEncounter());
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

    public void RemoveEnemyFromList(EnemyCombatBehaviour enemyToRemove)
    {
        _enemyCombatBehaviours.Remove(enemyToRemove);
    }
    
    private IEnumerator SetUpEncounter()
    {
        for(int enemyPrefabIndex = 0; enemyPrefabIndex < _currentEncounter.EnemyPrefabs.Length; enemyPrefabIndex++)
        {
            _enemyCombatBehaviours.Add(Instantiate(_currentEncounter.EnemyPrefabs[enemyPrefabIndex], _enemySpawns[enemyPrefabIndex].position, _enemySpawns[enemyPrefabIndex].rotation).GetComponent<EnemyCombatBehaviour>());
            _enemyCombatBehaviours[enemyPrefabIndex].SetupUiStatsRotation(enemyPrefabIndex);
            yield return new WaitForSeconds(enemyPrefabIndex * ENEMY_SPAWN_DELAY);
        }
        SwapToCombatState(CombatState.Idle);
        yield return new WaitForSeconds(1f);
        DiceRollerSingleton.Instance.SwitchToDiceState(DiceRollerSingleton.DiceRollingState.ClickToRoll);
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
            StartCoroutine(AllEnemyTurns());
                break;
            case CombatState.NewCombatRound:
            temporaryStateText.text = "COMBAT STATE: NEW COMBAT ROUND";
            foreach(EnemyCombatBehaviour enemy in _enemyCombatBehaviours)
                enemy.LoadNextAttack();
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

            if(_targettedEnemies.Count == PlayerCharacterCombatBehaviour.AttackCountCurrent)
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
        

        yield return new WaitForSeconds(END_OF_ACTION_DELAY);

        bool enemiesStillAlive = false;
        foreach(EnemyCombatBehaviour enemyCombatBehaviour in _enemyCombatBehaviours)
        {
            if(enemyCombatBehaviour.IsAlive)
                enemiesStillAlive = true;
        }

        if(enemiesStillAlive)
            SwapToCombatState(CombatState.EnemyExecutingAttacks);
        else
            SwapToCombatState(CombatState.Idle);
    }

    private IEnumerator PlayerAttackRoutine(EnemyCombatBehaviour target)
    {
        _playerCharacterCombatBehaviour.transform.position = _playerAttackStartPoint.position;
        target.transform.position = _enemyAttackStartPoint.position;
        float currentTimer = 0f;

        bool criticalStrike = _playerCharacterCombatBehaviour.IsAttackCritical();
        int damage = _playerCharacterCombatBehaviour.AttackDamageCurrent;
        if(criticalStrike)
            damage *= 2;

        _playerCharacterCombatBehaviour.CombatAnimationBehaviour.PlayAttackAnimation();
        if(target.DefenseCurrent < damage)
            target.CombatAnimationBehaviour.PlayHitAnimation();
        else
            target.CombatAnimationBehaviour.PlayDefenseAnimation();
            
        DamageNumberManagerSingleton.Instance.ShowEnemyDamageNumber(damage, damage <= target.DefenseCurrent, true, criticalStrike);
        if(damage > target.DefenseCurrent && PlayerCharacterCombatBehaviour.VamperismCurrent > 0)
            PlayerCharacterCombatBehaviour.HealHealth(PlayerCharacterCombatBehaviour.VamperismCurrent);

        while(currentTimer < ATTACK_SLIDE_ANIM_LENGTH)
        {
            currentTimer += Time.deltaTime;
            _playerCharacterCombatBehaviour.transform.position = Vector3.Lerp(_playerAttackStartPoint.position, _playerAttackEndPoint.position, currentTimer / ATTACK_SLIDE_ANIM_LENGTH);
            target.transform.position = Vector3.Lerp(_enemyAttackStartPoint.position, _enemyAttackEndPoint.position, currentTimer / ATTACK_SLIDE_ANIM_LENGTH);
            yield return _waitForEndOFrame;
        }
        
        target.transform.position = target.OriginalPosition;
        target.TakeDamage(damage);
        _playerCharacterCombatBehaviour.CombatAnimationBehaviour.SetAnimSpeedToNormal();
        target.CombatAnimationBehaviour.SetAnimSpeedToNormal();
    }

    private IEnumerator AllEnemyTurns()
    {
        foreach(EnemyCombatBehaviour enemy in _enemyCombatBehaviours)
        {
            if(!enemy.IsAlive)
                continue;
            enemy.SetTurnIndicatorUi(true);
            yield return new WaitForSeconds(END_OF_ACTION_DELAY);
            yield return StartCoroutine(AllEnemyAttackRoutines(enemy));
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

        enemy.SetTurnIndicatorUi(false);
        yield return new WaitForSeconds(END_OF_ACTION_DELAY);
    }

    private IEnumerator EnemyAttackRoutine(EnemyCombatBehaviour aggresor)
    {
        _playerCharacterCombatBehaviour.transform.position = _playerAttackStartPoint.position;
        aggresor.transform.position = _enemyAttackStartPoint.position;
        float currentTimer = 0f;

        bool criticalStrike = aggresor.AttacksAreCrtiical;
        int damage = aggresor.CurrentAttackSO.AttackDamage;
        if(criticalStrike)
            damage *= 2;

        aggresor.CombatAnimationBehaviour.PlayAttackAnimation();
        if(_playerCharacterCombatBehaviour.DefenseCurrent < damage)
            _playerCharacterCombatBehaviour.CombatAnimationBehaviour.PlayHitAnimation();
        else
            _playerCharacterCombatBehaviour.CombatAnimationBehaviour.PlayDefenseAnimation();
            
        DamageNumberManagerSingleton.Instance.ShowEnemyDamageNumber(damage, damage <= _playerCharacterCombatBehaviour.DefenseCurrent, false, criticalStrike);

        while(currentTimer < ATTACK_SLIDE_ANIM_LENGTH)
        {
            currentTimer += Time.deltaTime;
            _playerCharacterCombatBehaviour.transform.position = Vector3.Lerp(_playerAttackStartPoint.position, _playerAttackEndPoint.position, currentTimer / ATTACK_SLIDE_ANIM_LENGTH);
            aggresor.transform.position = Vector3.Lerp(_enemyAttackStartPoint.position, _enemyAttackEndPoint.position, currentTimer / ATTACK_SLIDE_ANIM_LENGTH);
            yield return _waitForEndOFrame;
        }
        
        aggresor.transform.position = aggresor.OriginalPosition;
        _playerCharacterCombatBehaviour.TakeDamage(damage);
        _playerCharacterCombatBehaviour.CombatAnimationBehaviour.SetAnimSpeedToNormal();
        aggresor.CombatAnimationBehaviour.SetAnimSpeedToNormal();
    }
}
