using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class HumanoidBoneTransformer : MonoBehaviour
{
    [SerializeField]
    AnimatorProxy _animatorProxy;

    List<LookAtPositionData> _lookAtPositionDataList = new List<LookAtPositionData>();

    public void SetLookAtPosition(Vector3 lookAtPosition)
    {
        _animatorProxy.SetLookAtPosition(lookAtPosition);
    }

    public void SetLookAtWeight(float weight, float bodyWeight, float headWeight)
    {
        _animatorProxy.SetLookAtWeight(weight, bodyWeight, headWeight);
    }

    public void SetLookAtPosition(HumanBodyBones humanBodyBones, Vector3 lookAtPosition, Axis lookAxis, Axis upAxis)
    {
        /*int index;
        if ((index = _lookAtPositionDataList.Select(x => x.humanBodyBones).ToList().IndexOf(humanBodyBones)) != -1)
        {
            _lookAtPositionDataList[index].humanBodyBones = humanBodyBones;
            _lookAtPositionDataList[index].lookAtPosition = lookAtPosition;
            _lookAtPositionDataList[index].axis = axis;
        }
        else
        {
            LookAtPositionData lookAtPositionData = new LookAtPositionData();
            lookAtPositionData.humanBodyBones = humanBodyBones;
            lookAtPositionData.lookAtPosition = lookAtPosition;
            lookAtPositionData.axis = axis;

            _lookAtPositionDataList.Add(lookAtPositionData);
        }*/

        Transform bone = _animatorProxy.animator.GetBoneTransform(humanBodyBones);
        Vector3 direction = lookAtPosition - bone.position;

        Vector3 upVector;
        switch (upAxis)
        {
            case Axis.Y:
                upVector = bone.up;
                bone.up = Vector3.up;
                break;
            case Axis.Z:
                upVector = bone.forward;
                bone.forward = Vector3.up;
                break;
            default:
                upVector = -bone.forward;
                bone.forward = -Vector3.up;
                break;
        }

        Vector3 lookVector;
        switch (lookAxis)
        {
            case Axis.Y:
                lookVector = bone.up;
                break;
            case Axis.Z:
                lookVector = bone.forward;
                break;
            default:
                lookVector = -bone.forward;
                break;
        }

        bone.transform.rotation = Quaternion.AngleAxis(Vector3.SignedAngle(lookVector, direction.RemoveY(), Vector3.up), Vector3.up) * bone.transform.rotation;

        switch (lookAxis)
        {
            case Axis.Y:
                lookVector = bone.up;
                break;
            case Axis.Z:
                lookVector = bone.forward;
                break;
            default:
                lookVector = -bone.forward;
                break;
        }

        bone.transform.rotation = Quaternion.FromToRotation(lookVector, direction) * bone.transform.rotation;
    }

    public void SetLookAtWeight(HumanBodyBones humanBodyBones, float weight)
    {
        /*int index;
        if ((index = _lookAtPositionDataList.Select(x => x.humanBodyBones).ToList().IndexOf(humanBodyBones)) != -1)
        {
            _lookAtPositionDataList[index].humanBodyBones = humanBodyBones;
            _lookAtPositionDataList[index].weight = weight;
        }
        else
        {
            LookAtPositionData lookAtPositionData = new LookAtPositionData();
            lookAtPositionData.humanBodyBones = humanBodyBones;
            lookAtPositionData.weight = weight;

            _lookAtPositionDataList.Add(lookAtPositionData);
        }*/
    }

    private void OnAnimatorIK(int layerIndex)
    {
        /*foreach (LookAtPositionData lookAtPositionData in _lookAtPositionDataList)
        {
            Transform bone = _animatorProxy.animator.GetBoneTransform(lookAtPositionData.humanBodyBones);
            bone.forward = lookAtPositionData.lookAtPosition - bone.position;
        }*/
    }

    public enum Axis
    {
        X,
        _X,
        Y,
        _Y,
        Z,
        _Z
    }

    class LookAtPositionData
    {
        public HumanBodyBones humanBodyBones;
        public Vector3 lookAtPosition;
        public Vector3 axis;
        public float weight;
    }
}
