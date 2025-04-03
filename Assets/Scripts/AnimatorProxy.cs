using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatorProxy : FlexUpdateMonoBehaviour
{
    [field: SerializeField]
    public Animator animator { get; set; }

    public void SetFloat(AnimatorParameterName name, float value)
    {
        animator.SetFloat(name.Name, value);
    }

    public void SetFloat(AnimatorParameterName name, float value, float dampTime, float deltaTime)
    {
        animator.SetFloat(name.Name, value, dampTime, deltaTime);
    }

    public void SetBool(AnimatorParameterName name, bool value)
    {
        animator.SetBool(name.Name, value);
    }

    public void SetTrigger(AnimatorParameterName name)
    {
        animator.SetTrigger(name.Name);
    }

    public void SetIKWeight(AvatarIKGoal goal, AvatarIKHint hint, float value)
    {
        animator.SetIKPositionWeight(goal, value);
        animator.SetIKRotationWeight(goal, value);
        animator.SetIKHintPositionWeight(hint, value);
    }

    public void SetLookAtPosition(Vector3 lookAtPosition)
    {
        animator.SetLookAtPosition(lookAtPosition);
    }

    public void SetLookAtWeight(float weight, float bodyWeight, float headWeight)
    {
        animator.SetLookAtWeight(weight, bodyWeight, headWeight);
    }

    public void CrossFadeInFixedTime(string stateName, float fixedTransitionDuration)
    {
        animator.CrossFadeInFixedTime(stateName, fixedTransitionDuration);
    }

    protected override void FlexUpdate()
    {
        animator.Update(base.FlexDeltaTime);
    }
}
