using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField]
    InventoryUI _inventoryUI;

    [SerializeField]
    GameObject _uiPannel;


    public void OpenAndCloseInventory()
    {
        if (!_uiPannel.activeSelf)
        {
            (GameStateManager.Instance as GameStateManager).ChangeStateToPauseState();
            _inventoryUI.Open();
        }
        else
        {
            (GameStateManager.Instance as GameStateManager).ChangeStateToPlayState();
            _inventoryUI.Close();
        }
    }

    private void Start()
    {
        _inventoryUI.Initialize();
    }
}
