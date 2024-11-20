using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerInventorySingleton : MonoBehaviour
{
    private const int MAXIMUM_INVENTORY_SIZE = 15;

    public static PlayerInventorySingleton Instance;

    public DiceFaceData[] CollectedDiceFaces { get => _collectedDiceFaces; }
    public int CollectedGold { get => _collectedGold; set => _collectedGold = value; }

    [SerializeField] private DiceFaceData[] _collectedDiceFaces;
    private int _collectedGold = 0;
    private int _currentMaxInventorySize = 5;
    [SerializeField] private DiceFaceData[] _diceFaceToAdd;


    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        if (Instance != this)
            Destroy(this);

        _collectedDiceFaces = new DiceFaceData[MAXIMUM_INVENTORY_SIZE];
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
            AddDiceFaceToInventory(_diceFaceToAdd[0]);
        if (Input.GetKeyDown(KeyCode.W))
            AddDiceFaceToInventory(_diceFaceToAdd[1]);
        if (Input.GetKeyDown(KeyCode.E))
            AddDiceFaceToInventory(_diceFaceToAdd[2]);
    }

    public void AddDiceFaceToInventory(DiceFaceData diceFace)
    {
        int inventoryIndex = GetNextOpenInventoryIndex();
        _collectedDiceFaces[inventoryIndex] = diceFace;
        InventoryUiManagerSingleton.Instance.UpdateInventorySlot(inventoryIndex);
    }

    private int GetNextOpenInventoryIndex()
    {
        for(int inventoryIndex = 0; inventoryIndex < MAXIMUM_INVENTORY_SIZE; inventoryIndex++)
            if (_collectedDiceFaces[inventoryIndex] == null)
                return inventoryIndex;

        return MAXIMUM_INVENTORY_SIZE;
    }
}
