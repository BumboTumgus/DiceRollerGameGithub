using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCharacterCombatBehaviour : MonoBehaviour
{
    private const float GROW_SIZE_GAIN = 0.01f;

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

    private float _sizeModifier = 0;

    private void Awake()
    {
        _healthMax = 20;
        _attackDamageBase = 2;
        _attackCountBase = 1;
        _defenseBase = 0;

        _buffManager = GetComponent<BuffManager>();
        _buffManager.BuffUiParent = _uiCombatStats.BuffUiParent;
        _buffManager.UiBuffDescriptionController = _uiCombatStats.UiBuffDescriptionController;
        _buffManager.OnBuffAffectsCombatDamage += DrawAttackReadout;

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

        _uiCombatStats.UpdateHealthReadout(_healthCurrent, _healthMax, true, false);;
        _uiCombatStats.UpdateDefenseReadout(_defenseCurrent, true, false);
        int attackDamage = _attackDamageCurrent + BuffManager.GetBuffStackCount(BuffScriptableObject.BuffType.Strength);
        if (BuffManager.IsBuffActive(BuffScriptableObject.BuffType.Weaken))
            attackDamage /= 2;
        _uiCombatStats.UpdateAttackReadout(attackDamage, GetCurrentAttackCount(), false);
    }

    public void TakeDamage(int value)
    {
        value -= BuffManager.GetBuffStackCount(BuffScriptableObject.BuffType.Brace);

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
        if (BuffManager.IsBuffActive(BuffScriptableObject.BuffType.Wither))
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
        if (BuffManager.IsBuffActive(BuffScriptableObject.BuffType.Anoint))
            return true;

        int randomNum = Random.Range(0, 10);
        return randomNum < 1 + BuffManager.GetBuffStackCount(BuffScriptableObject.BuffType.Luck);
    }

    public void AddAttackDamage(int v)
    {
        _attackDamageCurrent += v;
        DrawAttackReadout();
    }

    public void AddAttackCount( int v)
    {
        _attackCountCurrent += v;
        DrawAttackReadout();
    }

    public void AddDefense(int v)
    {
        _defenseCurrent += v;
        _uiCombatStats.UpdateDefenseReadout(_defenseCurrent, true);
    }

    public int GetCurrentAttackCount()
    {
        if (BuffManager.IsBuffActive(BuffScriptableObject.BuffType.Shackle))
            return 1;

        int attackCount = _attackCountCurrent;
        if (BuffManager.IsBuffActive(BuffScriptableObject.BuffType.Anoint))
            attackCount *= 2;
        return attackCount;
    }

    private void DrawAttackReadout()
    {
        int attackDamage = _attackDamageCurrent + BuffManager.GetBuffStackCount(BuffScriptableObject.BuffType.Strength);
        if (BuffManager.IsBuffActive(BuffScriptableObject.BuffType.Weaken))
            attackDamage /= 2;

        _uiCombatStats.UpdateAttackReadout(attackDamage, GetCurrentAttackCount());
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
        BuffManager.AddBuff(BuffSingleton.Instance.GetBuffDataByType(BuffScriptableObject.BuffType.Strength), v);
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

    public void AddPrayer(int v)
    {
        DivineDemonstrationSingleton.Instance.AddFavor(v);
    }

    public void AddGrow(int v)
    {
    }

    public void AddBoonAnoint(int v)
    {
        BuffManager.AddBuff(BuffSingleton.Instance.GetBuffDataByType(BuffScriptableObject.BuffType.Anoint), v);
    }
    
    public void AddBoonCommand(int v)
    {
        //TODO ADD a way to set a dice to be rolled on any face.
    }

    public void AddBoonCleanse(int v)
    {
        //TODO USE THE SAME TARGETTING AS DEBUFFS TO APPLY THIS EFFECT
    }

    public void AddBoonMirror(int v)
    {
        //TODO HAve a way to create a temporary dice that dissapears at the end of combat
    }

    public void AddBoonOmniscience(int v)
    {
        BuffManager.AddBuff(BuffSingleton.Instance.GetBuffDataByType(BuffScriptableObject.BuffType.Omniscience), v);
    }

    public void AddBoonWindfall(int v)
    {
        for (int i = 0; i < v; i++)
            PlayerInventorySingleton.Instance.UpdateGoldValue(PlayerInventorySingleton.Instance.CollectedGold + Random.Range(5, 25));
    }

    public void AddBoonRestoration(int v)
    {
        HealHealth(25 * v);
    }

    public void AddBoonSmite(int v)
    {
        //TODO USE THE SAME TARGETTING AS DEBUFFS TO DEAL THIS DAMAGE
    }

    public void AddBoonExalt(int v)
    {
        DivineDemonstrationSingleton.Instance.AddFavor(v * 5);
    }

    public void AddBoonWard(int v)
    {

    }

    public void AddCurseLock(int v)
    {

    }

    public void AddCurseSacrifice(int v)
    {
        //TODO have a way to kill a dice for combat and have it come back at combat start.
    }

    public void AddCurseExsanguinate(int v)
    {
        BuffManager.AddBuff(BuffSingleton.Instance.GetBuffDataByType(BuffScriptableObject.BuffType.Bleed), 5 * v);
    }

    public void AddCurseWither(int v)
    {
        BuffManager.AddBuff(BuffSingleton.Instance.GetBuffDataByType(BuffScriptableObject.BuffType.Wither), v);
    }

    public void AddCurseTithe(int v)
    {
        for (int i = 0; i < v; i++)
            PlayerInventorySingleton.Instance.UpdateGoldValue(PlayerInventorySingleton.Instance.CollectedGold - Random.Range(5, 25));
    }

    public void AddCurseStill(int v)
    {
        BuffManager.AddBuff(BuffSingleton.Instance.GetBuffDataByType(BuffScriptableObject.BuffType.Still), v);
    }

    public void AddCurseShackle (int v)
    {
        BuffManager.AddBuff(BuffSingleton.Instance.GetBuffDataByType(BuffScriptableObject.BuffType.Shackle), v);
    }

    public void AddCurseWhisper(int v)
    {
        BuffManager.AddBuff(BuffSingleton.Instance.GetBuffDataByType(BuffScriptableObject.BuffType.Whisper), v);
    }

    public void AddCurseEcho(int v)
    {
        for(int i = 0; i < v;i++)
            CurseManagerSingleton.Instance.AddRandomCurseFace(true);
    }

    public void AddCurseRust(int v)
    {
        AddDefense(DefenseCurrent * -1);
    }

    public void AddCurseWane(int v)
    {
        _healthMax -= v;
        if(_healthCurrent > _healthMax)
            _healthCurrent = _healthMax;
        _sizeModifier -= GROW_SIZE_GAIN * v;
        transform.localScale = Vector3.one + Vector3.one * _sizeModifier;
    }

}
