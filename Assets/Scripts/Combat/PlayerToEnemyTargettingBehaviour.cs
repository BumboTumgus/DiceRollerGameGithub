using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerToEnemyTargettingBehaviour : MonoBehaviour
{
    private const string OUTLINE_PROPERTY_NAME = "_OutlineColor";

    [SerializeField] private Color _highLightedOutlineColor;
    
    [SerializeField] private Color[] _baseOutlineColors;
    [SerializeField] private MaterialPropertyBlock[] _materialPropertyBlocks;
    [SerializeField] private Renderer[] _renderers;
    
    private void Awake()
    {
        _renderers = GetComponentsInChildren<Renderer>();
        _materialPropertyBlocks = new MaterialPropertyBlock[_renderers.Length];
        for(int propblockIndex = 0; propblockIndex < _materialPropertyBlocks.Length - 1; propblockIndex++)
            _materialPropertyBlocks[propblockIndex] = new MaterialPropertyBlock();
        _baseOutlineColors = new Color[_renderers.Length];
        SetHighlightStatus(false);
    }

    public void SetHighlightStatus(bool highlighted)
    {
        for(int renderIndex = 0; renderIndex < _renderers.Length - 1; renderIndex++)
        {
            Color outlineColor = highlighted ? _highLightedOutlineColor : _baseOutlineColors[renderIndex];
            _materialPropertyBlocks[renderIndex].SetColor(OUTLINE_PROPERTY_NAME, outlineColor);
            _renderers[renderIndex].SetPropertyBlock(_materialPropertyBlocks[renderIndex]);
        }
    }
}
