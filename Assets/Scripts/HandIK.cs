using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandIK : StateMachineBehaviour
{
    AnimatorProxy _animatorProxy;

    public override void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (_animatorProxy == null)
        {
            _animatorProxy = animator.GetComponent<AnimatorProxy>();
        }

        // 内積によってウェイトを変更する(0-1)
        float weight_Left = Mathf.Abs(ArmDot(HumanBodyBones.LeftUpperArm, HumanBodyBones.LeftLowerArm, HumanBodyBones.LeftHand) - 1) / 2;
        float weight_Right = Mathf.Abs(ArmDot(HumanBodyBones.RightUpperArm, HumanBodyBones.RightLowerArm, HumanBodyBones.RightHand) - 1) / 2;

        _animatorProxy.SetIKWeight(AvatarIKGoal.LeftHand, AvatarIKHint.LeftElbow, weight_Left);
        _animatorProxy.SetIKWeight(AvatarIKGoal.RightHand, AvatarIKHint.RightElbow, weight_Right);
    }

    /// <summary>
    /// 腕の内積を求める
    /// </summary>
    /// <param name="upperArm"></param>
    /// <param name="lowerArm"></param>
    /// <param name="hand"></param>
    /// <returns></returns>
    private float ArmDot(HumanBodyBones upperArm, HumanBodyBones lowerArm, HumanBodyBones hand)
    {
        Vector3 lowerArmPosition = _animatorProxy.animator.GetBoneTransform(lowerArm).position;
        Vector3 upperArmDirection = (lowerArmPosition - _animatorProxy.animator.GetBoneTransform(upperArm).position).normalized;

        Vector3 lowerArmDirection = (_animatorProxy.animator.GetBoneTransform(hand).position - lowerArmPosition).normalized;

        return Vector3.Dot(upperArmDirection, lowerArmDirection);
    }
}
