using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuController : MonoBehaviour
{
    [SerializeField] OptionMenuController _optionMenuController;
    [SerializeField] GameSetupController _gameSetupController;
    [SerializeField] GameObject _mainMenuContent;
    [SerializeField] GameObject _optionsContent;
    [SerializeField] GameObject _characterSetupContent;
    [SerializeField] GameObject _godSetupContent;


    private void Start()
    {
        ShowMainMenu();
    }

    public void ShowMainMenu()
    {
        _mainMenuContent.SetActive(true);
        _optionsContent.SetActive(false);
        _characterSetupContent.SetActive(false);
        _godSetupContent.SetActive(false);
    }

    public void ShowOptionsMenu()
    {
        _mainMenuContent.SetActive(false);
        _optionsContent.SetActive(true);
        _characterSetupContent.SetActive(false);
        _godSetupContent.SetActive(false);
    }

    public void ShowCharacterSetupMenu()
    {
        _mainMenuContent.SetActive(false);
        _optionsContent.SetActive(false);
        _characterSetupContent.SetActive(true);
        _godSetupContent.SetActive(false);
    }

    public void ShowGodSetupMenu()
    {
        _mainMenuContent.SetActive(false);
        _optionsContent.SetActive(false);
        _characterSetupContent.SetActive(false);
        _godSetupContent.SetActive(true);
    }

    public void StartGame()
    {

    }

    #region Button Handlers
    public void ButtonPress_QuitGame()
    {
        Application.Quit();
    }
    public void ButtonPress_StartGame()
    {
        StartGame();
    }
    public void ButtonPress_ShowMainMenu()
    {
        ShowMainMenu();
    }
    public void ButtonPress_ShowOptionsMenu()
    {
        ShowOptionsMenu();
    }
    public void ButtonPress_ShowCharacterSetupMenu()
    {
        ShowCharacterSetupMenu();
    }
    public void ButtonPress_ShowGodSetupMenu()
    {
        ShowGodSetupMenu();
    }
    #endregion
}
