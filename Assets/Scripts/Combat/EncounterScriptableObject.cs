using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class EncounterScriptableObject : ScriptableObject
{
    public GameObject[] EnemyPrefabs;
    public Vector2 GoldRange;
    public Vector2 DropCountRange;
    public DiceFaceData[] GarenteedDiceFaceDrops;
    public DiceFaceData[] PotentialDiceFaceDrops;
}
