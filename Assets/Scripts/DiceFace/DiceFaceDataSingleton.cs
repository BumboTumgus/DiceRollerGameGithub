using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class DiceFaceDataSingleton : MonoBehaviour
{
    [SerializeField] private DiceFaceData[] _diceFaceDataBank;

    public static DiceFaceDataSingleton Instance;
    
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        if (Instance != this)
            Destroy(this);
    }

    public DiceFaceData GetDiceFaceDataByName(string diceFaceName)
    {
        foreach(DiceFaceData diceFaceData in _diceFaceDataBank)
            if(diceFaceData.DiceFaceEnum.ToString() == diceFaceName)
                return diceFaceData;

        Debug.LogError("Dice Face of name " + diceFaceName + " does not exist");
        return null;
    }

    public DiceFaceData GetDiceFaceDataByType(DiceFaceData.DiceFace diceFace)
    {
        foreach(DiceFaceData diceFaceData in _diceFaceDataBank)
            if(diceFaceData.DiceFaceEnum == diceFace)
                return diceFaceData;

        return null;
    }
}
