using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyCombatBehaviour : MonoBehaviour
{
    public int DefenseCurrent { get { return _defenseCurrent;}}
    public Vector3 OriginalPosition { get { return _originalPosition;}}
    public CombatAnimationBehaviour CombatAnimationBehaviour { get { return _combatAnimationBehaviour;}}
    public bool IsAlive { get { return _isAlive;}}
    public bool AttacksAreCritical { get { return _attacksAreCritical;}}
    public EnemyAttackScriptableObject CurrentAttackSO { get { return _currentAttackSO;} }
    public UiEntityCombatStats UiCombatStats { get { return _uiCombatStats; } }
    public BuffManager BuffManager { get => _buffManager; }

    [SerializeField] private EnemyAttackScriptableObject[] _availableEnemyAttacks;
    [SerializeField] private GameObject _enemyStatsUiPrefab;
    [SerializeField] private Transform _statsUiSpawnTarget;
    [SerializeField] private bool _randomizeStartingAttack;

    private int _healthCurrent;
    private int _healthMax;
    private int _defenseBase;
    private int _defenseCurrent;
    private Vector3 _originalPosition;
    private CombatAnimationBehaviour _combatAnimationBehaviour;
    private bool _isAlive = true;
    private int _currentAttackIndex = 0;
    private EnemyAttackScriptableObject _currentAttackSO;
    private bool _attacksAreCritical = false;
    private UiEntityCombatStats _uiCombatStats;
    private BuffManager _buffManager;

    private void Awake()
    {
        _healthMax = 10;
        _defenseBase = 4;

        _healthCurrent = _healthMax;
        _originalPosition = transform.position;

        _uiCombatStats = Instantiate(_enemyStatsUiPrefab, _statsUiSpawnTarget.position, Quaternion.identity ).GetComponent<UiEntityCombatStats>();
        _uiCombatStats.ShowUi();

        _buffManager = GetComponent<BuffManager>();
        _buffManager.BuffUiParent = _uiCombatStats.BuffUiParent;
        _buffManager.UiBuffDescriptionController = _uiCombatStats.UiBuffDescriptionController;
        _buffManager.OnBuffAffectsCombatDamage += DrawActionUi;

        _combatAnimationBehaviour = GetComponentInChildren<CombatAnimationBehaviour>();
        _isAlive = true;

        NewTurnStatInitialization();
        if(_randomizeStartingAttack)
            _currentAttackIndex = Random.Range(0, _availableEnemyAttacks.Length);
        LoadNextAction();
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Alpha6))
            TakeDamage(10);
        if(Input.GetKeyDown(KeyCode.Alpha7))
            HealHealth(10);
    }

    public void NewTurnStatInitialization()
    {
        //_defenseCurrent = _defenseBase;

        _uiCombatStats.UpdateHealthReadout(_healthCurrent, _healthMax, true, false);
        _uiCombatStats.UpdateDefenseReadout(_defenseCurrent, true, false);
        _uiCombatStats.TriggerTurnIndicator(false, true);
    }

    public void TakeDamage(int value)
    {
        if(value < 0)
            return;
        
        if(_defenseCurrent > 0)
        {
            if(value > _defenseCurrent)
            {
                value -= _defenseCurrent;
                _defenseCurrent = 0;
            }
            else
            {
                _defenseCurrent -= value;
                value = 0;
            }
        _uiCombatStats.UpdateDefenseReadout(_defenseCurrent, false);
        }

        if(value > 0)
        {
            _healthCurrent -= value;
            if(_healthCurrent <= 0)
            {
                _healthCurrent = 0;
                _isAlive = false;
            }
            _uiCombatStats.UpdateHealthReadout(_healthCurrent, _healthMax, false);
        }
    }

    public void HealHealth(int value)
    {
        if(value < 0)
            return;

        _healthCurrent += value;
        if(_healthCurrent > _healthMax)
            _healthCurrent = _healthMax;

        _uiCombatStats.UpdateHealthReadout(_healthCurrent, _healthMax, true);
    }

    public void HideAttackUi()
    {
        _currentAttackSO = null;
        DrawActionUi();
    }

    public void LoadNextAction()
    {
        _currentAttackSO = _availableEnemyAttacks[_currentAttackIndex];
        DrawActionUi();

        _currentAttackIndex ++;
        if(_currentAttackIndex == _availableEnemyAttacks.Length)
            _currentAttackIndex = 0;
    }

    private void DrawActionUi()
    {
        if (CurrentAttackSO == null)
        {
            _uiCombatStats.ShowEnemyAttack(_currentAttackSO, 0, false);
            return;
        }

        if (CurrentAttackSO.AttackTypeEnum == EnemyAttackScriptableObject.AttackType.Attack)
            _uiCombatStats.ShowEnemyAttack(_currentAttackSO, BuffManager.GetBuffStackCount(BuffScriptableObject.BuffType.Strength), BuffManager.IsBuffActive(BuffScriptableObject.BuffType.Weaken));
        else
            _uiCombatStats.ShowEnemyAttack(_currentAttackSO, 0, false);
    }

    public void AddDefense(int value)
    {
        _defenseCurrent += value;
        _uiCombatStats.UpdateDefenseReadout(_defenseCurrent, true);
    }

    public void CharacterDeath()
    {
        _isAlive = false;
        _combatAnimationBehaviour.PlayDeathAnimation();
        Destroy(gameObject, 2f);
        gameObject.layer = 0;
        _uiCombatStats.HideUi();
        _uiCombatStats.ConnectedTarget = null;
    }
}
