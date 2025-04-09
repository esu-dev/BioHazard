using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    [SerializeField]
    GameObject _handHolder;

    [SerializeField]
    GameObject _backHolder;

    [SerializeField]
    Character _character;

    [SerializeField]
    Animator _animator;

    [SerializeField]
    Weapon _bareHands;


    [field: SerializeField] public Vector2Int ItemSlotMatrixNum = new Vector2Int(4, 2);

    List<List<ItemSet>> _itemList = new List<List<ItemSet>>();
    public IEnumerable<IEnumerable<ItemSet>> ItemList => _itemList;
    public Weapon EquippedWeapon { get; private set; }

    Weapon _mainWeapon;
    Weapon _subWeapon;


    public void Add(ItemData itemData, Vector2Int position, int amount = 1)
    {
        if (_itemList[position.y][position.x] == null)
        {
            _itemList[position.y][position.x] = new ItemSet(itemData);
            _itemList[position.y][position.x].amount = amount;

            if (itemData.Type == ItemData.ItemType.MainWeapon)
            {
                RegisterMainWeapon(itemData);
            }
        }

        /*for (int i = 1; i < itemData.Size; i++)
        {
            _itemList[position.y][position.x + i].itemData = itemData;
        }*/
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
        EquippedWeapon.Initialize(_character);
        _animator.SetInteger("Weapon", EquippedWeapon.GetWeaponNum());

        // ïêäÌÇéËÇÃà íuÇ…à⁄ìÆ
        EquippedWeapon.transform.parent = _handHolder.transform;
        EquippedWeapon.transform.localPosition = Vector3.zero;
        EquippedWeapon.transform.localRotation = Quaternion.Euler(Vector3.zero);
    }

    private void RegisterMainWeapon(ItemData itemData)
    {
        _mainWeapon = Instantiate(itemData.Prefab).GetComponent<Weapon>();
    }

    private void Start()
    {
        // ItemListÇÃèâä˙âª
        for (int i = 0; i < ItemSlotMatrixNum.y; i++)
        {
            _itemList.Add(new List<ItemSet>());
            for (int j = 0; j < ItemSlotMatrixNum.x; j++)
            {
                _itemList[i].Add(null);
            }
        }

        //RegisterMainWeapon(ItemList[0][0].itemData);
    }
}

[System.Serializable]
public class ItemSet
{
    public readonly ItemData itemData;
    public Vector2Int mainPosition;
    public int amount;

    public ItemSet(ItemData itemData)
    {
        this.itemData = itemData;
    }
}