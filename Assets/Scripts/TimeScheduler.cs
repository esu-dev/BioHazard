using System;
using UnityEngine;

public class TimeScheduler : MonoBehaviour
{
    public static TimeScheduler CreateSchedule(float time, Action action)
    {
        TimeScheduler timeScheduler = new GameObject(typeof(TimeScheduler).Name).AddComponent<TimeScheduler>();
        timeScheduler.Initialize(time, action);

        return timeScheduler;
    }

    float _time;
    Action _action;

    float _currentTime;

    public void ExecuteImmediately()
    {
        _action();
        Destroy(this.gameObject);
    }

    public void Initialize(float time, Action action)
    {
        _time = time;
        _action = action;
    }

    private void Update()
    {
        if (_currentTime >= _time)
        {
            _action();
            Destroy(this.gameObject);
        }

        _currentTime += Time.deltaTime;
    }
}
