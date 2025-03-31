using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Updater : MonoBehaviour
{
    List<UpdatedData> _updateActionList = new List<UpdatedData>();

    [field: SerializeField]
    public float TimeScale { get; private set; } = 1;

    public void SetTimeScale(float timeScale)
    {
        TimeScale = timeScale;
    }

    public void SetTimeScaleInSecond(float timeScale, float time)
    {
        float timeScale_ThatTime = TimeScale;

        SetTimeScale(timeScale);
        TimeScheduler.CreateSchedule(time, () => SetTimeScale(timeScale_ThatTime));
    }

    public void AddUpdateAction(FlexUpdateMonoBehaviour flexUpdateMonoBehaviour, Action updateAction)
    {
        _updateActionList.Add(new UpdatedData() { flexUpdateMonoBehaviour = flexUpdateMonoBehaviour, updateAction = updateAction });
    }

    private void Update()
    {
        foreach (UpdatedData updatedData in _updateActionList)
        {
            if (updatedData.flexUpdateMonoBehaviour.enabled)
            {
                updatedData.updateAction();
            }
        }
    }

    struct UpdatedData
    {
        public FlexUpdateMonoBehaviour flexUpdateMonoBehaviour;
        public Action updateAction;
    }
}


[RequireComponent(typeof(Updater))]
public abstract class FlexUpdateMonoBehaviour : MonoBehaviour
{
    Updater _updater;

    protected float FlexDeltaTime
    {
        get
        {
            return Time.deltaTime * LocalizedTime.timeScale * _updater.TimeScale;
        }
    }

    private void Awake()
    {
        _updater = this.GetComponent<Updater>();
        _updater.AddUpdateAction(this, FlexUpdate);
    }

    protected virtual void FlexUpdate() { }
}
