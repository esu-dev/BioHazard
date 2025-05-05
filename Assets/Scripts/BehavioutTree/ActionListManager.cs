using System;
using System.Linq;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu]
public class ActionListManager : Singleton_ScriptableObject<ActionListManager>
{
    [SerializeField]
    List<NodeParameter> _conditionParameterList = new List<NodeParameter>();

    [SerializeField]
    List<NodeParameter> _actionParameterList = new List<NodeParameter>();

    [SerializeField]
    List<BehaviourTreeData> _behaviourTreeDataList;

    public enum DataType
    {
        NOTHING,
        INT,
        FLOAT,
        STRING,
        VECTOR2,
        VECTOR3,
        GAMEOBJECT,
        BOOL,

        INT_BB = 20,
        FLOAT_BB,
        STRING_BB,
        VECTOR2_BB,
        VECTOR3_BB,
        GAMEOBJECT_BB,
        BOOL_BB,
    }

    [Serializable]
    public class NodeParameter
    {
        public string ActionName;
        public string Key;
        public string ClassName;
        public DataType[] ArgumentType;
        public DataType OutputType;
    }


    public string GetConditionNameByKey(string key)
    {
        return _conditionParameterList.Find(x => x.Key == key).ActionName;
    }

    // KeyとActionNameを対応させるための関数
    public string GetActionNameByKey(string key)
    {
        return _actionParameterList.Find(x => x.Key == key).ActionName;
    }

    public string GetConditionKeyByName(string name)
    {
        return _conditionParameterList.Find(x => x.ActionName == name).Key;
    }

    public string GetActionKeyByName(string name)
    {
        return _actionParameterList.Find(x => x.ActionName == name).Key;
    }

    public string GetClassNameByActionName(string name)
    {
        return _actionParameterList.Find(x => x.ActionName == name).ClassName;
    }

    public DataType[] GetArgumentTypeByName(string name)
    {
        NodeParameter parameter;
        if ((parameter = _conditionParameterList.Find(x => x.ActionName == name)) != null)
        {
            return parameter.ArgumentType;
        }
        else if((parameter = _actionParameterList.Find(x => x.ActionName == name)) != null)
        {
            return parameter.ArgumentType;
        }

        //Debug.Log("[エラー] ノードのActionNameが見つかりません。データが古い可能性があります。ノードを設定しなおしてください。");
        return new DataType[] { DataType.NOTHING };
    }

    public DataType GetOutputTypeByName(string name)
    {
        NodeParameter parameter;
        if ((parameter = _conditionParameterList.Find(x => x.ActionName == name)) != null)
        {
            return parameter.OutputType;
        }
        else if ((parameter = _actionParameterList.Find(x => x.ActionName == name)) != null)
        {
            return parameter.OutputType;
        }

        //Debug.Log("[エラー] ノードのActionNameが見つかりません。データが古い可能性があります。ノードを設定しなおしてください。");
        return DataType.NOTHING;
    }

    public List<string> GetExplanationListOfCondition()
    {
        return _conditionParameterList.Select(x => x.ActionName).ToList();
    }

    public List<string> GetExplanationListOfAction()
    {
        return _actionParameterList.Select(x => x.ActionName).ToList();
    }

    public BehaviourTreeData GetBehaviourTreeDataByName(string name)
    {
        return _behaviourTreeDataList.Find(x => x.ToString() == name);
    }

    public List<string> GetFunctionNameList()
    {
        return _behaviourTreeDataList.Select(x => x.ToString()).ToList();
    }
}

public static class ConditionType
{
    public const string ONCE = "Once";
    public const string TRUE = "True";
    public const string FALSE = "True -> False";
    public const string BOTH = "Both";

    public static List<string> ConditionTypeList = new List<string>()
    {
        ONCE,
        TRUE,
        FALSE,
        BOTH
    };
}