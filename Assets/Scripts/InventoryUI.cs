using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    [SerializeField]
    ItemSlotManager _itemSlotManager;

    [SerializeField]
    Inventory _inventory;

    public void Open()
    {
        this.gameObject.SetActive(true);

        _itemSlotManager.Initialize();
        _itemSlotManager.SetItem(_inventory.ItemList);
    }

    public void Close()
    {
        this.gameObject.SetActive(false);
    }
}
