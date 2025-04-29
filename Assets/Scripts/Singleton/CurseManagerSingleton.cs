using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CurseManagerSingleton : MonoBehaviour
{
    public static CurseManagerSingleton Instance;

    [SerializeField] private DiceFaceData[] _cursedFaces;

    public bool WardedFromCurses = false;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        if (Instance != this)
            Destroy(this);
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.C))
            AddRandomCurseFace();
    }

    public void AddRandomCurseFace(bool curseCleansedAfterCombat = false)
    {
        for (int dieRollIndex = 0; dieRollIndex < 3; dieRollIndex++)
        {
            DiceRollingBehaviour dieToCurse = DiceRollerSingleton.Instance.CurrentDice[Random.Range(0, DiceRollerSingleton.Instance.CurrentDice.Count)];
            if (dieToCurse.RemovedFromActiveCombat)
                continue;

            for (int diceFaceRollIndex = 0; diceFaceRollIndex < 3; diceFaceRollIndex++)
            {
                DiceFaceBehaviour diceFaceToCurse = dieToCurse.DiceFaces[Random.Range(0, dieToCurse.DiceFaces.Length)];
                if (diceFaceToCurse.MyTempDiceFaceData == null)
                {
                    diceFaceToCurse.TemporarySwitchDiceFace(_cursedFaces[Random.Range(0, _cursedFaces.Length)], curseCleansedAfterCombat);
                    return;
                }
            }

            foreach (DiceFaceBehaviour diceFace in dieToCurse.DiceFaces)
            {
                if (diceFace.MyTempDiceFaceData == null)
                {
                    diceFace.TemporarySwitchDiceFace(_cursedFaces[Random.Range(0, _cursedFaces.Length)], curseCleansedAfterCombat);
                    return;
                }
            }
        }

        foreach(DiceRollingBehaviour diceRollingBehaviour in DiceRollerSingleton.Instance.CurrentDice)
        {
            if (diceRollingBehaviour.RemovedFromActiveCombat)
                continue;

            foreach (DiceFaceBehaviour diceFace in diceRollingBehaviour.DiceFaces)
            {
                if (diceFace.MyTempDiceFaceData == null)
                {
                    diceFace.TemporarySwitchDiceFace(_cursedFaces[Random.Range(0, _cursedFaces.Length)], curseCleansedAfterCombat);
                    return;
                }
            }
        }
    }

    public void RemoveAllCurses()
    {
        foreach(DiceRollingBehaviour die in DiceRollerSingleton.Instance.CurrentDice)
            foreach(DiceFaceBehaviour dieFace in die.DiceFaces)
                dieFace.RevertToOriginalDiceFace();
    }
}
