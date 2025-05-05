using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Weapon : Item
{
    protected Character character;

    public abstract int GetWeaponNum();
    public abstract void Setup();
    public abstract void Lower();

    public void Initialize(Character character)
    {
        this.character = character;
    }
}
