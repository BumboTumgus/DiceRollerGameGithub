using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerInventorySingleton : MonoBehaviour
{
    public static PlayerInventorySingleton Instance;

    public List<DiceFaceData> CollectedDiceFaces { get => _collectedDiceFaces; }
    public int CollectedGold { get => _collectedGold; set => _collectedGold = value; }

    private List<DiceFaceData> _collectedDiceFaces = new List<DiceFaceData>();
    private int _collectedGold = 0;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        if (Instance != this)
            Destroy(this);
    }

    public void AddDiceFaceToInventory(DiceFaceData diceFace)
    {
        _collectedDiceFaces.Add(diceFace);
    }
}
