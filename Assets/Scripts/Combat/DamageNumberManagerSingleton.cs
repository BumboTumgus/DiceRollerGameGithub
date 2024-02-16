using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DamageNumberManagerSingleton : MonoBehaviour
{
    private const float DAMAGE_NUMBER_DURATION = 1.5f;

    public static DamageNumberManagerSingleton Instance;

    [SerializeField] private Color _healthDamageColor;
    [SerializeField] private Color _blockedDamageColor;
    [SerializeField] private TMP_Text _damageNumber;
    [SerializeField] private TMP_Text _criticalFlavorText;
    [SerializeField] private Transform _enemyDamageStartPosition;
    [SerializeField] private Transform _enemyDamageEndPosition;
    [SerializeField] private Transform _playerDamageStartPosition;
    [SerializeField] private Transform _playerDamageEndPosition;

    private WaitForEndOfFrame _waitForEndOfFrame= new WaitForEndOfFrame();
    private Coroutine _damageNumberMovementRoutine;
    
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        if (Instance != this)
            Destroy(this);
        
        _damageNumber.gameObject.SetActive(false);
    }

    public void ShowEnemyDamageNumber(int value, bool blocked, bool enemyDamage, bool critical)
    {
        _damageNumber.text = critical ? "-" + value + "!": "-" + value;

        _damageNumber.color = blocked ? _blockedDamageColor : _healthDamageColor;
        _criticalFlavorText.color = blocked ? _blockedDamageColor : _healthDamageColor;
        _criticalFlavorText.gameObject.SetActive(critical);

        if(_damageNumberMovementRoutine != null)
            StopCoroutine(_damageNumberMovementRoutine);

        if(enemyDamage)
            _damageNumberMovementRoutine = StartCoroutine(LerpToPosition(_enemyDamageStartPosition.position, _enemyDamageEndPosition.position));
        else
            _damageNumberMovementRoutine = StartCoroutine(LerpToPosition(_playerDamageStartPosition.position, _playerDamageEndPosition.position));
    }

    private IEnumerator LerpToPosition(Vector3 startPos, Vector3 endPos)
    {
        _damageNumber.gameObject.SetActive(true);
        float currentTimer = 0;
        while(currentTimer < DAMAGE_NUMBER_DURATION)
        {
            currentTimer += Time.deltaTime;
            _damageNumber.transform.position = Vector3.Lerp(startPos, endPos, currentTimer / DAMAGE_NUMBER_DURATION);
            yield return _waitForEndOfFrame;
        }
        _damageNumber.gameObject.SetActive(false);
    }
}
