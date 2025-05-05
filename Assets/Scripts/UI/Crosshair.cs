using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Crosshair : MonoBehaviour
{
    Vector3 _defaultPosition;

    public void Focus(int level, float time)
    {
        Vector3 direction = (this.transform.localPosition - Vector3.zero).normalized;
        this.transform.DOLocalMove(_defaultPosition + level * 30f * direction, time);
    }


    public void Initialize()
    {
        _defaultPosition = this.transform.localPosition;
    }
}
