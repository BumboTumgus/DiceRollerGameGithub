using System.Collections;
using System.Collections.Generic;
using UnityEditor.EditorTools;
using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class DiceHighlightingBehaviour : MonoBehaviour
{
    private const string OUTLINE_PROPERTY_NAME = "_OutlineColor";

    [SerializeField] private Color _baseOutlineColor;
    [SerializeField] private Color _highLightedOutlineColor;
    [SerializeField] private Color _selectedOutlineColor;

    private MaterialPropertyBlock _materialPropertyBlock;
    private Renderer _renderer;
    private bool _currentlySelected = false;
    private bool _currentlyHighlighted = false;

    private void Awake()
    {
        _materialPropertyBlock = new MaterialPropertyBlock();
        _renderer = GetComponent<Renderer>();
        SetHighlightStatus(false);
    }

    public void SetHighlightStatus(bool highlighted)
    {
        _currentlyHighlighted = highlighted;

        if(_currentlySelected)
            return;

        Color outlineColor = highlighted ? _highLightedOutlineColor : _baseOutlineColor;
        _materialPropertyBlock.SetColor(OUTLINE_PROPERTY_NAME, outlineColor);
        _renderer.SetPropertyBlock(_materialPropertyBlock);
    }

    public void SetSelectionStatus(bool selected)
    {
        _currentlySelected = selected;

        Color outlineColor = _baseOutlineColor;
        if(_currentlySelected)
            outlineColor = _selectedOutlineColor;
        else if(_currentlyHighlighted)
            outlineColor = _highLightedOutlineColor;

        _materialPropertyBlock.SetColor(OUTLINE_PROPERTY_NAME, outlineColor);
        _renderer.SetPropertyBlock(_materialPropertyBlock);
    }

    public void ClearStatus()
    {
        _currentlyHighlighted = false;
        _currentlySelected = false;
        
        _materialPropertyBlock.SetColor(OUTLINE_PROPERTY_NAME, _baseOutlineColor);
        _renderer.SetPropertyBlock(_materialPropertyBlock);
    }
}
