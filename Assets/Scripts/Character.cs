using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Character : Humanoid
{
    [SerializeField]
    GameObject _cameraRoot;

    [SerializeField]
    GameObject _aimSphere;

    [SerializeField]
    Inventory _inventory;

    [SerializeField]
    Mover _mover;

    [SerializeField]
    CameraRotater _cameraRotater;

    [SerializeField]
    CameraChanger _cameraChanger;

    [SerializeField]
    Rigidbody _rb;

    [SerializeField]
    LayerMask _interactionLayer;


    [SerializeField]
    float _aimDistance;

    [SerializeField]
    float _aimSpeed;


    public UnityEvent<InteractedObject> OnInterect = new UnityEvent<InteractedObject>();


    public State normalState { get; private set; }
    public State aimState { get; private set; }

    State _state;


    // 以下Debug用
    Vector3 _interactPosition;


    private void OnDrawGizmos() // 最終的には分離した方が良いよ
    {
        Gizmos.DrawWireSphere(_interactPosition, 0.5f);
    }

    public override void Damage(int value)
    {
        base.HP -= value;

        ExeHitAnimation();
    }

    protected override void ExeHitAnimation()
    {

    }

    public void ChangeStateTo(State state)
    {
        _state = state;
        _state.Enter();
    }

    public void Interact()
    {
        if (_state is NormalState)
        {
            _interactPosition = this.transform.position;

            Collider[] colliders;
            if ((colliders = Physics.OverlapCapsule(this.transform.position, this.transform.position.AddY(1.5f), 0.5f, _interactionLayer)).Length > 0)
            {
                Debug.Log(colliders[0].gameObject);

                if (colliders[0].gameObject.TryGetComponent(out InteractedObject interactedObject))
                {
                    if (interactedObject is DroppedItem)
                    {
                        //_inventory.Add((interactedObject as DroppedItem).getItem, (interactedObject as DroppedItem).amount);
                        OnInterect.Invoke(interactedObject);
                        (interactedObject as DroppedItem).Gotten();
                    }
                    else
                    {
                        interactedObject.Interacted();
                    }
                }
            }
        }
    }

    private void Start()
    {
        normalState = new NormalState(this);
        aimState = new AimState(this);

        ChangeStateTo(normalState);
    }

    private void Update()
    {
        _state.Update();
    }

    private void FixedUpdate()
    {
        //_rb.velocity = _currentVelocity.ToVector3XZ().AddY(_rb.velocity.y);
        //_animator.SetFloat("Speed", _rb.velocity.magnitude, 0.1f, Time.deltaTime);
        _animator.SetFloat("Speed", _currentVelocity.magnitude, base.SecondsToMaxSpeed, Time.deltaTime);
    }

    public abstract class State
    {
        protected Character man;

        public State(Character man)
        {
            this.man = man;
        }

        public virtual void Enter() { }
        public virtual void Update() { }
    }

    public class NormalState : State
    {
        public NormalState(Character man) : base(man) { }

        public override void Enter()
        {
            base.man._cameraChanger?.ChangeToNormalCamera();
            base.man._animator.SetBool("IsAiming", false);
        }

        public override void Update()
        {
            base.man._cameraRotater?.Rotate();

            // 進行方向に回転
            /*if (base.man._currentVelocity != Vector2.zero)
            {
                base.man.transform.rotation = Quaternion.Lerp(base.man.transform.rotation, Quaternion.LookRotation(base.man._currentVelocity.ToVector3XZ()), 0.2f);
            }*/

            base.man._mover.Rotate(Camera.main.transform.forward.ToVector2XZ());
        }
    }

    public class WalkState : State
    {
        public WalkState(Character character) : base(character) { }
    }

    public class AimState : State
    {
        public AimState(Character man) : base(man) { }

        public override void Enter()
        {
            base.man._cameraChanger?.ChangeToAimCamera();
            base.man._animator.SetBool("IsAiming", true);
        }

        public override void Update()
        {
            base.man._cameraRotater?.Rotate();

            // カメラ方向を向くように回転
            //base.man.transform.rotation = Quaternion.Slerp(base.man.transform.rotation, Quaternion.LookRotation(base.man._cameraRoot.transform.forward.RemoveY()), 0.5f);
            base.man.transform.forward = Vector3.Slerp(base.man.transform.forward, (base.man._aimSphere.transform.position - base.man.transform.position).RemoveY(), base.man._aimSpeed * Time.deltaTime);

            Ray ray = Camera.main.ViewportPointToRay(Vector2.one / 2);
            //RaycastHit hit;
            if (Physics.Raycast(ray, out RaycastHit hit, base.man._aimDistance))
            {
                base.man._aimSphere.transform.position = Vector3.Lerp(base.man._aimSphere.transform.position, hit.point, base.man._aimSpeed * Time.deltaTime);
            }
            else
            {
                base.man._aimSphere.transform.position = Vector3.Lerp(base.man._aimSphere.transform.position, Camera.main.transform.position + Camera.main.transform.forward * base.man._aimDistance, base.man._aimSpeed * Time.deltaTime);
            }
        }
    }
}
