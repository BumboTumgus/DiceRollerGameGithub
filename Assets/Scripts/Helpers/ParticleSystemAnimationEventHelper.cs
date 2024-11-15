using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleSystemAnimationEventHelper : MonoBehaviour
{
    [SerializeField] private ParticleSystem[] _particleSystemsToPlay;

    public void PlayParticleSystem(int particleSystemIndex)
    {
        if(particleSystemIndex >= _particleSystemsToPlay.Length)
            return;
        
        _particleSystemsToPlay[particleSystemIndex].Play();
    }
}
