using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DicePrerollPlacerSingleton : MonoBehaviour
{
    public static DicePrerollPlacerSingleton Instance;

    [SerializeField] private Transform[] _gridPoints_3x5;
    [SerializeField] private Transform[] _gridPoints_3x4;
    [SerializeField] private Transform[] _gridPoints_2x4;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        if (Instance != this)
            Destroy(this);
    }

    public void PlaceSelectedDiceInPattern(List<Transform> diceToPlace)
    {
        if(diceToPlace.Count <= 0 || diceToPlace.Count > 15)
            return;

        switch (diceToPlace.Count)
        {
            case 1:
                diceToPlace[0].position = _gridPoints_3x5[7].position;
                break;

            case 2:
                diceToPlace[0].position = _gridPoints_3x4[5].position;
                diceToPlace[1].position = _gridPoints_3x4[6].position;
                break;

            case 3:
                diceToPlace[0].position = _gridPoints_3x5[6].position;
                diceToPlace[1].position = _gridPoints_3x5[7].position;
                diceToPlace[2].position = _gridPoints_3x5[8].position;
                break;

            case 4:
                diceToPlace[0].position = _gridPoints_2x4[1].position;
                diceToPlace[1].position = _gridPoints_2x4[2].position;
                diceToPlace[2].position = _gridPoints_2x4[5].position;
                diceToPlace[3].position = _gridPoints_2x4[6].position;
                break;

            case 5:
                diceToPlace[0].position = _gridPoints_3x5[1].position;
                diceToPlace[1].position = _gridPoints_3x5[3].position;
                diceToPlace[2].position = _gridPoints_3x5[7].position;
                diceToPlace[3].position = _gridPoints_3x5[11].position;
                diceToPlace[4].position = _gridPoints_3x5[13].position;
                break;

            case 6:
                diceToPlace[0].position = _gridPoints_3x4[1].position;
                diceToPlace[1].position = _gridPoints_3x4[2].position;
                diceToPlace[2].position = _gridPoints_3x4[5].position;
                diceToPlace[3].position = _gridPoints_3x4[6].position;
                diceToPlace[4].position = _gridPoints_3x4[9].position;
                diceToPlace[5].position = _gridPoints_3x4[10].position;
                break;

            case 7:
                diceToPlace[0].position = _gridPoints_3x5[1].position;
                diceToPlace[1].position = _gridPoints_3x5[3].position;
                diceToPlace[2].position = _gridPoints_3x5[6].position;
                diceToPlace[3].position = _gridPoints_3x5[7].position;
                diceToPlace[4].position = _gridPoints_3x5[8].position;
                diceToPlace[5].position = _gridPoints_3x5[11].position;
                diceToPlace[6].position = _gridPoints_3x5[13].position;
                break;

            case 8:
                diceToPlace[0].position = _gridPoints_2x4[0].position;
                diceToPlace[1].position = _gridPoints_2x4[1].position;
                diceToPlace[2].position = _gridPoints_2x4[2].position;
                diceToPlace[3].position = _gridPoints_2x4[3].position;
                diceToPlace[4].position = _gridPoints_2x4[4].position;
                diceToPlace[5].position = _gridPoints_2x4[5].position;
                diceToPlace[6].position = _gridPoints_2x4[6].position;
                diceToPlace[7].position = _gridPoints_2x4[7].position;
                break;

            case 9:
                diceToPlace[0].position = _gridPoints_3x5[1].position;
                diceToPlace[1].position = _gridPoints_3x5[2].position;
                diceToPlace[2].position = _gridPoints_3x5[3].position;
                diceToPlace[3].position = _gridPoints_3x5[6].position;
                diceToPlace[4].position = _gridPoints_3x5[7].position;
                diceToPlace[5].position = _gridPoints_3x5[8].position;
                diceToPlace[6].position = _gridPoints_3x5[11].position;
                diceToPlace[7].position = _gridPoints_3x5[12].position;
                diceToPlace[8].position = _gridPoints_3x5[13].position;
                break;

            case 10:
                diceToPlace[0].position = _gridPoints_3x4[0].position;
                diceToPlace[1].position = _gridPoints_3x4[1].position;
                diceToPlace[2].position = _gridPoints_3x4[2].position;
                diceToPlace[3].position = _gridPoints_3x4[4].position;
                diceToPlace[4].position = _gridPoints_3x4[5].position;
                diceToPlace[5].position = _gridPoints_3x4[6].position;
                diceToPlace[6].position = _gridPoints_3x4[8].position;
                diceToPlace[7].position = _gridPoints_3x4[9].position;
                diceToPlace[8].position = _gridPoints_3x4[10].position;
                diceToPlace[9].position = _gridPoints_3x4[11].position;
                break;

            case 11:
                diceToPlace[0].position = _gridPoints_3x4[0].position;
                diceToPlace[1].position = _gridPoints_3x4[1].position;
                diceToPlace[2].position = _gridPoints_3x4[2].position;
                diceToPlace[3].position = _gridPoints_3x4[4].position;
                diceToPlace[4].position = _gridPoints_3x4[5].position;
                diceToPlace[5].position = _gridPoints_3x4[6].position;
                diceToPlace[6].position = _gridPoints_3x4[7].position;
                diceToPlace[7].position = _gridPoints_3x4[8].position;
                diceToPlace[8].position = _gridPoints_3x4[9].position;
                diceToPlace[9].position = _gridPoints_3x4[10].position;
                diceToPlace[10].position = _gridPoints_3x4[11].position;
                break;

            case 12:
                diceToPlace[0].position = _gridPoints_3x4[0].position;
                diceToPlace[1].position = _gridPoints_3x4[1].position;
                diceToPlace[2].position = _gridPoints_3x4[2].position;
                diceToPlace[3].position = _gridPoints_3x4[3].position;
                diceToPlace[4].position = _gridPoints_3x4[4].position;
                diceToPlace[5].position = _gridPoints_3x4[5].position;
                diceToPlace[6].position = _gridPoints_3x4[6].position;
                diceToPlace[7].position = _gridPoints_3x4[7].position;
                diceToPlace[8].position = _gridPoints_3x4[8].position;
                diceToPlace[9].position = _gridPoints_3x4[9].position;
                diceToPlace[10].position = _gridPoints_3x4[10].position;
                diceToPlace[11].position = _gridPoints_3x4[11].position;
                break;

            case 13:
                diceToPlace[0].position = _gridPoints_3x5[0].position;
                diceToPlace[1].position = _gridPoints_3x5[1].position;
                diceToPlace[2].position = _gridPoints_3x5[2].position;
                diceToPlace[3].position = _gridPoints_3x5[3].position;
                diceToPlace[4].position = _gridPoints_3x5[5].position;
                diceToPlace[5].position = _gridPoints_3x5[6].position;
                diceToPlace[6].position = _gridPoints_3x5[7].position;
                diceToPlace[7].position = _gridPoints_3x5[8].position;
                diceToPlace[8].position = _gridPoints_3x5[10].position;
                diceToPlace[9].position = _gridPoints_3x5[11].position;
                diceToPlace[10].position = _gridPoints_3x5[12].position;
                diceToPlace[11].position = _gridPoints_3x5[13].position;
                diceToPlace[12].position = _gridPoints_3x5[14].position;
                break;

            case 14:
                diceToPlace[0].position = _gridPoints_3x5[0].position;
                diceToPlace[1].position = _gridPoints_3x5[1].position;
                diceToPlace[2].position = _gridPoints_3x5[2].position;
                diceToPlace[3].position = _gridPoints_3x5[3].position;
                diceToPlace[4].position = _gridPoints_3x5[5].position;
                diceToPlace[5].position = _gridPoints_3x5[6].position;
                diceToPlace[6].position = _gridPoints_3x5[7].position;
                diceToPlace[7].position = _gridPoints_3x5[8].position;
                diceToPlace[8].position = _gridPoints_3x5[9].position;
                diceToPlace[9].position = _gridPoints_3x5[10].position;
                diceToPlace[10].position = _gridPoints_3x5[11].position;
                diceToPlace[11].position = _gridPoints_3x5[12].position;
                diceToPlace[12].position = _gridPoints_3x5[13].position;
                diceToPlace[13].position = _gridPoints_3x5[14].position;
                break;

            case 15:
                diceToPlace[0].position = _gridPoints_3x5[0].position;
                diceToPlace[1].position = _gridPoints_3x5[1].position;
                diceToPlace[2].position = _gridPoints_3x5[2].position;
                diceToPlace[3].position = _gridPoints_3x5[3].position;
                diceToPlace[4].position = _gridPoints_3x5[4].position;
                diceToPlace[5].position = _gridPoints_3x5[5].position;
                diceToPlace[6].position = _gridPoints_3x5[6].position;
                diceToPlace[7].position = _gridPoints_3x5[7].position;
                diceToPlace[8].position = _gridPoints_3x5[8].position;
                diceToPlace[9].position = _gridPoints_3x5[9].position;
                diceToPlace[10].position = _gridPoints_3x5[10].position;
                diceToPlace[11].position = _gridPoints_3x5[11].position;
                diceToPlace[12].position = _gridPoints_3x5[12].position;
                diceToPlace[13].position = _gridPoints_3x5[13].position;
                diceToPlace[14].position = _gridPoints_3x5[14].position;
                break;
        }

        foreach(Transform die in diceToPlace)
            die.rotation = Quaternion.identity;
    }
}
