using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

using BehaviourTreeLib;

public class Zombie : Humanoid
{
    [SerializeField]
    int _power;

    [SerializeField]
    LayerMask _playerLayer;

    [SerializeField]
    GameObject[] _bloodEffectPrefabs;

    [SerializeField]
    Mover _mover;

    [SerializeField]
    HumanoidBoneTransformer _humanoidBoneTransformer;

    [SerializeField]
    AnimatorProxy _animatorProxy;

    [SerializeField]
    AnimatedRagdoll _animatedRagdoll;

    [SerializeField]
    BehaviourTreeAI _behaviourTreeAI;

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
    RoamingState _roamingState;
    TrackingState _trackingState;
    BitingState _bittingState;
    AsleepState _asleepState;
    DeathState _deathState;

    bool _isReacting;
    float _ragdollWeight;


    /// <summary>
    /// ヒットリアクション
    /// </summary>
    /// <param name="direction"></param>
    public void React(GameObject bone, Vector3 direction)
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


        // エフェクト表示
        Instantiate(_bloodEffectPrefabs[Random.Range(0, _bloodEffectPrefabs.Length)], bone.transform.position, Quaternion.identity);

        // 攻撃された方を向く

    }

    /// <summary>
    /// ダメージ処理
    /// </summary>
    /// <param name="value"></param>
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
        _roamingState = new RoamingState(this);
        _trackingState = new TrackingState(this);
        _bittingState = new BitingState(this);
        _asleepState = new AsleepState(this);
        _deathState = new DeathState(this);

        //_state = _awakeState;
        _state = _roamingState;
        _state.Enter();

        // アニメーションの乱数を決定
        _animatorProxy.SetInteger(AnimatorParameterConst.ZombieAnimatorParameter.WALK_TYPE, Random.Range(0, 2));
        _animatorProxy.SetFloat(AnimatorParameterConst.ZombieAnimatorParameter.STRIDE, Random.Range(0.1f, 1f));


        // Ragdollとの衝突を無視
        Physics.IgnoreLayerCollision(this.gameObject.layer, LayerConst.RAGDOLL, true);


        SetKinematicDefault();


        // Idleボイス再生
        void playIdleVoice()
        {
            _voiceAudioPlayer.Play(_idleAudioData._audioClips[Random.Range(0, _idleAudioData._audioClips.Length)], playIdleVoice, Random.Range(1f, 5f));
        }

        playIdleVoice();


        _behaviourTreeAI.CreateTree();
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
        public virtual void Enter() { }
        public virtual void Exit() { }
        public virtual void Update() { }
        public virtual void LateUpdate() { }
    }

    public class AwakeState : State
    {
        public AwakeState(Zombie zombie) : base(zombie) { }

        public override void Enter()
        {
            
        }

        public override void Update()
        {
            // 敵を探す
            Collider[] colliders;
            if ((colliders = Physics.OverlapBox(base.zombie.transform.position.AddY(1), Vector3.one * 3f, Quaternion.identity, base.zombie._playerLayer)).Length > 0)
            {
                base.zombie._target = colliders[0].transform.gameObject;

                if (base.zombie._target.GetComponent<Character>().HP > 0)
                {
                    base.zombie.ChangeStateTo(base.zombie._trackingState);
                }
            }
        }
    }

    public class RoamingState : State
    {
        public RoamingState(Zombie zombie) : base(zombie) { }

        public override void Update()
        {
            
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
            // ターゲットが死んでいれば、終了
            if (base.zombie._target.GetComponent<Character>().HP <= 0)
            {
                base.zombie.ChangeStateTo(base.zombie._awakeState);
            }

            /*Vector2 direction = (base.zombie._target.transform.position - base.zombie.transform.position).ToVector2XZ();

            base.zombie._mover.Move(direction);

            // 回転
            base.zombie._mover.Rotate(direction);*/


            // playerが近い場合、噛みつく
            if (Vector3.Distance(base.zombie.transform.position, base.zombie._target.transform.position) <= 1.0f)
            {
                Ray ray = new Ray(base.zombie.transform.position.AddY(1), base.zombie._target.transform.position - base.zombie.transform.position);
                if (Physics.Raycast(ray, 2f, 1 << LayerConst.PLAYER))
                {
                    // 噛みつく
                    base.zombie.ChangeStateTo(base.zombie._bittingState);
                }
            }
        }

        public override void Exit()
        {
            base.zombie._mover.Move(Vector2.zero);
        }
    }

    public class BitingState : State
    {
        public BitingState(Zombie zombie) : base(zombie) { }

        public override void Enter()
        {
            // アニメーション再生
            base.zombie._animatorProxy.SetTrigger(AnimatorParameterConst.ZombieAnimatorParameter.BITE);

            // playerに噛みつく
            base.zombie._target.GetComponent<Character>().Bited(base.zombie.gameObject);

            // Playerとのあたり判定を無視
            Physics.IgnoreLayerCollision(LayerConst.PLAYER, LayerConst.ZOMBIE);
            Physics.IgnoreLayerCollision(LayerConst.PLAYER, LayerConst.RAGDOLL);

            TimeScheduler.CreateSchedule(2f, () =>
            {
                // 戻る
                base.zombie._animatorProxy.SetTrigger(AnimatorParameterConst.ZombieAnimatorParameter.EXIT);

                // ダメージ処理
                base.zombie._target.GetComponent<Character>().Damage((int)(base.zombie._power * Random.Range(0.9f, 1.1f)));
            });

            TimeScheduler.CreateSchedule(3f, () =>
            {
                // Playerとのあたり判定を戻す
                Physics.IgnoreLayerCollision(LayerConst.PLAYER, LayerConst.ZOMBIE, false);
                Physics.IgnoreLayerCollision(LayerConst.PLAYER, LayerConst.RAGDOLL, false);

                base.zombie.ChangeStateTo(base.zombie._awakeState);

                base.zombie._target.GetComponent<Character>().StopBited();
            });
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

            // ボイスを停止
            base.zombie._voiceAudioPlayer.Stop();
        }

        public override void Update()
        {

        }

        public override void LateUpdate()
        {

        }
    }

    public class IsPlayerSearched : ConditionClass
    {
        public override bool Execute()
        {
            Zombie zombie = base.TargetObject.GetComponent<Zombie>();

            Collider[] colliders;
            if ((colliders = Physics.OverlapBox(zombie.transform.position.AddY(1), new Vector3(5f, 1, 5f), zombie.transform.rotation, zombie._playerLayer)).Length > 0)
            {
                Ray ray = new Ray(zombie.transform.position.AddY(1), colliders[0].transform.position - zombie.transform.position);
                if (Physics.Raycast(ray, out RaycastHit hit, 5f, 1 << LayerConst.PLAYER))
                {
                    zombie._target = hit.transform.gameObject;

                    zombie.ChangeStateTo(zombie.GetComponent<Zombie>()._trackingState);

                    return true;
                }
            }

            return false;
        }
    }

    public class IsRoamingState : ConditionClass
    {
        public override bool Execute()
        {
            Zombie zombie = base.TargetObject.GetComponent<Zombie>();

            return zombie._state is RoamingState;
        }
    }

    public class IsTrackingState : ConditionClass
    {
        public override bool Execute()
        {
            Zombie zombie = base.TargetObject.GetComponent<Zombie>();

            return zombie._state is TrackingState;
        }
    }

    public class SerachPlayerAction : ActionClass
    {
        public override void Start(UnityAction<NodeState> callback)
        {
            Zombie zombie = base.TargetObject.GetComponent<Zombie>();

            Collider[] colliders;
            if ((colliders = Physics.OverlapBox(zombie.transform.position.AddY(1) + zombie.transform.forward * 3f / 2, Vector3.one * 3f, Quaternion.identity, zombie._playerLayer)).Length > 0)
            {
                zombie._target = colliders[0].transform.gameObject;

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

    public class RoamingAction : ActionClass
    {
        Vector2 _direction;
        Zombie _zombie;

        public override void Start(UnityAction<NodeState> callback)
        {
            _direction = (new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f))).normalized;
            _zombie = base.TargetObject.GetComponent<Zombie>();
        }

        public override void Update()
        {
            _zombie._mover.Move(_direction);
            _zombie._mover.Rotate(_direction);
        }

        public override void Stop()
        {

        }
    }

    public class TrackingAction : ActionClass
    {
        Vector2 _direction;
        Zombie _zombie;

        public override void Start(UnityAction<NodeState> callback)
        {
            _zombie = base.TargetObject.GetComponent<Zombie>();
            _direction = (Quaternion.AngleAxis(Random.Range(-90f, 90f), Vector3.up) * (_zombie._target.transform.position - base.TargetObject.transform.position)).ToVector2XZ().normalized;
        }

        public override void Update()
        {
            _zombie._mover.Move(_direction);
            _zombie._mover.Rotate(_direction);
        }

        public override void Stop()
        {

        }
    }
}
