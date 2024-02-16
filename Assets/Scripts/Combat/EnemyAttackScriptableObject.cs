using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class EnemyAttackScriptableObject : ScriptableObject
{

    public enum AttackType { Attack, Defense}
    public AttackType AttackTypeEnum = EnemyAttackScriptableObject.AttackType.Attack;
    public Sprite AttackIcon;
    public Color AttackIconColor;
    public Color AttackTextColor;
    public int AttackDamage = 5;
    public int AttackCount = 1;

    public EnemyAttackScriptableObject(AttackType attackType, Sprite attackIcon, int attackDamage, int attackCount, Color attackIconColor, Color attackTextColor)
    {
        AttackTypeEnum = attackType;
        AttackIcon = attackIcon;
        AttackDamage = attackDamage;
        AttackCount = attackCount;
        AttackIconColor = attackIconColor;
        AttackTextColor = attackTextColor;
    }
}
