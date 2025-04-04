using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UiBuffDescriptionController : MonoBehaviour
{
    private const float PARENT_SIZE_BUFFER = 35f;
    private const float OUTLINE_SCALE = 4;

    [SerializeField] private TMP_Text _buffDescriptionText;
    [SerializeField] private RectTransform _buffDescriptionParent;
    [SerializeField] private RectTransform _buffOutline;
    [SerializeField] private RectTransform _buffFade;
    [SerializeField] private Image _buffDescriptionIcon;

    public void InitializeWithBuffAndShow(BuffScriptableObject buffToInitialize, Transform target)
    {
        _buffDescriptionParent.gameObject.SetActive(true);
        _buffDescriptionParent.transform.position = target.position;

        _buffDescriptionText.text = buffToInitialize.BuffDescription;
        _buffDescriptionText.rectTransform.sizeDelta = new Vector2(_buffDescriptionText.rectTransform.sizeDelta.x, _buffDescriptionText.preferredHeight);
        _buffDescriptionParent.sizeDelta = new Vector2(_buffDescriptionParent.sizeDelta.x, _buffDescriptionText.preferredHeight + PARENT_SIZE_BUFFER);
        _buffDescriptionIcon.sprite = buffToInitialize.BuffIcon;
        _buffOutline.sizeDelta = _buffDescriptionParent.sizeDelta / OUTLINE_SCALE;
        _buffFade.sizeDelta = _buffDescriptionParent.sizeDelta;
    }

    public void HideDescription()
    {
        _buffDescriptionParent.gameObject.SetActive(false);
    }
}
