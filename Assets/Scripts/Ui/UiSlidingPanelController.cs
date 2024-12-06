using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class UiSlidingPanelController : MonoBehaviour
{
    public UnityEvent OnPanelSuccessfullyClosed_Callback;
    public UnityEvent OnPanelSuccessfullyOpened_Callback;
    public float SlideTargetShown { get => _slideTargetShown; set => _slideTargetShown = value; }
    public float SlideTargetHidden { get => _slideTargetHidden; set => _slideTargetHidden = value; }

    [SerializeField] private float _slideTargetHidden = 0;
    [SerializeField] private float _slideTargetShown = 0;
    [SerializeField] private float _slideSnapDistance = 2f;
    [SerializeField] private float _slideSpeed = 0.1f;
    [SerializeField] private RectTransform _panelSlideParent;

    private bool _panelOpened = false;
    private float _panelSlideTargetDistance = 0;
    private float _panelCurrentSlideDistance = 0;


    private void Awake()
    {
        _panelSlideParent.localPosition = new Vector2(_slideTargetHidden, _panelSlideParent.localPosition.y);
        _panelCurrentSlideDistance = _slideTargetHidden;
        _panelSlideTargetDistance = _slideTargetHidden;
    }

    private void Update()
    {
        SlideToTarget();
    }

    public void SetPanelOpenStatus(bool panelOpened)
    {
        if (_panelOpened == panelOpened)
            return;

        _panelOpened = panelOpened;
        if (_panelOpened)
        {
            _panelSlideTargetDistance = _slideTargetShown;
            OnPanelSuccessfullyOpened_Callback.Invoke();
        }
        else
        {
            _panelSlideTargetDistance = _slideTargetHidden;
            OnPanelSuccessfullyClosed_Callback.Invoke();
        }
    }

    private void SlideToTarget()
    {
        if (_panelCurrentSlideDistance == _panelSlideTargetDistance)
            return;

        _panelCurrentSlideDistance = Mathf.Lerp(_panelCurrentSlideDistance, _panelSlideTargetDistance, _slideSpeed);

        if (Mathf.Abs(_panelCurrentSlideDistance - _panelSlideTargetDistance) < _slideSnapDistance)
            _panelCurrentSlideDistance = _panelSlideTargetDistance;

        _panelSlideParent.localPosition = new Vector2(_panelCurrentSlideDistance, _panelSlideParent.localPosition.y);
    }
}
