using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DamageNumberManagerSingleton : MonoBehaviour
{
    private const float DAMAGE_NUMBER_DURATION = 1.5f;
    private const string BUFF_ANIM = "Ui_BuffIconPopIn";

    public static DamageNumberManagerSingleton Instance;

    [SerializeField] private Color _healthDamageColor;
    [SerializeField] private Color _blockedDamageColor;
    [SerializeField] private TMP_Text _enemyDamageNumber;
    [SerializeField] private TMP_Text _enemyCriticalFlavorText;
    [SerializeField] private Image _enemyBuffIcon;
    [SerializeField] private TMP_Text _playerDamageNumber;
    [SerializeField] private TMP_Text _playerCriticalFlavorText;
    [SerializeField] private Image _playerBuffIcon;
    [SerializeField] private Transform _enemyDamageStartPosition;
    [SerializeField] private Transform _enemyDamageEndPosition;
    [SerializeField] private Transform _playerDamageStartPosition;
    [SerializeField] private Transform _playerDamageEndPosition;

    private WaitForEndOfFrame _waitForEndOfFrame= new WaitForEndOfFrame();
    private Coroutine _enemyDamageNumberMovementRoutine;
    private Coroutine _playerDamageNumberMovementRoutine;
    private Coroutine _enemyBuffIconMovementRoutine;
    private Coroutine _playerBuffIconMovementRoutine;
    
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        if (Instance != this)
            Destroy(this);
        
        _enemyDamageNumber.gameObject.SetActive(false);
        _playerDamageNumber.gameObject.SetActive(false);
        _enemyBuffIcon.gameObject.SetActive(false);
        _playerBuffIcon.gameObject.SetActive(false);
    }

    public void ShowEnemyDamageNumber(int value, bool blocked, bool critical)
    {
        _enemyDamageNumber.text = critical ? "-" + value + "!": "-" + value;

        _enemyDamageNumber.color = blocked ? _blockedDamageColor : _healthDamageColor;
        _enemyCriticalFlavorText.color = blocked ? _blockedDamageColor : _healthDamageColor;
        _enemyCriticalFlavorText.gameObject.SetActive(critical);

        if(_enemyDamageNumberMovementRoutine != null)
            StopCoroutine(_enemyDamageNumberMovementRoutine);

        _enemyDamageNumberMovementRoutine = StartCoroutine(LerpToPosition(_enemyDamageStartPosition.position, _enemyDamageEndPosition.position, _enemyDamageNumber.gameObject));
    }

    public void ShowPlayerDamageNumber(int value, bool blocked, bool critical)
    {
        _playerDamageNumber.text = critical ? "-" + value + "!": "-" + value;

        _playerDamageNumber.color = blocked ? _blockedDamageColor : _healthDamageColor;
        _playerCriticalFlavorText.color = blocked ? _blockedDamageColor : _healthDamageColor;
        _playerCriticalFlavorText.gameObject.SetActive(critical);

        if(_playerDamageNumberMovementRoutine != null)
            StopCoroutine(_playerDamageNumberMovementRoutine);

        _playerDamageNumberMovementRoutine = StartCoroutine(LerpToPosition(_playerDamageStartPosition.position, _playerDamageEndPosition.position, _playerDamageNumber.gameObject));
    }

    public void ShowEnemyBuff(Sprite buffIcon)
    {
        _enemyBuffIcon.sprite = buffIcon;
        _enemyBuffIcon.GetComponent<Animation>().Play(BUFF_ANIM);

        if(_enemyBuffIconMovementRoutine != null)
            StopCoroutine(_enemyBuffIconMovementRoutine);

        _enemyBuffIconMovementRoutine = StartCoroutine(LerpToPosition(_enemyDamageStartPosition.position, _enemyDamageEndPosition.position, _enemyBuffIcon.gameObject));
    }

    public void ShowPlayerBuff(Sprite buffIcon, Color buffColor)
    {
        _playerBuffIcon.sprite = buffIcon;
        _playerBuffIcon.color = buffColor;
        _playerBuffIcon.GetComponent<Animation>().Play(BUFF_ANIM);

        if(_playerBuffIconMovementRoutine != null)
            StopCoroutine(_playerBuffIconMovementRoutine);

        _playerBuffIconMovementRoutine = StartCoroutine(LerpToPosition(_playerDamageStartPosition.position, _playerDamageEndPosition.position, _playerBuffIcon.gameObject));
    }

    private IEnumerator LerpToPosition(Vector3 startPos, Vector3 endPos, GameObject target)
    {
        target.SetActive(true);
        float currentTimer = 0;
        while(currentTimer < DAMAGE_NUMBER_DURATION)
        {
            currentTimer += Time.deltaTime;
            target.transform.position = Vector3.Lerp(startPos, endPos, currentTimer / DAMAGE_NUMBER_DURATION);
            yield return _waitForEndOfFrame;
        }
        target.SetActive(false);
    }
}
