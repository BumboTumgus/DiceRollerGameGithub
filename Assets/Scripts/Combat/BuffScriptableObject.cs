using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class BuffScriptableObject : ScriptableObject
{
    public enum BuffType { None, Strength, Thorns, Weaken, Stun, Bleed, Luck, Sunder, RerollAttack, RerollDefense, Brace, Regen, Tenacity };
    public BuffType MyBuffType;
    public Sprite BuffIcon;
    public string BuffDescription;
    public bool BuffStackingAllowed = true;
    public bool BuffIsPermanent = false;

    public BuffScriptableObject(BuffType myBuffType, Sprite buffIcon, string buffDescription, bool buffStackingAllowed, bool buffIsPermanent)
    {
        MyBuffType = myBuffType;
        BuffIcon = buffIcon;
        BuffDescription = buffDescription;
        BuffStackingAllowed = buffStackingAllowed;
        BuffIsPermanent = buffIsPermanent;
    }
}
