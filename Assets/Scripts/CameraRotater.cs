using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraRotater : MonoBehaviour
{
    [SerializeField]
    float _rotationSpeed;

    [SerializeField]
    GameObject _cameraRoot;

    [SerializeField]
    GameObject _player;

    Vector2 _currentRotation;
    Quaternion _absoluteQuaternion;

    public void SetRotation(Vector2 rotation)
    {
        _currentRotation = _rotationSpeed * rotation;
    }

    private void Update()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void Rotate()
    {
        _absoluteQuaternion = Quaternion.Euler(new Vector3(_absoluteQuaternion.eulerAngles.x - _currentRotation.y, _absoluteQuaternion.eulerAngles.y + _currentRotation.x, 0));
        _cameraRoot.transform.rotation = _absoluteQuaternion;
    }
}
