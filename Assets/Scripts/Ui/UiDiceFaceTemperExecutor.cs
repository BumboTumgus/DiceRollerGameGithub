using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UiDiceFaceTemperExecutor : MonoBehaviour
{
    [SerializeField] private DiceFaceViewerController _controller;
    private Image _uiDiceFaceImage;

    private void Start()
    {
        _uiDiceFaceImage = GetComponent<Image>();
    }

    public void UpdateDiceFaceAtIndex(int index, DiceFaceData tempDiceFaceFromTemper)
    {
        RestingSingleton.Instance.DiceFacesUsed.Add(tempDiceFaceFromTemper);

        _controller.RotateDieToTargetRotation(index);

        if (_controller.ConnectedDie.DiceFaces[index].MyTempDiceFaceData != null)
        {
            for(int diceFacesUsedIndex = 0; diceFacesUsedIndex < RestingSingleton.Instance.DiceFacesUsed.Count; diceFacesUsedIndex++) 
            {
                if(_controller.ConnectedDie.DiceFaces[index].MyTempDiceFaceData == RestingSingleton.Instance.DiceFacesUsed[diceFacesUsedIndex])
                {
                    PlayerInventorySingleton.Instance.AddDiceFaceToInventory(RestingSingleton.Instance.DiceFacesUsed[diceFacesUsedIndex]);
                    RestingSingleton.Instance.DiceFacesUsed.RemoveAt(diceFacesUsedIndex);
                }
            }
        }

        _controller.ConnectedDie.DiceFaces[index].TemporarySwitchDiceFace(tempDiceFaceFromTemper);
        _uiDiceFaceImage.sprite = tempDiceFaceFromTemper.DiceFaceUiSprite;
    }
}
