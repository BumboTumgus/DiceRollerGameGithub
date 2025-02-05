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

    [SerializeField] private Image _buffIcon;
    [SerializeField] private TMP_Text _buffStackCount;
    [SerializeField] private Animation _buffAnimation;
    
    private UiBuffDescriptionController _buffDescriptionController;

    private int _currentBuffCount = 0;
    private BuffScriptableObject _connectedBuff;

    public void InitializeWithBuff(BuffScriptableObject buffToInitialize, int initialStrength, UiBuffDescriptionController buffDescriptionController)
    {
        _connectedBuff = buffToInitialize;
        _buffIcon.sprite = buffToInitialize.BuffIcon;
        _buffStackCount.text = initialStrength + "";
        if (!buffToInitialize.BuffStackingAllowed)
            _buffStackCount.gameObject.SetActive(false);

        _buffDescriptionController = buffDescriptionController;

        _buffAnimation.Play(BUFF_INCREASED_ANIM_KEY);
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
        _buffDescriptionController.InitializeWithBuffAndShow(_connectedBuff, transform);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Debug.Log("Hiding Description");
        _buffDescriptionController.HideDescription();
    }
}
