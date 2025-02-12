using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiceBonusCalculatorSingleton : MonoBehaviour
{
    public static DiceBonusCalculatorSingleton Instance;

    private List<DiceFaceData.DiceFace> _rolledDiceFaces;
    private int _rolledAttackFaces = 0;
    private int _rolledDefenseFaces = 0;
    private int _rolledStrikeFaces = 0;
    private int _rolledLuckyFaces = 0;
    private int _rolledVamperismFaces = 0;
    private int _rolledHealingFaces = 0;
    private int _rolledBleedFaces = 0;
    private int _rolledBraceFaces = 0;
    private int _rolledEvadeFaces = 0;
    private int _rolledPlunderFaces = 0;
    private int _rolledRegenFaces = 0;
    private int _rolledRerollFaces = 0;
    private int _rolledRerollAttackFaces = 0;
    private int _rolledRerollDefenseFaces = 0;
    private int _rolledStrengthFaces = 0;
    private int _rolledStunFaces = 0;
    private int _rolledSunderFaces = 0;
    private int _rolledTenacityFaces = 0;
    private int _rolledThornsFaces = 0;
    private int _rolledWeakenFaces = 0;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        if (Instance != this)
            Destroy(this);
    }

    public void ResetRolledDiceBonuses()
    {
        CombatManagerSingleton.Instance.PlayerCharacterCombatBehaviour.NewTurnStatInitialization();
        _rolledDiceFaces = new List<DiceFaceData.DiceFace>();
        _rolledAttackFaces = 0;
        _rolledDefenseFaces = 0;
        _rolledStrikeFaces = 0;
        _rolledLuckyFaces = 0;
        _rolledVamperismFaces = 0;
        _rolledHealingFaces = 0;
        _rolledBleedFaces = 0;
        _rolledBraceFaces = 0;
        _rolledEvadeFaces = 0;
        _rolledPlunderFaces = 0;
        _rolledRegenFaces = 0;
        _rolledRerollFaces = 0;
        _rolledRerollAttackFaces = 0;
        _rolledRerollDefenseFaces = 0;
        _rolledStrengthFaces = 0;
        _rolledStunFaces = 0;
        _rolledSunderFaces = 0;
        _rolledTenacityFaces = 0;
        _rolledThornsFaces = 0;
        _rolledWeakenFaces = 0;
}

    public void CalculateBonusForRolledDiceFace(DiceFaceData.DiceFace diceFace)
    {
        switch(diceFace)
        {
            case DiceFaceData.DiceFace.Attack:
                _rolledAttackFaces++;
                CombatManagerSingleton.Instance.PlayerCharacterCombatBehaviour.AddAttackDamage(_rolledAttackFaces > 2 ? 2 : 1);
                break;

            case DiceFaceData.DiceFace.Defense:
                _rolledDefenseFaces++;
                CombatManagerSingleton.Instance.PlayerCharacterCombatBehaviour.AddDefense(_rolledDefenseFaces > 2 ? 2 : 1);
                break;

            case DiceFaceData.DiceFace.Strike:
                _rolledStrikeFaces++;
                CombatManagerSingleton.Instance.PlayerCharacterCombatBehaviour.AddAttackCount(_rolledStrikeFaces > 2 ? 2 : 1);
                break;

            case DiceFaceData.DiceFace.Lucky:
                _rolledLuckyFaces++;
                CombatManagerSingleton.Instance.PlayerCharacterCombatBehaviour.AddLuck(_rolledLuckyFaces > 2 ? 2 : 1);
                break;

            case DiceFaceData.DiceFace.Heal:
                _rolledHealingFaces++;
                CombatManagerSingleton.Instance.PlayerCharacterCombatBehaviour.HealHealth(_rolledHealingFaces > 2 ? 2 : 1);
                break;

            case DiceFaceData.DiceFace.Bleed:
                _rolledBleedFaces++;
                CombatManagerSingleton.Instance.PlayerCharacterCombatBehaviour.AddBleed(_rolledBleedFaces > 2 ? 2 : 1);
                break;

            case DiceFaceData.DiceFace.RerollAttack:
                _rolledRerollAttackFaces++;
                CombatManagerSingleton.Instance.PlayerCharacterCombatBehaviour.AddRerollAttack(_rolledRerollAttackFaces > 2 ? 2 : 1);
                break;

            case DiceFaceData.DiceFace.RerollDefense:
                _rolledRerollDefenseFaces++;
                CombatManagerSingleton.Instance.PlayerCharacterCombatBehaviour.AddRerollDefense(_rolledRerollDefenseFaces > 2 ? 2 : 1);
                break;

            case DiceFaceData.DiceFace.Thorns:
                _rolledThornsFaces++;
                CombatManagerSingleton.Instance.PlayerCharacterCombatBehaviour.AddThorns(_rolledTenacityFaces > 2 ? 2 : 1);
                break;

            //------------ PAIRS ------------------------------------------------------------------------------------------

            case DiceFaceData.DiceFace.Brace:
                _rolledBraceFaces++;
                if (_rolledBraceFaces >= 2)
                    CombatManagerSingleton.Instance.PlayerCharacterCombatBehaviour.AddBrace(_rolledBraceFaces > 3 ? 2 : 1);
                break;

            case DiceFaceData.DiceFace.Evade:
                _rolledEvadeFaces++;
                if (_rolledEvadeFaces >= 2)
                    CombatManagerSingleton.Instance.PlayerCharacterCombatBehaviour.AddEvade(_rolledEvadeFaces > 3 ? 2 : 1);
                break;

            case DiceFaceData.DiceFace.Plunder:
                _rolledPlunderFaces++;
                if (_rolledPlunderFaces >= 2)
                    CombatManagerSingleton.Instance.PlayerCharacterCombatBehaviour.AddPlunder(_rolledPlunderFaces > 3 ? 2 : 1);
                break;

            case DiceFaceData.DiceFace.Regen:
                _rolledRegenFaces++;
                if (_rolledRegenFaces >= 2)
                    CombatManagerSingleton.Instance.PlayerCharacterCombatBehaviour.AddRegen(_rolledRegenFaces > 3 ? 2 : 1);
                break;

            case DiceFaceData.DiceFace.Strength:
                _rolledStrengthFaces++;
                if (_rolledStrengthFaces >= 2)
                    CombatManagerSingleton.Instance.PlayerCharacterCombatBehaviour.AddStrength(_rolledStrengthFaces > 3 ? 2 : 1);
                break;

            case DiceFaceData.DiceFace.Stun:
                _rolledStunFaces++;
                if (_rolledStunFaces >= 2)
                    CombatManagerSingleton.Instance.PlayerCharacterCombatBehaviour.AddStun(_rolledStunFaces > 3 ? 2 : 1);
                break;

            case DiceFaceData.DiceFace.Sunder:
                _rolledSunderFaces++;
                if (_rolledSunderFaces >= 2)
                    CombatManagerSingleton.Instance.PlayerCharacterCombatBehaviour.AddSunder(_rolledSunderFaces > 3 ? 2 : 1);
                break;

            case DiceFaceData.DiceFace.Tenacity:
                _rolledTenacityFaces++;
                if (_rolledSunderFaces >= 2)
                    CombatManagerSingleton.Instance.PlayerCharacterCombatBehaviour.AddTenacity(_rolledTenacityFaces > 3 ? 2 : 1);
                break;

            case DiceFaceData.DiceFace.Weaken:
                _rolledWeakenFaces++;
                if (_rolledWeakenFaces >= 2)
                    CombatManagerSingleton.Instance.PlayerCharacterCombatBehaviour.AddWeaken(_rolledWeakenFaces > 3 ? 2 : 1);
                break;

            case DiceFaceData.DiceFace.Vamperism:
                _rolledVamperismFaces++;
                if(_rolledVamperismFaces >= 2)
                    CombatManagerSingleton.Instance.PlayerCharacterCombatBehaviour.AddVamperism(_rolledVamperismFaces > 3 ? 2 : 1);
                break;

                //------------ TRIPLES ------------------------------------------------------------------------------------------
        }
        _rolledDiceFaces.Add(diceFace);
    }
}
