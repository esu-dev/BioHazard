using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorKnob : InteractedObject
{
    [SerializeField]
    bool _isPushed;

    [SerializeField]
    bool _isLocked;

    [SerializeField]
    Door _door;

    public override void Interacted()
    {
        if (_isLocked)
        {
            // テキスト表示
            base._interectedFunctions[0].Invoke();

            return;
        }

        _door.Open(_isPushed);
    }
}
