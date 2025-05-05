using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class InteractedObject : MonoBehaviour
{
    public UnityEvent<bool> OnIsFocusedChanged { get; set; } = new UnityEvent<bool>();

    bool _isFocesed;

    public virtual void Interacted() { }

    public void SetIsFocused(bool isFocesed)
    {
        _isFocesed = isFocesed;
        OnIsFocusedChanged.Invoke(_isFocesed);
    }
}
