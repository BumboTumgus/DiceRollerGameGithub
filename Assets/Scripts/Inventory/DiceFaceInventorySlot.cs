using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DiceFaceInventorySlot : MonoBehaviour
{
    public void UpdateSlot(DiceFaceData diceFaceData)
    {
        Debug.LogFormat("CHANING THE INVENTORY SLOT {0} WITH A PARENT OF {1}", gameObject.name, transform.parent);
        DiceFaceDraggable connectedDiceFaceDraggable = GetComponentInChildren<DiceFaceDraggable>();
        Image connectedDiceFaceImage = GetComponentInChildren<Image>();

        if (connectedDiceFaceDraggable != null)
        {
            connectedDiceFaceDraggable.enabled = true;
            connectedDiceFaceDraggable.AttachedDiceFaceData = diceFaceData;
        }

        connectedDiceFaceImage.sprite = diceFaceData.DiceFaceUiSprite;
        connectedDiceFaceImage.color = diceFaceData.DiceFaceUiColor;
    }

    public void WipeSlot()
    {
        DiceFaceDraggable connectedDiceFaceDraggable = GetComponentInChildren<DiceFaceDraggable>();
        Image connectedDiceFaceImage = GetComponentInChildren<Image>();

        if (connectedDiceFaceDraggable != null)
        {
            connectedDiceFaceDraggable.enabled = false;
            connectedDiceFaceDraggable.AttachedDiceFaceData = null;
        }

        connectedDiceFaceImage.sprite = null;
        connectedDiceFaceImage.color = new Vector4(255, 255, 255, 0);
    }
}
