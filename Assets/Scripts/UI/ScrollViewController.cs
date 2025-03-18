using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScrollViewController : MonoBehaviour
{
    [SerializeField]
    GameObject _contentArea;


    public void Add(GameObject _content)
    {
        _content.transform.parent = _contentArea.transform;
    }
}
