using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
    [SerializeField]
    Rigidbody _rigidbody;

    [SerializeField]
    float _limitAngle;

    State _openState;
    State _closeState;
    State _currentState;


    private void OnTriggerExit(Collider other)
    {
        ChangeStateTo(_closeState);

        Debug.Log("Exit");
    }

    public void Open()
    {
        ChangeStateTo(_openState);

        // 少しドアが空くようにする
    }

    private void ChangeStateTo(State state)
    {
        _currentState?.Exit();
        _currentState = state;
        _currentState.Enter();
    }

    private void Start()
    {
        _rigidbody.centerOfMass = _rigidbody.centerOfMass.RemoveX();

        _openState = new OpenState(this);
        _closeState = new CloseState(this);
        ChangeStateTo(_openState);
    }

    private void FixedUpdate()
    {
        if (_rigidbody.rotation.eulerAngles.y > _limitAngle && _rigidbody.rotation.eulerAngles.y <= 180)
        {
            _rigidbody.angularVelocity = Vector3.zero;
            _rigidbody.rotation = Quaternion.Euler(_rigidbody.rotation.eulerAngles.x, _limitAngle, _rigidbody.rotation.eulerAngles.z);
        }
        else if (_rigidbody.rotation.eulerAngles.y < 360 - _limitAngle && _rigidbody.rotation.eulerAngles.y >= 180)
        {
            _rigidbody.angularVelocity = Vector3.zero;
            _rigidbody.rotation = Quaternion.Euler(_rigidbody.rotation.eulerAngles.x, 360 - _limitAngle, _rigidbody.rotation.eulerAngles.z);
        }

        _currentState.FixedUpdate();
    }

    private void LateUpdate()
    {
        _currentState.LateUpdate();
    }

    private abstract class State
    {
        protected Door Door;

        public State(Door door)
        {
            Door = door;
        }

        public abstract void Enter();
        public abstract void Exit();

        public abstract void FixedUpdate();
        public abstract void LateUpdate();
    }

    private class CloseState : State
    {
        public CloseState(Door door) : base(door) { }

        public override void Enter()
        {

        }

        public override void Exit()
        {
            base.Door._rigidbody.constraints = RigidbodyConstraints.FreezeAll ^ RigidbodyConstraints.FreezeRotationY;
        }

        public override void FixedUpdate()
        {
            // ドアが閉まっていいないならば
            if (180 - Mathf.Abs(base.Door._rigidbody.rotation.eulerAngles.y - 180) > 1)
            {
                //base.Door._rigidbody.rotation = Quaternion.Euler(base.Door._rigidbody.rotation.eulerAngles.x, Mathf.Lerp(base.Door._rigidbody.rotation.eulerAngles.y, Mathf.FloorToInt(base.Door._rigidbody.rotation.eulerAngles.y / 180.0f) * 360, Time.deltaTime), base.Door._rigidbody.rotation.eulerAngles.z);
                base.Door._rigidbody.rotation = Quaternion.Euler(base.Door._rigidbody.rotation.eulerAngles.x, base.Door._rigidbody.rotation.eulerAngles.y + (base.Door._rigidbody.rotation.eulerAngles.y < 180 ? -1 : 1), base.Door._rigidbody.rotation.eulerAngles.z);
            }
            else
            {
                base.Door._rigidbody.rotation = Quaternion.Euler(base.Door._rigidbody.rotation.eulerAngles.x, 0, base.Door._rigidbody.rotation.eulerAngles.z);
                //base.Door._rigidbody.constraints = RigidbodyConstraints.FreezeAll;
            }
        }

        public override void LateUpdate()
        {
            
        }
    }

    private class OpenState : State
    {
        public OpenState(Door door) : base(door) { }

        public override void Enter()
        {

        }

        public override void Exit()
        {

        }

        public override void FixedUpdate()
        {

        }

        public override void LateUpdate()
        {

        }
    }
}
