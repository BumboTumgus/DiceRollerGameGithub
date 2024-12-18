using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelDataSingleton : MonoBehaviour
{
    public static LevelDataSingleton Instance;

    public List<EventData> EventEncounters { get => eventEncounters; }
    public EncounterScriptableObject BossEncounter { get => bossEncounter; }
    public List<EncounterScriptableObject> EliteEncounters { get => eliteEncounters; }
    public List<EncounterScriptableObject> BasicEncounters { get => basicEncounters; }

    //TODO: Remvoe serialize field and create a level data scriptable object thats is used to load all level data.
    [SerializeField] private List<EncounterScriptableObject> basicEncounters = new List<EncounterScriptableObject>();
    [SerializeField] private List<EncounterScriptableObject> eliteEncounters = new List<EncounterScriptableObject>();
    [SerializeField] private EncounterScriptableObject bossEncounter;
    [SerializeField] private List<EventData> eventEncounters = new List<EventData>();


    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        if (Instance != this)
            Destroy(this);
    }

    public EventData GetRandomEventDetails()
    {
        EventData eventToReturn = EventEncounters[Random.Range(0, EventEncounters.Count)];
        //EventEncounters.Remove(eventToReturn);
        return eventToReturn;
    }

    public EncounterScriptableObject GetBossEncounter()
    {
        return bossEncounter;
    }

    public EncounterScriptableObject GetEliteEncounter()
    {
        EncounterScriptableObject eliteEncounter = EliteEncounters[Random.Range(0, EliteEncounters.Count)];
        //EliteEncounters.Remove(eliteEncounter);
        return eliteEncounter;
    }

    public EncounterScriptableObject GetBasicEncounter()
    {
        EncounterScriptableObject basicEncounter = BasicEncounters[Random.Range(0, BasicEncounters.Count)];
        return basicEncounter;
    }
}
