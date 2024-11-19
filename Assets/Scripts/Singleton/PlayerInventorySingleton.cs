using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerInventorySingleton : MonoBehaviour
{
    public static PlayerInventorySingleton Instance;

    private const float UI_SLIDE_TARGET_HIDDEN = 650f;
    private const float UI_SLIDE_TARGET_BASE = 330f;
    private const float UI_SLIDE_TARGET_INCREMENT = 165f;
    private const float UI_SLIDE_SNAP_DISTANCE = 2f;
    private const float UI_SLIDE_SPEED = 0.1f;

    public List<DiceFaceData> CollectedDiceFaces { get => _collectedDiceFaces; }
    public int CollectedGold { get => _collectedGold; set => _collectedGold = value; }

    [SerializeField] private RectTransform _inventorySlideParent;

    private List<DiceFaceData> _collectedDiceFaces = new List<DiceFaceData>();
    private int _collectedGold = 0;
    //private int _maximumInventorySize = 5;

    private bool _inventoryOpened = false;
    private float _inventorySlideTargetDistance = 0;
    private float _inventoryCurrentSlideDistance = 0;
    private float _inventoryColumnCount = 3;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        if (Instance != this)
            Destroy(this);
    }

    private void Start()
    {
        SetInventoryOpenStatus(false);
        _inventorySlideParent.localPosition = new Vector2(UI_SLIDE_TARGET_HIDDEN, _inventorySlideParent.localPosition.y);
        _inventoryCurrentSlideDistance = UI_SLIDE_TARGET_HIDDEN;
        _inventorySlideTargetDistance = UI_SLIDE_TARGET_HIDDEN;
    }

    private void Update()
    {
        SlideToTarget();
    }

    public void AddDiceFaceToInventory(DiceFaceData diceFace)
    {
        _collectedDiceFaces.Add(diceFace);
    }

    public void UiButtonPress_OpenInventory()
    {
        SetInventoryOpenStatus(true);
    }

    public void UiButtonPress_CloseInventory()
    {
        SetInventoryOpenStatus(false);
    }

    private void SetInventoryOpenStatus(bool inventoryOpened)
    {
        if (_inventoryOpened == inventoryOpened)
            return;

        _inventoryOpened = inventoryOpened;
        if (_inventoryOpened)
            _inventorySlideTargetDistance = UI_SLIDE_TARGET_BASE - (_inventoryColumnCount - 1) * UI_SLIDE_TARGET_INCREMENT;
        else
            _inventorySlideTargetDistance = UI_SLIDE_TARGET_HIDDEN;
    }

    private void SlideToTarget()
    {
        if (_inventoryCurrentSlideDistance == _inventorySlideTargetDistance)
            return;

        _inventoryCurrentSlideDistance = Mathf.Lerp(_inventoryCurrentSlideDistance, _inventorySlideTargetDistance, UI_SLIDE_SPEED);

        if(Mathf.Abs(_inventoryCurrentSlideDistance - _inventorySlideTargetDistance) < UI_SLIDE_SNAP_DISTANCE)
            _inventoryCurrentSlideDistance = _inventorySlideTargetDistance;

        _inventorySlideParent.localPosition = new Vector2(_inventoryCurrentSlideDistance, _inventorySlideParent.localPosition.y);
    }
}
