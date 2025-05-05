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

    bool _isEnabled = true;
    Vector2 _currentRotation;
    Quaternion _absoluteQuaternion;

    public void SetIsEnabled(bool isEnabled)
    {
        _isEnabled = isEnabled;
    }

    public void SetRotation(Vector2 rotation)
    {
        if (_isEnabled)
        {
            _currentRotation = _rotationSpeed * rotation;
        }
    }

    private void Start()
    {
        GameStateManager.Instance.OnPlayStateEnter.AddListener(() =>
        {
            _isEnabled = true;
            this.enabled = true;
        });

        GameStateManager.Instance.OnPauseStateEnter.AddListener(() =>
        {
            _isEnabled = false;
            this.enabled = false;
        });
    }

    public void Update()
    {
        _absoluteQuaternion = Quaternion.Euler(new Vector3(_absoluteQuaternion.eulerAngles.x - _currentRotation.y, _absoluteQuaternion.eulerAngles.y + _currentRotation.x, 0));
        _cameraRoot.transform.rotation = _absoluteQuaternion;
    }
}
