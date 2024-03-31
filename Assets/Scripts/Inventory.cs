using System.Collections;
using System.Collections.Generic;
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
    RigManager _rigManager;

    [SerializeField]
    Animator _animator;

    [SerializeField]
    Weapon _bareHands;

    [SerializeField]
    List<Item> _itemList = new List<Item>();

    public Weapon EquippedWeapon { get; private set; }

    Weapon _mainWeapon;
    Weapon _subWeapon;

    public void EquipMain()
    {
        Equip(_mainWeapon);
    }

    public void EquipSub()
    {
        Equip(_subWeapon);
    }

    void Equip(Weapon weapon)
    {
        // •Ší‚ğ”w’†‚ÉˆÚ“®
        if (EquippedWeapon == _mainWeapon)
        {
            EquippedWeapon.transform.parent = _backHolder.transform;
            EquippedWeapon.transform.localPosition = Vector3.zero;
            EquippedWeapon.transform.localRotation = Quaternion.Euler(Vector3.zero);
        }

        EquippedWeapon = weapon ? weapon : _bareHands;
        EquippedWeapon.Initialize(_character, _rigManager);
        _animator.SetInteger("Weapon", EquippedWeapon.GetWeaponNum());

        // •Ší‚ğè‚ÌˆÊ’u‚ÉˆÚ“®
        EquippedWeapon.transform.parent = _handHolder.transform;
        EquippedWeapon.transform.localPosition = Vector3.zero;
        EquippedWeapon.transform.localRotation = Quaternion.Euler(Vector3.zero);
    }

    void RegisterMainWeapon(Weapon weapon)
    {
        _mainWeapon = weapon;
    }

    private void Start()
    {
        RegisterMainWeapon(_itemList[0] as Weapon);
    }
}
