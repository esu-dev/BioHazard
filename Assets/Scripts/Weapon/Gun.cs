using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Gun : Weapon
{
    [field: SerializeField]
    public int MaxBulletNum { get; private set; }

    [SerializeField]
    int _power;

    [SerializeField]
    AnimatorProxy _animatorProxy;

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


    [field: SerializeField]
    public float FocusTime { get; private set; }

    [field: SerializeField]
    public ItemData BulletItemData;

    public int BulletNum { get; private set; }
    public UnityEvent<int> OnBulletNumChanged { get; private set; } = new UnityEvent<int>();
    public UnityEvent OnReloaded { get; private set; } = new UnityEvent();

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

    public void Fire(bool _isFocused)
    {
        if (BulletNum <= 0)
        {
            return;
        }

        if (_isSettingUp)
        {
            Ray ray = Camera.main.ViewportPointToRay(new Vector2(0.5f, 0.5f));
            Debug.DrawRay(ray.origin, ray.direction * 10, Color.blue, 1f);
            if (Physics.Raycast(ray, out RaycastHit hit, _hitDistance, ~(1 << LayerConst.ZOMBIE)))
            {
                Debug.Log(hit.transform.gameObject);

                AnimatedRagdoll animatedRagdoll = hit.transform.GetComponentInParent<AnimatedRagdoll>();
                if (animatedRagdoll && animatedRagdoll.MainBodyGameObject.TryGetComponent(out Humanoid humanoid))
                {
                    (humanoid as Zombie)?.React(hit.transform.gameObject, this.transform.forward);
                    humanoid?.Damage((int)(_power * Random.Range(0.9f, 1.1f)));
                }
            }

            // 弾の消費
            BulletNum--;
            OnBulletNumChanged.Invoke(BulletNum);


            // アニメーション
            _animatorProxy.SetTrigger(AnimatorParameterConst.GunAnimatorParameter.FIRE);


            _muzzleFlash.gameObject.SetActive(true);
            _muzzleFlash.Play();

            audioSource.Play();
        }
    }

    public void AddBullet(int num)
    {
        BulletNum += num;
        OnBulletNumChanged.Invoke(BulletNum);
        OnReloaded.Invoke();
    }

    private void Start()
    {
        BulletNum = MaxBulletNum;

        _muzzleFlash = Instantiate(_muzzleFlashPrefab, _firePos.transform).GetComponent<ParticleSystem>();
    }
}
