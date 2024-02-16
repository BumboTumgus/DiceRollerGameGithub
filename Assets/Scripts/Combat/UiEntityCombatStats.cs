using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UiEntityCombatStats : MonoBehaviour
{
    private const string HIDE_UI_ANIM = "Ui_HideOnDeath";
    private const string SHOW_UI_ANIM = "Ui_ShowOnSpawn";

    public Transform ConnectedTarget;

    [SerializeField] private TMP_Text _attackReadout;
    [SerializeField] private TMP_Text _defenseReadout;
    [SerializeField] private TMP_Text _healthReadout;
    [SerializeField] private Image _healthBarScalingImage;
    [SerializeField] private Animation _attackAnimation;
    [SerializeField] private Animation _defenseAnimation;
    [SerializeField] private Animation _healthAnimation;
    [SerializeField] private Image _enemyAttackIcon;
    [SerializeField] private TMP_Text _enemyAttackText;
    [SerializeField] private Animation _enemyAttackContainerAnimation;
    [SerializeField] private Animation _enemyTurnIndicatorAnimation;
    [SerializeField] private RectTransform _playerAttackMarkerCounter;
    [SerializeField] private GameObject _playerAttackMarker;


    public void UpdateAttackReadout(int attackDamage, int attackCount, bool animateChange = true)
    {
        if(attackCount > 1)
            _attackReadout.text = attackDamage + " x " + attackCount;
        else
            _attackReadout.text = attackDamage + "";
            
        _attackReadout.rectTransform.sizeDelta = new Vector2(_attackReadout.preferredWidth, _attackReadout.rectTransform.sizeDelta.y);

        if(!animateChange)
            return;

        _attackAnimation.Stop();
        _attackAnimation.Play("Ui_AttackIncrease");
    }

    public void UpdateDefenseReadout(int defense, bool defenseIncreased, bool animateChange = true)
    {
        _defenseReadout.text = defense + "";
        _defenseReadout.rectTransform.sizeDelta = new Vector2(_defenseReadout.preferredWidth, _defenseReadout.rectTransform.sizeDelta.y);

        if(!animateChange)
            return;

        _defenseAnimation.Stop();
        if(defenseIncreased)
            _defenseAnimation.Play("Ui_DefenseIncrease");
        else
            _defenseAnimation.Play("Ui_DefenseDecrease");
    }

    public void UpdateHealthReadout(int healthCurrent, int healthMax, bool healthIncreased, bool animateChange = true)
    {
        _healthReadout.text = healthCurrent + " / " + healthMax;
        _healthBarScalingImage.fillAmount = (float) healthCurrent / (float) healthMax;

        if(!animateChange)
            return;

        _healthAnimation.Stop();
        if(healthIncreased)
            _healthAnimation.Play("Ui_HealthIncrease");
        else
            _healthAnimation.Play("Ui_HealthDecrease");
    }

    public void ShowEnemyAttack(EnemyAttackScriptableObject enemyAttackSO)
    {
        if(enemyAttackSO == null)
        {
            _enemyAttackContainerAnimation.Play("Ui_EnemyAttack_Disappear");
            return;
        }

        _enemyAttackContainerAnimation.Play("Ui_EnemyAttack_Appear");
        _enemyAttackIcon.sprite = enemyAttackSO.AttackIcon;
        _enemyAttackIcon.color = enemyAttackSO.AttackIconColor;
        _enemyAttackText.color = enemyAttackSO.AttackTextColor;
        if(enemyAttackSO.AttackDamage > 0)
        {
            _enemyAttackText.gameObject.SetActive(true);
            if(enemyAttackSO.AttackCount > 1)
                _enemyAttackText.text = enemyAttackSO.AttackDamage + " x " + enemyAttackSO.AttackCount;
            else
                _enemyAttackText.text = enemyAttackSO.AttackDamage + "";
        }
        else
            _enemyAttackText.gameObject.SetActive(false);
    }

    public void HideUi()
    {
        GetComponent<Animation>().Play(HIDE_UI_ANIM);
    }
    
    public void ShowUi()
    {
        GetComponent<Animation>().Play(SHOW_UI_ANIM);
    }

    public void TriggerTurnIndicator(bool enemyTurn, bool snapToTarget = false)
    {
        if(!snapToTarget)
        {
            if(enemyTurn)
                _enemyTurnIndicatorAnimation.Play("Ui_TurnIndicator_Appear");
            else
                _enemyTurnIndicatorAnimation.Play("Ui_TurnIndicator_Disappear");
        }
        else
        {
            if(enemyTurn)
                _enemyTurnIndicatorAnimation.transform.localScale = new Vector3(1,1,1);
            else
                _enemyTurnIndicatorAnimation.transform.localScale = new Vector3(0,1,1);
        }
    }

    public void AddPlayerAttackMarker()
    {
        Instantiate(_playerAttackMarker, _playerAttackMarkerCounter);
        StartCoroutine(UpdatePlayerMarkerHolderAtEndOfFrame());
    }

    private IEnumerator UpdatePlayerMarkerHolderAtEndOfFrame()
    {
        yield return new WaitForEndOfFrame();
        _playerAttackMarkerCounter.sizeDelta = new Vector2(_playerAttackMarkerCounter.GetComponent<HorizontalLayoutGroup>().preferredWidth, _playerAttackMarkerCounter.sizeDelta.y);

    }

    public void RemovePlayerAttackMarker()
    {
        if(_playerAttackMarkerCounter.GetChild(0) != null)
            Destroy(_playerAttackMarkerCounter.GetChild(0).gameObject);
        StartCoroutine(UpdatePlayerMarkerHolderAtEndOfFrame());
    }
}
