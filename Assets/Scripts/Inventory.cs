using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    [SerializeField]
    InventoryUI _inventoryUI;

    [SerializeField]
    GameObject _handHolder;

    [SerializeField]
    GameObject _backHolder;

    [SerializeField]
    Character _character;

    [SerializeField]
    RigManager _rigManager;

    [SerializeField]
    Animator _animator;

    [SerializeField]
    Weapon _bareHands;


    [field: SerializeField] public Vector2Int ItemSlotMatrixNum = new Vector2Int(4, 2);

    [field: SerializeField] public List<List<ItemSet>> ItemList { get; private set; } = new List<List<ItemSet>>();

    public Weapon EquippedWeapon { get; private set; }

    Weapon _mainWeapon;
    Weapon _subWeapon;


    public void Add(ItemData itemData, Vector2Int position, int amount = 1)
    {
        ItemList[position.y][position.x].itemData = itemData;
        ItemList[position.y][position.x].amount += amount;

        for (int i = 1; i < itemData.Size; i++)
        {
            ItemList[position.y][position.x + i].itemData = itemData;
        }
    }

    public void EquipMain()
    {
        Equip(_mainWeapon);
    }

    public void EquipSub()
    {
        Equip(_subWeapon);
    }

    private void Equip(Weapon weapon)
    {
        // ïêäÌÇîwíÜÇ…à⁄ìÆ
        if (EquippedWeapon == _mainWeapon)
        {
            EquippedWeapon.transform.parent = _backHolder.transform;
            EquippedWeapon.transform.localPosition = Vector3.zero;
            EquippedWeapon.transform.localRotation = Quaternion.Euler(Vector3.zero);
        }

        EquippedWeapon = weapon ? weapon : _bareHands;
        EquippedWeapon.Initialize(_character, _rigManager);
        _animator.SetInteger("Weapon", EquippedWeapon.GetWeaponNum());

        // ïêäÌÇéËÇÃà íuÇ…à⁄ìÆ
        EquippedWeapon.transform.parent = _handHolder.transform;
        EquippedWeapon.transform.localPosition = Vector3.zero;
        EquippedWeapon.transform.localRotation = Quaternion.Euler(Vector3.zero);
    }

    private void RegisterMainWeapon(ItemData itemData)
    {
        _mainWeapon = Instantiate(itemData.Prefab).AddComponent<Weapon>(); ;
    }

    private void Start()
    {
        // ItemListÇÃèâä˙âª
        for (int i = 0; i < ItemSlotMatrixNum.y; i++)
        {
            ItemList.Add(new List<ItemSet>());
            for (int j = 0; j < ItemSlotMatrixNum.x; j++)
            {
                ItemList[i].Add(new ItemSet());
            }
        }

        RegisterMainWeapon(ItemList[0][0].itemData);
    }
}

[System.Serializable]
public class ItemSet
{
    public ItemData itemData;
    public int amount;
}