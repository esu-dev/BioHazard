using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraChanger : MonoBehaviour
{
    [SerializeField]
    GameObject _normalCamera;

    [SerializeField]
    GameObject _aimCamera;

    public void ChangeToNormalCamera()
    {
        _normalCamera.SetActive(true);
        _aimCamera.SetActive(false);
    }

    public void ChangeToAimCamera()
    {
        _normalCamera.SetActive(false);
        _aimCamera.SetActive(true);
    }
}
