using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class ItemSlotManager : MonoBehaviour
{
    [SerializeField]
    Image _holdItemImage;

    [SerializeField]
    Inventory _inventory;

    State _state;

    List<List<ItemSlot>> _itemSlotList = new List<List<ItemSlot>>();


    public void Initialize()
    {
        _itemSlotList.Clear();

        for (int i = 0; i < this.transform.childCount; i++)
        {
            _itemSlotList.Add(new List<ItemSlot>());
            for (int j = 0; j < this.transform.GetChild(i).childCount; j++)
            {
                if (this.transform.GetChild(i).GetChild(j).TryGetComponent(out ItemSlot itemSlot))
                {
                    _itemSlotList[i].Add(itemSlot);
                    itemSlot.SetSlotPosition(new Vector2Int(j, i));
                }

                int _i = i;
                int _j = j;
                //_itemSlotList[i][j].SetClickEvent(() => Debug.Log(new Vector2Int(_j, _i)));
            }
        }

        _state = new ShowState(this);
        _state.Enter();
    }

    public void SetItem(IEnumerable<IEnumerable<ItemSet>> itemSetList)
    {
        _state = new ShowState(this);
        _state.Enter();

        int i = 0;
        foreach (IEnumerable<ItemSet> itemSets in itemSetList)
        {
            int j = 0;
            foreach (ItemSet itemSet in itemSets)
            {
                if (itemSet?.itemData != null)
                {
                    _itemSlotList[i][j].SetImage(itemSet.itemData.Image);
                    Debug.Log(itemSet.itemData.Name);
                }

                j++;
            }

            i++;
        }
    }

    public void AddItem(ItemData itemData, int amount, Action onComplete)
    {
        _state = new AddItemState(this, itemData, amount, onComplete);
        _state.Enter();
    }

    private void Update()
    {
        _state.Update();
    }

    abstract class State
    {
        protected ItemSlotManager ItemSlotManager;

        public State(ItemSlotManager itemSlotManager)
        {
            ItemSlotManager = itemSlotManager;
        }

        public virtual void Enter() { }
        public virtual void Exit() { }
        public virtual void Update() { }
    }

    class ShowState : State
    {
        public ShowState(ItemSlotManager itemSlotManager) : base(itemSlotManager) { }

        public override void Enter()
        {
            // ItemSlotのクリックイベント登録
            for (int i = 0; i < base.ItemSlotManager._itemSlotList.Count(); i++)
            {
                for (int j = 0; j < base.ItemSlotManager._itemSlotList[i].Count(); j++)
                {
                    int _i = i;
                    int _j = j;
                    base.ItemSlotManager._itemSlotList[i][j].RemoveAllClickEvent();
                }
            }
        }
    }

    class AddItemState : State
    {
        float _itemSlotSize;
        Vector2 _itemSlotOriginPoint;

        ItemData _itemData;
        int _amount;
        Action _onComplete;

        public AddItemState(ItemSlotManager itemSlotManager, ItemData itemData, int amount, Action onComplete) : base(itemSlotManager)
        {
            _itemData = itemData;
            _amount = amount;
            _onComplete = onComplete;
        }


        public override void Enter()
        {
            _itemSlotSize = base.ItemSlotManager._itemSlotList[0][1].gameObject.transform.position.x - base.ItemSlotManager._itemSlotList[0][0].gameObject.transform.position.x;
            _itemSlotOriginPoint = base.ItemSlotManager._itemSlotList[0][0].gameObject.transform.position;


            // ItemSlotのクリックイベント登録
            for (int i = 0; i < base.ItemSlotManager._itemSlotList.Count(); i++)
            {
                for (int j = 0; j < base.ItemSlotManager._itemSlotList[i].Count(); j++)
                {
                    int _i = i;
                    int _j = j;
                    base.ItemSlotManager._itemSlotList[i][j].SetClickEvent(() =>
                    {
                        Debug.Log(new Vector2Int(_j, _i));
                        base.ItemSlotManager._inventory.Add(_itemData, new Vector2Int(_j, _i), _amount);
                        base.ItemSlotManager._itemSlotList[_i][_j].SetImage(_itemData.Image);
                        base.ItemSlotManager._holdItemImage.gameObject.SetActive(false);
                        base.ItemSlotManager._holdItemImage.DOKill();
                        _onComplete();
                    });
                }
            }


            base.ItemSlotManager._holdItemImage.gameObject.SetActive(true);
            base.ItemSlotManager._holdItemImage.sprite = _itemData.Image;
            base.ItemSlotManager._holdItemImage.DOFade(0, 0.65f).SetEase(Ease.Linear).SetLoops(-1, LoopType.Yoyo);
        }

        public override void Update()
        {
            // アイテムスロットの左上を原点とした座標
            Vector2 itemSlotCoordinatePosition = (Vector2)Input.mousePosition - _itemSlotOriginPoint - new Vector2(-_itemSlotSize, _itemSlotSize) / 2;

            Vector2Int itemSlotPosition = (itemSlotCoordinatePosition / _itemSlotSize).ToVector2Int();
            if (-itemSlotPosition.y >= 0 && -itemSlotPosition.y < base.ItemSlotManager._itemSlotList.Count() &&
                itemSlotPosition.x >= 0 && itemSlotPosition.x < base.ItemSlotManager._itemSlotList[-itemSlotPosition.y].Count())
            {
                base.ItemSlotManager._holdItemImage.transform.position = (Vector2)itemSlotPosition * _itemSlotSize + _itemSlotOriginPoint;
            }
        }
    }
}
