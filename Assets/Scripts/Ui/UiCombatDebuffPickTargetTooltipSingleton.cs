using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static DiceRollerSingleton;

public class UiCombatDebuffPickTargetTooltipSingleton : MonoBehaviour
{
    private const float DESCRIPTION_PADDING = 30f;

    public static UiCombatDebuffPickTargetTooltipSingleton Instance;

    [SerializeField] private RectTransform _tooltipParent;
    [SerializeField] private RectTransform _tooltipOutline;
    [SerializeField] private TMP_Text _tooltipTitleText;
    [SerializeField] private TMP_Text _tooltipDescriptionText;
    [SerializeField] private RectTransform _tooltipDivider;

    private float _tooltipOutlineScale = 1;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        if (Instance != this)
            Destroy(this);

        _tooltipOutlineScale = _tooltipOutline.localScale.x;
        HideTooltip();
    }

    public void HideTooltip()
    {
        _tooltipParent.gameObject.SetActive(false);
    }

    public void ShowTooltip(BuffScriptableObject debuffInfo, int value)
    {
        _tooltipParent.gameObject.SetActive(true);
        _tooltipTitleText.text = "Choose a target: <sprite name=\"" + debuffInfo.MyBuffType.ToString() + "\"> " + debuffInfo.MyBuffType.ToString();
        if (value > 1)
            _tooltipTitleText.text += " x" + value;

        _tooltipDescriptionText.text = debuffInfo.BuffDescription;
        _tooltipDescriptionText.rectTransform.sizeDelta = new Vector2(_tooltipDescriptionText.rectTransform.sizeDelta.x, _tooltipDescriptionText.preferredHeight + DESCRIPTION_PADDING);

        _tooltipParent.sizeDelta = new Vector2(_tooltipParent.sizeDelta.x, _tooltipTitleText.rectTransform.sizeDelta.y + _tooltipDivider.sizeDelta.y + _tooltipDescriptionText.rectTransform.sizeDelta.y);
        _tooltipOutline.sizeDelta = _tooltipParent.sizeDelta / _tooltipOutlineScale;
    }
}
