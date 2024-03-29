#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEditorInternal;
using UnityEngine;

namespace Yorozu.DB
{
    /// <summary>
    /// 定義情報をいじる用
    /// </summary>
    [Serializable]
    internal class DefineEditModule : YorozuDBEditorModule
    {
        private static class Style
        {
            internal static GUIStyle DeleteStyle;
            internal static GUILayoutOption EnumWidth;
            internal static GUILayoutOption ButtonWidth;
            internal static GUIContent Delete;
            internal static GUIContent Key;
            internal static GUIContent UnKey;

            internal const float RemoveButtonWidth = 16f;

            static Style()
            {
                EnumWidth = GUILayout.Width(300);
                ButtonWidth = GUILayout.Width(100);
                Delete = EditorGUIUtility.TrIconContent("Toolbar Minus");
                Key = EditorGUIUtility.TrIconContent("Favorite On Icon");
                UnKey = EditorGUIUtility.TrIconContent("TestIgnored");
                DeleteStyle = "RL FooterButton";
            }
        }

        [SerializeField]
        private YorozuDBDataDefineObject _data;

        [SerializeField]
        private YorozuDBEnumDataObject _enumData;

        [SerializeField]
        private string[] _enums;

        [SerializeField]
        private string[] _defineClasses;

        [NonSerialized]
        private string _name;

        [NonSerialized]
        private DataType _dataType;

        [NonSerialized]
        private string _targetName;

        [NonSerialized]
        private bool _repaint;

        [NonSerialized]
        private List<int> _addFieldIds = new List<int>();

        private int _renameID = -1;
        private string _temp;

        private static readonly string EditorField = "EditorField";

        private ReorderableList _reorderableList;
        private ReorderableList _extendReorderableList;

        internal void SetData(YorozuDBDataDefineObject data)
        {
            _data = data;
            _enumData = YorozuDBEditorInternalUtility.LoadEnumDataAsset();
            _reorderableList = null;
            _extendReorderableList = null;
            _addFieldIds.Clear();
            // 存在するScriptableObjectを取得
        }

        internal override bool OnGUI()
        {
            if (_data == null)
                return false;

            _reorderableList ??= CreateReorderableList(_data);
            _extendReorderableList ??= CreateExtendReorderableList(_data);

            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                EditorGUILayout.LabelField($"Define Editor: 【{_data.name}】", EditorStyles.boldLabel);
            }

            GUILayout.Space(10);

            EditorGUILayout.LabelField("Add Field", EditorStyles.boldLabel);
            using (new EditorGUILayout.VerticalScope("box"))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    _name = EditorGUILayout.TextField("Name", _name);
                    using (var check = new EditorGUI.ChangeCheckScope())
                    {
                        _dataType = (DataType) EditorGUILayout.EnumPopup("DataType", _dataType);
                        if (check.changed &&
                            (_dataType == DataType.Enum || _dataType == DataType.Flags))
                        {
                            var enumData = YorozuDBEditorInternalUtility.LoadEnumDataAsset();
                            if (enumData != null)
                            {
                                _enums = enumData.Defines
                                    .Where(d => d.Flags == (_dataType == DataType.Flags) || _dataType == DataType.Enum)
                                    .Select(d => d.Name)
                                    .ToArray();
                                _targetName = _enums.Length > 0 ? _enums[0] : "";
                            }
                        }
                        else if (check.changed &&
                                 _dataType == DataType.DBClass)
                        {
                            _defineClasses = YorozuDBEditorInternalUtility.LoadAllDefineAsset()
                                .Where(d => d.KeyField != null)
                                .Select(d => d.ClassName)
                                .ToArray();
                            _targetName = _defineClasses.Length > 0 ? _defineClasses[0] : "";
                        }
                    }
                }

                using (new EditorGUILayout.HorizontalScope())
                {
                    // enumなら候補を表示
                    if ((_dataType == DataType.Enum || _dataType == DataType.Flags) &&
                        _enums != null &&
                        _enums.Length > 0)
                    {
                        var index = Array.IndexOf(_enums, _targetName);
                        using (var check = new EditorGUI.ChangeCheckScope())
                        {
                            index = EditorGUILayout.Popup("Enum Select", index, _enums, Style.EnumWidth);
                            if (check.changed)
                            {
                                _targetName = _enums[index];
                            }
                        }
                    }
                    // 内部定義データの場合
                    else if (_dataType == DataType.DBClass)
                    {
                        var index = Array.IndexOf(_defineClasses, _targetName);
                        using (var check = new EditorGUI.ChangeCheckScope())
                        {
                            index = EditorGUILayout.Popup("Class Select", index, _defineClasses, Style.EnumWidth);
                            if (check.changed)
                            {
                                _targetName = _defineClasses[index];
                            }
                        }
                    }

                    GUILayout.FlexibleSpace();

                    using (new EditorGUI.DisabledScope(
                               string.IsNullOrEmpty(_name) ||
                               (_dataType == DataType.Enum && (_enums == null || _enums.Length <= 0)) ||
                               (_dataType == DataType.Enum && _enumData == null) ||
                               (_dataType == DataType.Flags && (_enums == null || _enums.Length <= 0)) ||
                               (_dataType == DataType.Flags && _enumData == null) ||
                               (_dataType == DataType.DBClass &&
                                (_defineClasses == null || _defineClasses.Length <= 0) &&
                                string.IsNullOrEmpty(_targetName))
                           ))
                    {
                        if (GUILayout.Button("Add", Style.ButtonWidth))
                        {
                            var id = _data.AddField(_name, _dataType, _targetName);
                            _name = "";
                            _dataType = default;
                            if (id >= 0)
                                _addFieldIds.Add(id);
                        }
                    }
                }
            }

            _reorderableList.DoLayoutList();

            GUILayout.Space(20);

            // 拡張フィールド
            EditorGUILayout.LabelField("Extend Fields",
                string.IsNullOrEmpty(_data.ExtendFieldsTypeName) ? "None" : _data.ExtendFieldsTypeName,
                EditorStyles.boldLabel);
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Set None"))
                {
                    _data.SetExtendFieldsTypeName("");
                    _extendReorderableList = CreateExtendReorderableList(_data);
                    // 更新したら関連データを初期化する
                    var data = YorozuDBEditorInternalUtility.LoadAllDataAsset(_data);
                    foreach (var d in data)
                    {
                        d.ExtendFieldsObject = null;
                    }
                }

                var content = new GUIContent("Change Extend Type");
                var rect = GUILayoutUtility.GetRect(content, GUI.skin.button);
                if (GUI.Button(rect, content))
                {
                    new TypeDropdown(rect, fullname =>
                    {
                        _data.SetExtendFieldsTypeName(fullname);
                        _extendReorderableList = CreateExtendReorderableList(_data);
                        // 更新したら関連データを初期化する
                        var data = YorozuDBEditorInternalUtility.LoadAllDataAsset(_data);
                        foreach (var d in data)
                        {
                            d.ExtendFieldsObject = null;
                        }
                    });
                }
            }

            if (_extendReorderableList.count > 0)
            {
                using (new EditorGUI.DisabledScope(true))
                {
                    _extendReorderableList.DoLayoutList();
                }
            }

            if (_repaint)
            {
                _repaint = false;
                return true;
            }

            return false;
        }

        private ReorderableList CreateReorderableList(YorozuDBDataDefineObject data)
        {
            return new ReorderableList(data.Fields, typeof(DataField), true, true, false, false)
            {
                drawHeaderCallback = rect =>
                {
                    EditorGUI.LabelField(rect, "Fields", EditorStyles.boldLabel);
                },
                elementHeightCallback = index =>
                {
                    if (data.Fields.Count <= index)
                        return DataContainer.ArrayControlWidth;

                    var filed = data.Fields[index];
                    return Mathf.Max(filed.DefaultValue.GetSize(filed.DataType), 1) * DataContainer.ArrayControlWidth;
                },
                drawElementCallback = (rect, index, isActive, isFocused) =>
                {
                    if (data.Fields.Count <= index)
                        return;

                    var width = rect.width;
                    var x = rect.x;
                    var field = data.Fields[index];
                    rect.width = 20;
                    var isKey = _data.IsKeyField(field);
                    // Key として有効なのであればボタンを表示
                    if (field.ValidKey())
                    {
                        var content = isKey ? Style.Key : Style.UnKey;
                        if (GUI.Button(rect, content, EditorStyles.label))
                        {
                            field.IsArray = false;
                            _data.SetKey(field.ID);
                        }
                    }

                    rect.x += rect.width;

                    rect.width = 130;
                    // リネーム状態だったら TextField を表示
                    if (_renameID == field.ID)
                    {
                        GUI.SetNextControlName(EditorField);
                        _temp = GUI.TextField(rect, _temp);
                        var e = Event.current;
                        if (e.keyCode == KeyCode.Return &&
                            _renameID != -1)
                        {
                            _data.RenameField(_renameID, _temp);
                            _renameID = -1;
                            _repaint = true;
                        }

                        if (e.keyCode == KeyCode.Escape)
                        {
                            _renameID = -1;
                            _repaint = true;
                        }

                        rect.x -= EditorGUI.indentLevel * 15f;
                    }
                    else
                    {
                        if (GUI.Button(rect, field.Name, EditorStyles.label))
                        {
                            _renameID = field.ID;

                            _temp = field.Name;
                            GUI.FocusControl(EditorField);
                            _repaint = true;
                        }
                    }

                    rect.x += rect.width;

                    // 型情報
                    rect.width = 140;
                    using (new EditorGUI.DisabledScope(true))
                    {
                        if (_enumData != null &&
                            (field.DataType == DataType.Enum || field.DataType == DataType.Flags))
                        {
                            var enumIndex = _enumData.Defines.FindIndex(d => d.ID == field.EnumDefineId);
                            if (enumIndex >= 0)
                            {
                                EditorGUI.LabelField(rect, $"{field.DataType} ({_enumData.Defines[enumIndex].Name})",
                                    EditorStyles.popup);
                            }
                        }
                        else if (field.DataType == DataType.DBClass)
                        {
                            EditorGUI.LabelField(rect,
                                field.ReferenceDefine != null ?
                                    $"{field.DataType} ({field.ReferenceDefine.ClassName})" :
                                    $"{field.DataType} (Not Exist)", EditorStyles.popup);
                        }
                        else
                        {
                            EditorGUI.LabelField(rect, field.DataType.ToString(), EditorStyles.popup);
                        }
                    }

                    rect.x += rect.width + EditorGUIUtility.standardVerticalSpacing;

                    // 配列とするか
                    rect.width = 50;
                    using (new EditorGUI.DisabledScope(isKey))
                    {
                        field.IsArray = EditorGUI.ToggleLeft(rect, "Array", field.IsArray);
                    }

                    rect.x += rect.width + EditorGUIUtility.standardVerticalSpacing;

                    rect.width = 200;
                    using (var check = new EditorGUI.ChangeCheckScope())
                    {
                        field.DefaultValue.DrawField(rect, field, GUIContent.none, _enumData);
                        if (check.changed)
                        {
                            // 直近で追加したフィールドなら初期値を反映
                            if (_addFieldIds.Contains(field.ID))
                            {
                                _data.UpdateDefaultValue(field.ID, field.DefaultValue);
                            }
                        }
                    }

                    rect.x += rect.width + EditorGUIUtility.standardVerticalSpacing * 2;
                    rect.width = width - rect.x + rect.width + 16;
                    field.Memo = EditorGUI.TextField(rect, GUIContent.none, field.Memo);
                    if (string.IsNullOrEmpty(field.Memo))
                    {
                        EditorGUI.LabelField(rect, "memo", EditorStyles.centeredGreyMiniLabel);
                    }

                    rect.x = Mathf.Max(x + width - Style.RemoveButtonWidth,
                        rect.x + rect.width + EditorGUIUtility.standardVerticalSpacing);

                    rect.width = Style.RemoveButtonWidth;
                    if (GUI.Button(rect, Style.Delete, Style.DeleteStyle))
                    {
                        if (EditorUtility.DisplayDialog("Warning", $"Delete {field.Name} Field?",
                                "YES",
                                "NO"))
                        {
                            _data.RemoveField(field.ID);
                            _repaint = true;
                        }
                    }
                },

                drawFooterCallback = rect =>
                {
                },
                footerHeight = 0f,
            };
        }

        private ReorderableList CreateExtendReorderableList(YorozuDBDataDefineObject data)
        {
            var type = data.ExtendFieldsType;
            if (type == null)
                return new ReorderableList(new List<int>(), typeof(int));

            var fields = YorozuDBExtendUtility.FindFields(type);
            if (fields.Count <= 0)
                return new ReorderableList(new List<int>(), typeof(int));

            return new ReorderableList(fields, typeof(DataField), true, true, false, false)
            {
                headerHeight = 0f,
                drawElementCallback = (rect, index, isActive, isFocused) =>
                {
                    if (fields.Count <= index)
                        return;

                    rect.x += 20;
                    rect.width = 130;
                    EditorGUI.LabelField(rect, fields[index].Name);

                    rect.x += rect.width;
                    rect.width = 140;
                    rect.y += 2;
                    using (new EditorGUI.DisabledScope(true))
                    {
                        EditorGUI.LabelField(rect, fields[index].FieldType.GetArrayType().ConvertGenerateString(false),
                            EditorStyles.popup);
                    }
                },

                drawFooterCallback = rect =>
                {
                },
                footerHeight = 0f,
            };
        }
    }


    internal class TypeDropdown : AdvancedDropdown
    {
        private event Action<string> _select;

        public TypeDropdown(Rect rect, Action<string> action) : base(new AdvancedDropdownState())
        {
            _select = action;
            Show(rect);
        }

        protected override AdvancedDropdownItem BuildRoot()
        {
            var root = new AdvancedDropdownItem("ScriptableObjectTypes");
            foreach (var typeName in Types())
            {
                var item = new AdvancedDropdownItem(typeName);
                root.AddChild(item);
            }

            return root;
        }

        protected override void ItemSelected(AdvancedDropdownItem item)
        {
            _select?.Invoke(item.name);
        }

        private IEnumerable<string> Types()
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(a => a.GetTypes())
                    .Where(t => t.IsSubclassOf(typeof(ScriptableObject)) &&
                                !t.IsSubclassOf(typeof(EditorWindow)) &&
                                !t.IsSubclassOf(typeof(ScriptableSingleton<>)) &&
                                !t.IsSubclassOf(typeof(UnityEditor.EditorTools.EditorTool)) &&
                                !t.IsSubclassOf(typeof(Editor)) &&
                                t != typeof(EditorWindow) &&
                                t != typeof(Editor) &&
                                !(!string.IsNullOrEmpty(t.Namespace) && t.Namespace.StartsWith("UnityEditor")) &&
                                !(!string.IsNullOrEmpty(t.Namespace) && t.Namespace.StartsWith("UnityEngine")) &&
                                !(!string.IsNullOrEmpty(t.Namespace) && t.Namespace.StartsWith("Yorozu.DB")) &&
                                !(!string.IsNullOrEmpty(t.Namespace) && t.Namespace.StartsWith("Packages.Rider")) &&
                                !(!string.IsNullOrEmpty(t.Namespace) && t.Namespace.StartsWith("TMPro")) &&
                                !t.IsAbstract &&
                                !t.IsGenericType
                    )
                    .Select(t => t.FullName)
                    .OrderBy(t => t)
                ;
        }
    }
}

#endif