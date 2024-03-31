using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shotgun : Weapon
{
    [SerializeField]
    GameObject _firePos;

    [SerializeField]
    GameObject _muzzleFlashPrefab;

    bool _isSettingUp;
    ParticleSystem _muzzleFlash;

    public override int GetWeaponNum()
    {
        return 1;
    }

    public override void Setup()
    {
        base.character.ChangeStateTo(character.aimState);

        RigManager.WeightData weightData = new RigManager.WeightData();
        weightData.spine = 0.25f;
        weightData.rightHand = 1.0f;
        base.rigManager.SetWeight(weightData);

        _isSettingUp = true;
    }

    public override void Lower()
    {
        base.character.ChangeStateTo(character.normalState);

        RigManager.WeightData weightData = new RigManager.WeightData();
        weightData.spine = 0.0f;
        weightData.rightHand = 0.0f;
        base.rigManager.SetWeight(weightData);

        _isSettingUp = false;
    }

    public override void Fire()
    {
        if (_isSettingUp)
        {
            Debug.DrawRay(_firePos.transform.position, this.transform.forward * 10, Color.blue, 1f);
            _muzzleFlash.gameObject.SetActive(true);
            _muzzleFlash.Play();
        }
    }

    private void Start()
    {
        _muzzleFlash = Instantiate(_muzzleFlashPrefab, _firePos.transform).GetComponent<ParticleSystem>();
    }
}
