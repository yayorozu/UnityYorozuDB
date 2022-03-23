using System;
using System.Linq;
using UnityEditor;
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

        [NonSerialized]
        private string _name;
        [NonSerialized]
        private DataType _dataType;
        [NonSerialized]
        private string _enumName;

        [NonSerialized]
        private bool _repaint;
        
        private int _renameID = -1;
        private string _temp;
        
        private static readonly string EditorField = "EditorField";

        private ReorderableList _reorderableList;
        
        internal void SetData(YorozuDBDataDefineObject data)
        {
            _data = data;
            _enumData = YorozuDBEditorUtility.LoadEnumDataAsset();
            _reorderableList = null;
        }

        internal override bool OnGUI()
        {
            if (_data == null)
                return false;

            if (_reorderableList == null)
            {
                _reorderableList = CreateReorderableList(_data);
            }

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
                        if (check.changed && _dataType == DataType.Enum)
                        {
                            var enumData = YorozuDBEditorUtility.LoadEnumDataAsset();
                            if (enumData != null)
                            {
                                _enums = enumData.Defines.Select(d => d.Name).ToArray();
                                _enumName = _enums.Length > 0 ? _enums[0] : "";
                            }
                        }
                    }
                }

                using (new EditorGUILayout.HorizontalScope())
                {
                    // enumなら候補を表示
                    if (_dataType == DataType.Enum && _enums != null && _enums.Length > 0)
                    {
                        var index = Array.IndexOf(_enums, _enumName);
                        using (var check = new EditorGUI.ChangeCheckScope())
                        {
                            index = EditorGUILayout.Popup("Enum Select", index, _enums, Style.EnumWidth);
                            if (check.changed)
                            {
                                _enumName = _enums[index];
                            }
                        }
                    }
                    
                    GUILayout.FlexibleSpace();
                    
                    using (new EditorGUI.DisabledScope(string.IsNullOrEmpty(_name) || (_dataType == DataType.Enum && _enums == null && string.IsNullOrEmpty(_enumName))))
                    {
                        if (GUILayout.Button("Add Field", Style.ButtonWidth))
                        {
                            _data.AddField(_name, _dataType, _enumName);
                            _name = "";
                            _dataType = default;
                        }
                    }
                }
            }

            _reorderableList.DoLayoutList();
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
                drawElementCallback = (rect, index, isActive, isFocused) =>
                {
                    if (data.Fields.Count <= index)
                        return;

                    var width = rect.width;
                    var x = rect.x;
                    var field = data.Fields[index];
                    rect.width = 20;
                    // Key として有効なのであればボタンを表示
                    if (field.ValidKey())
                    {
                        var content = _data.IsKeyField(field) ? Style.Key : Style.UnKey;
                        if (GUI.Button(rect, content, EditorStyles.label))
                        {
                            _data.SetKey(field.ID);
                        }
                    }

                    rect.x += rect.width;
                    
                    rect.width = 150;
                    // リネーム状態だったら TextField を表示
                    if (_renameID == field.ID)
                    {
                        GUI.SetNextControlName(EditorField);
                        _temp = GUI.TextField(rect, _temp);
                        var e = Event.current;
                        if (e.keyCode == KeyCode.Return && _renameID != -1)
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
                        field.DataType = (DataType) EditorGUI.EnumPopup(rect, GUIContent.none, field.DataType);
                    }
                    
                    rect.x += rect.width + EditorGUIUtility.standardVerticalSpacing;

                    // Enum だったらどのEnumかを表示 
                    if (field.DataType == DataType.Enum && _enumData != null)
                    {
                        var enumIndex = _enumData.Defines.FindIndex(d => d.ID == field.EnumDefineId);
                        if (enumIndex >= 0)
                            EditorGUI.LabelField(rect, _enumData.Defines[enumIndex].Name);
                    }
                    
                    rect.x = x + width - Style.RemoveButtonWidth;

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
     
                drawFooterCallback = rect => { },
                footerHeight = 0f,
            };
        }
    }
}