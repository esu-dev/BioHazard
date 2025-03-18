using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : Weapon
{
    [SerializeField]
    protected AudioSource audioSource;

    [SerializeField]
    protected GameObject _firePos;

    [SerializeField]
    protected GameObject _muzzleFlashPrefab;

    [SerializeField]
    protected float _hitDistance;

    bool _isSettingUp;
    protected ParticleSystem _muzzleFlash;


    public override int GetWeaponNum()
    {
        Debug.LogError("GetWeaponNum is not overrided.");
        return -1;
    }

    public override void Setup()
    {
        base.character.ChangeStateTo(character.aimState);

        RigManager.WeightData weightData = new RigManager.WeightData();
        weightData.spine1 = 0.75f;
        weightData.spine2 = 0.75f;
        weightData.shoulderL = 0.0f;
        weightData.shoulderR = 1.0f;
        weightData.neck = 0.25f;
        weightData.head = 0.5f;
        weightData.rightHand = 1.0f;
        base.rigManager.SetWeight(weightData);

        _isSettingUp = true;
    }

    public override void Lower()
    {
        base.character.ChangeStateTo(character.normalState);

        RigManager.WeightData weightData = new RigManager.WeightData();
        weightData.spine1 = 0.0f;
        weightData.spine2 = 0.0f;
        weightData.shoulderL = 0.0f;
        weightData.shoulderR = 0.0f;
        weightData.neck = 0.0f;
        weightData.head = 0.0f;
        weightData.rightHand = 0.0f;
        base.rigManager.SetWeight(weightData);

        _isSettingUp = false;
    }

    public override void Fire()
    {
        if (_isSettingUp)
        {
            Debug.DrawRay(_firePos.transform.position, this.transform.forward * 10, Color.blue, 1f);
            Ray ray = new Ray(_firePos.transform.position, this.transform.forward);
            if (Physics.Raycast(ray, out RaycastHit hit, _hitDistance))
            {
                Debug.Log(hit.transform.gameObject);

                if (hit.transform.TryGetComponent(out Humanoid humanoid))
                {
                    humanoid?.Damage(10);
                }
            }
            _muzzleFlash.gameObject.SetActive(true);
            _muzzleFlash.Play();

            audioSource.Play();
        }
    }

    private void Start()
    {
        _muzzleFlash = Instantiate(_muzzleFlashPrefab, _firePos.transform).GetComponent<ParticleSystem>();
    }
}
