using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class RigManager : MonoBehaviour
{
    [SerializeField]
    MultiAimConstraint _spine1;

    [SerializeField]
    MultiAimConstraint _spine2;

    [SerializeField]
    MultiAimConstraint _shoulderL;

    [SerializeField]
    MultiAimConstraint _shoulderR;

    [SerializeField]
    MultiAimConstraint _neck;

    [SerializeField]
    MultiAimConstraint _head;

    [SerializeField]
    MultiAimConstraint _rightHand;

    WeightData _weightData;

    public void SetWeight(WeightData weightData)
    {
        _weightData = weightData;
    }

    public class WeightData
    {
        public float spine1;
        public float spine2;
        public float shoulderL;
        public float shoulderR;
        public float neck;
        public float head;
        public float rightHand;
    }

    private void Update()
    {
        if (_weightData != null)
        {
            _spine1.weight = _weightData.spine1;
            _spine2.weight = _weightData.spine2;
            _shoulderL.weight = _weightData.shoulderL;
            _shoulderR.weight = _weightData.shoulderR;
            _neck.weight = _weightData.neck;
            _head.weight = _weightData.head;
            _rightHand.weight = _weightData.rightHand;
        }
    }
}
