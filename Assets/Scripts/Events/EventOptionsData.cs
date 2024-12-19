using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class EventOptionsData : ScriptableObject
{
    public string OptionText;
    public List<EventOptionOutcomeData> OptionOutcomes;
    public List<DiceFaceData> OutcomeRollBoosterDiceFaces;

    public List<DiceFaceData> OptionRequiredDiceFaceData;
    public int OptionRequirementGold = 0;
    public PlayerInventorySingleton.PickableGods OptionRequirementGod = PlayerInventorySingleton.PickableGods.None;
    public PlayerInventorySingleton.PlayableCharacters OptionRequirementCharacter = PlayerInventorySingleton.PlayableCharacters.None;
}
