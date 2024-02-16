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
            CombatManagerSingleton.Instance.PlayerCharacterCombatBehaviour.HealHealth(_rolledLuckyFaces > 2 ? 2 : 1);
                break;
                
            case DiceFaceData.DiceFace.Vamperism:
            _rolledVamperismFaces++;
            CombatManagerSingleton.Instance.PlayerCharacterCombatBehaviour.AddVamperism(_rolledVamperismFaces > 2 ? 2 : 1);
                break;
        }
        _rolledDiceFaces.Add(diceFace);
    }
}
