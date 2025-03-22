using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UiErrorPopup : MonoBehaviour
{
    private const string ANIMAION_APPEAR_NAME = "UiErrorPopup_Appear";
    private const float TEXT_PADDING = 10;
    private const float INNER_PADDING = 40;

    [SerializeField] private TMP_Text _errorText;
    [SerializeField] private RectTransform _panelBase;
    [SerializeField] private RectTransform _panelOutline;

    private Animation _animation;

    private void Start()
    {
        _animation = GetComponent<Animation>();
        GetComponent<CanvasGroup>().alpha = 0;
    }

    public void SetText(string text)
    {
        _errorText.text = text;
        float prefWidth = _errorText.preferredWidth;
        float prefHeight = _errorText.preferredHeight;

        _errorText.transform.GetComponent<RectTransform>().sizeDelta = new Vector2(prefWidth + TEXT_PADDING, prefHeight + TEXT_PADDING);
        _panelBase.sizeDelta = new Vector2(prefWidth + TEXT_PADDING + INNER_PADDING * 2, prefHeight + TEXT_PADDING + INNER_PADDING);
        _panelOutline.sizeDelta = new Vector2((prefWidth + TEXT_PADDING + INNER_PADDING * 2) / 4, (prefHeight + TEXT_PADDING + INNER_PADDING) / 4);
    }

    public void ShowWarning()
    {
        _animation.Play(ANIMAION_APPEAR_NAME);
    }
}
