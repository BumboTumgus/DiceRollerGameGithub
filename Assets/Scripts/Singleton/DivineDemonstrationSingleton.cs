using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DivineDemonstrationSingleton : MonoBehaviour
{
    public static DivineDemonstrationSingleton Instance;

    private int _currentFavor = 0;
    private int _targetFavor = 5;
    private int _activationsLeft = 0;
    private int _maximumActivations = 3;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        if (Instance != this)
            Destroy(this);
    }

    public void AddFavor(int v)
    {
        _currentFavor += v;
        while(_currentFavor > _targetFavor)
        {
            if(_activationsLeft < _maximumActivations)
            {
                _currentFavor -= _targetFavor;
                _activationsLeft++;
            }
            else
            {
                _currentFavor = 0;
            }
        }
    }

    public void AttemptActivation()
    {
        //TODO add the divine demonstration logic.
    }
}
