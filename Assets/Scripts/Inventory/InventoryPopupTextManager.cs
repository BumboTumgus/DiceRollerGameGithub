using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryPopupTextManager : MonoBehaviour
{
    private const float POPUP_OFFSET = 10;

    public static InventoryPopupTextManager Instance;

    public bool LockPointer = false;
    public bool AllowPopupsToAppear = true;
    public enum PopUpDirection { TL, TR, BL, BR }
    public GameObject itemPopUp;
    public GameObject moreInfoPopUp;
    public Color[] itemOutlineColors;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        if (Instance != this)
            Destroy(this);
    }

    // Start is called before the first frame update
    void Start()
    {
        return;
        itemPopUp = transform.Find("ItemPopUp").gameObject;
        //itemPopUp.GetComponent<UiItemPopUpResizer>().Initialize();
        itemPopUp.SetActive(false);

        moreInfoPopUp = transform.Find("ItemAdvancedDescription").gameObject;
        //moreInfoPopUp.GetComponent<UiMoreInfoPopup>().Initialize();
        moreInfoPopUp.SetActive(false);
    }
    
    // Used to enable the popup.
    public void ShowPopup(Transform popUpParent, PopUpDirection direction)
    {
        return;
        if (!AllowPopupsToAppear)
        {
            //Debug.Log("This popup cannot be shown right now");
            return;
        }

        itemPopUp.transform.SetParent(popUpParent);
        itemPopUp.transform.SetAsLastSibling();
        moreInfoPopUp.transform.SetAsLastSibling();
        itemPopUp.transform.localPosition = Vector3.zero;

        //Hide the hotbar number if there is one in the parent.
        if (itemPopUp.transform.parent.parent.Find("HotbarNumber") != null)
            itemPopUp.transform.parent.parent.Find("HotbarNumber").gameObject.SetActive(false);

        //itemPopUp.GetComponent<UiItemPopUpResizer>().ShowPopUp(popUpParent.GetComponent<DiceFaceDraggable>().attachedItem.GetComponent<Item>());

        //SetPopUpStats(popUpParent.GetComponent<ItemDraggable>().attachedItem.GetComponent<Item>());

        RectTransform popUpRect = itemPopUp.GetComponent<RectTransform>();
        switch (direction)
        {
            case PopUpDirection.TL:
                popUpRect.anchorMin = new Vector2(1, 0);
                popUpRect.anchorMax = new Vector2(1, 0);
                popUpRect.pivot = new Vector2(1, 0);
                popUpRect.anchoredPosition = new Vector3(POPUP_OFFSET, -POPUP_OFFSET, 0);
                break;
            case PopUpDirection.TR:
                popUpRect.anchorMin = new Vector2(0, 0);
                popUpRect.anchorMax = new Vector2(0, 0);
                popUpRect.pivot = new Vector2(0, 0);
                popUpRect.anchoredPosition = new Vector3(-POPUP_OFFSET, -POPUP_OFFSET, 0);
                break;
            case PopUpDirection.BL:
                popUpRect.anchorMin = new Vector2(1, 1);
                popUpRect.anchorMax = new Vector2(1, 1);
                popUpRect.pivot = new Vector2(1, 1);
                popUpRect.anchoredPosition = new Vector3(POPUP_OFFSET, POPUP_OFFSET, 0);
                break;
            case PopUpDirection.BR:
                popUpRect.anchorMin = new Vector2(0, 1);
                popUpRect.anchorMax = new Vector2(0, 1);
                popUpRect.pivot = new Vector2(0, 1);
                popUpRect.anchoredPosition = new Vector3(-POPUP_OFFSET, POPUP_OFFSET, 0);
                break;
            default:
                break;
        }

        itemPopUp.SetActive(true);
    }

    // USed to show the advanced info popup
    public void ShowMoreInfoPopup(Transform popUpParent)
    {
        return;

        moreInfoPopUp.transform.SetAsLastSibling();
        //moreInfoPopUp.GetComponent<UiMoreInfoPopup>().ShowPopUp(popUpParent.GetComponent<DiceFaceDraggable>().attachedItem.GetComponent<Item>());
        moreInfoPopUp.SetActive(true);
    }

    // Used to hide the Popups
    public void HidePopups(bool hideMoreInfoPanel)
    {
        return;

        //Show the hotbar number if there is one in the parent.
        if (itemPopUp.transform.parent.parent.Find("HotbarNumber") != null)
            itemPopUp.transform.parent.parent.Find("HotbarNumber").gameObject.SetActive(true);

        if (!LockPointer)
         itemPopUp.SetActive(false);

        if(hideMoreInfoPanel)
            moreInfoPopUp.SetActive(false);

        //foreach (MoreInfoPopUpManager panel in moreInfoPanels)
        //    panel.HideElements();
    }
    
}
