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

    GameObject _target;
    State _state;
    AwakeState _awakeState;
    TrackingState _trackingState;
    AsleepState _asleepState;


    public override void Damage(int value)
    {
        if (base.HP > 0)
        {
            base.HP -= value;

            ExeHitAnimation();

            if (base.HP <= 0)
            {
                base._animator.SetTrigger("Die");
            }
        }
    }

    protected override void ExeHitAnimation()
    {
        base._animator.SetTrigger("Damage");
    }

    private void ChangeStateTo(State state)
    {
        _state = state;
        _state.Start();
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

        _state = _awakeState;
    }

    private void Update()
    {
        _state.Update();
    }

    public abstract class State
    {
        protected Zombie zombie;

        public State(Zombie zombie)
        {
            this.zombie = zombie;
        }

        public virtual void OnAnimatorIK() { }
        public abstract void Start();
        public abstract void Update();
    }

    public class AwakeState : State
    {
        public AwakeState(Zombie zombie) : base(zombie) { }

        public override void Start()
        {
            
        }

        public override void Update()
        {
            // ìGÇíTÇ∑
            if (Physics.BoxCast(base.zombie.transform.position.AddY(1), Vector3.one * 3f, base.zombie.transform.forward, out RaycastHit hit, Quaternion.identity, 10, base.zombie._playerLayer))
            {
                base.zombie._target = hit.transform.gameObject;

                base.zombie.ChangeStateTo(base.zombie._trackingState);
            }
        }
    }

    public class TrackingState : State
    {
        public TrackingState(Zombie zombie) : base(zombie) { }

        public override void OnAnimatorIK()
        {
            // êgëÃÇÉ^Å[ÉQÉbÉgÇÃì™Ç…å¸ÇØÇÈ
            base.zombie._humanoidBoneTransformer.SetLookAtWeight(1, 1, 1);
            base.zombie._humanoidBoneTransformer.SetLookAtPosition(base.zombie._target.GetComponent<Animator>().GetBoneTransform(HumanBodyBones.Head).transform.position);
        }

        public override void Start()
        {
            
        }

        public override void Update()
        {
            base.zombie.Move((base.zombie._target.transform.position - base.zombie.transform.position).ToVector2XZ());

            base.zombie._animator.SetFloat("Speed", base.zombie._currentVelocity.magnitude, base.zombie.SecondsToMaxSpeed, Time.deltaTime);

            // êiçsï˚å¸Ç…âÒì]
            if (base.zombie._currentVelocity != Vector2.zero)
            {
                base.zombie._mover.Rotate((base.zombie._target.transform.position - base.zombie.transform.position).ToVector2XZ());
            }
        }
    }

    public class AsleepState : State
    {
        public AsleepState(Zombie zombie) : base(zombie) { }

        public override void Start()
        {
            
        }

        public override void Update()
        {
            
        }
    }
}
