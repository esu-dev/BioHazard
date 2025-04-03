using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemData", menuName = "ScriptableObjects/ItemData")]
public class ItemData : ScriptableObject
{
    [field: SerializeField]
    public ItemType Type { get; private set; }

    [field: SerializeField]
    public Sprite Image { get; private set; }

    [field: SerializeField]
    public string ID { get; private set; }

    [field: SerializeField]
    public string Name { get; private set; }

    [field: SerializeField]
    public int Size { get; private set; }

    [field: SerializeField]
    public bool CanStack { get; private set; }

    [field: SerializeField]
    public string Explanation { get; private set; }


    [field: SerializeField]
    public GameObject Prefab { get; private set; }


    [field: SerializeField]
    public Vector3 DefaultPosition;

    [field: SerializeField]
    public Vector3 DefaultRotation;


    public void SetImage(Sprite sprite)
    {
        Image = sprite;
    }

    public enum ItemType
    {
        Consumption,
        MainWeapon,
        SubWeapon
    }
}
