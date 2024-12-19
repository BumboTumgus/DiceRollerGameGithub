using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EventSingelton : MonoBehaviour
{
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

    public void StartEvent(EventData eventData)
    {
        _slideController.SetPanelOpenStatus(true);
        LoadEvent(eventData);
    }

    public void UiButtonPress_OptionButtonPress(int index)
    {
        EventOptionOutcomeData outcome = RollForEventOptionOutcome(_currentEvent.EventOptions[index]);
        LoadOutcome(outcome);
    }

    public void UiButtonPress_OutcomeContinueButtonPress()
    {
        if(_currentOutcome.OutcomeEncounter != null)
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
        MapSingleton.Instance.UiShowMapWithDelay(1f);
    }

    private void LoadEvent(EventData eventData) 
    {
        _currentEvent = eventData;
        _eventContainer.SetActive(true);
        _outcomeContainer.SetActive(false);

        _eventTitle.text = eventData.EventTitle;
        _eventTitle.GetComponent<RectTransform>().sizeDelta = new Vector2(_eventTitle.GetComponent<RectTransform>().sizeDelta.x, _eventTitle.preferredHeight);
        _eventSubtitle.text = eventData.EventDescription;
        _eventSubtitle.GetComponent<RectTransform>().sizeDelta = new Vector2(_eventSubtitle.GetComponent<RectTransform>().sizeDelta.x, _eventSubtitle.preferredHeight);
        _eventSplashImage.sprite = eventData.EventImage;

        for(int index = 0; index < _eventOptionButtons.Length; index++) 
        { 
            if(index >= eventData.EventOptions.Count || !OptionRequirementsAreMet(eventData.EventOptions[index]))
            {
                _eventOptionButtons[index].gameObject.SetActive(false);
                continue;
            }

            _eventOptionButtons[index].GetComponent<TMP_Text>().text = eventData.EventOptions[index].OptionText;
            _eventOptionButtons[index].GetComponent<RectTransform>().sizeDelta = new Vector2(_eventOptionButtons[index].GetComponent<RectTransform>().sizeDelta.x, _eventTitle.preferredHeight);
        }

        _eventOptionVerticalLayoutContainer.sizeDelta = new Vector2(_eventOptionVerticalLayoutContainer.sizeDelta.x, _eventOptionVerticalLayoutContainer.GetComponent<LayoutGroup>().preferredHeight);
    }

    private void LoadOutcome(EventOptionOutcomeData outcomeData)
    {
        _currentOutcome = outcomeData;
        _eventContainer.SetActive(false);
        _outcomeContainer.SetActive(true);

        _outcomeTitle.text = outcomeData.OutcomeTitle;
        _outcomeTitle.GetComponent<RectTransform>().sizeDelta = new Vector2(_outcomeTitle.GetComponent<RectTransform>().sizeDelta.x, _outcomeTitle.preferredHeight);
        _eventSubtitle.text = outcomeData.OutcomeDescription;
        _eventSubtitle.GetComponent<RectTransform>().sizeDelta = new Vector2(_eventSubtitle.GetComponent<RectTransform>().sizeDelta.x, _eventSubtitle.preferredHeight);
        _eventSplashImage.sprite = outcomeData.OutcomeImage;

        _outcomeVerticalLayoutContainer.sizeDelta = new Vector2(_outcomeVerticalLayoutContainer.sizeDelta.x, _outcomeVerticalLayoutContainer.GetComponent<LayoutGroup>().preferredHeight);
    }

    private bool OptionRequirementsAreMet(EventOptionsData optionData)
    {
        if (optionData.OptionRequirementGold > PlayerInventorySingleton.Instance.CollectedGold ||
            (optionData.OptionRequirementCharacter != PlayerInventorySingleton.Instance.SelectedCharacter && optionData.OptionRequirementCharacter != PlayerInventorySingleton.PlayableCharacters.None) ||
            (optionData.OptionRequirementGod != PlayerInventorySingleton.Instance.SelectedGod && optionData.OptionRequirementGod != PlayerInventorySingleton.PickableGods.None) ||
            DiceRollerSingleton.Instance.DiceContainsRequiredDiceFaces(optionData.OptionRequiredDiceFaceData))
            return false;

        return true;
    }

    private EventOptionOutcomeData RollForEventOptionOutcome(EventOptionsData optionData)
    {
        if (optionData.OptionOutcomes.Count == 0)
            Debug.LogError("No option outcomes for option data " + optionData);
        if (optionData.OptionOutcomes.Count == 1)
            return optionData.OptionOutcomes[0];

        int currentRoll = Random.Range(1, 21);
        foreach (DiceFaceData supportingFaceData in optionData.OutcomeRollBoosterDiceFaces)
            currentRoll += DiceRollerSingleton.Instance.DieFacesThatMatch(supportingFaceData);


        for(int i = 1; i < optionData.OptionOutcomes.Count; i++)
        {
            if (optionData.OptionOutcomes[i].OutcomeRollDC > currentRoll)
                return optionData.OptionOutcomes[i - 1];
        }

        return null;
    }
}
