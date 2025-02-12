using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuffSingleton : MonoBehaviour
{
    [SerializeField] private BuffScriptableObject[] _buffBank;

    public static BuffSingleton Instance;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        if (Instance != this)
            Destroy(this);
    }

    public BuffScriptableObject GetBuffDataDataByName(string buffName)
    {
        foreach (BuffScriptableObject buffData in _buffBank)
            if (buffData.MyBuffType.ToString() == buffName)
                return buffData;

        Debug.LogError("BuffType of of name " + buffName + " does not exist");
        return null;
    }

    public BuffScriptableObject GetBuffDataByType(BuffScriptableObject.BuffType buffType)
    {
        foreach (BuffScriptableObject buffData in _buffBank)
            if (buffData.MyBuffType == buffType)
                return buffData;

        return null;
    }

    public string GetRandomDiceFaceDataIdentifier()
    {
        return _buffBank[Random.Range(0, _buffBank.Length)].MyBuffType.ToString();
    }
}
