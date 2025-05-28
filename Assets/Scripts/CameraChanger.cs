using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraChanger : MonoBehaviour
{
    [SerializeField]
    GameObject _normalCamera;

    [SerializeField]
    GameObject _aimCamera;

    [SerializeField]
    GameObject _bitedCamera;

    GameObject _currentCamera;

    public void ChangeToNormalCamera()
    {
        if (_currentCamera == _normalCamera)
        {
            return;
        }

        _currentCamera.SetActive(false);
        _currentCamera = _normalCamera;
        _currentCamera.SetActive(true);
    }

    public void ChangeToAimCamera()
    {
        _currentCamera.SetActive(false);
        _currentCamera = _aimCamera;
        _currentCamera.SetActive(true);
    }

    public void ChangeToBitedCamera()
    {
        _currentCamera.SetActive(false);
        _currentCamera = _bitedCamera;
        _currentCamera.SetActive(true);
    }

    private void Start()
    {
        _currentCamera = _bitedCamera;
    }
}
