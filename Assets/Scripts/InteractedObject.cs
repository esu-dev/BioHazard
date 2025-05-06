using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class InteractedObject : MonoBehaviour
{
    [SerializeField]
    protected InterectedFunction[] _interectedFunctions;

    public UnityEvent InteractedEvent = new UnityEvent();

    public UnityEvent<bool> OnIsFocusedChanged { get; set; } = new UnityEvent<bool>();

    bool _isFocesed;

    public virtual void Interacted()
    {
        _interectedFunctions[0].Invoke();
        InteractedEvent.Invoke();
    }

    public void SetIsFocused(bool isFocesed)
    {
        _isFocesed = isFocesed;
        OnIsFocusedChanged.Invoke(_isFocesed);
    }
}
