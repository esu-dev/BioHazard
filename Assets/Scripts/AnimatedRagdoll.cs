using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatedRagdoll : MonoBehaviour
{
    [SerializeField]
    GameObject _ragdollRoot;

    [SerializeField]
    Animator _ragdollAnimator;

    [SerializeField]
    Animator _mainBodyAnimator;

    GameObject _mainRoot;


    [field: SerializeField]
    public GameObject MainBodyGameObject { get; private set; }


    public void AddForce(HumanBodyBones humanBodyBones, Vector3 force)
    {
        _ragdollAnimator.GetBoneTransform(humanBodyBones).GetComponent<Rigidbody>()?.AddForce(force, ForceMode.Impulse);
    }

    public void SetRagdollChild(HumanBodyBones humanBodyBones)
    {
        Transform root = _ragdollAnimator.GetBoneTransform(humanBodyBones);
        SetKinematicChild(root, false);
    }

    public void SetKinematic(HumanBodyBones humanBodyBones, bool isKinematic)
    {
        SetKinematic(_ragdollAnimator.GetBoneTransform(humanBodyBones), isKinematic);
    }

    public void SetKinematicAll()
    {
        SetKinematicChild(_ragdollRoot.transform, true);
    }

    void SetKinematic(Transform ragdollBone, bool isKinematic)
    {
        if (ragdollBone.TryGetComponent(out Rigidbody rb))
        {
            rb.isKinematic = isKinematic;
        }
    }

    void SetKinematicChild(Transform ragdollBone, bool isKinematic)
    {
        SetKinematic(ragdollBone, isKinematic);

        for (int i = 0; i < ragdollBone.childCount; i++)
        {
            SetKinematicChild(ragdollBone.transform.GetChild(i), isKinematic);
        }
    }

    private void Start()
    {
        Transform hips = _mainBodyAnimator.GetBoneTransform(HumanBodyBones.Hips);
        _mainRoot = hips.parent.gameObject;
    }

    private void LateUpdate()
    {
        // BoneがRagdollのBoneのTransformを参照する

        Copy(_mainRoot.transform, _ragdollRoot.transform);

        void Copy(Transform bone, Transform ragdollBone)
        {
            for (int i = 0; i < bone.childCount; i++)
            {
                Transform childBone = bone.GetChild(i);
                Transform childRagdollBone = ragdollBone.GetChild(i);

                // もしragdollのboneがkinematicでなかったらコピー
                if (childRagdollBone.TryGetComponent(out Rigidbody rb) && !rb.isKinematic)
                {
                    childBone.localPosition = childRagdollBone.localPosition;
                    childBone.rotation = childRagdollBone.rotation;
                }
                else
                {
                    childRagdollBone.localPosition = childBone.localPosition;
                    childRagdollBone.rotation = childBone.rotation;
                }

                Copy(bone.GetChild(i), ragdollBone.transform.GetChild(i));
            }
        }

        //_animator.GetBoneTransform(HumanBodyBones.Spine).localRotation = Quaternion.Euler(0, 0, -60);
    }
}
