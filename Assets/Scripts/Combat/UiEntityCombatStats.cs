using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UiEntityCombatStats : MonoBehaviour
{
    private const string HIDE_UI_ANIM = "Ui_HideOnDeath";
    private const string SHOW_UI_ANIM = "Ui_ShowOnSpawn";
    private const float ATTACK_DEFENSE_SPACER = 20f;
    private const float ATTACK_DEFENSE_PARENT_PADDING = 60f;

    private const string PLAYER_ACTION_MARKER_TITLE = "Ui_PlayerActionMarker_";
    private const string PLAYER_ACTION_MARKER_ATTACK_SUFFIX = "Attack";

    public Transform ConnectedTarget;
    public UiBuffDescriptionController UiBuffDescriptionController;
    public Transform BuffUiParent;

    [SerializeField] private TMP_Text _attackReadout;
    [SerializeField] private TMP_Text _defenseReadout;
    [SerializeField] private RectTransform _attackDefenseReadoutHorizontalLayout;
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

    private RectTransform _attackReadoutParent;
    private RectTransform _defenseReadoutParent;

    private void Awake()
    {
        _attackReadoutParent = _attackReadout.transform.parent.GetComponent<RectTransform>();
        _defenseReadoutParent = _defenseReadout.transform.parent.GetComponent<RectTransform>();

        ResetReadoutLengthAttack();
        ResetReadoutLengthDefense();
    }

    public void UpdateAttackReadout(int attackDamage, int attackCount, bool animateChange = true)
    {
        if(attackCount > 1)
            _attackReadout.text = attackDamage + " x " + attackCount;
        else
            _attackReadout.text = attackDamage + "";

        ResetReadoutLengthAttack();

        if (!animateChange)
            return;

        _attackAnimation.Stop();
        _attackAnimation.Play("Ui_AttackIncrease");
    }

    public void UpdateDefenseReadout(int defense, bool defenseIncreased, bool animateChange = true)
    {
        _defenseReadout.text = defense + "";

        ResetReadoutLengthDefense();

        if (!animateChange)
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

    public void ShowEnemyAttack(EnemyAttackScriptableObject enemyAttackSO, int strengthBonus, bool weakenedPenalty)
    {
        if(enemyAttackSO == null)
        {
            _enemyAttackContainerAnimation.Play("Ui_EnemyAttack_Disappear");
            return;
        }

        _enemyAttackContainerAnimation.Play("Ui_EnemyAttack_Appear");
        _enemyAttackIcon.sprite = enemyAttackSO.AttackIcon;
        _enemyAttackText.color = enemyAttackSO.AttackTextColor;
        if(enemyAttackSO.AttackDamage > 0)
        {
            _enemyAttackText.gameObject.SetActive(true);
            int damageDealt = enemyAttackSO.AttackDamage + strengthBonus;
            if(weakenedPenalty)
                damageDealt /= 2;

            if(enemyAttackSO.AttackCount > 1)
                _enemyAttackText.text = damageDealt + " x " + enemyAttackSO.AttackCount;
            else
                _enemyAttackText.text = damageDealt + "";
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

    public void AddPlayerActionMarker(BuffScriptableObject buffAdded = null)
    {
        Image playerActionMarker = Instantiate(_playerAttackMarker, _playerAttackMarkerCounter).GetComponent<Image>();

        if(buffAdded)
        {
            playerActionMarker.sprite = buffAdded.BuffIcon;
            playerActionMarker.gameObject.name = PLAYER_ACTION_MARKER_TITLE + buffAdded.MyBuffType.ToString();
        }
        else
            playerActionMarker.gameObject.name = PLAYER_ACTION_MARKER_TITLE + PLAYER_ACTION_MARKER_ATTACK_SUFFIX;

        StartCoroutine(UpdatePlayerMarkerHolderAtEndOfFrame());
    }

    private IEnumerator UpdatePlayerMarkerHolderAtEndOfFrame()
    {
        yield return new WaitForEndOfFrame();
        _playerAttackMarkerCounter.sizeDelta = new Vector2(_playerAttackMarkerCounter.GetComponent<HorizontalLayoutGroup>().preferredWidth, _playerAttackMarkerCounter.sizeDelta.y);

    }

    public void RemovePlayerActionMarker(BuffScriptableObject buffActionToRemove = null)
    {
        string nameOfActionToRemove = PLAYER_ACTION_MARKER_TITLE + PLAYER_ACTION_MARKER_ATTACK_SUFFIX;
        if (buffActionToRemove)
            nameOfActionToRemove = PLAYER_ACTION_MARKER_TITLE + buffActionToRemove.MyBuffType.ToString();

        for(int index = 0; index < _playerAttackMarkerCounter.childCount; index++) 
        { 
            if(_playerAttackMarkerCounter.GetChild(index).name == nameOfActionToRemove)
            {
                Destroy(_playerAttackMarkerCounter.GetChild(index).gameObject);
                break;
            }
        }

        StartCoroutine(UpdatePlayerMarkerHolderAtEndOfFrame());
    }

    private void ResetReadoutLengthAttack()
    {
        _attackReadout.rectTransform.sizeDelta = new Vector2(_attackReadout.preferredWidth, _attackReadout.rectTransform.sizeDelta.y);
        if (_attackReadoutParent == null)
            return;
        _attackReadoutParent.sizeDelta = new Vector2(_attackReadout.preferredWidth + ATTACK_DEFENSE_PARENT_PADDING, _attackReadoutParent.sizeDelta.y);
        _attackDefenseReadoutHorizontalLayout.sizeDelta = new Vector2(_attackReadoutParent.sizeDelta.x + _defenseReadoutParent.sizeDelta.x + ATTACK_DEFENSE_SPACER, _attackDefenseReadoutHorizontalLayout.sizeDelta.y);
    }

    private void ResetReadoutLengthDefense()
    {
        _defenseReadout.rectTransform.sizeDelta = new Vector2(_defenseReadout.preferredWidth, _defenseReadout.rectTransform.sizeDelta.y);
        if (_defenseReadoutParent == null)
            return;
        _defenseReadoutParent.sizeDelta = new Vector2(_defenseReadout.preferredWidth + ATTACK_DEFENSE_PARENT_PADDING + ATTACK_DEFENSE_SPACER, _defenseReadoutParent.sizeDelta.y);
        _attackDefenseReadoutHorizontalLayout.sizeDelta = new Vector2(_attackReadoutParent.sizeDelta.x + _defenseReadoutParent.sizeDelta.x + ATTACK_DEFENSE_SPACER, _attackDefenseReadoutHorizontalLayout.sizeDelta.y);
    }
}
