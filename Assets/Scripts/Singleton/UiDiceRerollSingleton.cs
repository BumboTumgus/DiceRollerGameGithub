using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UiDiceRerollSingleton : MonoBehaviour
{
    private const string NOT_ENOUGH_REROLLS_ANIM_CODE = "Ui_RerollCounter_NotEnough";

    public static UiDiceRerollSingleton Instance;
    
    [SerializeField] private TMP_Text _rerollCounterText;
    [SerializeField] private Animation _animation;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        if (Instance != this)
            Destroy(this);
    }

    public void OnSetRerollCounter(int value)
    {
        _rerollCounterText.text = value + "";
    }

    public void OnNotEnoughRerolls()
    {
        _animation.Play(NOT_ENOUGH_REROLLS_ANIM_CODE);
    }
}
