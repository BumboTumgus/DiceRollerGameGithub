using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UiBuffIcon : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private const string BUFF_INCREASED_ANIM_KEY = "Ui_BuffIncrease";
    private const string BUFF_DECREASED_ANIM_KEY = "Ui_BuffDecrease";
    private const float PARENT_SIZE_BUFFER = 20f;

    [SerializeField] private Image _buffIcon;
    [SerializeField] private TMP_Text _buffStackCount;
    [SerializeField] private TMP_Text _buffDescriptionText;
    [SerializeField] private RectTransform _buffDescriptionParent;
    [SerializeField] private Image _buffDescriptionIcon;
    [SerializeField] private Animation _buffAnimation;

    private int _currentBuffCount = 0;

    public void InitializeWithBuff(BuffScriptableObject buffToInitialize, int initialStrength)
    {
        _buffIcon.sprite = buffToInitialize.BuffIcon;
        _buffStackCount.text = initialStrength + "";
        if (!buffToInitialize.BuffStackingAllowed)
            _buffStackCount.gameObject.SetActive(false);

        _buffDescriptionText.text = buffToInitialize.BuffDescription;
        _buffDescriptionText.rectTransform.sizeDelta = new Vector2(_buffDescriptionText.rectTransform.sizeDelta.x, _buffDescriptionText.preferredHeight);
        _buffDescriptionParent.sizeDelta = new Vector2(_buffDescriptionParent.sizeDelta.x, _buffDescriptionText.preferredHeight + PARENT_SIZE_BUFFER);
        _buffDescriptionParent.gameObject.SetActive(false);
    }

    public void IncrementBuffCount(int buffValue)
    {
        if (buffValue == _currentBuffCount)
            return;

        if(buffValue > _currentBuffCount)
            _buffAnimation.Play(BUFF_INCREASED_ANIM_KEY);
        else
            _buffAnimation.Play(BUFF_DECREASED_ANIM_KEY);

        _buffStackCount.text = buffValue + "";
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log("Showing Description");
        _buffDescriptionParent.gameObject.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Debug.Log("Hiding Description");
        _buffDescriptionParent.gameObject.SetActive(false);
    }
}
