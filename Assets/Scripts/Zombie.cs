using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

using BehaviourTreeLib;

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

    [SerializeField]
    Rigidbody _rigidbody;

    [SerializeField]
    Collider _collider;

    GameObject _target;
    State _state;
    AwakeState _awakeState;
    TrackingState _trackingState;
    BitingState _bittingState;
    AsleepState _asleepState;
    DeathState _deathState;

    bool _isReacting;
    float _ragdollWeight;

    public override void React(Vector3 direction)
    {
        Vector3 d = this.transform.rotation * direction;

        _animatorProxy.SetFloat(AnimatorParameterConst.ZombieAnimatorParameter.DIRECTION_X, d.x);
        _animatorProxy.SetFloat(AnimatorParameterConst.ZombieAnimatorParameter.DIRECTION_Y, d.z);

        
        // Rigidbodyでリアクション
        _animatedRagdoll.SetWeight(1);
        _animatedRagdoll.SetRagdollChild(HumanBodyBones.Spine);
        _animatedRagdoll.AddForce(HumanBodyBones.Head, 50 * direction.normalized);
        _isReacting = true;
        _ragdollWeight = 1;

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
        _animatedRagdoll.SetWeightChild(HumanBodyBones.RightUpperArm, 1);
        _animatedRagdoll.SetWeightChild(HumanBodyBones.LeftUpperArm, 1);
    }

    private void OnAnimatorIK(int layerIndex)
    {
        _state.OnAnimatorIK();
    }

    private void Start()
    {
        // HPをランダムにセット
        base.HP = Random.Range(100, 200);

        _awakeState = new AwakeState(this);
        _trackingState = new TrackingState(this);
        _bittingState = new BitingState(this);
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


        //_behaviourTreeAI.CreateTree();
    }

    private void Update()
    {
        _state.Update();

        // 徐々にweightを減らしていく
        if (_isReacting)
        {
            _ragdollWeight -= FlexDeltaTime / 2f;
            _animatedRagdoll.SetWeight(_ragdollWeight);

            if (_ragdollWeight <= 0)
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
            Collider[] colliders;
            if ((colliders = Physics.OverlapBox(base.zombie.transform.position.AddY(1), Vector3.one * 3f, Quaternion.identity, base.zombie._playerLayer)).Length > 0)
            {
                base.zombie._target = colliders[0].transform.gameObject;

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

            // 回転
            base.zombie._mover.Rotate(direction);


            // playerが近い場合、噛みつく
            if (Vector3.Distance(base.zombie.transform.position, base.zombie._target.transform.position) <= 1.25f)
            {
                // 噛みつく
                base.zombie.ChangeStateTo(base.zombie._bittingState);
            }
        }

        public override void Exit()
        {
            base.zombie._mover.Move(Vector2.zero);
        }
    }

    public class BitingState : State
    {
        float _bitingTimer;

        public BitingState(Zombie zombie) : base(zombie) { }

        public override void Enter()
        {
            _bitingTimer = 0;

            // アニメーション再生
            base.zombie._animatorProxy.SetTrigger(AnimatorParameterConst.ZombieAnimatorParameter.BITE);

            // playerに噛みつく
            base.zombie._target.GetComponent<Character>().Bited(base.zombie.gameObject);

            // Playerとのあたり判定を無視
            Physics.IgnoreLayerCollision(LayerConst.PLAYER, LayerConst.ZOMBIE);
            Physics.IgnoreLayerCollision(LayerConst.PLAYER, LayerConst.RAGDOLL);
        }

        public override void Update()
        {
            _bitingTimer += base.zombie.FlexDeltaTime;

            if (_bitingTimer > 3f)
            {
                // Playerとのあたり判定を戻す
                Physics.IgnoreLayerCollision(LayerConst.PLAYER, LayerConst.ZOMBIE, false);
                Physics.IgnoreLayerCollision(LayerConst.PLAYER, LayerConst.RAGDOLL, false);

                base.zombie.ChangeStateTo(base.zombie._awakeState);

                base.zombie._target.GetComponent<Character>().StopBited();
            }
            else if (_bitingTimer > 2f)
            {
                // 戻る
                base.zombie._animatorProxy.SetTrigger(AnimatorParameterConst.ZombieAnimatorParameter.EXIT);
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
            // あたり判定を失くす
            Destroy(base.zombie._rigidbody);
            Destroy(base.zombie._collider);
            Physics.IgnoreLayerCollision(LayerConst.RAGDOLL, LayerConst.PLAYER);
        }

        public override void Update()
        {

        }

        public override void LateUpdate()
        {

        }
    }

    public class SerachPlayerAction : ActionClass
    {
        public override void Start(UnityAction<NodeState> callback)
        {
            Zombie zombie = base.TargetObject.GetComponent<Zombie>();

            // 敵を探す
            if (Physics.BoxCast(base.TargetObject.transform.position.AddY(1), Vector3.one * 3f, base.TargetObject.transform.forward, out RaycastHit hit, Quaternion.identity, 10, zombie._playerLayer))
            {
                zombie._target = hit.transform.gameObject;

                zombie.ChangeStateTo(zombie.GetComponent<Zombie>()._trackingState);

                callback(NodeState.True);
                return;
            }

            callback(NodeState.False);
        }

        public override void Update()
        {

        }

        public override void Stop()
        {

        }
    }
}
