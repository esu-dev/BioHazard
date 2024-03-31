using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PositionTracker : MonoBehaviour
{
    [SerializeField]
    GameObject _cameraPos;

    private void FixedUpdate()
    {
        this.transform.position = _cameraPos.transform.position;
    }
}
