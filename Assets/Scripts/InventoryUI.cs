using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class InventoryUI : MonoBehaviour
{
    [SerializeField]
    Image _getItemImage;

    [SerializeField]
    GameObject _getItemImageMarker1;

    [SerializeField]
    GameObject _getItemImageMarker2;

    [SerializeField]
    GameObject _getItemImageMarker3;

    [SerializeField]
    TextMeshProUGUI _getItemText;

    [SerializeField]
    GameObject _inventoryGroup;

    [SerializeField]
    Character _character;

    [SerializeField]
    ItemSlotManager _itemSlotManager;

    [SerializeField]
    Inventory _inventory;

    [SerializeField]
    EventTriggerProxy _eventTriggerProxy;


    public void Initialize()
    {
        _character.OnInterect.AddListener(InteractedObject =>
        {
            if (InteractedObject is DroppedItem)
            {
                (GameStateManager.Instance as GameStateManager).ChangeStateToPauseState();

                //Open();
                this.gameObject.SetActive(true);
                this.gameObject.GetComponent<FadeUI>().Transparency(0);
                this.gameObject.GetComponent<FadeUI>().Visualize(0.25f).onComplete = () =>
                {
                    // アイテムを画面全体に表示
                    _getItemImage.gameObject.SetActive(true);
                    _getItemImage.sprite = (InteractedObject as DroppedItem).getItem.Image;
                    _getItemImage.transform.position = _getItemImageMarker1.transform.position;
                    _getItemImage.transform.localScale = _getItemImageMarker1.transform.localScale;
                    _getItemImage.transform.DOMove(_getItemImageMarker2.transform.position, 0.5f).onComplete = () => _eventTriggerProxy.gameObject.SetActive(true);

                    _getItemText.gameObject.SetActive(true);
                    _getItemText.text = (InteractedObject as DroppedItem).getItem.name + ((InteractedObject as DroppedItem).getItem.CanStack ? " x" + (InteractedObject as DroppedItem).amount : "");
                    _getItemImage.GetComponent<TextMeshProUGUI>().color = _getItemImage.GetComponent<TextMeshProUGUI>().color.Transparency();
                    _getItemImage.GetComponent<TextMeshProUGUI>().DOFade(1, 0.5f);
                };
                
                // 画面クリック時のイベント
                _eventTriggerProxy.AddEvent(UnityEngine.EventSystems.EventTriggerType.PointerDown, () =>
                {
                    // アイテム画像の移動・縮小
                    var sequence = DOTween.Sequence();
                    sequence.Append(_getItemImage.transform.DOMove(_getItemImageMarker3.transform.position, 0.25f))
                            .Join(_getItemImage.transform.DOScale(_getItemImageMarker3.transform.localScale, 0.25f));
            
                    _getItemText.gameObject.SetActive(false);
                    _eventTriggerProxy.gameObject.SetActive(false);

                    // インベントリの表示
                    Open();

                    _itemSlotManager.AddItem((InteractedObject as DroppedItem).getItem, (InteractedObject as DroppedItem).amount, () =>
                    {
                        var sequence = DOTween.Sequence();
                        sequence.Append(_getItemImage.transform.DOScale(0, 0.2f))
                                .AppendInterval(0.25f)
                                .AppendCallback(() =>
                                {
                                    this.gameObject.GetComponent<FadeUI>().Transparency(0.2f);
                                    _inventoryGroup.GetComponent<FadeUI>().Transparency(0.2f);
                                })
                                .AppendInterval(0.2f)
                                .AppendCallback(() =>
                                {
                                    this.gameObject.SetActive(false);
                                    _inventoryGroup.SetActive(false);

                                    (GameStateManager.Instance as GameStateManager).ChangeStateToPlayState();
                                });
                    });
                });
            }
        });

        _itemSlotManager.Initialize();
    }

    /// <summary>
    /// インベントリを表示する
    /// </summary>
    public void Open()
    {
        this.gameObject.SetActive(true);
        _inventoryGroup.SetActive(true);

        this.gameObject.GetComponent<FadeUI>().VisualizeImmediately();
        _inventoryGroup.GetComponent<FadeUI>().VisualizeImmediately();
        _itemSlotManager.SetItem(_inventory.ItemList);
    }

    public void Close()
    {
        this.gameObject.SetActive(false);
        _inventoryGroup.SetActive(false);
    }
}
