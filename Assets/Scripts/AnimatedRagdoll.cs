using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AnimatedRagdoll : MonoBehaviour
{
    [SerializeField]
    GameObject _ragdollRoot;

    [SerializeField]
    Animator _ragdollAnimator;

    [SerializeField]
    Animator _mainBodyAnimator;

    float _weight = 1;
    GameObject _mainRoot;

    List<BoneData> _boneDataList = new List<BoneData>();

    [field: SerializeField]
    public GameObject MainBodyGameObject { get; private set; }


    public void SetWeight(float weight)
    {
        _weight = weight;
    }

    public void SetWeight(HumanBodyBones humanBodyBone, float weight)
    {
        Transform bone = _ragdollAnimator.GetBoneTransform(humanBodyBone);

        SetWeight(bone, weight);
    }

    public void SetWeightChild(HumanBodyBones humanBodyBone, float weight)
    {
        SetWeightChild(_ragdollAnimator.GetBoneTransform(humanBodyBone), weight);
        
        void SetWeightChild(Transform bone, float weight)
        {
            SetWeight(bone, weight);

            foreach (Transform child in bone)
            {
                SetWeightChild(child, weight);
            }
        }
    }

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

    public void SetIsTriggerAll()
    {
        SetIsTrigger(_ragdollRoot.transform);

        void SetIsTrigger(Transform parent)
        {
            if (parent.TryGetComponent(out Collider collider))
            {
                collider.isTrigger = true;
            }

            foreach (Transform child in parent)
            {
                SetIsTrigger(child);
            }
        }
    }

    public void ExeActionToAllBone(Action<Transform> action)
    {
        ExeAction(_ragdollRoot.transform, action);

        void ExeAction(Transform parent, Action<Transform> action)
        {
            action(parent);

            foreach (Transform child in parent)
            {
                ExeAction(child, action);
            }
        }
    }

    void SetWeight(Transform bone, float weight)
    {
        if (_boneDataList.Any(x => x.boneTransform == bone))
        {
            _boneDataList.Find(x => x.boneTransform == bone).weight = weight;
        }
        else
        {
            BoneData boneData = new BoneData();
            boneData.boneTransform = bone;
            boneData.weight = weight;
            _boneDataList.Add(boneData);
        }
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
        // 本体の回転をコピー
        this.transform.rotation = MainBodyGameObject.transform.rotation;

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
                    // 本体にragdollの情報をコピー
                    float weight = _weight;
                    BoneData boneData = _boneDataList.Find(x => x.boneTransform == childRagdollBone);
                    if (boneData != null)
                    {
                        weight = boneData.weight;
                    }
                    childBone.localPosition = Vector3.Lerp(childBone.localPosition, childRagdollBone.localPosition, weight);
                    childBone.rotation = Quaternion.Lerp(childBone.rotation, childRagdollBone.rotation, weight);
                }
                else
                {
                    // ragdoll側にアニメーション情報をコピー
                    childRagdollBone.localPosition = childBone.localPosition;
                    childRagdollBone.rotation = childBone.rotation;
                }

                Copy(bone.GetChild(i), ragdollBone.transform.GetChild(i));
            }
        }
    }

    class BoneData
    {
        public Transform boneTransform;
        public float weight;
    }
}
