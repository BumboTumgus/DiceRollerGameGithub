using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UiDiceFaceForgeExecutor : MonoBehaviour
{
    [SerializeField] private Sprite _noDiceFaceSprite;
    [SerializeField] private Color _noDiceFaceColor;
    [SerializeField] private Image _uiDiceFaceImage;
    private DiceFaceData _attachedDiceFaceData;

    public void AddDiceFaceToForgeList(DiceFaceData diceFaceToAddToForge)
    {
        if(_attachedDiceFaceData != null)
        {
            PlayerInventorySingleton.Instance.AddDiceFaceToInventory(_attachedDiceFaceData);
            RestingSingleton.Instance.DiceFacesUsed.Remove(_attachedDiceFaceData);
        }

        RestingSingleton.Instance.DiceFacesUsed.Add(diceFaceToAddToForge);
        _attachedDiceFaceData = diceFaceToAddToForge;

        _uiDiceFaceImage.sprite = diceFaceToAddToForge.DiceFaceUiSprite;
    }

    public void ResetDiceFaceForgeExecutor()
    {
        _attachedDiceFaceData = null;
        _uiDiceFaceImage.sprite = _noDiceFaceSprite;
        _uiDiceFaceImage.color = _noDiceFaceColor;
    }
}
