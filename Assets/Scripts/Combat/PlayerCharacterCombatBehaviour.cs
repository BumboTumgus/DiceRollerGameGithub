using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCharacterCombatBehaviour : MonoBehaviour
{
    [SerializeField] private UiEntityCombatStats _uiCombatStats;

    public int AttackCountCurrent { get { return _attackCountCurrent;}}
    public int AttackDamageCurrent { get { return _attackDamageCurrent;}}
    public int DefenseCurrent { get { return _defenseCurrent;}}
    public Vector3 OriginalPosition { get { return _originalPosition;}}
    public CombatAnimationBehaviour CombatAnimationBehaviour { get { return _combatAnimationBehaviour;}}
    public BuffManager BuffManager { get => _buffManager; }
    public PlayerDebuffsToInflictManager DebuffInflictionManager { get => _debuffInflictionManager; }

    private int _healthCurrent;
    private int _healthMax;
    private int _attackDamageBase;
    private int _attackCountBase;
    private int _attackDamageCurrent;
    private int _attackCountCurrent;
    private int _defenseBase;
    private int _defenseCurrent;
    private Vector3 _originalPosition;
    private CombatAnimationBehaviour _combatAnimationBehaviour;
    private BuffManager _buffManager;
    private PlayerDebuffsToInflictManager _debuffInflictionManager;

    private void Awake()
    {
        _healthMax = 20;
        _attackDamageBase = 2;
        _attackCountBase = 1;
        _defenseBase = 0;

        _buffManager = GetComponent<BuffManager>();
        _buffManager.BuffUiParent = _uiCombatStats.BuffUiParent;
        _buffManager.UiBuffDescriptionController = _uiCombatStats.UiBuffDescriptionController;

        _debuffInflictionManager = GetComponent<PlayerDebuffsToInflictManager>();

        _healthCurrent = _healthMax;
        _originalPosition = transform.position;
        _combatAnimationBehaviour = GetComponentInChildren<CombatAnimationBehaviour>();
        NewTurnStatInitialization();
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Alpha1))
            TakeDamage(10);
        if(Input.GetKeyDown(KeyCode.Alpha2))
            HealHealth(10);
        if(Input.GetKeyDown(KeyCode.Alpha3))
            AddAttackDamage(1);
        if(Input.GetKeyDown(KeyCode.Alpha4))
            AddAttackCount(1);
        if(Input.GetKeyDown(KeyCode.Alpha5))
            AddDefense(1);
    }

    public void NewTurnStatInitialization()
    {
        _attackDamageCurrent = _attackDamageBase;
        _attackCountCurrent = _attackCountBase;
        //_defenseCurrent = _defenseBase;

        _uiCombatStats.UpdateHealthReadout(_healthCurrent, _healthMax, true, false);
        _uiCombatStats.UpdateAttackReadout(_attackDamageCurrent, _attackCountCurrent, false);
        _uiCombatStats.UpdateDefenseReadout(_defenseCurrent, true, false);
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
                CharacterDeath();
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

    private void CharacterDeath()
    {

    }

    public bool IsAtMaxHealth()
    {
        return _healthCurrent >= _healthMax;
    }

    public int CurrentMissingHealth()
    {
        return _healthMax - _healthCurrent;
    }

    public bool IsAttackCritical()
    {
        int randomNum = Random.Range(0, 10);
        return randomNum < 1 + BuffManager.GetBuffStackCount(BuffScriptableObject.BuffType.Luck);
    }

    public void AddAttackDamage(int v)
    {
        _attackDamageCurrent += v;
        _uiCombatStats.UpdateAttackReadout(_attackDamageCurrent, _attackCountCurrent);
    }

    public void AddAttackCount( int v)
    {
        _attackCountCurrent += v;
        _uiCombatStats.UpdateAttackReadout(_attackDamageCurrent, _attackCountCurrent);
    }

    public void AddDefense(int v)
    {
        _defenseCurrent += v;
        _uiCombatStats.UpdateDefenseReadout(_defenseCurrent, true);
    }

    public void AddLuck(int v)
    {
        BuffManager.AddBuff(BuffSingleton.Instance.GetBuffDataByType(BuffScriptableObject.BuffType.Luck), v);
    }

    public void AddVamperism(int v)
    {
        BuffManager.AddBuff(BuffSingleton.Instance.GetBuffDataByType(BuffScriptableObject.BuffType.Vamperism), v);
    }

    public void AddBleed(int v)
    {
        DebuffInflictionManager.AddDebuffToInflict(BuffSingleton.Instance.GetBuffDataByType(BuffScriptableObject.BuffType.Bleed), v);
    }

    public void AddRerollAttack(int v)
    {
        BuffManager.AddBuff(BuffSingleton.Instance.GetBuffDataByType(BuffScriptableObject.BuffType.RerollAttack), v);
    }

    public void AddRerollDefense(int v)
    {
        BuffManager.AddBuff(BuffSingleton.Instance.GetBuffDataByType(BuffScriptableObject.BuffType.RerollDefense), v);
    }

    public void AddThorns(int v)
    {
        BuffManager.AddBuff(BuffSingleton.Instance.GetBuffDataByType(BuffScriptableObject.BuffType.Thorns), v);
    }

    public void AddBrace(int v)
    {
        BuffManager.AddBuff(BuffSingleton.Instance.GetBuffDataByType(BuffScriptableObject.BuffType.Brace), v);
    }

    public void AddEvade(int v)
    {
        BuffManager.AddBuff(BuffSingleton.Instance.GetBuffDataByType(BuffScriptableObject.BuffType.Evade), v);
    }

    public void AddPlunder(int v)
    {
        for (int i = 0; i < v; i++)
            PlayerInventorySingleton.Instance.UpdateGoldValue(PlayerInventorySingleton.Instance.CollectedGold + Random.Range(1, 7));
    }

    public void AddRegen(int v)
    {
        BuffManager.AddBuff(BuffSingleton.Instance.GetBuffDataByType(BuffScriptableObject.BuffType.Regen), v);
    }

    public void AddStrength(int v)
    {
        BuffManager.AddBuff(BuffSingleton.Instance.GetBuffDataByType(BuffScriptableObject.BuffType.Stun), v);
    }

    public void AddStun(int v)
    {
        DebuffInflictionManager.AddDebuffToInflict(BuffSingleton.Instance.GetBuffDataByType(BuffScriptableObject.BuffType.Stun), v);
    }

    public void AddSunder(int v)
    {
        DebuffInflictionManager.AddDebuffToInflict(BuffSingleton.Instance.GetBuffDataByType(BuffScriptableObject.BuffType.Sunder), v);
    }

    public void AddTenacity(int v)
    {
        BuffManager.AddBuff(BuffSingleton.Instance.GetBuffDataByType(BuffScriptableObject.BuffType.Tenacity), v);
    }

    public void AddWeaken(int v)
    {
        DebuffInflictionManager.AddDebuffToInflict(BuffSingleton.Instance.GetBuffDataByType(BuffScriptableObject.BuffType.Weaken), v);
    }
}
