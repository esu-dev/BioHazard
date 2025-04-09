using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public class InputReceiver : MonoBehaviour
{
    [SerializeField]
    Character _character;

    [SerializeField]
    Mover _mover;

    [SerializeField]
    Inventory _inventory;

    [SerializeField]
    CameraRotater _cameraRotater;

    [SerializeField]
    UIManager _uiManager;

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
        _inventory.EquippedWeapon?.Fire(); // Characterのメソッドを経由した方が良いのでは？
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
            _character.SetUpWeapon();
        }
        else if (callbackContext.canceled)
        {
            _character.LowerWeapon();
        }
    }

    private void OnInteract(InputAction.CallbackContext callbackContext)
    {
        _character.Interact();
    }

    private void OnOpenInventory(InputAction.CallbackContext callbackContext)
    {
        _uiManager.OpenAndCloseInventory();
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

        inputAction.Player.Interact.performed += OnInteract;

        inputAction.Player.OpenInventory.performed += OnOpenInventory;

        inputAction.Enable();
    }

    private void Update()
    {
        Vector3 vel = Quaternion.FromToRotation(this.transform.forward, Camera.main.transform.forward.RemoveY()) * _inputVelocity.ToVector3XZ();
        _mover.StrafeMove(vel.ToVector2XZ());
    }
}
