using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

using BehaviourTreeLib;

public class CommonBehaviour
{
    public class TestAction : ActionClass
    {
        float _counter;
        UnityAction<NodeState> _callback;

        public override void Start(UnityAction<NodeState> callback)
        {
            Debug.Log("Start");

            _counter = 0;
            _callback = callback;
        }

        public override void Update()
        {
            Debug.Log("Update");

            _counter += Time.deltaTime;
            if (_counter >= 3)
            {
                _callback(NodeState.True);
            }
        }

        public override void Stop()
        {
            Debug.Log("Stop");
        }
    }

    public class WaitOneFrame : ActionClass
    {
        UnityAction<NodeState> _callback;
        float _counter;

        public override void Start(UnityAction<NodeState> callback)
        {
            _callback = callback;

            _counter = 0;
        }

        public override void Update()
        {
            if (_counter++ > 0)
            {
                _callback(NodeState.True);
            }
        }

        public override void Stop()
        {

        }
    }

    public class WaitSecond : ActionClass
    {
        BT_Input<float> minTime;
        BT_Input<float> maxTime;
        UnityAction<NodeState> _callback;
        float _waitTime;
        float _counter;

        public override void Start(UnityAction<NodeState> callback)
        {
            _callback = callback;

            _waitTime = Random.Range(minTime.value, maxTime.value);
            _counter = 0;
        }

        public override void Update()
        {
            _counter += Time.deltaTime;
            if (_counter >= _waitTime)
            {
                _callback(NodeState.True);
            }
        }

        public override void Stop()
        {

        }
    }

    public class IsSmaller : ConditionClass
    {
        BT_Input<float> a;
        BT_Input<float> b;


        public override bool Execute()
        {
            return a.value < b.value;
        }
    }
}
