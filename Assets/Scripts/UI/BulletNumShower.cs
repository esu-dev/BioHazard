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

    [SerializeField]
    TextMeshProUGUI _inventoryBulletNumText;

    private void Start()
    {
        // エイム状態が変化したときのイベント登録
        _character.OnAimStateChange.AddListener(isAimState =>
        {
            this.gameObject.SetActive(isAimState);

            if (isAimState)
            {
                /*// 残弾数表示イベント登録
                // 毎回イベント登録をするのは武器が変わる可能があるから
                (_inventory.EquippedWeapon as Gun).OnBulletNumChanged.RemoveListener(OnBulletNumChange);
                (_inventory.EquippedWeapon as Gun).OnBulletNumChanged.AddListener(OnBulletNumChange);

                // 残弾数表示
                _reloadedBulletNumText.text = (_inventory.EquippedWeapon as Gun).BulletNum.ToString();

                // 残弾数表示イベント
                void OnBulletNumChange(int bulletNum)
                {
                    _reloadedBulletNumText.text = bulletNum.ToString();
                }*/

                // 処理不可が怖いので、インベントリ残弾数の表示だけコールバックで実装
                (_inventory.EquippedWeapon as Gun).OnReloaded.RemoveListener(updateInventoryBulletNumText);
                (_inventory.EquippedWeapon as Gun).OnReloaded.AddListener(updateInventoryBulletNumText);

                updateInventoryBulletNumText();

                void updateInventoryBulletNumText()
                {
                    _inventoryBulletNumText.text = _inventory.CountItemNum((_inventory.EquippedWeapon as Gun)?.BulletItemData).ToString();
                }
            }
        });

        this.gameObject.SetActive(false);
    }

    private void Update()
    {
        _reloadedBulletNumText.text = (_inventory.EquippedWeapon as Gun)?.BulletNum.ToString();
    }
}
