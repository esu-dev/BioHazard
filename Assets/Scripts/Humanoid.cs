using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Humanoid : FlexUpdateMonoBehaviour
{
    [SerializeField]
    protected int HP;

    [SerializeField]
    protected float _speed;

    [SerializeField]
    protected float SecondsToMaxSpeed;

    [SerializeField]
    protected float _AngularSpeed;

    [SerializeField]
    protected Animator _animator;

    protected Vector2 _currentVelocity;


    public virtual void React(Vector3 direction) { }
    public abstract void Damage(int value);
    protected abstract void ExeHitAnimation();
}
