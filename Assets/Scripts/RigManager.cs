using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class RigManager : MonoBehaviour
{
    [SerializeField]
    MultiAimConstraint _spine;

    [SerializeField]
    MultiAimConstraint _rightHand;

    WeightData _weightData;

    public void SetWeight(WeightData weightData)
    {
        _weightData = weightData;
    }

    public class WeightData
    {
        public float spine;
        public float rightHand;
    }

    private void Update()
    {
        if (_weightData != null)
        {
            _spine.weight = _weightData.spine;
            _rightHand.weight = _weightData.rightHand;
        }
    }
}
