using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Weapon : Item
{
    protected Character character;
    protected RigManager rigManager;

    public abstract int GetWeaponNum();
    public abstract void Setup();
    public abstract void Lower();
    public abstract void Fire();
    

    public void Initialize(Character character, RigManager rigManager)
    {
        this.character = character;
        this.rigManager = rigManager;
    }
}
