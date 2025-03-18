using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorKnob : InteractedObject
{
    [SerializeField]
    Door _door;

    public override void Interacted()
    {
        Debug.Log("Open");

        _door.Open();
    }
}
