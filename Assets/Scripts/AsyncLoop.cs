using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AsyncLoop : MonoBehaviour
{
    public static AsyncLoop Create()
    {
        return new GameObject(typeof(AsyncLoop).Name).AddComponent<AsyncLoop>();
    }

    public static AsyncLoop Create(Func<bool> condition, Action iterator, Action action, float interval)
    {
        AsyncLoop asyncLoop = new GameObject(typeof(AsyncLoop).Name).AddComponent<AsyncLoop>();
        asyncLoop.For(condition, iterator, action, interval);
        return asyncLoop;
    }

    float _timer;
    ForLoopData _forLoopData;

    public void For(Func<bool> condition, Action iterator, Action action, float interval)
    {
        _timer = 0;

        _forLoopData = new ForLoopData();
        _forLoopData.condition = condition;
        _forLoopData.iterator = iterator;
        _forLoopData.action = action;
        _forLoopData.interval = interval;
    }

    private void Update()
    {
        if (_forLoopData == null)
        {
            return;
        }

        if (_timer >= _forLoopData.interval)
        {
            if (_forLoopData.condition())
            {
                _forLoopData.action();
                _forLoopData.iterator();

                _timer = 0;
            }
            else
            {
                _forLoopData = null;
            }
        }

        _timer += Time.deltaTime;
    }

    class ForLoopData
    {
        public Func<bool> condition;
        public Action iterator;
        public Action action;
        public float interval;
    }
}
