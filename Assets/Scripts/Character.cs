using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    CameraProxy _cameraProxy;

    [SerializeField]
    HumanoidBoneTransformer _humanoidBoneTransformer;

    [SerializeField]
    AnimatorProxy _animatorProxy;

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


    public UnityEvent<InteractedObject> OnInterectDroppedItem { get; private set; } = new UnityEvent<InteractedObject>();
    public UnityEvent<bool> OnAimStateChange { get; private set; } = new UnityEvent<bool>();
    public UnityEvent OnFocus { get; private set; } = new UnityEvent();
    public UnityEvent OnWalk { get; private set; } = new UnityEvent();
    public UnityEvent OnStand { get; private set; } = new UnityEvent();
    public UnityEvent OnDie { get; private set; } = new UnityEvent();


    public State normalState { get; private set; }
    State _aimState;
    State _walkState;
    BitedState _bitedState;
    DeadState _deadState;

    State _state;

    bool _isRun;

    InteractedObject _focused;

    // 以下Debug用
    Vector3 _interactPosition;


    private void OnDrawGizmos() // 最終的には分離した方が良いよ
    {
        Gizmos.DrawWireSphere(_interactPosition, 0.5f);
    }

    public override void Damage(int value)
    {
        base.HP -= value;

        if (base.HP <= 0)
        {
            // 死亡
            ChangeStateTo(_deadState);
        }
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

    public void Fire()
    {
        (_state as AimState)?.Fire();
    }

    public void Bited(GameObject target)
    {
        ChangeStateTo(_bitedState);
        this.transform.forward = (target.transform.position - this.transform.position).RemoveY();
    }

    public void StopBited()
    {
        if (_state is BitedState)
        {
            ChangeStateTo(_walkState);
        }
    }

    public void ChangeStateTo(State state)
    {
        _state?.Exit();
        _state = state;
        _state.Enter();
    }

    public void Interact()
    {
        if (!_focused)
        {
            return;
        }

        if (_state is WalkState)
        {
            if (_focused is DroppedItem)
            {
                OnInterectDroppedItem.Invoke(_focused);
                (_focused as DroppedItem).Gotten();
            }
            else
            {
                _focused.Interacted();
            }
        }
    }

    public void SetUpWeapon()
    {
        if (!(_state is BitedState) & _inventory.EquippedWeapon)
        {
            ChangeStateTo(_aimState);
        }
    }

    public void LowerWeapon()
    {
        if (!(_state is BitedState))
        {
            ChangeStateTo(_walkState);
        }
    }

    private void OnAnimatorIK(int layerIndex)
    {
        _state.OnAnimatorIK();
    }

    private void Start()
    {
        _aimState = new AimState(this);
        _walkState = new WalkState(this);
        _bitedState = new BitedState(this);
        _deadState = new DeadState(this);

        ChangeStateTo(_walkState);
    }

    private void Update()
    {
        _state.Update();


        _interactPosition = this.transform.position;

        Collider[] colliders;

        // 周囲のInterectedOjectのフォーカスを外す
        if ((colliders = Physics.OverlapCapsule(this.transform.position, this.transform.position.AddY(1.5f), 2f, _interactionLayer)).Length > 0)
        {
            foreach (Collider collider in colliders)
            {
                collider.GetComponent<InteractedObject>()?.SetIsFocused(false);
            }
        }

        // 一番近いInterectedObjectをフォーカス状態にする
        if ((colliders = Physics.OverlapCapsule(this.transform.position, this.transform.position.AddY(1.5f), 0.5f, _interactionLayer)).Length > 0)
        {
            float[] distances = colliders.Select(x => Vector3.Distance(this.transform.position, x.transform.position)).ToArray();
            int index = System.Array.IndexOf(distances, distances.Min());

            if (colliders[index].gameObject.TryGetComponent(out InteractedObject interactedObject))
            {
                interactedObject.SetIsFocused(true);
                _focused = interactedObject;
            }
        }
        else
        {
            _focused = null;
        }


        // AimSphereの移動
        Ray ray = Camera.main.ViewportPointToRay(Vector2.one / 2);
        if (Physics.Raycast(ray, out RaycastHit hit, _aimDistance))
        {
            _aimSphere.transform.position = Vector3.Lerp(_aimSphere.transform.position, hit.point, _aimSpeed * Time.deltaTime);
        }
        else
        {
            _aimSphere.transform.position = Vector3.Lerp(_aimSphere.transform.position, Camera.main.transform.position + Camera.main.transform.forward * _aimDistance, _aimSpeed * Time.deltaTime);
        }
    }

    private void FixedUpdate()
    {
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
        public virtual void Exit() { }
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
        SubState _subState;
        IdleState _idleState;
        FocusState _focusState;
        WalkState _walkState;

        Character _character;

        public AimState(Character character) : base(character)
        {
            _idleState = new IdleState(this);
            _focusState = new FocusState(this);
            _walkState = new WalkState(this);

            _character = character;
        }

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
            base.character._inventory.EquippedWeapon.Setup();

            base.character.OnAimStateChange.Invoke(true);

            ChangeSubstateTo(_idleState);
        }

        public override void Exit()
        {
            base.character._animator.SetBool("IsAiming", false);
            base.character._inventory.EquippedWeapon?.Lower();

            base.character.OnAimStateChange.Invoke(false);
        }

        public override void Update()
        {
            // カメラ方向を向くように回転
            base.character.transform.forward = Vector3.Slerp(base.character.transform.forward, (base.character._aimSphere.transform.position - base.character.transform.position).RemoveY(), base.character._aimSpeed * Time.deltaTime);


            // 手ブレの変化
            float s = base.character._cameraProxy.GetNoiseStrength();
            float r;
            if (Mathf.Abs(s - _subState.NoiseStrength) < Time.deltaTime)
            {
                r = _subState.NoiseStrength;
            }
            else
            {
                r = s + Mathf.Sign(_subState.NoiseStrength - s) * Time.deltaTime;
            }
            base.character._cameraProxy.SetNoiseStrength(r);


            _subState.Update();
        }

        public override void LateUpdate()
        {
            // 腕の向き調整
            base.character._humanoidBoneTransformer.SetLookAtPosition(HumanBodyBones.RightHand, base.character._aimSphere.transform.position, base.character._aimAxis, base.character._upAxis);
        }

        public override void Move(Vector2 direction)
        {
            if (direction != Vector2.zero)
            {
                if (!(_subState is WalkState))
                {
                    ChangeSubstateTo(_walkState);
                }
            }
            else if (_subState is WalkState)
            {
                ChangeSubstateTo(_idleState);
            }

            base.character._mover.StrafeMove((Quaternion.FromToRotation(base.character.transform.forward, Camera.main.transform.forward.RemoveY()) * direction.ToVector3XZ()).ToVector2XZ());
        }

        public void Fire()
        {
            (base.character._inventory.EquippedWeapon as Gun)?.Fire(_subState is FocusState);
        }

        void ChangeSubstateTo(SubState subState)
        {
            _subState = subState;
            _subState.Enter();
        }


        class SubState
        {
            protected AimState AimState;

            public float NoiseStrength { get; protected set; }

            public SubState(AimState state)
            {
                AimState = state;
            }

            public virtual void Enter() { }
            public virtual void Update() { }
        }

        class IdleState : SubState
        {
            float _time;

            public IdleState(AimState state) : base(state) { }

            public override void Enter()
            {
                _time = 0;

                base.AimState._character.OnStand.Invoke();

                base.NoiseStrength = 1.0f;
            }

            public override void Update()
            {
                // 一定時間経過後遷移
                _time += Time.deltaTime;
                if (_time > (base.AimState._character._inventory.EquippedWeapon as Gun)?.FocusTime)
                {
                    // FocusStateに遷移
                    base.AimState.ChangeSubstateTo(base.AimState._focusState);
                }
            }
        }

        class FocusState : SubState
        {
            public FocusState(AimState state) : base(state) { }


            public override void Enter()
            {
                // レティクルを小さくする
                base.AimState._character.OnFocus.Invoke();

                // 手ブレを小さくする
                base.NoiseStrength = 0.5f;
            }

            public override void Update()
            {

            }
        }

        class WalkState : SubState
        {
            public WalkState(AimState state) : base(state) { }

            public override void Enter()
            {
                base.AimState._character.OnWalk.Invoke();

                base.NoiseStrength = 1.5f;
            }
        }
    }

    public class BitedState : State
    {
        public BitedState(Character character) : base(character) { }

        public override void Enter()
        {
            // カメラを切り替える
            base.character._cameraChanger.ChangeToBitedCamera();

            // カメラの回転を無効に
            base.character._cameraRotater.SetIsEnabled(false);

            // アニメーション再生
            base.character._animatorProxy.SetTrigger(AnimatorParameterConst.PlayerAnimatorParameter.BITED);
        }

        public override void Exit()
        {
            // カメラを戻す
            base.character._cameraChanger.ChangeToNormalCamera();

            // カメラの回転を有効に
            base.character._cameraRotater.SetIsEnabled(true);

            // アニメーションを戻す
            base.character._animatorProxy.SetTrigger(AnimatorParameterConst.PlayerAnimatorParameter.EXIT);
        }
    }

    public class DeadState : State
    {
        public DeadState(Character character) : base(character) { }

        public override void Enter()
        {
            // 死亡アニメーション
            base.character._animatorProxy.SetTrigger(AnimatorParameterConst.PlayerAnimatorParameter.DIE);

            base.character.OnDie.Invoke();
        }
    }
}
