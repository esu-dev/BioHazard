using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class ItemSlot : MonoBehaviour
{
    [SerializeField]
    Image _image;

    [SerializeField]
    Button _button;

    [field: SerializeField] public int SlotID { get; private set; }

    Vector2Int _slotPosition;


    public void SetImage(Sprite sprite)
    {
        _image.sprite = sprite;
        _image.color = _image.color.Visualize();
    }

    public void SetClickEvent(UnityAction callback)
    {
        _button.onClick.AddListener(callback);
    }

    public void RemoveAllClickEvent()
    {
        _button.onClick.RemoveAllListeners();
    }

    public void SetSlotPosition(Vector2Int slotPosition)
    {
        _slotPosition = slotPosition;
    }
}
