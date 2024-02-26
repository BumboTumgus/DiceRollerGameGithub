using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UiDiceSummarySingleton : MonoBehaviour
{
    private const float DICE_SUMMARY_WINDOW_Y = 140f;

    public static UiDiceSummarySingleton Instance;

    [SerializeField] private GameObject _diceSummaryWindow;
    [SerializeField] private Image[] _diceSummaryFaceIcons;
    [SerializeField] private LayerMask _diceLayerMask;
    
    private Transform _currentHoveredDice;
    private bool _showSummaryWindows = false;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        if (Instance != this)
            Destroy(this);

        _diceSummaryWindow.SetActive(false);
    }

    private void Update()
    {
        if(_showSummaryWindows)
            HighlightHoveredDice();
    }
    
    private void HighlightHoveredDice()
    {
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit rayhit, 10f, _diceLayerMask))
        {
            if(_currentHoveredDice != rayhit.transform)
            {                    
                _currentHoveredDice = rayhit.transform;
                _diceSummaryWindow.SetActive(true);
                SetWindowLocation(_currentHoveredDice.position);
                if(_currentHoveredDice.GetComponent<DiceRollingBehaviour>() != null)
                    SetWindowFaces(_currentHoveredDice.GetComponent<DiceRollingBehaviour>().DiceFaces);
            }
        }
        else if (_currentHoveredDice != null)
        {
            _diceSummaryWindow.SetActive(false);
            _currentHoveredDice = null;
        }
    }

    public void SetWindowVisibility(bool visible)
    {
        _showSummaryWindows = visible;
        _diceSummaryWindow.SetActive(false);
    }

    private void SetWindowLocation(Vector3 diceWorldPoint)
    {
        Vector3 screenPoint = Camera.main.WorldToScreenPoint(diceWorldPoint);
        screenPoint.y += DICE_SUMMARY_WINDOW_Y;
        _diceSummaryWindow.transform.position = screenPoint;
    }

    private void SetWindowFaces(DiceFaceBehaviour[] diceFaces)
    {
        for(int index = 0; index < _diceSummaryFaceIcons.Length; index++)
        {
            _diceSummaryFaceIcons[index].sprite = diceFaces[index].MyDiceFaceData.DiceFaceUiSprite;
            _diceSummaryFaceIcons[index].color = diceFaces[index].MyDiceFaceData.DiceFaceUiColor;
        }
    }
}
