using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CenterOfMassAdjuster : MonoBehaviour
{
    [SerializeField]
    Rigidbody _rigidbody;

    private void Start()
    {
        _rigidbody.centerOfMass = _rigidbody.centerOfMass.RemoveX();
    }
}
