using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class CombatAnimationBehaviour : MonoBehaviour
{
    private const string ATTACK_ANIM = "Attack";
    private const string DEFENSE_ANIM = "Defense";
    private const string BUFF_ANIM = "Buff";
    private const string DEBUFF_ANIM = "Debuff";
    private const string HIT_ANIM = "Hit";
    private const string DEATH_ANIM = "Death";
    private const float SLOMO_ANIM_SPEED = 0.05f;
    private const int SINGLE_ANIM_REPLAY_COUNT = 2;

    [SerializeField] private int _attackAnimCount;

    private int _attackAnimIndex = 1;
    private int _hitAnimIndex = 1;
    private int _defenseAnimIndex = 1;
    private int _buffAnimIndex = 1;
    private int _debuffAnimIndex = 1;
    private Animator _animator;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    public void PlayAttackAnimation()
    {
        _attackAnimIndex++;
        if(_attackAnimIndex > _attackAnimCount)
            _attackAnimIndex = 1;
        _animator.Play(ATTACK_ANIM + _attackAnimIndex);
        _animator.SetFloat("Speed", SLOMO_ANIM_SPEED);
    }

    public void PlayHitAnimation()
    {
        _hitAnimIndex++;
        if(_hitAnimIndex > SINGLE_ANIM_REPLAY_COUNT)
            _hitAnimIndex = 1;
        _animator.Play(HIT_ANIM + _hitAnimIndex);
        _animator.SetFloat("Speed", SLOMO_ANIM_SPEED);
    }

    public void PlayDefenseAnimation()
    {
        _defenseAnimIndex++;
        if(_defenseAnimIndex > SINGLE_ANIM_REPLAY_COUNT)
            _defenseAnimIndex = 1;
        _animator.Play(DEFENSE_ANIM + _defenseAnimIndex);
        _animator.SetFloat("Speed", SLOMO_ANIM_SPEED);
    }

    public void PlayDebuffAnimation()
    {
        _animator.Play(DEBUFF_ANIM + "1");
        _animator.SetFloat("Speed", SLOMO_ANIM_SPEED);
    }

    public void PlayDeathAnimation()
    {
        _animator.Play(DEATH_ANIM);
        //_animator.SetFloat("Speed", SLOMO_ANIM_SPEED);
    }

    public void PlayBuffAnimation(int buffAnimIndex)
    {
        _animator.Play(BUFF_ANIM + _buffAnimIndex);
        _animator.SetFloat("Speed", SLOMO_ANIM_SPEED);
    }

    public void SetAnimSpeedToNormal()
    {
        _animator.SetFloat("Speed", 1f);
    }
}
