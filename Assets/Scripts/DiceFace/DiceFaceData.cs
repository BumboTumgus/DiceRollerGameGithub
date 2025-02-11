using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class DiceFaceData : ScriptableObject
{
    public enum DiceFace { Attack, Defense, Strike, Lucky, Vamperism, Heal, Evade, Thorns, Reroll, RerollAttack, RerollDefense, Brace, Bleed, Regen, Plunder, Strength,
        Tenacity, Sunder, Weaken, Stun }
    public Material DiceFaceMat;
    public GameObject RolledDiceParticles;
    public GameObject PlayerPowerUpParticles;
    public DiceFace DiceFaceEnum;
    public Sprite DiceFaceUiSprite;
    public int DiceFaceCost;

    public DiceFaceData(Material diceFaceMat, GameObject rolledDiceParticles, GameObject playerPowerUpParticles, DiceFace diceFace, Sprite diceFaceUiSprite, int diceFaceCost)
    {
        RolledDiceParticles = rolledDiceParticles;
        PlayerPowerUpParticles = playerPowerUpParticles;
        DiceFaceMat = diceFaceMat;
        DiceFaceEnum = diceFace;
        DiceFaceUiSprite = diceFaceUiSprite;
        DiceFaceCost = diceFaceCost;
    }
}
