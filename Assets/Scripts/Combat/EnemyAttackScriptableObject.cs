using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class EnemyAttackScriptableObject : ScriptableObject
{

    public enum AttackType { Attack, Defense, Buff}
    public AttackType AttackTypeEnum = EnemyAttackScriptableObject.AttackType.Attack;
    public Sprite AttackIcon;
    public Color AttackTextColor;
    public int AttackDamage = 5;
    public int AttackCount = 1;
    public BuffScriptableObject BuffToCastOnSelf;
    public BuffScriptableObject DebuffToCastOnPlayer;
    public int BuffCount = 1;

    public EnemyAttackScriptableObject(AttackType attackType, Sprite attackIcon, int attackDamage, int attackCount, Color attackTextColor, BuffScriptableObject buffToCastOnSelf, BuffScriptableObject debuffToCastOnPlayer, int buffCount)
    {
        AttackTypeEnum = attackType;
        AttackIcon = attackIcon;
        AttackDamage = attackDamage;
        AttackCount = attackCount;
        AttackTextColor = attackTextColor;
        BuffToCastOnSelf = buffToCastOnSelf;
        DebuffToCastOnPlayer = debuffToCastOnPlayer;
        BuffCount = buffCount;
    }
}
