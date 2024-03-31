using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class DebugInputReceiver : MonoBehaviour
{
    void OnSttopEditor(InputAction.CallbackContext callbackContext)
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPaused = true;
#endif
    }

    private void Start()
    {
        BioHazardInputAction inputAction = new BioHazardInputAction();

        inputAction.Player.StopEditor.performed += OnSttopEditor;

        inputAction.Enable();
    }
}
