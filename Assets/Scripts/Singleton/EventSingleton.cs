using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class EventSingelton : MonoBehaviour
{
    private const float SUBTITLE_PADDING = 90f;
    private const float EVENT_BUTTON_PADDING = 50f;

    private const string CHARACTER_EVENT_OPTION_BUTTON_COLOR = "#C8382F";
    private const string GOD_EVENT_OPTION_BUTTON_COLOR = "#577CCF";

    public static EventSingelton Instance;

    [SerializeField] private UiSlidingPanelController _slideController;
    [SerializeField] private GameObject _eventContainer;
    [SerializeField] private TMP_Text _eventTitle;
    [SerializeField] private TMP_Text _eventSubtitle;
    [SerializeField] private Image _eventSplashImage;
    [SerializeField] private Button[] _eventOptionButtons;
    [SerializeField] private RectTransform _eventOptionVerticalLayoutContainer;

    [SerializeField] private GameObject _outcomeContainer;
    [SerializeField] private TMP_Text _outcomeTitle;
    [SerializeField] private TMP_Text _outcomeSubtitle;
    [SerializeField] private Image _outcomeSplashImage;
    [SerializeField] private Button _outcomeContinueButton;
    [SerializeField] private RectTransform _outcomeVerticalLayoutContainer;

    private EventData _currentEvent;
    private EventOptionOutcomeData _currentOutcome;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        if (Instance != this)
            Destroy(this);
    }

    public void StartEvent(EventData eventData, float delay)
    {
        LoadEvent(eventData);
        Invoke(nameof(OpenEventPanel), delay);
    }

    public void UiButtonPress_OptionButtonPress(int index)
    {
        EventOptionOutcomeData outcome = RollForEventOptionOutcome(_currentEvent.EventOptions[index]);
        LoadOutcome(outcome);
    }

    public void UiButtonPress_OutcomeContinueButtonPress()
    {
        PlayerInventorySingleton.Instance.UpdateGoldValue(PlayerInventorySingleton.Instance.CollectedGold + _currentOutcome.OutcomeRewardGold);
        if (_currentOutcome.OutcomeRewardHealth > 0)
            CombatManagerSingleton.Instance.PlayerCharacterCombatBehaviour.HealHealth(_currentOutcome.OutcomeRewardHealth);
        else
            CombatManagerSingleton.Instance.PlayerCharacterCombatBehaviour.TakeDamage(-_currentOutcome.OutcomeRewardHealth);

        if (_currentOutcome.OutcomeRewardDiceFaceData.Count > 0)
            foreach (DiceFaceData rewardDiceFace in _currentOutcome.OutcomeRewardDiceFaceData)
                PlayerInventorySingleton.Instance.AddDiceFaceToInventory(rewardDiceFace);
        if (_currentOutcome.NewFutureEventDataToAddToPool.Count > 0)
            foreach (EventData futureEvent in _currentOutcome.NewFutureEventDataToAddToPool)
                LevelDataSingleton.Instance.EventEncounterContinuations.Add(futureEvent);

        if (_currentOutcome.OutcomeEncounter != null)
        {
            _slideController.SetPanelOpenStatus(false);
            CombatManagerSingleton.Instance.StartCombat(_currentOutcome.OutcomeEncounter);
            return;
        }
        if(_currentOutcome.OutcomeEvent != null)
        {
            LoadEvent(_currentOutcome.OutcomeEvent);
            return;
        }

        _slideController.SetPanelOpenStatus(false);
        MapSingleton.Instance.SetMapInteractibility(true);
        MapSingleton.Instance.UiShowMapWithDelay(1f);
    }

    public void OpenEventPanel()
    {
        _slideController.SetPanelOpenStatus(true);
    }

    private void LoadEvent(EventData eventData) 
    {
        Debug.Log("EventLoaded");
        _currentEvent = eventData;
        _eventContainer.SetActive(true);
        _outcomeContainer.SetActive(false);

        _eventTitle.text = eventData.EventTitle;
        _eventTitle.GetComponent<RectTransform>().sizeDelta = new Vector2(_eventTitle.GetComponent<RectTransform>().sizeDelta.x, _eventTitle.preferredHeight);
        _eventSubtitle.text = eventData.EventDescription;
        _eventSubtitle.GetComponent<RectTransform>().sizeDelta = new Vector2(_eventSubtitle.GetComponent<RectTransform>().sizeDelta.x, _eventSubtitle.preferredHeight + SUBTITLE_PADDING);
        _eventSplashImage.sprite = eventData.EventImage;

        for(int index = 0; index < _eventOptionButtons.Length; index++) 
        {
            if(index >= eventData.EventOptions.Count || !OptionRequirementsAreMet(eventData.EventOptions[index]))
            {
                _eventOptionButtons[index].gameObject.SetActive(false);
                Debug.Log("Event button at index " + index + " should be hidden");
                continue;
            }

            _eventOptionButtons[index].gameObject.SetActive(true);
            _eventOptionButtons[index].GetComponent<TMP_Text>().text = "";
            if (eventData.EventOptions[index].OptionRequirementCharacter != PlayerInventorySingleton.PlayableCharacters.None)
                _eventOptionButtons[index].GetComponent<TMP_Text>().text += "<color=" + CHARACTER_EVENT_OPTION_BUTTON_COLOR + ">[" + eventData.EventOptions[index].OptionRequirementCharacter.ToString() + "]</color>";
            if (eventData.EventOptions[index].OptionRequirementGod != PlayerInventorySingleton.PickableGods.None)
                _eventOptionButtons[index].GetComponent<TMP_Text>().text += "<color=" + GOD_EVENT_OPTION_BUTTON_COLOR + ">[The " + eventData.EventOptions[index].OptionRequirementGod.ToString() + "]</color>";
            if (_eventOptionButtons[index].GetComponent<TMP_Text>().text.Length > 0)
                _eventOptionButtons[index].GetComponent<TMP_Text>().text += " ";

            _eventOptionButtons[index].GetComponent<TMP_Text>().text += eventData.EventOptions[index].OptionText;
            if (eventData.EventOptions[index].OutcomeRollBoosterDiceFaces.Count > 0)
            {
                _eventOptionButtons[index].GetComponent<TMP_Text>().text += " [Aided by: ";
                foreach (DiceFaceData diceFace in eventData.EventOptions[index].OutcomeRollBoosterDiceFaces)
                {
                    _eventOptionButtons[index].GetComponent<TMP_Text>().text += "<sprite name=\"" + diceFace.DiceFaceEnum.ToString() + "\">";
                }
                _eventOptionButtons[index].GetComponent<TMP_Text>().text += "]";
            }
            _eventOptionButtons[index].GetComponent<RectTransform>().sizeDelta = new Vector2(_eventOptionButtons[index].GetComponent<RectTransform>().sizeDelta.x, _eventOptionButtons[index].GetComponent<TMP_Text>().preferredHeight + EVENT_BUTTON_PADDING);

            _eventOptionButtons[index].transform.Find("Highlight").gameObject.SetActive(false);
        }

        StartCoroutine(ResizeOptionContainerAfterFrame(_eventOptionVerticalLayoutContainer));
    }

    private IEnumerator ResizeOptionContainerAfterFrame(RectTransform optionContainer)
    {
        yield return new WaitForEndOfFrame();
        optionContainer.sizeDelta = new Vector2(optionContainer.sizeDelta.x, optionContainer.GetComponent<LayoutGroup>().preferredHeight);

    }

    private void LoadOutcome(EventOptionOutcomeData outcomeData)
    {
        _currentOutcome = outcomeData;
        _eventContainer.SetActive(false);
        _outcomeContainer.SetActive(true);

        _outcomeTitle.text = outcomeData.OutcomeTitle;
        _outcomeTitle.GetComponent<RectTransform>().sizeDelta = new Vector2(_outcomeTitle.GetComponent<RectTransform>().sizeDelta.x, _outcomeTitle.preferredHeight + EVENT_BUTTON_PADDING);
        _outcomeSubtitle.text = outcomeData.OutcomeDescription;
        _outcomeSubtitle.GetComponent<RectTransform>().sizeDelta = new Vector2(_outcomeSubtitle.GetComponent<RectTransform>().sizeDelta.x, _outcomeSubtitle.preferredHeight + EVENT_BUTTON_PADDING);
        _outcomeSplashImage.sprite = outcomeData.OutcomeImage;

        StartCoroutine(ResizeOptionContainerAfterFrame(_outcomeVerticalLayoutContainer));
    }

    private bool OptionRequirementsAreMet(EventOptionsData optionData)
    {
        if (optionData.OptionRequirementGold > PlayerInventorySingleton.Instance.CollectedGold ||
            (optionData.OptionRequirementCharacter != PlayerInventorySingleton.Instance.SelectedCharacter && optionData.OptionRequirementCharacter != PlayerInventorySingleton.PlayableCharacters.None) ||
            (optionData.OptionRequirementGod != PlayerInventorySingleton.Instance.SelectedGod && optionData.OptionRequirementGod != PlayerInventorySingleton.PickableGods.None) ||
            !DiceRollerSingleton.Instance.DiceContainsRequiredDiceFaces(optionData.OptionRequiredDiceFaceData))
            return false;

        return true;
    }

    private EventOptionOutcomeData RollForEventOptionOutcome(EventOptionsData optionData)
    {
        if (optionData.OptionOutcomes.Count == 0)
            Debug.LogError("No option outcomes for option data " + optionData);
        if (optionData.OptionOutcomes.Count == 1)
            return optionData.OptionOutcomes[0];

        Debug.Log("Roling for outcome");
        int currentRoll = Random.Range(1, 21);
        Debug.Log("our roll is " + currentRoll);
        foreach (DiceFaceData supportingFaceData in optionData.OutcomeRollBoosterDiceFaces)
            currentRoll += DiceRollerSingleton.Instance.DieFacesThatMatch(supportingFaceData);

        Debug.Log("our roll after assistance from dice faces is " + currentRoll);


        for (int i = 1; i < optionData.OptionOutcomes.Count; i++)
        {
            Debug.Log("comparing roll of " + currentRoll + " to the DC " + optionData.OptionOutcomes[i].OutcomeRollDC);
            if (optionData.OptionOutcomes[i].OutcomeRollDC > currentRoll)
                return optionData.OptionOutcomes[i - 1];
        }

        return optionData.OptionOutcomes[optionData.OptionOutcomes.Count - 1];
    }
}
