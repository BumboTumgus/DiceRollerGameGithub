using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerInventorySingleton : MonoBehaviour
{
    private const int MAXIMUM_INVENTORY_SIZE = 15;

    public static PlayerInventorySingleton Instance;

    public DiceFaceData[] CollectedDiceFaces { get => _collectedDiceFaces; }
    public int CollectedGold { get => _collectedGold; }

    public enum PlayableCharacters { None, Warrior }
    public enum PickableGods { None, TheVoid, TheWanderer, TheOracle, TheJester, TheFool }
    public PlayableCharacters SelectedCharacter;
    public PickableGods SelectedGod;

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
        UpdateGoldValue(0);
    }

    private void Start()
    {
        InventoryUiManagerSingleton.Instance.SetInventoryUiBasedOnMaxSpace(_currentMaxInventorySize);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
            UpdateGoldValue(_collectedGold + 100);
        if (Input.GetKeyDown(KeyCode.T))
            UpdateGoldValue(_collectedGold - 100);
        if (Input.GetKeyDown(KeyCode.Alpha8))
            AddDiceFaceToInventory(_diceFaceToAdd[0]);
        if (Input.GetKeyDown(KeyCode.Alpha9))
            AddDiceFaceToInventory(_diceFaceToAdd[1]);
        if (Input.GetKeyDown(KeyCode.Alpha0))
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

    public void SwapDiceFacesInventoryIndexes(int index1, int index2)
    {
        DiceFaceData tempDiceFace = _collectedDiceFaces[index1];
        _collectedDiceFaces[index1] = _collectedDiceFaces[index2];
        _collectedDiceFaces[index2] = tempDiceFace;
    }

    public void RemoveDiceFaceAtIndex(int index)
    {
        _collectedDiceFaces[index] = null;
        InventoryUiManagerSingleton.Instance.DiceFaceDataInventorySlots[index].WipeSlot();
    }

    public void UpdateGoldValue(int newGoldValue)
    {
        _collectedGold = newGoldValue;
        InventoryUiManagerSingleton.Instance.SetGoldReadoutValue(_collectedGold);
    }

    public bool RoomInInventory()
    {
        int count = 0;
        foreach (DiceFaceData diceFaceData in _collectedDiceFaces)
            if (diceFaceData != null)
                count++;

        return count < _currentMaxInventorySize;
    }
}
