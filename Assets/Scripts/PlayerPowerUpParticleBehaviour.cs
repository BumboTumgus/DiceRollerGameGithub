using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerPowerUpParticleBehaviour : MonoBehaviour
{
    public UnityAction OnParticleReachedDestination;

    private const float FLOAT_UP_DURATION = 0.5f; 
    private const float AFTER_FLOAT_DELAY = 0.5f; 
    private const float SHOOT_TOWARDS_PLAYER_DURATION = 1f; 
    private const float EXPLOSION_DURATION = 1.5f; 

    [SerializeField] private ParticleSystem _trailParticleSystem;
    [SerializeField] private ParticleSystem _explosionOnPlayerParticleSystem;

    private Transform _target;
    private float _currentTimer = 0;
    private Vector3 _initialPosition;
    private Vector3 _targetPosition;
    private WaitForEndOfFrame _waitForEndOfFrame;
    private Light _connectedLight;

    public void InitializeBehaviour(Transform target)
    {
        _target = target;
        _waitForEndOfFrame = new WaitForEndOfFrame();
        _connectedLight = GetComponent<Light>();

        StartCoroutine(FlyToPlayerAndExplode());
    }

    private IEnumerator FlyToPlayerAndExplode()
    {
        _initialPosition = transform.position;
        _targetPosition = transform.position + Vector3.forward * -0.5f + Vector3.up * 0.2f;

        while(_currentTimer < FLOAT_UP_DURATION)
        {
            _currentTimer += Time.deltaTime;
            transform.position = Vector3.Lerp(_initialPosition, _targetPosition, _currentTimer / FLOAT_UP_DURATION);
            yield return _waitForEndOfFrame;
        }

        _currentTimer = 0;
        transform.position = _targetPosition;
        _initialPosition = transform.position;
        _targetPosition = _target.position + Vector3.up * 0.7f;

        yield return new WaitForSeconds(AFTER_FLOAT_DELAY);
        
        while(_currentTimer < SHOOT_TOWARDS_PLAYER_DURATION)
        {
            _currentTimer += Time.deltaTime;
            transform.position = Vector3.Slerp(_initialPosition, _targetPosition, _currentTimer / SHOOT_TOWARDS_PLAYER_DURATION);
            yield return _waitForEndOfFrame;
        }

        _trailParticleSystem.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        _connectedLight.intensity = 0;
        _explosionOnPlayerParticleSystem.Play();
        
        OnParticleReachedDestination?.Invoke();
        
        yield return new WaitForSeconds(EXPLOSION_DURATION);

        Destroy(gameObject);
    }
}
