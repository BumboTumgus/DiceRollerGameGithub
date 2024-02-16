using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class DiceFaceData : ScriptableObject
{
    public enum DiceFace { Attack, Defense, Strike, Lucky, Vamperism, Heal }
    public Material DiceFaceMat;
    public GameObject RolledDiceParticles;
    public GameObject PlayerPowerUpParticles;
    public DiceFace DiceFaceEnum;

    public DiceFaceData(Material diceFaceMat, GameObject rolledDiceParticles, GameObject playerPowerUpParticles, DiceFace diceFace)
    {
        RolledDiceParticles = rolledDiceParticles;
        PlayerPowerUpParticles = playerPowerUpParticles;
        DiceFaceMat = diceFaceMat;
        DiceFaceEnum = diceFace;
    }
}
