using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static DiceRollerSingleton;

public class UiCombatDebuffPickTargetTooltipSingleton : MonoBehaviour
{
    public static UiCombatDebuffPickTargetTooltipSingleton Instance;

    [SerializeField] private GameObject _tooltipParent;
    [SerializeField] private TMP_Text _tooltipTitleText;
    [SerializeField] private TMP_Text _tooltipDescriptionText;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        if (Instance != this)
            Destroy(this);

        HideTooltip();
    }

    public void HideTooltip()
    {
        _tooltipParent.SetActive(false);
    }

    public void ShowTooltip(BuffScriptableObject debuffInfo, int value)
    {
        _tooltipParent.SetActive(true);
        _tooltipTitleText.text = "Choose a target: <sprite name=\"" + debuffInfo.MyBuffType.ToString() + "\"> " + debuffInfo.MyBuffType.ToString();
        if (value > 1)
            _tooltipTitleText.text += " x" + value;
    }
}
