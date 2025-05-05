using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
    [SerializeField]
    Rigidbody _rigidbody;

    [SerializeField]
    float _limitAngle;

    float _defaultY;
    Vector3 _defaultForward;

    State _openState;
    State _closeState;
    State _currentState;


    private void OnCollisionEnter(Collision collision)
    {
        ChangeStateTo(_openState);
    }

    private void OnTriggerExit(Collider other)
    {
        ChangeStateTo(_closeState);
    }

    public void Open(bool isPushed)
    {
        ChangeStateTo(_openState);

        // 少しドアが空くようにする
        // トルクを加える
        _rigidbody.AddTorque(new Vector3(0, isPushed ? 0.5f : -0.5f, 0), ForceMode.Impulse);
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

        _defaultY = _rigidbody.rotation.eulerAngles.y;
        _defaultForward = _rigidbody.transform.forward;

        _openState = new OpenState(this);
        _closeState = new CloseState(this);
        ChangeStateTo(_closeState);
    }

    private void FixedUpdate()
    {
        /*if (_rigidbody.rotation.eulerAngles.y > _limitAngle && _rigidbody.rotation.eulerAngles.y <= 180)
        {
            _rigidbody.angularVelocity = Vector3.zero;
            _rigidbody.rotation = Quaternion.Euler(_rigidbody.rotation.eulerAngles.x, _limitAngle, _rigidbody.rotation.eulerAngles.z);
        }
        else if (_rigidbody.rotation.eulerAngles.y < 360 - _limitAngle && _rigidbody.rotation.eulerAngles.y >= 180)
        {
            _rigidbody.angularVelocity = Vector3.zero;
            _rigidbody.rotation = Quaternion.Euler(_rigidbody.rotation.eulerAngles.x, 360 - _limitAngle, _rigidbody.rotation.eulerAngles.z);
        }*/

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
            float angle = Vector3.SignedAngle(base.Door._rigidbody.transform.forward, base.Door._defaultForward, Vector3.up);

            // ドアが閉まっていいないならば
            if (Mathf.Abs(angle) > 45f * Time.fixedDeltaTime)
            {
                base.Door._rigidbody.transform.forward = Quaternion.AngleAxis(Mathf.Sign(angle) * 45f * Time.fixedDeltaTime, Vector3.up) * base.Door._rigidbody.transform.forward;
            }
            else
            {
                base.Door.transform.forward = base.Door._defaultForward;
                base.Door._rigidbody.rotation = Quaternion.Euler(0, base.Door._defaultY, 0);
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
