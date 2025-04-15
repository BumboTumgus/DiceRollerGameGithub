using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.ShaderGraph.Drawing;
using UnityEngine;
using UnityEngine.UI;

public class GameSetupController : MonoBehaviour
{
    [SerializeField] private PlayableCharacter[] _playableCharacters;
    [SerializeField] private PickableGod[] _pickableGods;

    [SerializeField] private Image _currentSelectedCharacterPortrait;
    [SerializeField] private TMP_Text _currentSelectedCharacterTitle;
    [SerializeField] private Image _currentPickedGodPortrait;
    [SerializeField] private TMP_Text _currentPickedGodTitle;

    private int _currentCharacterIndex = 0;
    private int _currentGodIndex = 0;

    public void ShowGameSetupMenu()
    {
        _currentCharacterIndex = 0;
        _currentGodIndex = 0;
        LoadCharacterData();
        LoadGodData();
    }

    public void HideGameSetupMenu()
    {

    }

    #region Button Handlers
    public void OnButtonPress_NextCharacter()
    {
        _currentCharacterIndex++;
        if (_currentCharacterIndex >= _playableCharacters.Length)
            _currentCharacterIndex = 0;
        LoadCharacterData();
    }

    public void OnButtonPress_PreviousCharacter()
    {
        _currentCharacterIndex--;
        if (_currentCharacterIndex < 0)
            _currentCharacterIndex = _playableCharacters.Length - 1;
        LoadCharacterData();
    }

    public void OnButtonPress_NextGod()
    {
        _currentGodIndex++;
        if (_currentGodIndex >= _pickableGods.Length)
            _currentGodIndex = 0;
        LoadGodData();
    }

    public void OnButtonPress_PreviousGod()
    {
        _currentGodIndex--;
        if (_currentGodIndex < 0)
            _currentGodIndex = _pickableGods.Length - 1;
        LoadGodData();
    }
    #endregion

    private void LoadCharacterData()
    {
        _currentSelectedCharacterPortrait.sprite = _playableCharacters[_currentCharacterIndex].CharacterArt;
        _currentSelectedCharacterTitle.text = _playableCharacters[_currentCharacterIndex].CharacterName;
    }

    private void LoadGodData()
    {
        _currentPickedGodPortrait.sprite = _pickableGods[_currentGodIndex].GodArt;
        _currentPickedGodTitle.text = _pickableGods[_currentGodIndex].GodName;
    }

    [System.Serializable]
    public class PlayableCharacter
    {
        public string CharacterName;
        public Sprite CharacterArt;
    }

    [System.Serializable]
    public class PickableGod
    {
        public string GodName;
        public Sprite GodArt;
    }
}
