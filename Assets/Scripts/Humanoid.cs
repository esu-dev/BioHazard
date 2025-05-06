using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Humanoid : FlexUpdateMonoBehaviour
{
    [field: SerializeField]
    public int HP { get; protected set; }

    [SerializeField]
    protected float _speed;

    [SerializeField]
    protected float SecondsToMaxSpeed;

    [SerializeField]
    protected float _AngularSpeed;

    [SerializeField]
    protected Animator _animator;

    protected Vector2 _currentVelocity;


    public abstract void Damage(int value);
}
