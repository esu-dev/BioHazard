// 参考
// https://light11.hatenadiary.com/entry/2020/06/16/200750
// https://qiita.com/dwl/items/dd14f5f2a187084d317b
// https://hirukotime.hatenablog.jp/entry/2022/11/17/200000
// https://ruchi12377.hatenablog.com/entry/2020/04/18/175019

using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.UIElements;
using UnityEditor.Experimental.GraphView;

namespace BehaviourTreeLib
{
    public class BehaviourTreeEditorWindow : EditorWindow
    {
        public static BehaviourTreeEditorWindow instance;


        protected BehaviourTreeData _behaviourTreeData;

        // EditWindowはコンパイル時にシリアライズされ一度情報が対比された後、復元される。
        // よってシリアライズされないstatic変数は破棄されてしまう。
        private BehaviourTreeEditorWindow _permanentInstance;

        public BehaviourTreeGraphView behaviourTreeGraphView { get; private set; }
        BehaviourTreeAI _latestBehaviourTreeAI;

        internal Vector2 MousePosition;

        [OnOpenAsset(0)]
        public static bool OnOpenAsset(int instanceID, int line)
        {
            BehaviourTreeData asset = EditorUtility.InstanceIDToObject(instanceID) as BehaviourTreeData;

            // BehaviourTreeではないなら普通に開く
            if (asset == null)
            {
                return false;
            }

            Open(asset);
            return true;
        }

        public static void Open(BehaviourTreeData behaviourTree)
        {
            instance = GetWindow<BehaviourTreeEditorWindow>(behaviourTree.name);
            instance.Initialize(behaviourTree);
        }

        private void Initialize(BehaviourTreeData behaviourTree)
        {
            _permanentInstance = instance;

            _behaviourTreeData = behaviourTree;
            _behaviourTreeData.CopyDataForLoad(); // これを入れておかないと実行時以外でロードが機能しない

            // 任意のタイミングで更新してもらえるようにする
            _behaviourTreeData.SetStateChangedCallback(LoadOnlyState);

            // ロード処理
            Load();
        }

        private void OnGUI()
        {
            wantsMouseMove = true;
            MousePosition = Event.current.mousePosition;
        }

        private void OnEnable()
        {
            instance = _permanentInstance;

            // GraphViewを生成し、追加する
            if (behaviourTreeGraphView == null)
            {
                behaviourTreeGraphView = new BehaviourTreeGraphView(this);
                this.rootVisualElement.Add(behaviourTreeGraphView);

                // ツールバーの追加
                Toolbar toolbar = new Toolbar();
                ToolbarButton saveButton = new ToolbarButton(Save) { text = "Save" };
                toolbar.Add(saveButton);
                this.rootVisualElement.Add(toolbar);
            }

            if (_behaviourTreeData != null)
            {
                _behaviourTreeData.CopyDataForLoad();
                _behaviourTreeData.SetStateChangedCallback(LoadOnlyState);
                Load();
            }
        }

        private void Update()
        {
            _behaviourTreeData.SetTarget(Selection.gameObjects[0]);
        }

        /// <summary>
        /// データのロード
        /// </summary>
        private void Load()
        {
            // BlackBoardの設置
            Blackboard blackboard = new Blackboard(behaviourTreeGraphView);
            blackboard.title = "Blackboard";

            void AddBlackBoardField(string text)
            {
                blackboard.Add(new BlackboardField() { text = text, typeText = "T" });
            }

            blackboard.addItemRequested = (Blackboard b) =>
            {
                AddBlackBoardField("new BBF");
            };
            blackboard.SetPosition(new Rect(0, 100, 200, 200));

            // Blackboardのロード
            foreach (string text in _behaviourTreeData.GetBlackboardFieldTextList())
            {
                AddBlackBoardField(text);
            }
            behaviourTreeGraphView.Add(blackboard);
            behaviourTreeGraphView.SetBlackboard(blackboard);

            // ノードの設置
            foreach (NodeData nodeData in _behaviourTreeData.GetNodeDataListForLoad())
            {
                BaseNode instance = Activator.CreateInstance(Type.GetType(nodeData.typeName)) as BaseNode;
                instance.SetID(nodeData.id);
                instance.SetParameterJSON(nodeData.parameterJSON);
                instance.SetPosition(nodeData.position);

                behaviourTreeGraphView.AddElement(instance);
            }

            // エッジの設置
            foreach (NodeData nodeData in _behaviourTreeData.GetNodeDataListForLoad())
            {
                string id = nodeData.id;
                BaseNode from = behaviourTreeGraphView.nodes.FirstOrDefault(x => (x as BaseNode).ID == id) as BaseNode;

                if (nodeData.outputIDList.Count() == 0)
                {
                    continue;
                }

                Port outputPort = from.outputContainer.Children().First() as Port;
                foreach (string outputID in nodeData.outputIDList)
                {
                    BaseNode to = behaviourTreeGraphView.nodes.FirstOrDefault(x => (x as BaseNode).ID == outputID) as BaseNode;
                    Port inputPort = to.inputContainer.Children().First() as Port; // 今回はポート数が１という前提があるからこれで成立する

                    Edge edge = new Edge() { input = inputPort, output = outputPort };
                    inputPort.Connect(edge);
                    outputPort.Connect(edge);
                    behaviourTreeGraphView.Add(edge);
                }
            }

            // ルートノードが無ければ設置
            if (_behaviourTreeData.GetNodeDataListForLoad().Find(x => x.nodeTypeID == NodeTypeID.RootNode) == null)
            {
                RootNode rootNode = new RootNode();
                rootNode.SetPosition(new Rect(new Vector2(0, 100), new Vector2(100, 100)));
                rootNode.SetID("root");
                behaviourTreeGraphView.AddElement(rootNode);
            }
        }

        /// <summary>
        /// ノードの状態だけ更新する
        /// </summary>
        /// <param name="nodeData"></param>
        private void LoadOnlyState(NodeData nodeData)
        {
            // 何も選択していないなら状態を追跡しない
            if (Selection.gameObjects.Length == 0)
            {
                return;
            }

            if (Selection.gameObjects[0].TryGetComponent(out BehaviourTreeAI behaviourTreeAI))
            {
                BaseNode baseNode;
                if (_latestBehaviourTreeAI != behaviourTreeAI)
                {
                    _latestBehaviourTreeAI = behaviourTreeAI;

                    foreach (NodeData n in _behaviourTreeData.GetNodeDataListForLoad())
                    {
                        baseNode = behaviourTreeGraphView.nodes.ToList().Find(x => (x as BaseNode).ID == n.id) as BaseNode;
                        baseNode.SetState(n.state);
                    }

                    return;
                }

                baseNode = behaviourTreeGraphView.nodes.ToList().Find(x => (x as BaseNode).ID == nodeData.id) as BaseNode;
                baseNode.SetState(nodeData.state);
            }
        }

        /// <summary>
        /// データの保存
        /// </summary>
        private void Save()
        {
            _behaviourTreeData.AllClear();

            foreach (BaseNode baseNode in behaviourTreeGraphView.nodes)
            {
                NodeData nodeData = new NodeData();
                nodeData.typeName = baseNode.GetType().FullName;

                switch (baseNode.GetType().Name)
                {
                    case nameof(RootNode):
                        nodeData.nodeTypeID = NodeTypeID.RootNode;
                        break;
                    case nameof(Selector):
                        nodeData.nodeTypeID = NodeTypeID.Selector;
                        break;
                    case nameof(WeightedRandomSelector):
                        nodeData.nodeTypeID = NodeTypeID.WeightedRandomSelector;
                        break;
                    case nameof(Sequencer):
                        nodeData.nodeTypeID = NodeTypeID.Sequencer;
                        break;
                    case nameof(SimpleParallel):
                        nodeData.nodeTypeID = NodeTypeID.SimpleParallel;
                        break;
                    case nameof(Repeater):
                        nodeData.nodeTypeID = NodeTypeID.Repeater;
                        break;
                    case nameof(LoopNode):
                        nodeData.nodeTypeID = NodeTypeID.LoopNode;
                        break;
                    case nameof(InverseDecorater):
                        nodeData.nodeTypeID = NodeTypeID.InverseDecorater;
                        break;
                    /*case nameof(LoopConditionNode):
                        nodeData.nodeTypeID = NodeTypeID.LoopConditionNode;
                        break;*/
                    case nameof(LoopConditionNode):
                        nodeData.nodeTypeID = NodeTypeID.LoopConditionNode;
                        break;
                    case nameof(ActionNode):
                        nodeData.nodeTypeID = NodeTypeID.ActionNode;
                        break;
                    case nameof(FunctionNode):
                        nodeData.nodeTypeID = NodeTypeID.FunctionNode;
                        break;
                    default:
                        nodeData.nodeTypeID = NodeTypeID.ActionNode;
                        string name = baseNode.GetType().Name;
                        Debug.Log($"[ Error ] NodeTypeID is not defined! ( '{name}' )");
                        break;
                }

                nodeData.id = baseNode.ID;
                nodeData.parameterJSON = baseNode.GetParameterJSON();
                nodeData.position = baseNode.GetPosition();
                nodeData.state = NodeState.Waiting;

                _behaviourTreeData.AddNodeData(nodeData);
            }

            foreach (Edge edge in behaviourTreeGraphView.edges)
            {
                BaseNode from = edge.output.node as BaseNode;
                BaseNode to = edge.input.node as BaseNode;
                _behaviourTreeData.AddOutputID(from.ID, to.ID);
            }

            _behaviourTreeData.MakeRootFirst();

            // 全てのノードの子要素をノードの位置が高い順にソート
            _behaviourTreeData.SortAllChild();

            // Blackboardのセーブ
            foreach (string text in this.behaviourTreeGraphView.blackboard.contentContainer.Children().Select(x => (x as BlackboardField).text))
            {
                _behaviourTreeData.AddBlackBoardFieldText(text);
            }

            _behaviourTreeData.CopyDataForLoad();

            // 変更を通知
            EditorUtility.SetDirty(_behaviourTreeData);

            // 保存
            AssetDatabase.SaveAssets();

            Debug.Log("BehaviourTreeのセーブが完了しました。");
        }

        public class BehaviourTreeGraphView : GraphView
        {
            public Blackboard blackboard { get; private set; }

            public BehaviourTreeGraphView(BehaviourTreeEditorWindow editorWindow)
            {
                this.StretchToParentSize();

                SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);
                this.AddManipulator(new ContentDragger());
                this.AddManipulator(new SelectionDragger());
                this.AddManipulator(new RectangleSelector());

                var menuWindowProvider = CreateInstance<SearchMenuWindowProvider>();
                menuWindowProvider.Initialize(this, editorWindow);
                this.nodeCreationRequest += context =>
                {
                    SearchWindow.Open(new SearchWindowContext(context.screenMousePosition), menuWindowProvider);
                };

                // コピー
                this.serializeGraphElements += (IEnumerable<GraphElement> g) =>
                {
                    // コピー不可
                    if (g.Count() == 0)
                    {
                        return null;
                    }

                    NodeDataForJSON nodeDataForJSON = null;
                    foreach (GraphElement graphElement in g)
                    {
                        if (!(graphElement is BaseNode))
                        {
                            continue;
                        }

                        nodeDataForJSON = new NodeDataForJSON() { NodeTypeName = graphElement.GetType().FullName, ParameterForJSON = (graphElement as BaseNode).GetParameterJSON() };
                        break;
                    }

                    return JsonUtility.ToJson(nodeDataForJSON);
                };

                // ペースト
                this.unserializeAndPaste += (string operation, string data) =>
                {
                    // コピーされていないならペーストできない
                    if (data == null)
                    {
                        return;
                    }

                    NodeDataForJSON nodeDataForJSON = JsonUtility.FromJson<NodeDataForJSON>(data);

                    BaseNode baseNode = Activator.CreateInstance(Type.GetType(nodeDataForJSON.NodeTypeName)) as BaseNode;
                    baseNode.SetID(Guid.NewGuid().ToString("N"));
                    baseNode.SetParameterJSON(nodeDataForJSON.ParameterForJSON);

                    baseNode.SetPosition(new Rect(this.contentViewContainer.WorldToLocal(editorWindow.MousePosition), new Vector2(100, 100)));

                    this.AddElement(baseNode);
                };
            }

            // GraphViewの設定リストをoverrideして条件を記載
            public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
            {
                var compatiblePorts = new List<Port>();

                compatiblePorts.AddRange(ports.ToList().Where(port =>
                {
                    if (startPort.node == port.node)
                    {
                        return false;
                    }

                    if (port.direction == startPort.direction)
                    {
                        return false;
                    }

                    if (port.portType != startPort.portType)
                    {
                        return false;
                    }

                    return true;
                }));

                return compatiblePorts;
            }

            /// <summary>
            /// BlackBoardをセットする
            /// </summary>
            /// <param name="blackboard"></param>
            public void SetBlackboard(Blackboard blackboard)
            {
                this.blackboard = blackboard;
            }

            /// <summary>
            /// NodeをJSON形式に変換するためのデータセット
            /// </summary>
            private class NodeDataForJSON
            {
                public string NodeTypeName;
                public string ParameterForJSON;
            }
        }

        // どんなSearchMenuWindowを表示するかを設定する
        // 呼び出しにScriptableObjectであることが必要らしい
        public class SearchMenuWindowProvider : ScriptableObject, ISearchWindowProvider
        {
            private BehaviourTreeGraphView _graphView;
            private BehaviourTreeEditorWindow _editorWindow;

            public void Initialize(BehaviourTreeGraphView graphView, BehaviourTreeEditorWindow editorWindow)
            {
                _graphView = graphView;
                _editorWindow = editorWindow;
            }

            /// <summary>
            /// SearchTreeの内容
            /// </summary>
            /// <param name="context"></param>
            /// <returns></returns>
            public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
            {
                var entries = new List<SearchTreeEntry>();
                entries.Add(new SearchTreeGroupEntry(new GUIContent("Create Node")));

                entries.Add(new SearchTreeEntry(new GUIContent(nameof(Selector))) { level = 1, userData = typeof(Selector) });
                entries.Add(new SearchTreeEntry(new GUIContent(nameof(WeightedRandomSelector))) { level = 1, userData = typeof(WeightedRandomSelector) });
                entries.Add(new SearchTreeEntry(new GUIContent(nameof(Sequencer))) { level = 1, userData = typeof(Sequencer) });
                entries.Add(new SearchTreeEntry(new GUIContent(nameof(SimpleParallel))) { level = 1, userData = typeof(SimpleParallel) });
                entries.Add(new SearchTreeEntry(new GUIContent(nameof(Repeater))) { level = 1, userData = typeof(Repeater) });
                entries.Add(new SearchTreeEntry(new GUIContent(nameof(LoopNode))) { level = 1, userData = typeof(LoopNode) });
                entries.Add(new SearchTreeEntry(new GUIContent(nameof(InverseDecorater))) { level = 1, userData = typeof(InverseDecorater) });
                //entries.Add(new SearchTreeEntry(new GUIContent(nameof(LoopConditionNode))) { level = 1, userData = typeof(LoopConditionNode) });
                entries.Add(new SearchTreeEntry(new GUIContent(nameof(LoopConditionNode))) { level = 1, userData = typeof(LoopConditionNode) });
                entries.Add(new SearchTreeEntry(new GUIContent(nameof(ActionNode))) { level = 1, userData = typeof(ActionNode) });
                entries.Add(new SearchTreeEntry(new GUIContent(nameof(FunctionNode))) { level = 1, userData = typeof(FunctionNode) });
                entries.Add(new SearchTreeEntry(new GUIContent(nameof(BlackboardNode))) { level = 1, userData = typeof(BlackboardNode) });

                return entries;
            }

            /// <summary>
            /// 選択されたときの処理
            /// </summary>
            /// <param name="searchTreeEntry"></param>
            /// <param name="context"></param>
            /// <returns></returns>
            public bool OnSelectEntry(SearchTreeEntry searchTreeEntry, SearchWindowContext context)
            {
                Type type = searchTreeEntry.userData as Type;
                BaseNode baseNode = Activator.CreateInstance(type) as BaseNode;

                Vector2 worldMousePosition = _editorWindow.rootVisualElement.ChangeCoordinatesTo(_editorWindow.rootVisualElement.parent, context.screenMousePosition - _editorWindow.position.position);
                Vector2 localMousePosition = _graphView.contentViewContainer.WorldToLocal(worldMousePosition);
                baseNode.SetPosition(new Rect(localMousePosition, new Vector2(100, 100)));

                baseNode.SetID(Guid.NewGuid().ToString("N"));

                _graphView.AddElement(baseNode);
                return true;
            }
        }

        public class SearchActionWindowProviderBase : ScriptableObject
        {
            public Action<Type> OnSelected;

            /// <summary>
            /// 選択されたときの処理
            /// </summary>
            /// <param name="searchTreeEntry"></param>
            /// <param name="context"></param>
            /// <returns></returns>
            public bool OnSelectEntry(SearchTreeEntry searchTreeEntry, SearchWindowContext context)
            {
                OnSelected(searchTreeEntry.userData as Type);
                return true;
            }

            protected void SetAction(List<SearchTreeEntry> entries, Type rootType, int level)
            {
                List<Type> typeList = System.Reflection.Assembly.GetAssembly(rootType).GetTypes().Where(x => x.IsSubclassOf(rootType)).ToList();
                foreach (Type type in typeList)
                {
                    if (entries.Find(x => x.content.text == type.Name) == null)
                    {
                        if (type.IsAbstract)
                        {
                            entries.Add(new SearchTreeGroupEntry(new GUIContent(type.Name)) { level = level });

                            SetAction(entries, type, level + 1);
                        }
                        else if (!type.IsAbstract)
                        {
                            // 日本語名を入れても良いかも
                            entries.Add(new SearchTreeEntry(new GUIContent(type.Name)) { level = level, userData = type });
                        }
                    }
                }
            }
        }

        public class SearchWindowProvider_ActionNode : SearchActionWindowProviderBase, ISearchWindowProvider
        {
            /// <summary>
            /// SearchTreeの内容
            /// </summary>
            /// <param name="context"></param>
            /// <returns></returns>
            public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
            {
                var entries = new List<SearchTreeEntry>();
                entries.Add(new SearchTreeGroupEntry(new GUIContent("Action")));

                SetAction(entries, typeof(ActionClass), 1);

                return entries;
            }
        }

        public class SearchWindowProvider_ConditionNode : SearchActionWindowProviderBase, ISearchWindowProvider
        {
            /// <summary>
            /// SearchTreeの内容
            /// </summary>
            /// <param name="context"></param>
            /// <returns></returns>
            public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
            {
                var entries = new List<SearchTreeEntry>();
                entries.Add(new SearchTreeGroupEntry(new GUIContent("Condition")));

                SetAction(entries, typeof(ConditionClass), 1);

                return entries;
            }
        }
    }

    public abstract class BaseNode : Node
    {
        public string ID { get; private set; }

        public virtual string GetParameterJSON() { return ""; }
        public virtual void SetParameterJSON(string parameterJSON) { }

        public void SetID(string ID)
        {
            this.ID = ID;
        }

        public void SetState(NodeState state)
        {
            switch (state)
            {
                case NodeState.Waiting:
                    this.titleContainer.style.backgroundColor = Color.black;
                    break;
                case NodeState.Running:
                    this.titleContainer.style.backgroundColor = Color.blue;
                    break;
                case NodeState.True:
                    this.titleContainer.style.backgroundColor = new Color(0, 0.5f, 0, 1);
                    break;
                case NodeState.False:
                    this.titleContainer.style.backgroundColor = Color.red;
                    break;
                case NodeState.Completed:
                    this.titleContainer.style.backgroundColor = new Color(0.75f, 0.25f, 0, 1);
                    break;
                case NodeState.Warning:
                    this.titleContainer.style.backgroundColor = new Color(0.5f, 0.5f, 0, 1);
                    break;
                default:
                    break;
            }
        }

        protected void CreateInputPort()
        {
            Port inputPort = Port.Create<Edge>(Orientation.Horizontal, Direction.Input, Port.Capacity.Single, typeof(BaseNode));
            inputPort.portName = "Node";
            this.inputContainer.Add(inputPort);
        }

        protected void CreateOutputPort(Port.Capacity outputCapacity)
        {
            Port outputPort = Port.Create<Edge>(Orientation.Horizontal, Direction.Output, outputCapacity, typeof(BaseNode));
            outputPort.portName = "Node";
            this.outputContainer.Add(outputPort);
        }
    }

    public class RootNode : BaseNode
    {
        public RootNode()
        {
            this.title = "Root";

            // 削除不可とする
            capabilities -= Capabilities.Deletable;
            capabilities -= Capabilities.Copiable;

            base.CreateOutputPort(Port.Capacity.Single);

            this.extensionContainer.Add(new BlackboardField());
        }
    }

    public class Selector : BaseNode
    {
        public Selector()
        {
            this.title = "Selector";
            base.CreateInputPort();
            base.CreateOutputPort(Port.Capacity.Multi);
        }
    }

    public class WeightedRandomSelector : BaseNode
    {
        IntegerField _weightNumField = new IntegerField("Num");
        List<FloatField> _weightFieldList = new List<FloatField>();

        public WeightedRandomSelector()
        {
            this.title = "WeightedRandomSelector";
            base.CreateInputPort();
            base.CreateOutputPort(Port.Capacity.Multi);

            extensionContainer.Add(_weightNumField);
            _weightNumField.RegisterCallback((ChangeEvent<int> value) =>
            {
                AdjustWeightFieldNum();
            });

            RefreshExpandedState();
        }

        // 一般化して省略できるはず
        public override string GetParameterJSON()
        {
            NodeParameter parameter = new NodeParameter();

            parameter.inputs_int = new int[] { _weightNumField.value };
            parameter.argument = new float[_weightFieldList.Count()];

            for (int i = 0; i < _weightFieldList.Count(); i++)
            {
                parameter.argument[i] = _weightFieldList[i].value;
            }

            return JsonUtility.ToJson(parameter);
        }

        public override void SetParameterJSON(string parameterJSON)
        {
            NodeParameter parameter = JsonUtility.FromJson<NodeParameter>(parameterJSON);

            _weightNumField.value = parameter.inputs_int[0];

            AdjustWeightFieldNum();

            for (int i = 0; i < parameter.argument.Length; i++)
            {
                _weightFieldList[i].value = parameter.argument[i];
            }
        }

        private void AdjustWeightFieldNum()
        {
            int current = _weightFieldList.Count();
            if (_weightFieldList.Count() < _weightNumField.value)
            {
                for (int i = 0; i < _weightNumField.value - current; i++)
                {
                    FloatField floatField = new FloatField("Weight");
                    _weightFieldList.Add(floatField);
                    extensionContainer.Add(floatField);
                }
            }
            else if (_weightFieldList.Count() > _weightNumField.value)
            {
                for (int i = 0; i < current - _weightNumField.value; i++)
                {
                    _weightFieldList.RemoveAt(_weightFieldList.Count() - 1);
                    extensionContainer.RemoveAt(extensionContainer.childCount - 1);
                }
            }
        }
    }

    public class Sequencer : BaseNode
    {
        public Sequencer()
        {
            this.title = "Sequencer";
            base.CreateInputPort();
            base.CreateOutputPort(Port.Capacity.Multi);
        }
    }

    public class SimpleParallel : BaseNode
    {
        public SimpleParallel()
        {
            this.title = "Simple Parallel";
            base.CreateInputPort();
            base.CreateOutputPort(Port.Capacity.Multi);
        }
    }

    public class Repeater : BaseNode
    {
        public Repeater()
        {
            this.title = "Repeater";
            base.CreateInputPort();
            base.CreateOutputPort(Port.Capacity.Single);
        }
    }

    public class LoopNode : BaseNode
    {
        public LoopNode()
        {
            this.title = "LoopNode";
            base.CreateInputPort();
            base.CreateOutputPort(Port.Capacity.Single);
        }
    }

    public class InverseDecorater : BaseNode
    {
        public InverseDecorater()
        {
            this.title = "InverseDecorater";
            base.CreateInputPort();
            base.CreateOutputPort(Port.Capacity.Single);
        }
    }

    /*public class LoopConditionNode : BaseNode
    {
        DropdownField _typeDropdownField;

        DropdownField _dropdownField;
        List<FloatField> _floatFieldList = new List<FloatField>();
        List<DropdownField> _inputList = new List<DropdownField>();
        DropdownField _output;

        public LoopConditionNode()
        {
            title = "LoopCondition";
            base.CreateInputPort();
            base.CreateOutputPort(Port.Capacity.Single);


            extensionContainer.Add(_typeDropdownField = new DropdownField("Type", ConditionType.ConditionTypeList, 0));

            _dropdownField = new DropdownField("Condition", ActionListManager.Instance.GetExplanationListOfCondition(), 0);
            _dropdownField.RegisterCallback((MouseDownEvent e) =>
            {
                UpdateExtensionContainer();
            });
            extensionContainer.Add(_dropdownField);

            // 更新
            RefreshExpandedState();

            UpdateExtensionContainer();
        }

        public override string GetParameterJSON()
        {
            NodeParameter parameter = new NodeParameter();

            parameter.conditionNodeType = _typeDropdownField.value;
            parameter.actionName = _dropdownField.value;
            parameter.argument = new float[_floatFieldList.Count()];
            parameter.inputNames = new string[_inputList.Count()];
            parameter.outputName = _output != null ? _output.value : null;

            for (int i = 0; i < parameter.argument.Length; i++)
            {
                parameter.argument[i] = _floatFieldList[i].value;
            }

            for (int i = 0; i < parameter.inputNames.Length; i++)
            {
                parameter.inputNames[i] = _inputList[i].value;
            }

            return JsonUtility.ToJson(parameter);
        }

        public override void SetParameterJSON(string parameterJSON)
        {
            NodeParameter parameter = JsonUtility.FromJson<NodeParameter>(parameterJSON);

            _typeDropdownField.value = parameter.conditionNodeType;
            _dropdownField.value = parameter.actionName;

            UpdateExtensionContainer();

            for (int i = 0; i < Math.Min(_floatFieldList.Count(), parameter.argument.Length); i++)
            {
                _floatFieldList[i].value = parameter.argument[i];
            }

            for (int i = 0; i < Math.Min(_inputList.Count(), parameter.inputNames.Length); i++)
            {
                _inputList[i].value = parameter.inputNames[i];
            }

            if (_output != null)
            {
                _output.value = parameter.outputName;
            }
        }

        private void TryToAddExtensionField()
        {
            List<string> choiceList = new List<string>();
            var children = BehaviourTreeEditorWindow.instance.behaviourTreeGraphView.blackboard.Children();
            int num = children.Count();
            if (num > 0)
            {
                choiceList = children.Select(x => (x as BlackboardField).text).ToList();
            }

            var argumentTypes = ActionListManager.Instance.GetArgumentTypeByName(_dropdownField.value);
            for (int i = 0; i < argumentTypes.Length; i++)
            {
                if (argumentTypes[i] == ActionListManager.DataType.FLOAT)
                {
                    _floatFieldList.Add(new FloatField("float"));
                    this.extensionContainer.Add(_floatFieldList.Last());
                }

                if (argumentTypes[i] == ActionListManager.DataType.FLOAT_BB ||
                    argumentTypes[i] == ActionListManager.DataType.GAMEOBJECT_BB ||
                    argumentTypes[i] == ActionListManager.DataType.VECTOR3_BB ||
                    argumentTypes[i] == ActionListManager.DataType.BOOL_BB)
                {
                    _inputList.Add(new DropdownField("Input", choiceList, 0));
                    this.extensionContainer.Add(_inputList.Last());
                }
            }

            var outputTypes = ActionListManager.Instance.GetOutputTypeByName(_dropdownField.value);
            if (outputTypes != ActionListManager.DataType.NOTHING)
            {
                //for (int i = 0; i < outputTypes.Length; i++)
                {
                    //if (outputTypes.Length > 0)
                    {
                        _output = new DropdownField("Output", choiceList, 0);
                        this.extensionContainer.Add(_output);
                    }
                }
            }

            RefreshExpandedState();
        }

        private void UpdateExtensionContainer()
        {
            // 削除
            extensionContainer.Clear();

            extensionContainer.Add(_typeDropdownField);
            extensionContainer.Add(_dropdownField);

            _floatFieldList.Clear();
            _inputList.Clear();
            _output = null;

            TryToAddExtensionField();
        }
    }*/
    /*public class ConditionNode : BaseNode
    {
        DropdownField _conditionField;

        public ConditionNode()
        {
            title = "Condition";
            base.CreateInputPort();

            _conditionField = new DropdownField("Condition", ActionListManager.Instance.GetExplanationListOfCondition(), 0);
            extensionContainer.Add(_conditionField);
            RefreshExpandedState();
        }

        public override string GetParameterJSON()
        {
            NodeParameter parameter = new NodeParameter();
            parameter.actionName = _conditionField.value;
            return JsonUtility.ToJson(parameter);
        }

        public override void SetParameterJSON(string parameterJSON)
        {
            NodeParameter parameter = JsonUtility.FromJson<NodeParameter>(parameterJSON);

            _conditionField.value = parameter.actionName;
        }
    }*/

    public class ActionNodeBase : BaseNode
    {
        protected Label _pushableLabel;

        protected List<ArgumentData> _argumentDataList = new List<ArgumentData>();

        // BlackBoard
        protected List<DropdownField> _inputList = new List<DropdownField>();
        protected DropdownField _output;

        
        protected string _actionClassName;
        

        protected void TryToAddExtensionField()
        {
            List<string> choiceList = new List<string>();
            var children = BehaviourTreeEditorWindow.instance.behaviourTreeGraphView.blackboard.Children();
            if (children.Count() > 0)
            {
                choiceList = children.Select(x => (x as BlackboardField).text).ToList();
            }

            Type type = Type.GetType($"{_actionClassName}, Assembly-CSharp");


            var fieldInfos = type.GetFields(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            // ArgumentDataListの初期化
            int argumentNum = _argumentDataList.Count();
            int inputFieldNum = fieldInfos.Where(x => x.FieldType.BaseType == typeof(InputBase)).Count();
            for (int i = argumentNum; i < inputFieldNum; i++)
            {
                _argumentDataList.Add(new ArgumentData());
            }

            // フィールドの設置
            int fieldCount = 0;
            foreach (var field in fieldInfos)
            {
                // Output
                if (field.FieldType.BaseType == typeof(OutputBase))
                {
                    _output = new DropdownField("Output", choiceList, 0);
                    this.extensionContainer.Add(_output);

                    continue;
                }

                if (fieldCount >= _argumentDataList.Count())
                {
                    continue;
                }

                FieldWithBb fieldWithBb = new();
                ArgumentData argumentData = _argumentDataList[fieldCount];

                // データ型毎にフィールドを追加
                if (field.FieldType == typeof(BT_Input<int>))
                {
                    IntegerField integerField = new IntegerField();
                    integerField.value = argumentData.value == "" ? 0 : int.Parse(argumentData.value);
                    integerField.RegisterCallback<ChangeEvent<int>>((ChangeEvent<int> callback) =>
                    {
                        argumentData.value = integerField.value.ToString();
                    });

                    fieldWithBb.AddField(integerField);

                    argumentData.value = integerField.value.ToString();
                }
                else if (field.FieldType == typeof(BT_Input<float>))
                {
                    FloatField floatField = new FloatField();
                    floatField.value = argumentData.value == "" ? 0 : float.Parse(argumentData.value);
                    floatField.RegisterCallback((ChangeEvent<float> callback) =>
                    {
                        argumentData.value = floatField.value.ToString();
                    });

                    fieldWithBb.AddField(floatField);

                    argumentData.value = floatField.value.ToString();
                }
                else if (field.FieldType == typeof(BT_Input<bool>))
                {
                    DropdownField boolField = new DropdownField(new List<string>() { "false", "true" }, argumentData.value == "" ? 0 : int.Parse(argumentData.value));
                    boolField.RegisterCallback((ChangeEvent<string> callback) =>
                    {
                        argumentData.value = (boolField.value == "true" ? "1" : "0");
                    });

                    fieldWithBb.AddField(boolField);

                    argumentData.value = (boolField.value == "true" ? "1" : "0");
                }
                else if (field.FieldType.BaseType == typeof(InputBase))
                {
                    ObjectField objectField = new ObjectField(field.Name);
                    objectField.value = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(argumentData.value);
                    objectField.RegisterCallback((ChangeEvent<UnityEngine.Object> callback) =>
                    {
                        argumentData.value = AssetDatabase.GetAssetPath(objectField.value);
                    });

                    this.extensionContainer.Add(objectField);

                    continue;
                }
                /*else if (field.FieldType.BaseType == typeof(InputBase))
                {
                    DropdownField blackboardDdF = new DropdownField(field.Name, choiceList, 0);
                    blackboardDdF.RegisterCallback((ChangeEvent<string> callback) =>
                    {
                        argumentData.variableName = blackboardDdF.value;
                    });
                    argumentData.variableName = blackboardDdF.value;
                    argumentData.isDynamic = true;

                    this.extensionContainer.Add(blackboardDdF);

                    continue;
                }*/
                else
                {
                    continue;
                }

                // ブラックボード付きのフィールドを設定
                //if (field.FieldType.BaseType == typeof(InputBase))
                fieldWithBb.label = field.Name;
                fieldWithBb.SetBbVariableList(choiceList, 0, () =>
                {
                    argumentData.variableName = fieldWithBb.VariableName;
                    argumentData.isDynamic = fieldWithBb.BlackboardIndex > 0;
                });

                // ブラックボード変数をセット
                if (argumentData.isDynamic)
                {
                    fieldWithBb.VariableName = argumentData.variableName;
                }

                this.extensionContainer.Add(fieldWithBb);



                fieldCount++;
            }

            RefreshExpandedState();
        }
        

        protected class ArgumentData
        {
            public string value { get; set; } = "";
            public string variableName { get; set; } = "";
            public bool isDynamic { get; set; }
        }
    }

    public class LoopConditionNode : ActionNodeBase
    {
        BehaviourTreeEditorWindow.SearchWindowProvider_ConditionNode _searchWindowProvider;
        DropdownField _typeDropdownField;

        public LoopConditionNode()
        {
            title = "LoopCondition";
            base.CreateInputPort();
            base.CreateOutputPort(Port.Capacity.Single);

            extensionContainer.Add(_typeDropdownField = new DropdownField("Type", ConditionType.ConditionTypeList, 0));

            _pushableLabel = new Label("select...");
            _pushableLabel.style.backgroundColor = new Color(88f / 255, 88f / 255, 88f / 255, 1);
            extensionContainer.Add(_pushableLabel);

            // アクションラベルクリック時のコールバック
            _pushableLabel.RegisterCallback<MouseDownEvent>((MouseDownEvent e) =>
            {
                // 結局windowの位置を取得しないといけない
                SearchWindow.Open(new SearchWindowContext(BehaviourTreeEditorWindow.instance.position.position + e.mousePosition), _searchWindowProvider);
            });

            // アクション選択時のコールバック
            _searchWindowProvider = ScriptableObject.CreateInstance<BehaviourTreeEditorWindow.SearchWindowProvider_ConditionNode>();
            _searchWindowProvider.OnSelected = (Type type) =>
            {
                _pushableLabel.text = type.Name;
                _actionClassName = type.FullName;
                UpdateExtensionContainer();
            };

            this.extensionContainer.Add(_pushableLabel);
            RefreshExpandedState();
        }

        public override string GetParameterJSON()
        {
            NodeParameter parameter = new NodeParameter();
            parameter.conditionNodeType = _typeDropdownField.value;
            parameter.actionName = _pushableLabel.text;
            parameter.actionClassName = _actionClassName;
            parameter.inputs = new NodeParameter.ArgumentData[_argumentDataList.Count()];
            parameter.inputNames = new string[_inputList.Count()];
            parameter.outputName = _output != null ? _output.value : null;

            for (int i = 0; i < _argumentDataList.Count(); i++)
            {
                parameter.inputs[i] = new NodeParameter.ArgumentData();
                parameter.inputs[i].value = _argumentDataList[i].value;
                parameter.inputs[i].variableName = _argumentDataList[i].variableName;
                parameter.inputs[i].isDynamic = _argumentDataList[i].isDynamic;
            }

            void MakeParameterArray<T>(T[] parameterArray, List<T> valueList)
            {
                for (int i = 0; i < parameterArray.Length; i++)
                {
                    parameterArray[i] = valueList[i];
                }
            }

            MakeParameterArray<string>(parameter.inputNames, _inputList.Select(x => x.value).ToList());

            return JsonUtility.ToJson(parameter);
        }

        public override void SetParameterJSON(string parameterJSON)
        {
            NodeParameter parameter = JsonUtility.FromJson<NodeParameter>(parameterJSON);
            _typeDropdownField.value = parameter.conditionNodeType;
            _pushableLabel.text = parameter.actionName;
            _actionClassName = parameter.actionClassName;

            for (int i = 0; i < parameter.inputs.Length; i++)
            {
                _argumentDataList.Add(new ArgumentData()
                {
                    value = parameter.inputs[i].value,
                    variableName = parameter.inputs[i].variableName,
                    isDynamic = parameter.inputs[i].isDynamic
                });
            }

            // アクションが選択されていないなら、これ以上のデータはロードできない
            if (_actionClassName == "") return;

            // フィールド部分の設置
            UpdateExtensionContainer();

            for (int i = 0; i < _inputList.Count(); i++)
            {
                _inputList[i].value = parameter.inputNames[i];
            }

            if (_output != null)
            {
                _output.value = parameter.outputName;
            }
        }

        private void UpdateExtensionContainer()
        {
            // 削除
            extensionContainer.Clear();

            extensionContainer.Add(_typeDropdownField);

            CompositField compositField = new CompositField("Action");
            compositField.AddField(_pushableLabel);
            extensionContainer.Add(compositField);

            _inputList.Clear();
            _output = null;

            TryToAddExtensionField();
        }
    }

    public class ActionNode : ActionNodeBase
    {
        BehaviourTreeEditorWindow.SearchWindowProvider_ActionNode _searchActionWindowProvider;

        public ActionNode()
        {
            title = "Action";
            base.CreateInputPort();

            _pushableLabel = new Label("select...");
            _pushableLabel.style.backgroundColor = new Color(88f / 255, 88f / 255, 88f / 255, 1);
            extensionContainer.Add(_pushableLabel);

            // アクションラベルクリック時のコールバック
            _pushableLabel.RegisterCallback<MouseDownEvent>((MouseDownEvent e) =>
            {
                // 結局windowの位置を取得しないといけない
                SearchWindow.Open(new SearchWindowContext(BehaviourTreeEditorWindow.instance.position.position + e.mousePosition), _searchActionWindowProvider);
            });

            // アクション選択時のコールバック
            _searchActionWindowProvider = ScriptableObject.CreateInstance<BehaviourTreeEditorWindow.SearchWindowProvider_ActionNode>();
            _searchActionWindowProvider.OnSelected = (Type type) =>
            {
                _pushableLabel.text = type.Name;
                _actionClassName = type.FullName;
                UpdateExtensionContainer();
            };

            this.extensionContainer.Add(_pushableLabel);
            RefreshExpandedState();
        }

        public override string GetParameterJSON()
        {
            NodeParameter parameter = new NodeParameter();
            parameter.actionName = _pushableLabel.text;
            parameter.actionClassName = _actionClassName;
            parameter.inputs = new NodeParameter.ArgumentData[_argumentDataList.Count()];
            parameter.inputNames = new string[_inputList.Count()];
            parameter.outputName = _output != null ? _output.value : null;

            for (int i = 0; i < _argumentDataList.Count(); i++)
            {
                parameter.inputs[i] = new NodeParameter.ArgumentData();
                parameter.inputs[i].value = _argumentDataList[i].value;
                parameter.inputs[i].variableName = _argumentDataList[i].variableName;
                parameter.inputs[i].isDynamic = _argumentDataList[i].isDynamic;
            }

            void MakeParameterArray<T>(T[] parameterArray, List<T> valueList)
            {
                for (int i = 0; i < parameterArray.Length; i++)
                {
                    parameterArray[i] = valueList[i];
                }
            }

            MakeParameterArray<string>(parameter.inputNames, _inputList.Select(x => x.value).ToList());
            return JsonUtility.ToJson(parameter);
        }

        public override void SetParameterJSON(string parameterJSON)
        {
            NodeParameter parameter = JsonUtility.FromJson<NodeParameter>(parameterJSON);
            _pushableLabel.text = parameter.actionName;
            _actionClassName = parameter.actionClassName;

            for (int i = 0; i < parameter.inputs.Length; i++)
            {
                _argumentDataList.Add(new ArgumentData()
                {
                    value = parameter.inputs[i].value,
                    variableName = parameter.inputs[i].variableName,
                    isDynamic = parameter.inputs[i].isDynamic
                });
            }

            // アクションが選択されていないなら、これ以上のデータはロードできない
            if (_actionClassName == "") return;

            // フィールド部分の設置
            UpdateExtensionContainer();

            for (int i = 0; i < _inputList.Count(); i++)
            {
                _inputList[i].value = parameter.inputNames[i];
            }

            if (_output != null)
            {
                _output.value = parameter.outputName;
            }
        }

        private void UpdateExtensionContainer()
        {
            // 削除
            extensionContainer.Clear();


            CompositField compositField = new CompositField("Action");
            compositField.AddField(_pushableLabel);
            extensionContainer.Add(compositField);

            _inputList.Clear();
            _output = null;

            TryToAddExtensionField();
        }
    }

    public class FunctionNode : BaseNode
    {
        DropdownField _functionsDD;

        public FunctionNode()
        {
            base.title = "Function";
            base.CreateInputPort();

            _functionsDD = new DropdownField("Function", ActionListManager.Instance.GetFunctionNameList(), 0);
            extensionContainer.Add(_functionsDD);
            RefreshExpandedState();
        }

        public override string GetParameterJSON()
        {
            NodeParameter parameter = new NodeParameter();
            parameter.actionName = _functionsDD.value;
            return JsonUtility.ToJson(parameter);
        }

        public override void SetParameterJSON(string parameterJSON)
        {
            NodeParameter parameter = JsonUtility.FromJson<NodeParameter>(parameterJSON);

            _functionsDD.value = parameter.actionName;
        }
    }

    public class BlackboardNode : BaseNode
    {
        DropdownField _variableDF;

        public BlackboardNode()
        {
            base.title = "Blackboard";
            base.CreateOutputPort(Port.Capacity.Multi);

            List<string> choiceList = new List<string>();
            var children = BehaviourTreeEditorWindow.instance.behaviourTreeGraphView.blackboard.Children();
            if (children.Count() > 0)
            {
                choiceList = children.Select(x => (x as BlackboardField).text).ToList();
            }

            _variableDF = new DropdownField(choiceList, 0);
            this.extensionContainer.Add(_variableDF);
            RefreshExpandedState();
        }

        public override string GetParameterJSON()
        {
            NodeParameter parameter = new();
            parameter.actionName = _variableDF.value;
            return JsonUtility.ToJson(parameter);
        }

        public override void SetParameterJSON(string parameterJSON)
        {
            NodeParameter parameter = JsonUtility.FromJson<NodeParameter>(parameterJSON);

            _variableDF.value = parameter.actionName;
        }
    }


    public class CompositField : VisualElement
    {
        public string label
        {
            get
            {
                return label;
            }
            set
            {
                labelGUI.text = value;
            }
        }

        public int _fieldLimit { get; set; } = 2;

        protected List<BindableElement> _fieldList = new List<BindableElement>();
        Label labelGUI = new Label();


        public CompositField()
        {
            Initialize();
        }

        public CompositField(string label)
        {
            Initialize();
            this.label = label;
        }

        public void AddField(BindableElement field)
        {
            if (_fieldList.Count() >= _fieldLimit)
            {
                return;
            }

            _fieldList.Add(field);

            field.style.flexGrow = 0;
            this.Add(field);
        }

        private void Initialize()
        {
            this.style.marginLeft = 2.5f;
            this.style.flexDirection = FlexDirection.Row;

            labelGUI.style.flexGrow = 1;
            this.Add(labelGUI);
        }
    }

    public class FieldWithBb : CompositField
    {
        public string VariableName
        {
            get
            {
                return _blackboardDdF.value;
            }
            set
            {
                _blackboardDdF.value = value;
                TryDisableField();
            }
        }

        public int BlackboardIndex
        {
            get
            {
                return _blackboardDdF.index;
            }
        }

        List<string> _BbVariableList;
        DropdownField _blackboardDdF;

        public void SetBbVariableList(List<string> BbVariableList, int defaultIndex, Action changeEvent = null)
        {
            _BbVariableList = new List<string>(BbVariableList);

            _BbVariableList.Insert(0, "select...");
            _blackboardDdF = new DropdownField(_BbVariableList, defaultIndex);

            _blackboardDdF.RegisterCallback((ChangeEvent<string> callback) =>
            {
                TryDisableField();

                changeEvent?.Invoke();
            });
            
            this.AddField(_blackboardDdF);
        }

        private void TryDisableField()
        {
            if (_blackboardDdF.index > 0)
            {
                _fieldList[0].SetEnabled(false);
            }
            else
            {
                _fieldList[0].SetEnabled(true);
            }
        }
    }
}