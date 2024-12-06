using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animation))]
public class UiAnimationPlayer : MonoBehaviour
{
    Animation _animation;

    private void Start()
    {
        _animation = GetComponent<Animation>();
    }

    public void PlayAnimation()
    {
        _animation.Play();
    }

    public void PlayAnimationByName(string animationName)
    {
        _animation.Play(animationName);
    }
}
