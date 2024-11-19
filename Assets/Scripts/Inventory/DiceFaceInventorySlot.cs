using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DiceFaceInventorySlot : MonoBehaviour
{
    public DiceFaceDraggable ConnectedDiceFaceDraggable;
    public Image DiceFaceImage;

    public void SetImageToDiceFaceData(DiceFaceData diceFaceData)
    {
        DiceFaceImage.sprite = diceFaceData.DiceFaceUiSprite;
        DiceFaceImage.color = diceFaceData.DiceFaceUiColor;
    }

    public void WipeSlot()
    {
        if (ConnectedDiceFaceDraggable != null)
        {
            ConnectedDiceFaceDraggable.enabled = false;
            ConnectedDiceFaceDraggable.AttachedDiceFaceData = null;
        }

        DiceFaceImage.sprite = null;
        DiceFaceImage.color = new Vector4(255, 255, 255, 0);
    }
}
