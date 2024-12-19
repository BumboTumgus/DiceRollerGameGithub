using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelDataSingleton : MonoBehaviour
{
    public static LevelDataSingleton Instance;

    private const float EVENT_CONTINUATION_CHANCE = 0.15f;

    public List<EventData> EventEncounters { get => eventEncounters; }
    public List<EventData> EventEncounterContinuations { get => eventEncounterContinuations; set => eventEncounterContinuations = value; }
    public EncounterScriptableObject BossEncounter { get => bossEncounter; }
    public List<EncounterScriptableObject> EliteEncounters { get => eliteEncounters; }
    public List<EncounterScriptableObject> BasicEncounters { get => basicEncounters; }

    //TODO: Remvoe serialize field and create a level data scriptable object thats is used to load all level data.
    [SerializeField] private List<EncounterScriptableObject> basicEncounters = new List<EncounterScriptableObject>();
    [SerializeField] private List<EncounterScriptableObject> eliteEncounters = new List<EncounterScriptableObject>();
    [SerializeField] private EncounterScriptableObject bossEncounter;
    [SerializeField] private List<EventData> eventEncounters = new List<EventData>();
    [SerializeField] private List<EventData> eventEncounterContinuations = new List<EventData>();

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        if (Instance != this)
            Destroy(this);
    }

    public EventData GetRandomEventDetails()
    {
        EventData eventToReturn;
        if (Random.Range(0f, 1f) < EVENT_CONTINUATION_CHANCE && eventEncounterContinuations.Count > 1)
        {
            eventToReturn = eventEncounterContinuations[Random.Range(0, eventEncounterContinuations.Count)];
            //eventEncounterContinuations.Remove(eventToReturn);
        }
        else
        {
            eventToReturn = eventEncounters[Random.Range(0, eventEncounters.Count)];
            //EventEncounters.Remove(eventToReturn);
        }
        return eventToReturn;
    }

    public EncounterScriptableObject GetBossEncounter()
    {
        return bossEncounter;
    }

    public EncounterScriptableObject GetEliteEncounter()
    {
        EncounterScriptableObject eliteEncounter = eliteEncounters[Random.Range(0, eliteEncounters.Count)];
        //EliteEncounters.Remove(eliteEncounter);
        return eliteEncounter;
    }

    public EncounterScriptableObject GetBasicEncounter()
    {
        EncounterScriptableObject basicEncounter = basicEncounters[Random.Range(0, basicEncounters.Count)];
        return basicEncounter;
    }
}
