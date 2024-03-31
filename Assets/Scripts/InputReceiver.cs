using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputReceiver : MonoBehaviour
{
    [SerializeField]
    Character _man;

    [SerializeField]
    Inventory _inventory;

    [SerializeField]
    CameraRotater _cameraRotater;

    Vector2 _inputVelocity;

    private void OnMove(InputAction.CallbackContext callbackContext)
    {
        _inputVelocity = callbackContext.ReadValue<Vector2>();
    }

    private void OnLook(InputAction.CallbackContext callbackContext)
    {
        _cameraRotater.SetRotation(callbackContext.ReadValue<Vector2>());
    }

    private void OnFire(InputAction.CallbackContext callbackContext)
    {
        _inventory.EquippedWeapon?.Fire();
    }

    private void OnEquipMain(InputAction.CallbackContext callbackContext)
    {
        _inventory.EquipMain();
    }

    private void OnEquipSub(InputAction.CallbackContext callbackContext)
    {
        _inventory.EquipSub();
    }

    private void OnSetup(InputAction.CallbackContext callbackContext)
    {
        if (callbackContext.performed)
        {
            _inventory.EquippedWeapon?.Setup();
        }
        else if (callbackContext.canceled)
        {
            _inventory.EquippedWeapon?.Lower();
        }
    }

    private void Start()
    {
        BioHazardInputAction inputAction = new BioHazardInputAction();

        inputAction.Player.Move.performed += OnMove;
        inputAction.Player.Move.canceled += OnMove;

        inputAction.Player.Look.performed += OnLook;
        inputAction.Player.Look.canceled += OnLook;

        inputAction.Player.Fire.performed += OnFire;

        inputAction.Player.EquipMain.performed += OnEquipMain;
        inputAction.Player.EquipSub.performed += OnEquipSub;

        inputAction.Player.Setup.performed += OnSetup;
        inputAction.Player.Setup.canceled += OnSetup;

        inputAction.Enable();
    }

    private void Update()
    {
        Vector3 vel = Quaternion.FromToRotation(Vector3.forward, Camera.main.transform.forward.RemoveY()) * _inputVelocity.ToVector3XZ();
        _man.Move(vel.ToVector2XZ());
    }
}
