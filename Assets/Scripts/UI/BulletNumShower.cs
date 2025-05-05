using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class BulletNumShower : MonoBehaviour
{
    [SerializeField]
    Character _character;

    [SerializeField]
    Inventory _inventory;

    [SerializeField]
    TextMeshProUGUI _reloadedBulletNumText;

    private void Start()
    {
        _character.OnAimStateChange.AddListener(isAimState =>
        {
            this.gameObject.SetActive(isAimState);

            if (isAimState)
            {
                // 残弾数表示イベント登録
                (_inventory.EquippedWeapon as Gun).OnBulletNumChanged.RemoveListener(OnBulletNumChange);
                (_inventory.EquippedWeapon as Gun).OnBulletNumChanged.AddListener(OnBulletNumChange);

                // 残弾数表示
                _reloadedBulletNumText.text = (_inventory.EquippedWeapon as Gun).BulletNum.ToString();

                // 残弾数表示イベント
                void OnBulletNumChange(int bulletNum)
                {
                    _reloadedBulletNumText.text = bulletNum.ToString();
                }
            }
        });

        this.gameObject.SetActive(false);
    }
}
