using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class MainMenuController : MonoBehaviour
{
    private const string MAIN_GAME_SCENE_NAME = "GameScene";
    private const string MAIN_MENU_SCENE_NAME = "MainMenu";

    [SerializeField] OptionMenuController _optionMenuController;
    [SerializeField] GameSetupController _gameSetupController;
    [SerializeField] GameObject _mainMenuContent;
    [SerializeField] GameObject _optionsContent;
    [SerializeField] GameObject _characterSetupContent;
    [SerializeField] GameObject _godSetupContent;
    [SerializeField] GameObject _loadingScreenContent;

    [SerializeField] TMP_Text _loadingTipText;
    [SerializeField] Image _loadingFillBar;
    [SerializeField][TextArea] string[] _loadingScreenTips;

    [SerializeField] EventSystem _eventSystem;
    [SerializeField] Camera _mainCamera;


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
        _loadingScreenContent.SetActive(false);
    }

    public void ShowOptionsMenu()
    {
        _mainMenuContent.SetActive(false);
        _optionsContent.SetActive(true);
        _characterSetupContent.SetActive(false);
        _godSetupContent.SetActive(false);
        _loadingScreenContent.SetActive(false);
    }

    public void ShowCharacterSetupMenu()
    {
        _mainMenuContent.SetActive(false);
        _optionsContent.SetActive(false);
        _characterSetupContent.SetActive(true);
        _godSetupContent.SetActive(false);
        _loadingScreenContent.SetActive(false);
    }

    public void ShowGodSetupMenu()
    {
        _mainMenuContent.SetActive(false);
        _optionsContent.SetActive(false);
        _characterSetupContent.SetActive(false);
        _godSetupContent.SetActive(true);
        _loadingScreenContent.SetActive(false);
    }

    public void StartGame()
    {
        _mainMenuContent.SetActive(false);
        _optionsContent.SetActive(false);
        _characterSetupContent.SetActive(false);
        _godSetupContent.SetActive(false);
        _loadingScreenContent.SetActive(true);

        _loadingTipText.text = _loadingScreenTips[Random.Range(0, _loadingScreenTips.Length)]; 
        _loadingFillBar.fillAmount = 0;

        StartCoroutine(LoadMainGameScenAsync());
    }

    IEnumerator LoadMainGameScenAsync()
    {
        AsyncOperation sceneLoadOperation = SceneManager.LoadSceneAsync(MAIN_GAME_SCENE_NAME, LoadSceneMode.Additive);

        while(!sceneLoadOperation.isDone) 
        {
            Debug.Log(sceneLoadOperation.progress);
            _loadingFillBar.fillAmount = Mathf.Clamp01(sceneLoadOperation.progress / 0.9f);

            yield return null;
        }

        yield return new WaitForSeconds(1f);

        Destroy(_eventSystem.gameObject);
        Destroy(_mainCamera.gameObject);

        SceneManager.SetActiveScene(SceneManager.GetSceneAt(1));
        AsyncOperation sceneUnloadOperation = SceneManager.UnloadSceneAsync(MAIN_MENU_SCENE_NAME, UnloadSceneOptions.None);
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
