using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ItemSlotManager : MonoBehaviour
{
    List<ItemSlot> _itemSlotList = new List<ItemSlot>();


    public void Initialize()
    {
        _itemSlotList.Clear();

        foreach (Transform child in this.transform)
        {
            if (child.TryGetComponent(out ItemSlot itemSlot))
            {
                _itemSlotList.Add(itemSlot);
            }
        }
    }

    public void SetItem(List<List<ItemSet>> itemSetList)
    {
        for (int i = 0; i < itemSetList.Count(); i++)
        {
            for (int j = 0; j < itemSetList[i].Count(); j++)
            {
                if (itemSetList[i] != null)
                {
                    _itemSlotList[i * itemSetList[0].Count() + j].SetImage(itemSetList[i][j].itemData.Image);
                }

                _itemSlotList[i].SetClickEvent(() => Debug.Log(i));
            }
        }
    }
}
