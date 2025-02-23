using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDebuffsToInflictManager : MonoBehaviour
{
    public List<BuffScriptableObject> DebuffsToInflict { get => _debuffsToInflict; set => _debuffsToInflict = value; }
    public List<int> DebuffsToInflictCount { get => _debuffsToInflictCount; set => _debuffsToInflictCount = value; }

    private List<BuffScriptableObject> _debuffsToInflict = new List<BuffScriptableObject>();
    private List<int> _debuffsToInflictCount = new List<int>();


    public void AddDebuffToInflict(BuffScriptableObject debuff, int value)
    {
        Debug.Log("addijg the debuff of " + debuff.name + " to the players aresenal for them to use");
        if(_debuffsToInflict.Count == 0)
        {
            _debuffsToInflict.Add(debuff);
            _debuffsToInflictCount.Add(value);
            return;
        }

        for(int debuffIndex = 0; debuffIndex < _debuffsToInflict.Count; debuffIndex++)
        {
            if (_debuffsToInflict[debuffIndex] == debuff)
            {
                _debuffsToInflictCount[debuffIndex] += value;
                return;
            }
        }

        _debuffsToInflict.Add(debuff);
        _debuffsToInflictCount.Add(value);
    }

    public void RemoveFirstDebuffFromList()
    {
        if (_debuffsToInflict.Count <= 0)
            return;

        _debuffsToInflict.RemoveAt(0);
        _debuffsToInflictCount.RemoveAt(0);
    }
}
