using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PositionTracker : MonoBehaviour
{
    [SerializeField]
    GameObject _targetPos;

    private void FixedUpdate()
    {
        this.transform.position = _targetPos.transform.position;
    }
}
