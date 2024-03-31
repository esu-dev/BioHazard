using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour
{
    [SerializeField]
    float _speed;

    [SerializeField]
    GameObject _cameraRoot;

    [SerializeField]
    GameObject _aimSphere;

    [SerializeField]
    Inventory _inventory;

    [SerializeField]
    CameraRotater _cameraRotater;

    [SerializeField]
    CameraChanger _cameraChanger;

    [SerializeField]
    Rigidbody _rb;

    [SerializeField]
    Animator _animator;

    public State normalState { get; private set; }
    public State aimState { get; private set; }

    State _state;
    Vector2 _currentVelocity;

    public void Move(Vector2 direction)
    {
        _currentVelocity = _speed * direction;
    }

    public void ChangeStateTo(State state)
    {
        _state = state;
        _state.Start();
    }

    private void Start()
    {
        normalState = new NormalState(this);
        aimState = new AimState(this);

        ChangeStateTo(normalState);
    }

    private void Update()
    {
    }

    private void FixedUpdate()
    {
        _state.Update();

        _rb.velocity = _currentVelocity.ToVector3XZ().AddY(_rb.velocity.y);
        _animator.SetFloat("Speed", _rb.velocity.magnitude, 0.1f, Time.deltaTime);
    }

    public abstract class State
    {
        protected Character man;

        public State(Character man)
        {
            this.man = man;
        }

        public abstract void Start();
        public abstract void Update();
    }

    public class NormalState : State
    {
        public NormalState(Character man) : base(man) { }

        public override void Start()
        {
            base.man._cameraChanger?.ChangeToNormalCamera();
            base.man._animator.SetBool("IsAiming", false);
        }

        public override void Update()
        {
            base.man._cameraRotater?.Rotate();

            // êiçsï˚å¸Ç…âÒì]
            if (base.man._currentVelocity != Vector2.zero)
            {
                base.man.transform.rotation = Quaternion.Lerp(base.man.transform.rotation, Quaternion.LookRotation(base.man._currentVelocity.ToVector3XZ()), 0.2f);
            }
        }
    }

    public class AimState : State
    {
        public AimState(Character man) : base(man) { }

        public override void Start()
        {
            base.man._cameraChanger?.ChangeToAimCamera();
            base.man._animator.SetBool("IsAiming", true);
        }

        public override void Update()
        {
            base.man._cameraRotater?.Rotate();

            // ÉJÉÅÉâï˚å¸Çå¸Ç≠ÇÊÇ§Ç…âÒì]
            base.man.transform.rotation = Quaternion.Slerp(base.man.transform.rotation, Quaternion.LookRotation(base.man._cameraRoot.transform.forward.RemoveY()), 0.5f);

            base.man._aimSphere.transform.position = Vector3.Slerp(base.man._aimSphere.transform.position, base.man._cameraRoot.transform.position + 10 * base.man._cameraRoot.transform.forward, 0.5f);

        }
    }
}
