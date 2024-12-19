using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class EventOptionOutcomeData : ScriptableObject
{
    public string OutcomeTitle;
    public string OutcomeDescription;
    public Sprite OutcomeImage;
    public int OutcomeRollDC = -999;

    // Optional
    public int OutcomeRewardGold = 0;
    public int OutcomeRewardHealth = 0;
    public List<DiceFaceData> OutcomeRewardDiceFaceData;
    public List<EventData> NewFutureEventDataToAddToPool;

    public EventData OutcomeEvent;
    public EncounterScriptableObject OutcomeEncounter;
}
