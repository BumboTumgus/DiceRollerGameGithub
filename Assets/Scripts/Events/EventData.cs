using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class EventData : ScriptableObject
{
    public string EventTitle;
    public string EventDescription;
    public Sprite EventImage;
    public List<EventOptionsData> EventOptions;
}
