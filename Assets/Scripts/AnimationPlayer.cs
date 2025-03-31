using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class AnimationPlayer : FlexUpdateMonoBehaviour
{
    [SerializeField]
    AnimatorProxy _animatorProxy;

    List<AnimationEventManager> _animationEventManagerList = new List<AnimationEventManager>();


    public void Play(string stateName, float fixedTransitionDuration, float endFrame)
    {
        _animatorProxy.CrossFadeInFixedTime(stateName, fixedTransitionDuration);
        AnimationEventManager animationEventManager = new AnimationEventManager(stateName);
        animationEventManager.AddAnimationEvent(endFrame, () => Stop(stateName));
        _animationEventManagerList.Add(animationEventManager);
    }

    public void Stop(string stateName)
    {
        _animationEventManagerList.Remove(_animationEventManagerList.Find(x => x.StateName == stateName));
    }

    public void StopAll()
    {
        _animationEventManagerList.Clear();
    }

    public void AddAnimationEvent(string stateName, float frame, Action action)
    {
        _animationEventManagerList.Find(x => x.StateName == stateName).AddAnimationEvent(frame, action);
    }

    protected override void FlexUpdate()
    {
        for (int i = _animationEventManagerList.Count() - 1; i >= 0; i--)
        {
            _animationEventManagerList[i].Update(base.FlexDeltaTime);
        }
    }

    class AnimationEventManager
    {
        public string StateName { get; private set; }
        List<AnimationEvent> _animationEventList = new List<AnimationEvent>();

        float _frame;


        public AnimationEventManager(string stateName)
        {
            StateName = stateName;
        }

        public void AddAnimationEvent(float frame, Action action)
        {
            _animationEventList.Add(new AnimationEvent(frame, action));
        }

        public void Update(float deltaTime)
        {
            for (int i = _animationEventList.Count() - 1; i >= 0; i--)
            {
                if (!_animationEventList[i].IsCompleted && Math.Abs(_frame - _animationEventList[i].frame / 60f) < deltaTime)
                {
                    _animationEventList[i].action();
                    _animationEventList.RemoveAt(i);
                }
            }

            _frame += deltaTime;
        }
    }

    class AnimationEvent
    {
        public float frame { get; private set; }
        public Action action { get; private set; }
        public bool IsCompleted { get; private set; }

        public AnimationEvent(float frame, Action action)
        {
            this.frame = frame;
            this.action = action;
        }

        public void SetIsCompleted(bool isCompleted)
        {
            IsCompleted = isCompleted;
        }
    }
}
