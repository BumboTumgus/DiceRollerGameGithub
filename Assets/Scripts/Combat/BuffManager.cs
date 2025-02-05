using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuffManager : MonoBehaviour
{
    [SerializeField] List<BuffScriptableObject> DebugBuffs;

    [SerializeField] List<BuffScriptableObject> _activeBuffs = new List<BuffScriptableObject>();
    [SerializeField] List<int> _activeBuffIncrements = new List<int>();
    [SerializeField] List<UiBuffIcon> _uiBuffIcons = new List<UiBuffIcon>();

    [SerializeField] Transform _buffUiParent;
    [SerializeField] GameObject _uiBuffPrefab;
    [SerializeField] UiBuffDescriptionController _uiBuffDescriptionController;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
            AddBuff(DebugBuffs[0], 1);
        if (Input.GetKeyDown(KeyCode.S))
            AddBuff(DebugBuffs[0], 5);
        if (Input.GetKeyDown(KeyCode.D))
            AddBuff(DebugBuffs[1], 1);
        if (Input.GetKeyDown(KeyCode.F))
            AddBuff(DebugBuffs[1], 5);
        if (Input.GetKeyDown(KeyCode.G))
            ClearAllBuffs();
        if (Input.GetKeyDown(KeyCode.H))
            DecrementAllBuffs();
    }

    public void AddBuff(BuffScriptableObject buffToAdd, int buffCount)
    {
        for (int buffIndex = 0; buffIndex < _activeBuffs.Count; buffIndex++)
        {
            if (_activeBuffs[buffIndex] != buffToAdd)
                continue;

            _activeBuffIncrements[buffIndex] += buffCount;
            _uiBuffIcons[buffIndex].IncrementBuffCount(_activeBuffIncrements[buffIndex]);
            return;
        }

        _activeBuffs.Add(buffToAdd);
        _activeBuffIncrements.Add(buffCount);

        UiBuffIcon uiBuffIcon = Instantiate(_uiBuffPrefab, _buffUiParent.position, _buffUiParent.rotation, _buffUiParent).GetComponent<UiBuffIcon>();
        _uiBuffIcons.Add(uiBuffIcon);
        uiBuffIcon.InitializeWithBuff(buffToAdd, buffCount, _uiBuffDescriptionController);
    }

    public void ClearAllBuffs()
    {
        _activeBuffs.Clear();
        _activeBuffIncrements.Clear();

        for (int buffIndex = 0; buffIndex < _uiBuffIcons.Count; buffIndex++)
        {
            Destroy(_uiBuffIcons[buffIndex].gameObject);
        }

        _uiBuffIcons.Clear();
    }

    public void DecrementAllBuffs()
    {
        for(int buffIndex = 0; buffIndex < _activeBuffs.Count; buffIndex++) 
        {
            if (_activeBuffs[buffIndex].BuffIsPermanent)
                continue;

            _activeBuffIncrements[buffIndex]--;
            _uiBuffIcons[buffIndex].IncrementBuffCount(_activeBuffIncrements[buffIndex]);

            if (_activeBuffIncrements[buffIndex] <= 0)
            {
                _activeBuffs.RemoveAt(buffIndex);
                _activeBuffIncrements.RemoveAt(buffIndex);
                Destroy(_uiBuffIcons[buffIndex].gameObject);
                _uiBuffIcons.RemoveAt(buffIndex);
                buffIndex--;
            }
        }
    }

    public void DecrementBuff(BuffScriptableObject.BuffType buffType)
    {
        for(int buffIndex = 0; buffIndex < _activeBuffIncrements.Count; buffIndex++) 
        {
            if (_activeBuffs[buffIndex].MyBuffType != buffType)
                continue;

            _activeBuffIncrements[buffIndex]--;
            _uiBuffIcons[buffIndex].IncrementBuffCount(_activeBuffIncrements[buffIndex]);

            if (_activeBuffIncrements[buffIndex] <= 0)
            {
                _activeBuffs.RemoveAt(buffIndex);
                _activeBuffIncrements.RemoveAt(buffIndex);
                Destroy(_uiBuffIcons[buffIndex].gameObject);
                _uiBuffIcons.RemoveAt(buffIndex);
                buffIndex--;
            }
        }
    }

    public bool IsBuffActive(BuffScriptableObject.BuffType buffType)
    {
        for (int buffIndex = 0; buffIndex < _activeBuffIncrements.Count; buffIndex++)
        {
            if (_activeBuffs[buffIndex].MyBuffType == buffType)
                return true;
        }
        return false;
    }
}
