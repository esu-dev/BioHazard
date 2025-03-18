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
            _inventoryUI.Open();
        }
        else
        {
            _inventoryUI.Close();
        }
    }
}
