using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Zombie : Humanoid
{
    [SerializeField]
    LayerMask _playerLayer;

    [SerializeField]
    Mover _mover;

    [SerializeField]
    HumanoidBoneTransformer _humanoidBoneTransformer;

    [SerializeField]
    AnimatorProxy _animatorProxy;

    [SerializeField]
    AnimatedRagdoll _animatedRagdoll;

    [SerializeField]
    AudioData _idleAudioData;

    [SerializeField]
    AudioData _bloodAudioData;

    [SerializeField]
    AudioPlayer _voiceAudioPlayer;

    [SerializeField]
    AudioSource _hitAudioSource;

    GameObject _target;
    State _state;
    AwakeState _awakeState;
    TrackingState _trackingState;
    AsleepState _asleepState;
    DeathState _deathState;

    bool _isReacting;
    float _reactionTimer;

    public override void React(Vector3 direction)
    {
        Vector3 d = this.transform.rotation * direction;

        _animatorProxy.SetFloat(AnimatorParameterConst.ZombieAnimatorParameter.DIRECTION_X, d.x);
        _animatorProxy.SetFloat(AnimatorParameterConst.ZombieAnimatorParameter.DIRECTION_Y, d.z);

        // アニメーション再生
        //_animatorProxy.SetTrigger(AnimatorParameterConst.ZombieAnimatorParameter.DAMAGE);

        // Rigidbodyでリアクション
        _animatedRagdoll.SetRagdollChild(HumanBodyBones.Spine);
        //_animatedRagdoll.SetKinematic(HumanBodyBones.Head, true);
        _animatedRagdoll.AddForce(HumanBodyBones.Head, 50 * direction.normalized);
        _isReacting = true;
        _reactionTimer = 0;

        Debug.DrawRay(this.transform.position.AddY(1), direction.normalized, Color.yellow, 10 * Time.deltaTime);


        // サウンド再生
        _hitAudioSource.clip = _bloodAudioData._audioClips[Random.Range(0, _bloodAudioData._audioClips.Length)];
        _hitAudioSource.Play();
    }

    public override void Damage(int value)
    {
        if (base.HP > 0)
        {
            base.HP -= value;

            //ExeHitAnimation();

            if (base.HP <= 0)
            {
                base._animator.SetTrigger("Die");
                ChangeStateTo(_deathState);
            }
        }
    }

    protected override void ExeHitAnimation()
    {
        base._animator.SetTrigger("Damage");
    }

    private void ChangeStateTo(State state)
    {
        _state.Exit();
        _state = state;
        _state.Enter();
    }

    void SetKinematicDefault()
    {
        _animatedRagdoll.SetKinematicAll();
        _animatedRagdoll.SetRagdollChild(HumanBodyBones.RightUpperArm);
        _animatedRagdoll.SetRagdollChild(HumanBodyBones.LeftUpperArm);
    }

    private void OnAnimatorIK(int layerIndex)
    {
        _state.OnAnimatorIK();
    }

    private void Start()
    {
        _awakeState = new AwakeState(this);
        _trackingState = new TrackingState(this);
        _asleepState = new AsleepState(this);
        _deathState = new DeathState(this);

        _state = _awakeState;
        _state.Enter();

        // アニメーションの乱数を決定
        _animatorProxy.SetInteger(AnimatorParameterConst.ZombieAnimatorParameter.WALK_TYPE, Random.Range(0, 2));
        _animatorProxy.SetFloat(AnimatorParameterConst.ZombieAnimatorParameter.STRIDE, Random.Range(0.1f, 1f));


        // Ragdollとの衝突を無視
        Physics.IgnoreLayerCollision(this.gameObject.layer, LayerConst.RAGDOLL, true);


        SetKinematicDefault();
    }

    private void Update()
    {
        _state.Update();

        // 一定時間経過後にリアクション状態を解除する
        if (_isReacting)
        {
            _reactionTimer += FlexDeltaTime;

            if (_reactionTimer > 1f)
            {
                SetKinematicDefault();

                _isReacting = false;
            }
        }
    }

    public abstract class State
    {
        protected Zombie zombie;

        public State(Zombie zombie)
        {
            this.zombie = zombie;
        }

        public virtual void OnAnimatorIK() { }
        public abstract void Enter();
        public virtual void Exit() { }
        public abstract void Update();
        public virtual void LateUpdate() { }
    }

    public class AwakeState : State
    {
        public AwakeState(Zombie zombie) : base(zombie) { }

        public override void Enter()
        {
            // Idleボイス再生
            void PlayIdleVoice()
            {
                base.zombie._voiceAudioPlayer.Play(base.zombie._idleAudioData._audioClips[Random.Range(0, base.zombie._idleAudioData._audioClips.Length)], PlayIdleVoice, Random.Range(1f, 5f));
            }

            PlayIdleVoice();
        }

        public override void Update()
        {
            // 敵を探す
            if (Physics.BoxCast(base.zombie.transform.position.AddY(1), Vector3.one * 3f, base.zombie.transform.forward, out RaycastHit hit, Quaternion.identity, 10, base.zombie._playerLayer))
            {
                base.zombie._target = hit.transform.gameObject;

                base.zombie.ChangeStateTo(base.zombie._trackingState);
            }
        }

        public override void Exit()
        {
            base.zombie._voiceAudioPlayer.Stop();
        }
    }

    public class TrackingState : State
    {
        public TrackingState(Zombie zombie) : base(zombie) { }

        public override void OnAnimatorIK()
        {
            // 身体をターゲットの頭に向ける
            base.zombie._humanoidBoneTransformer.SetLookAtWeight(1, 1, 1);
            //base.zombie._humanoidBoneTransformer.SetLookAtWeight(1, 0, 1);
            base.zombie._humanoidBoneTransformer.SetLookAtPosition(base.zombie._target.GetComponent<Animator>().GetBoneTransform(HumanBodyBones.Head).transform.position);
        }

        public override void Enter()
        {

        }

        public override void Update()
        {
            Vector2 direction = (base.zombie._target.transform.position - base.zombie.transform.position).ToVector2XZ();

            base.zombie._mover.Move(direction);

            //base.zombie._animator.SetFloat("Speed", base.zombie._currentVelocity.magnitude, base.zombie.SecondsToMaxSpeed, Time.deltaTime);

            // 進行方向に回転
            //if (base.zombie._currentVelocity != Vector2.zero)
            {
                base.zombie._mover.Rotate(direction);
            }
        }
    }

    public class AsleepState : State
    {
        public AsleepState(Zombie zombie) : base(zombie) { }

        public override void Enter()
        {
            
        }

        public override void Update()
        {
            
        }
    }

    public class DeathState : State
    {
        public DeathState(Zombie zombie) : base(zombie) { }

        public override void Enter()
        {

        }

        public override void Update()
        {

        }

        public override void LateUpdate()
        {

        }
    }
}
