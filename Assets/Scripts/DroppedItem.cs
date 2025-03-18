using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroppedItem : InteractedObject
{
    [field: SerializeField] public ItemData getItem { get; private set; }

    [field: SerializeField] public int amount { get; private set; }


    public void Gotten()
    {
        Debug.Log("gotten");
        this.gameObject.SetActive(false);
    }
}
