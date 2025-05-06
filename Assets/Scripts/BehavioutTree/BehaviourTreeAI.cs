using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace BehaviourTreeLib
{
    public class BehaviourTreeAI : MonoBehaviour
    {
        [SerializeField]
        internal BehaviourTreeData _behaviourTreeData;

        List<Condition> _conditionList = new List<Condition>();
        List<ActionFunction> _actionFunctionList = new List<ActionFunction>();
        List<VariableBase> _variableList = new List<VariableBase>();

        // こっちのノードとDataのノードIDを対応させた情報を格納するリスト
        List<NodeMatchingData> _nodeMatchingDataList = new List<NodeMatchingData>();

        Node _rootNode;

        private void OnDisable()
        {
            _rootNode.StopEvaluate();
            StopAllCoroutines();
        }

        private class Condition
        {
            public string key;
            public Func<bool> func;
        }

        private class Condition_Input : Condition
        {
            public new Func<InputBase[], bool> func;
        }

        private class ActionFunction
        {
            public string key;
            public Func<IEnumerator> func;
        }

        private class ActionClassData
        {
            public string key;
            public ActionClass actionClass;
        }

        private abstract class VariableBase
        {
            public string name;
        }

        private class Variable<T> : VariableBase
        {
            public T value;
        }

        private class DynamicVariable
        {
            public string name;
            public dynamic value;
        }

        public void SetBoolAction(string actionKey, Func<bool> func)
        {
            Condition actionFunction = new Condition();
            actionFunction.key = actionKey;
            actionFunction.func = func;

            _conditionList.Add(actionFunction);
        }

        public void SetBoolAction(string actionKey, Func<InputBase[], bool> func)
        {
            Condition_Input actionFunction = new Condition_Input();
            actionFunction.key = actionKey;
            actionFunction.func = func;

            _conditionList.Add(actionFunction);
        }

        public void CreateTree()
        {
            Node CreateNode(NodeData nodeData, BehaviourTreeData tree)
            {
                Node node;
                NodeParameter parameter = JsonUtility.FromJson<NodeParameter>(nodeData.parameterJSON);
                switch (nodeData.nodeTypeID)
                {
                    case NodeTypeID.RootNode:
                        node = new RootNode(this, CreateNode(tree.FindNodeDataByIDForLoad(nodeData.outputIDList[0]), tree));
                        break;
                    case NodeTypeID.Selector:
                        node = new Selector(this);
                        foreach (string id in nodeData.outputIDList)
                        {
                            (node as Selector).AddNode(CreateNode(tree.FindNodeDataByIDForLoad(id), tree));
                        }
                        break;
                    case NodeTypeID.WeightedRandomSelector:
                        node = new WeightedRandomSelector(this, parameter.argument);
                        foreach (string id in nodeData.outputIDList)
                        {
                            (node as WeightedRandomSelector).AddNode(CreateNode(tree.FindNodeDataByIDForLoad(id), tree));
                        }
                        break;
                    case NodeTypeID.Sequencer:
                        node = new Sequencer(this);
                        foreach (string id in nodeData.outputIDList)
                        {
                            (node as Sequencer).AddNode(CreateNode(tree.FindNodeDataByIDForLoad(id), tree));
                        }
                        break;
                    case NodeTypeID.SimpleParallel:
                        node = new SimpleParallel(this, CreateNode(tree.FindNodeDataByIDForLoad(nodeData.outputIDList[0]), tree), CreateNode(tree.FindNodeDataByIDForLoad(nodeData.outputIDList[1]), tree));
                        break;
                    case NodeTypeID.Repeater:
                        node = new Repeater(this, CreateNode(tree.FindNodeDataByIDForLoad(nodeData.outputIDList[0]), tree));
                        break;
                    case NodeTypeID.LoopNode:
                        node = new Loop(this, CreateNode(tree.FindNodeDataByIDForLoad(nodeData.outputIDList[0]), tree));
                        break;
                    case NodeTypeID.InverseDecorater:
                        node = new InverseDecorater(this, CreateNode(tree.FindNodeDataByIDForLoad(nodeData.outputIDList[0]), tree));
                        break;
                    case NodeTypeID.LoopConditionNode:
                        // ClassNameによる実装
                        string behaviorClassName = parameter.actionClassName;
                        Type behaviorType = Type.GetType($"{behaviorClassName}, Assembly-CSharp");
                        object behaviorTypeObject = Activator.CreateInstance(behaviorType);

                        SetBehaviorFieldValue_Input(behaviorType, behaviorTypeObject);

                        ConditionClass conditionClass = behaviorTypeObject as ConditionClass;
                        conditionClass.TargetObject = this.gameObject;
                        node = new LoopConditionNode(this, CreateNode(tree.FindNodeDataByIDForLoad(nodeData.outputIDList[0]), tree), parameter.conditionNodeType, conditionClass);
                        break;
                    case NodeTypeID.ConditionNode:
                        Func<bool> boolAction = _conditionList.Find(x => ActionListManager.Instance.GetConditionNameByKey(x.key) == parameter.actionName).func;
                        node = new ConditionNode(this, boolAction);
                        break;
                    case NodeTypeID.ActionNode:
                        // Outputの設定
                        OutputBase SetOutput<T>()
                        {
                            OutputBase output = new BT_Output<T>()
                            {
                                returnFunc = (T t) =>
                                {
                                // 変数の更新
                                Variable<T> variable = _variableList.Find(x => x.name == parameter.outputName) as Variable<T>;
                                    if (variable == null)
                                    {
                                        _variableList.Add(new Variable<T>() { name = parameter.outputName, value = t });
                                    }
                                    else
                                    {
                                        variable.value = t;
                                    }
                                }
                            };

                            return output;
                        }

                        // ClassNameによる実装
                        string className = parameter.actionClassName;
                        Type type = Type.GetType($"{className}, Assembly-CSharp");
                        object typeObject = Activator.CreateInstance(type);

                        int fieldCount = 0;
                        foreach (var field in type.GetFields(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance))
                        {
                            // Outputの設定
                            if (field.FieldType == typeof(BT_Output<int>))
                            {
                                field.SetValue(typeObject, SetOutput<int>());
                            }
                            else if (field.FieldType == typeof(BT_Output<float>))
                            {
                                field.SetValue(typeObject, SetOutput<float>());
                            }
                            else if (field.FieldType == typeof(BT_Output<Vector3>))
                            {
                                field.SetValue(typeObject, SetOutput<Vector3>());
                            }
                            else if (field.FieldType.BaseType == typeof(OutputBase))
                            {
                                Debug.LogError("追加しろ");
                            }

                            fieldCount++;
                        }

                        SetBehaviorFieldValue_Input(type, typeObject);

                        ActionClass actionClass = typeObject as ActionClass;
                        actionClass.TargetObject = this.gameObject;
                        node = new ActionNode(this, actionClass);
                        break;
                    case NodeTypeID.FunctionNode:
                        // treeを取得
                        BehaviourTreeData funcTree = ActionListManager.Instance.GetBehaviourTreeDataByName(parameter.actionName);
                        NodeData root = funcTree.GetNodeDataListForLoad()[0];
                        node = new RootNode(this, CreateNode(funcTree.FindNodeDataByID(root.outputIDList[0]), funcTree));
                        break;
                    default:
                        Debug.Log("[ Error ]");
                        node = null;
                        break;

                    void SetBehaviorFieldValue_Input(Type type, object typeObject)
                    {
                        T GetInput<T>(int index)
                        {
                            return (_variableList.Find(x => x.name == parameter.inputs[index].variableName) as Variable<T>).value;
                        }

                        InputBase SetInput<T>(int index)
                        {
                            return new BT_Input<T>() { valueFunc = () => (_variableList.Find(x => x.name == parameter.inputs[index].variableName) as Variable<T>).value };
                        }

                        InputBase SetInputValue<T>(T value)
                        {
                            return new BT_Input<T>() { valueFunc = () => value };
                        }

                        int fieldCount = 0;
                        foreach (var field in type.GetFields(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance))
                        {
                            // Inputの設定
                            if (field.FieldType == typeof(BT_Input<int>))
                            {
                                if (parameter.inputs[fieldCount].isDynamic)
                                {
                                    field.SetValue(typeObject, SetInput<int>(fieldCount));
                                }
                                else
                                {
                                    int input_int = int.Parse(parameter.inputs[fieldCount].value);
                                    field.SetValue(typeObject, SetInputValue<int>(input_int));
                                }
                            }
                            else if (field.FieldType == typeof(BT_Input<float>))
                            {
                                if (parameter.inputs[fieldCount].isDynamic)
                                {
                                    field.SetValue(typeObject, SetInput<float>(fieldCount));
                                }
                                else
                                {
                                    float input_float = float.Parse(parameter.inputs[fieldCount].value);
                                    field.SetValue(typeObject, SetInputValue<float>(input_float));
                                }
                            }
                            else if (field.FieldType == typeof(BT_Input<bool>))
                            {
                                if (parameter.inputs[fieldCount].isDynamic)
                                {
                                    field.SetValue(typeObject, SetInput<bool>(fieldCount));
                                }
                                else
                                {
                                    bool input_bool = parameter.inputs[fieldCount].value == "1";
                                    field.SetValue(typeObject, SetInputValue<bool>(input_bool));
                                }
                            }
                            else if (field.FieldType == typeof(BT_Input<Vector3>))
                            {
                                if (parameter.inputs[fieldCount].isDynamic)
                                {
                                    field.SetValue(typeObject, SetInput<Vector3>(fieldCount));
                                }
                            }
                            else if (field.FieldType == typeof(BT_Input<GameObject>))
                            {
                                if (parameter.inputs[fieldCount].isDynamic)
                                {
                                    field.SetValue(typeObject, SetInput<GameObject>(fieldCount));
                                }
                            }
                            else if (field.FieldType.BaseType == typeof(InputBase))
                            {
                                Debug.Log("ScriptableObjectのロード方法を検討する");
                                //UnityEngine.Object input_Object = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(parameter.inputs[fieldCount].value);
                                //field.SetValue(typeObject, SetInputValue<UnityEngine.Object>(input_Object));
                            }
                            else
                            {
                                continue;
                            }

                            fieldCount++;
                        }
                    }
                }

                NodeMatchingData nodeMatchingData = new NodeMatchingData();
                nodeMatchingData.node = node;
                nodeMatchingData.tree = tree;
                nodeMatchingData.id = nodeData.id;

                _nodeMatchingDataList.Add(nodeMatchingData);

                return node;
            }

            _rootNode = CreateNode(_behaviourTreeData.GetNodeDataListForLoad()[0], _behaviourTreeData);
            _rootNode.Evaluate((NodeState result) =>
            {
                if (result == NodeState.True)
                {
                    Debug.Log("Complete!!");
                }
                else
                {
                    Debug.Log("fail...");
                }
            });
        }

        public void StopTree()
        {
            _rootNode.StopEvaluate();
        }

        /// <summary>
        /// ノードの色を変更する
        /// </summary>
        /// <param name="node">プログラム上のノード</param>
        internal void ChangeColor(Node node)
        {
            NodeMatchingData nodeMatchingData = _nodeMatchingDataList.Find(x => x.node == node);
            string id = nodeMatchingData.id;

            nodeMatchingData.tree.ChangeState(id, node.state, this.gameObject);
        }


        internal class NodeMatchingData
        {
            public Node node;
            public BehaviourTreeData tree;
            public string id;
        }

        internal abstract class Node
        {
            internal NodeState state = NodeState.Waiting;

            protected BehaviourTreeAI behaviourTreeAI;

            public abstract void Evaluate(UnityAction<NodeState> callback);
            public abstract void StopEvaluate();

            /// <summary>
            /// ノードの状態を変更する
            /// </summary>
            /// <param name="state"></param>
            protected void ChangeState(NodeState state)
            {
                this.state = state;

                behaviourTreeAI.ChangeColor(this);
            }
        }

        internal class RootNode : Node
        {
            Node _node;
            NodeState _result;

            public RootNode(BehaviourTreeAI npc_Behaviour, Node node)
            {
                base.behaviourTreeAI = npc_Behaviour;

                _node = node;
            }

            public override void Evaluate(UnityAction<NodeState> callback)
            {
                ChangeState(NodeState.Running);
                _node.Evaluate((NodeState state) =>
                {
                    if (state == NodeState.True)
                    {
                        ChangeState(NodeState.True);
                        callback(NodeState.True);
                    }
                    else if (state == NodeState.False)
                    {
                        ChangeState(NodeState.False);
                        callback(NodeState.False);
                    }
                });
            }

            public override void StopEvaluate()
            {
                _node.StopEvaluate();

                ChangeState(NodeState.Waiting);
            }
        }

        internal class Selector : Node
        {
            List<Node> nodeList = new List<Node>();

            public Selector(BehaviourTreeAI npc_Behaviour)
            {
                base.behaviourTreeAI = npc_Behaviour;
            }

            public override void Evaluate(UnityAction<NodeState> callback)
            {
                if (base.state == NodeState.Running)
                {
                    return;
                }

                ChangeState(NodeState.Running);

                void ChildEvaluate(int index)
                {
                    if (index >= nodeList.Count())
                    {
                        ChangeState(NodeState.False);
                        callback(NodeState.False);
                        return;
                    }

                    nodeList[index].Evaluate((NodeState state) =>
                    {
                        if (state == NodeState.True)
                        {
                            // 以降の子ノードの終了
                            for (int i = index + 1; i < nodeList.Count(); i++)
                                {
                                    nodeList[i].StopEvaluate();
                                }

                                ChangeState(NodeState.True);
                                callback(NodeState.True);
                            }
                        // LoopConditionがTrueになって現在のタスクに中断命令が下る
                        else if (state == NodeState.Running)
                        {
                            // Runningより優先順位の低い子ノードの終了
                            for (int i = index + 1; i < nodeList.Count(); i++)
                            {
                                nodeList[i].StopEvaluate();
                            }

                            ChangeState(NodeState.Running);
                            callback(NodeState.Running);
                        }
                        else if (state == NodeState.False)
                        {
                            ChildEvaluate(index + 1);
                        }
                    });
                }

                ChildEvaluate(0);
            }

            public override void StopEvaluate()
            {
                // 子ノードの終了
                for (int i = 0; i < nodeList.Count(); i++)
                {
                    nodeList[i].StopEvaluate();
                }

                ChangeState(NodeState.Waiting);
            }

            public void AddNode(Node node)
            {
                nodeList.Add(node);
            }
        }

        private class WeightedRandomSelector : Node
        {
            List<Node> nodeList = new List<Node>();
            float[] _inputs;

            public WeightedRandomSelector(BehaviourTreeAI npc_Behaviour, float[] inputs)
            {
                base.behaviourTreeAI = npc_Behaviour;

                _inputs = new float[inputs.Length];
                Array.Copy(inputs, _inputs, inputs.Length);

                // 念のため正規化
                float sum = _inputs.Sum();
                for (int i = 0; i < _inputs.Length; i++)
                {
                    _inputs[i] /= sum;
                }

                // 累積確率分布化
                for (int i = 1; i < _inputs.Length; i++)
                {
                    for (int j = 0; j < i; j++)
                    {
                        _inputs[i] += _inputs[j];
                    }
                }
            }

            public override void Evaluate(UnityAction<NodeState> callback)
            {
                ChangeState(NodeState.Running);

                // ランダム選択
                float rand = UnityEngine.Random.Range(0f, 1.0f);
                for (int i = 0; i < _inputs.Length; i++)
                {
                    if (rand < _inputs[i])
                    {
                        nodeList[i].Evaluate((NodeState state) =>
                        {
                            if (state == NodeState.True)
                            {
                                ChangeState(NodeState.True);
                                callback(NodeState.True);
                            }
                        });
                        break;
                    }
                }
            }

            public override void StopEvaluate()
            {
                // 子ノードの終了
                for (int i = 0; i < nodeList.Count(); i++)
                {
                    nodeList[i].StopEvaluate();
                }

                ChangeState(NodeState.Waiting);
            }

            public void AddNode(Node node)
            {
                nodeList.Add(node);
            }
        }

        internal class Sequencer : Node
        {
            List<Node> nodeList = new List<Node>();

            public Sequencer(BehaviourTreeAI npc_Behaviour)
            {
                base.behaviourTreeAI = npc_Behaviour;
            }

            public override void Evaluate(UnityAction<NodeState> callback)
            {
                // 待機中ではないなら何もしない
                if (state == NodeState.Running)
                {
                    return;
                }
                /*if (state != State.Waiting &&
                    state != State.True)
                {
                    return;
                }*/

                ChangeState(NodeState.Running);

                void ChildEvaluate(int index)
                {
                    if (index >= nodeList.Count())
                    {
                        ChangeState(NodeState.True);
                        callback(NodeState.True);
                        return;
                    }

                    nodeList[index].Evaluate((NodeState state) =>
                    {
                        if (state == NodeState.True)
                        {
                            ChangeState(NodeState.Running);
                            callback(NodeState.Running);
                            ChildEvaluate(index + 1);
                        }
                        else if (state == NodeState.Running)
                        {
                            ChangeState(NodeState.Running);
                            callback(NodeState.Running);
                        }
                        else if (state == NodeState.False)
                        {
                            // 子ノードの終了
                            for (int i = index + 1; i < nodeList.Count(); i++)
                            {
                                nodeList[i].StopEvaluate();
                            }

                            ChangeState(NodeState.False);
                            callback(NodeState.False);
                        }
                    });
                }

                ChildEvaluate(0);
            }

            public override void StopEvaluate()
            {
                // 子ノードの終了
                for (int i = 0; i < nodeList.Count(); i++)
                {
                    nodeList[i].StopEvaluate();
                }

                ChangeState(NodeState.Waiting);
            }

            public void AddNode(Node node)
            {
                nodeList.Add(node);
            }
        }

        internal class SimpleParallel : Node
        {
            Node _mainNode;
            Node _subNode;

            public SimpleParallel(BehaviourTreeAI behaviourTreeAI, Node mainNode, Node subNode)
            {
                base.behaviourTreeAI = behaviourTreeAI;

                _mainNode = mainNode;
                _subNode = subNode;
            }

            public override void Evaluate(UnityAction<NodeState> callback)
            {
                if (state == NodeState.Running)
                {
                    return;
                }

                ChangeState(NodeState.Running);

                _mainNode.Evaluate((NodeState result) =>
                {
                    if (result == NodeState.True)
                    {
                        _subNode.StopEvaluate();
                        ChangeState(result);
                    }
                    else if (result == NodeState.False)
                    {
                        _subNode.StopEvaluate();
                        ChangeState(result);
                    }
                    else if (result == NodeState.Running)
                    {
                        if (_subNode.state == NodeState.Waiting)
                        {
                            _subNode.Evaluate((NodeState result) => { });
                        }

                        ChangeState(result);
                        //_subNode.StopEvaluate();
                    }

                    callback(result);
                });

                _subNode.Evaluate((NodeState result) =>
                {

                });
            }

            public override void StopEvaluate()
            {
                ChangeState(NodeState.Waiting);
                _mainNode.StopEvaluate();
                _subNode.StopEvaluate();
            }
        }

        internal class Repeater : Node
        {
            Node childNode;

            public Repeater(BehaviourTreeAI npc_Behaviour, Node childNode)
            {
                base.behaviourTreeAI = npc_Behaviour;

                this.childNode = childNode;
            }

            public override void Evaluate(UnityAction<NodeState> callback)
            {
                if (base.state == NodeState.Running)
                {
                    return;
                }

                ChangeState(NodeState.Running);

                void ChildEvaluate()
                {
                    childNode.Evaluate((NodeState result) =>
                    {
                        if (result == NodeState.True || result == NodeState.False)
                        {
                            //childNode.StopEvaluate();
                            ChildEvaluate();
                        }
                    });
                }

                ChildEvaluate();
            }

            public override void StopEvaluate()
            {
                ChangeState(NodeState.Waiting);

                childNode.StopEvaluate();
            }
        }

        internal class Loop : Node
        {
            Node childNode;

            Coroutine _loopCoroutine;

            public Loop(BehaviourTreeAI npc_Behaviour, Node childNode)
            {
                base.behaviourTreeAI = npc_Behaviour;

                this.childNode = childNode;
            }

            public override void Evaluate(UnityAction<NodeState> callback)
            {
                if (base.state != NodeState.Waiting)
                {
                    return;
                }

                _loopCoroutine = behaviourTreeAI.StartCoroutine(LoopEvaluate(callback));

                callback(NodeState.True);
            }

            private IEnumerator LoopEvaluate(UnityAction<NodeState> callback)
            {
                while (true)
                {
                    ChangeState(NodeState.Running);

                    childNode.Evaluate((NodeState result) =>
                    {
                    /*if (result == State.True || result == State.False)
                    {
                        ChangeState(result);
                        callback(result);
                    }*/

                        ChangeState(result);
                    });

                    yield return new WaitForSeconds(0.5f);
                }
            }

            public override void StopEvaluate()
            {
                ChangeState(NodeState.Waiting);

                childNode.StopEvaluate();

                if (_loopCoroutine != null)
                {
                    behaviourTreeAI.StopCoroutine(_loopCoroutine);
                    _loopCoroutine = null;
                }
            }
        }

        internal class InverseDecorater : Node
        {
            Node childNode;

            public InverseDecorater(BehaviourTreeAI npc_Behaviour, Node childNode)
            {
                base.behaviourTreeAI = npc_Behaviour;

                this.childNode = childNode;
            }

            public override void Evaluate(UnityAction<NodeState> callback)
            {
                ChangeState(NodeState.Running);

                childNode.Evaluate((NodeState result) =>
                {
                    if (result == NodeState.True)
                    {
                        ChangeState(NodeState.False);
                        callback(NodeState.False);
                    }
                    else
                    {
                        ChangeState(NodeState.True);
                        callback(NodeState.True);
                    }
                });
            }

            public override void StopEvaluate()
            {
                childNode.StopEvaluate();

                ChangeState(NodeState.Waiting);
            }
        }

        internal class ConditionNode : Node
        {
            Func<bool> action;

            public ConditionNode(BehaviourTreeAI npc_Behaviour, Func<bool> action)
            {
                base.behaviourTreeAI = npc_Behaviour;

                this.action = action;
            }

            public override void Evaluate(UnityAction<NodeState> callback)
            {
                ChangeState(NodeState.Running);
                if (action())
                {
                    ChangeState(NodeState.True);
                    callback(NodeState.True);
                }
                else
                {
                    ChangeState(NodeState.False);
                    callback(NodeState.False);
                }
            }

            public override void StopEvaluate()
            {

            }
        }

        internal class LoopConditionNode : Node
        {
            Node _childNode;
            ConditionClass action;
            Coroutine _loopCoroutine;
            NodeState _currentState;
            string _type;

            public LoopConditionNode(BehaviourTreeAI npc_Behaviour, Node childNode, string type, ConditionClass actionClass)
            {
                base.behaviourTreeAI = npc_Behaviour;
                _childNode = childNode;
                _type = type;
                this.action = actionClass;
            }

            public override void Evaluate(UnityAction<NodeState> callback)
            {
                ChangeState(NodeState.Running);
                _currentState = NodeState.Running;

                _loopCoroutine = behaviourTreeAI.StartCoroutine(LoopEvaluate(callback));
            }

            /// <summary>
            /// 繰り返し評価する
            /// </summary>
            /// <param name="callback"></param>
            /// <returns></returns>
            private IEnumerator LoopEvaluate(UnityAction<NodeState> callback)
            {
                bool isContinuous = true;

                while (isContinuous)
                {
                    if (action.Execute())
                    {
                        if (_currentState != NodeState.True)
                        {
                            ChangeState(NodeState.Running);
                            callback(NodeState.Running);

                            _childNode.Evaluate((NodeState result) =>
                            {
                                ChangeState(result);
                                callback(result);
                                _currentState = result;

                                if (result == NodeState.False && _type == ConditionType.FALSE)
                                {
                                    isContinuous = false;
                                }
                            });

                            if (_type == ConditionType.TRUE)
                            {
                                break;
                            }
                        }
                    }
                    else if (!action.Execute())
                    {
                        if (_currentState != NodeState.False)
                        {
                            ChangeState(NodeState.False);
                            callback(NodeState.False);
                            _currentState = NodeState.False;

                            _childNode.StopEvaluate();

                            if (_type == ConditionType.FALSE)
                            {
                                break;
                            }
                        }
                    }

                    if (_type == ConditionType.ONCE)
                    {
                        break;
                    }

                    yield return new WaitForSeconds(0.25f);
                }
            }

            public override void StopEvaluate()
            {
                ChangeState(NodeState.Waiting);

                if (_loopCoroutine != null)
                {
                    behaviourTreeAI.StopCoroutine(_loopCoroutine);
                    _loopCoroutine = null;
                }

                _childNode.StopEvaluate();
            }
        }

        internal class ActionNode : Node
        {
            private ActionClass _actionClass;
            private Coroutine _updateCoroutine;

            public ActionNode(BehaviourTreeAI npc_Behaviour, ActionClass actionClass)
            {
                base.behaviourTreeAI = npc_Behaviour;

                _actionClass = actionClass;
            }

            public override void Evaluate(UnityAction<NodeState> callback)
            {
                if (state == NodeState.Running)
                {
                    return;
                }

                ChangeState(NodeState.Running);

                _actionClass.Start((NodeState state) =>
                {
                    if (_updateCoroutine != null)
                    {
                        base.behaviourTreeAI.StopCoroutine(_updateCoroutine);
                    }

                    ChangeState(state);
                    callback(state);
                });

                _updateCoroutine = base.behaviourTreeAI.StartCoroutine(Update());
            }

            public override void StopEvaluate()
            {
                if (base.state == NodeState.Running)
                {
                    base.behaviourTreeAI.StopCoroutine(_updateCoroutine);
                    _actionClass.Stop();
                }

                ChangeState(NodeState.Waiting);
            }

            private IEnumerator Update()
            {
                while (true)
                {
                    _actionClass.Update();
                    yield return null;
                }
            }
        }
    }

    public class InputBase
    {
        //public Func<dynamic> getValue;

        public T GetInput<T>()
        {
            return (this as BT_Input<T>).valueFunc();
        }
    }

    public class BT_Input<T> : InputBase
    {
        public T value
        {
            get
            {
                return (T)valueFunc();
            }
        }
        public Func<T> valueFunc;
    }

    public class OutputBase
    {
        //public Action<dynamic> returnValue;

        public Action<T> GetReturnFunc<T>()
        {
            return (this as BT_Output<T>).returnFunc;
        }
    }

    public class BT_Output<T> : OutputBase
    {
        public Action<T> returnFunc;
    }

    public abstract class ActionClassBase
    {
        public GameObject TargetObject { get; set; }
    }

    public abstract class ConditionClass : ActionClassBase
    {
        public abstract bool Execute();
    }

    public abstract class ActionClass : ActionClassBase
    {
        public abstract void Start(UnityAction<NodeState> callback);

        public abstract void Update();

        public abstract void Stop();
    }

    public class BehaviourProperty : System.Attribute
    {

    }
}
