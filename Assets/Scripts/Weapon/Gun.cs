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
        _isSettingUp = true;
    }

    public override void Lower()
    {
        _isSettingUp = false;
    }

    public override void Fire()
    {
        if (_isSettingUp)
        {
            Debug.DrawRay(_firePos.transform.position, this.transform.forward * 10, Color.blue, 1f);
            Ray ray = new Ray(_firePos.transform.position, this.transform.forward);
            if (Physics.Raycast(ray, out RaycastHit hit, _hitDistance, ~(1 << LayerConst.ZOMBIE)))
            {
                Debug.Log(hit.transform.gameObject);

                if (hit.transform.root.TryGetComponent(out AnimatedRagdoll animatedRagdoll) && animatedRagdoll.MainBodyGameObject.TryGetComponent(out Humanoid humanoid))
                {
                    humanoid?.React(ray.direction);
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
