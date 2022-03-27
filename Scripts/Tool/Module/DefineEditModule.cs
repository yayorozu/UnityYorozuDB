#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
        private ReorderableList _extendReorderableList;
        private int _relativeDataCount;
        
        internal void SetData(YorozuDBDataDefineObject data)
        {
            _data = data;
            _enumData = YorozuDBEditorUtility.LoadEnumDataAsset();
            _reorderableList = null;
            _extendReorderableList = null;
            var dataAssets= YorozuDBEditorUtility.LoadAllDataAsset(_data);
            _relativeDataCount = dataAssets.Length;
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
                    
                    using (new EditorGUI.DisabledScope(
                               string.IsNullOrEmpty(_name) || 
                               (_dataType == DataType.Enum && (_enums == null || _enums.Length <= 0)) ||
                               (_dataType == DataType.Enum && _enumData == null)
                        ))
                    {
                        if (GUILayout.Button("Add", Style.ButtonWidth))
                        {
                            _data.AddField(_name, _dataType, _enumName);
                            _name = "";
                            _dataType = default;
                        }
                    }
                }
            }

            _reorderableList.DoLayoutList();
            
            GUILayout.Space(20);
            // 複数データがあれば許可しない\
            using (new EditorGUI.DisabledScope(_relativeDataCount > 1))
            {
                if (_relativeDataCount > 1)
                {
                    EditorGUILayout.HelpBox("To Use The Extend, The Number Of Data Must Be Set To 1", MessageType.Info);
                }

                using (var check = new EditorGUI.ChangeCheckScope())
                {
                    _data.ExtendFieldsObject = (ScriptableObject) EditorGUILayout.ObjectField("Extend Fields", _data.ExtendFieldsObject, typeof(ScriptableObject), false);
                    if (check.changed)
                    {
                        _extendReorderableList = null;
                        GUIUtility.ExitGUI();
                    }
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
                    
                    rect.width = 130;
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
                        if (field.DataType == DataType.Enum && _enumData != null)
                        {
                            var enumIndex = _enumData.Defines.FindIndex(d => d.ID == field.EnumDefineId);
                            if (enumIndex >= 0)
                                EditorGUI.LabelField(rect, $"Enum ({_enumData.Defines[enumIndex].Name})", EditorStyles.popup);
                        }
                        else
                        {
                            EditorGUI.LabelField(rect, field.DataType.ToString(), EditorStyles.popup);
                        }
                    }
                    
                    rect.x += rect.width + EditorGUIUtility.standardVerticalSpacing;
                    
                    rect.width = 200;
                    field.DefaultValue.DrawField(rect, field, GUIContent.none, _enumData);
                    
                    rect.x = Mathf.Max(x + width - Style.RemoveButtonWidth, rect.x + rect.width + EditorGUIUtility.standardVerticalSpacing);

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

        private ReorderableList CreateExtendReorderableList(YorozuDBDataDefineObject data)
        {
            var fields = YorozuDBExtendUtility.FindFields(data.ExtendFieldsObject);
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
                        EditorGUI.LabelField(rect, fields[index].FieldType.GetArrayType().ConvertGenerateString(false), EditorStyles.popup);
                    }
                },
     
                drawFooterCallback = rect => { },
                footerHeight = 0f,
            };
        }
    }
}

#endif