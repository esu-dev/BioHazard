using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Character : Humanoid
{
    [SerializeField]
    Inventory _inventory;

    [SerializeField]
    Mover _mover;

    [SerializeField]
    CameraRotater _cameraRotater;

    [SerializeField]
    CameraChanger _cameraChanger;

    [SerializeField]
    HumanoidBoneTransformer _humanoidBoneTransformer;

    [SerializeField]
    Rigidbody _rb;

    [SerializeField]
    GameObject _cameraRoot;

    [SerializeField]
    GameObject _aimSphere;

    [SerializeField]
    LayerMask _interactionLayer;


    [SerializeField]
    float _aimDistance;

    [SerializeField]
    float _aimSpeed;

    [SerializeField]
    Axis _aimAxis;

    [SerializeField]
    Axis _upAxis;


    public UnityEvent<InteractedObject> OnInterect = new UnityEvent<InteractedObject>();


    public State normalState { get; private set; }
    public State aimState { get; private set; }
    State _walkState;

    State _state;

    bool _isRun;

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

    public void Move(Vector2 dirOnCamCoord)
    {
        _state.Move(dirOnCamCoord);
    }

    public void Run()
    {
        _isRun = true;
    }

    public void StopRunning()
    {
        _isRun = false;
    }

    public void ChangeStateTo(State state)
    {
        _state = state;
        _state.Enter();
    }

    public void Interact()
    {
        if (_state is WalkState)
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

    public void SetUpWeapon()
    {
        if (_inventory.EquippedWeapon)
        {
            ChangeStateTo(aimState);
            _inventory.EquippedWeapon.Setup();
        }
    }

    public void LowerWeapon()
    {
        ChangeStateTo(_walkState);
        _inventory.EquippedWeapon?.Lower();
    }

    private void OnAnimatorIK(int layerIndex)
    {
        _state.OnAnimatorIK();
    }

    private void Start()
    {
        aimState = new AimState(this);
        _walkState = new WalkState(this);

        ChangeStateTo(new WalkState(this));
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

    private void LateUpdate()
    {
        _state.LateUpdate();
    }

    public abstract class State
    {
        protected Character character;

        public State(Character man)
        {
            this.character = man;
        }

        public virtual void OnAnimatorIK() { }
        public virtual void Enter() { }
        public virtual void Update() { }
        public virtual void LateUpdate() { }

        public virtual void Move(Vector2 direction) { }
    }

    public class NormalState : State
    {
        public NormalState(Character man) : base(man) { }

        public override void Enter()
        {
            base.character._cameraChanger?.ChangeToNormalCamera();
            base.character._animator.SetBool("IsAiming", false);
        }

        public override void Update()
        {
            base.character._mover.Rotate(Camera.main.transform.forward.ToVector2XZ());
        }

        public override void Move(Vector2 direction)
        {
            
        }
    }

    public class WalkState : State
    {
        public WalkState(Character character) : base(character) { }

        public override void Enter()
        {
            base.character._cameraChanger?.ChangeToNormalCamera();
            base.character._animator.SetBool("IsAiming", false);
        }

        public override void Update()
        {
            base.character._mover.Rotate(Camera.main.transform.forward.ToVector2XZ());

            if (base.character._isRun)
            {
                base.character.ChangeStateTo(new RunState(base.character));
            }
        }

        public override void Move(Vector2 direction)
        {
            base.character._mover.StrafeMove((Quaternion.FromToRotation(base.character.transform.forward, Camera.main.transform.forward.RemoveY()) * direction.ToVector3XZ()).ToVector2XZ());
        }
    }

    public class RunState : State
    {
        public RunState(Character character) : base(character) { }

        public override void Enter()
        {
            base.character._cameraChanger?.ChangeToNormalCamera();
            base.character._animator.SetBool("IsAiming", false);
        }

        public override void Update()
        {
            if (!base.character._isRun)
            {
                base.character.ChangeStateTo(base.character._walkState);
            }
        }

        public override void Move(Vector2 direction)
        {
            Vector2 d = (Quaternion.AngleAxis(Camera.main.transform.rotation.eulerAngles.y, Vector3.up) * direction.ToVector3XZ()).ToVector2XZ();

            base.character._mover.Move(d);
            base.character._mover.Rotate(d);
        }
    }

    public class AimState : State
    {
        public AimState(Character man) : base(man) { }

        public override void OnAnimatorIK()
        {
            // 身体の向きを調整
            base.character._humanoidBoneTransformer.SetLookAtWeight(0.75f, 1f, 1);
            base.character._humanoidBoneTransformer.SetLookAtPosition(base.character._aimSphere.transform.position);
        }

        public override void Enter()
        {
            base.character._cameraChanger?.ChangeToAimCamera();
            base.character._animator.SetBool("IsAiming", true);
        }

        public override void Update()
        {
            // カメラ方向を向くように回転
            base.character.transform.forward = Vector3.Slerp(base.character.transform.forward, (base.character._aimSphere.transform.position - base.character.transform.position).RemoveY(), base.character._aimSpeed * Time.deltaTime);

            // AimSphereの移動
            Ray ray = Camera.main.ViewportPointToRay(Vector2.one / 2);
            if (Physics.Raycast(ray, out RaycastHit hit, base.character._aimDistance))
            {
                base.character._aimSphere.transform.position = Vector3.Lerp(base.character._aimSphere.transform.position, hit.point, base.character._aimSpeed * Time.deltaTime);
            }
            else
            {
                base.character._aimSphere.transform.position = Vector3.Lerp(base.character._aimSphere.transform.position, Camera.main.transform.position + Camera.main.transform.forward * base.character._aimDistance, base.character._aimSpeed * Time.deltaTime);
            }
        }

        public override void LateUpdate()
        {
            // 腕の向き調整
            base.character._humanoidBoneTransformer.SetLookAtPosition(HumanBodyBones.RightHand, base.character._aimSphere.transform.position, base.character._aimAxis, base.character._upAxis);
        }

        public override void Move(Vector2 direction)
        {
            base.character._mover.StrafeMove((Quaternion.FromToRotation(base.character.transform.forward, Camera.main.transform.forward.RemoveY()) * direction.ToVector3XZ()).ToVector2XZ());
        }
    }
}
