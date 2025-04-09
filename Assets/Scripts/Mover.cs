using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mover : MonoBehaviour
{
    [SerializeField]
    float _maxSpeed;

    [SerializeField]
    float _accelationTime;

    [SerializeField]
    float _rotationSpeed;

    [SerializeField]
    AnimatorProxy _animatorProxy;

    public void StrafeMove(Vector2 direction)
    {
        Vector2 velocity = _maxSpeed * direction;

        _animatorProxy.SetFloat(AnimatorParameterConst.PlayerAnimatorParameter.SPEED, velocity.magnitude, _accelationTime, LocalizedTime.timeScale);
        _animatorProxy.SetFloat(AnimatorParameterConst.PlayerAnimatorParameter.VELOCITY_X, velocity.x, _accelationTime, LocalizedTime.timeScale);
        _animatorProxy.SetFloat(AnimatorParameterConst.PlayerAnimatorParameter.vELOCITY_Y, velocity.y, _accelationTime, LocalizedTime.timeScale);
    }

    public void Rotate(Vector2 direction)
    {
        this.transform.forward = Vector3.Slerp(this.transform.forward, direction.ToVector3XZ(), _rotationSpeed * Time.deltaTime);
    }

    private void Update()
    {
        
    }
}
