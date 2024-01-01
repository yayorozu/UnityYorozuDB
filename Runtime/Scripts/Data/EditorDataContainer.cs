#if UNITY_EDITOR

using System;
using UnityEditor;
using UnityEngine;
using Yorozu.DB.TreeView;

namespace Yorozu.DB
{
    internal partial class DataContainer
    {
        internal static float ArrayControlWidth = 18f;

        internal void Set(int value, int index) => _ints[index] = value;
        internal void Set(float value, int index) => _floats[index] = value;
        internal void Set(bool value, int index) => _bools[index] = value;
        internal void Set(string value, int index) => _strings[index] = value;
        internal void Set(UnityEngine.Object value, int index) => _unityObjects[index] = value;
        internal void Set(object value, int index) => _strings[index] = JsonUtility.ToJson(value);
        
        internal void Add(int value) => ArrayUtility.Add(ref _ints, value);
        internal void Add(float value) => ArrayUtility.Add(ref _floats, value);
        internal void Add(bool value) => ArrayUtility.Add(ref _bools, value);
        internal void Add(string value) => ArrayUtility.Add(ref _strings, value);
        internal void Add(UnityEngine.Object value) => ArrayUtility.Add(ref _unityObjects, value);
        internal void Add(object value) => ArrayUtility.Add(ref _strings, JsonUtility.ToJson(value));
        
        private static class Style
        {
            internal static GUIContent OpenButton;
            internal static GUIContent[] Vector2IntContents = new GUIContent[]
            {
                new GUIContent("x"),
                new GUIContent("y"),
            };
            
            internal static GUIContent[] Vector3IntContents = new GUIContent[]
            {
                new GUIContent("x"),
                new GUIContent("y"),
                new GUIContent("z"),
            };
            
            static Style()
            {
                OpenButton = EditorGUIUtility.TrIconContent("d_UnityEditor.InspectorWindow", "Open Properties");
            }
        }
        
        public DataContainer(){}

        public DataContainer(DataType dataType)
        {
            this.Initialize(dataType);
        }
        
        internal bool DrawField(Rect rect, DataField field, GUIContent content, YorozuDBEnumDataObject enumData)
        {
            if (!field.IsArray)
            {
                rect.y += rect.height / 2f - YorozuDBEditorDataTreeView.RowHeight / 2f;
                rect.height = YorozuDBEditorDataTreeView.RowHeight;
                DrawField(rect, field, content, enumData, 0);
                return false;
            }
            
            var width = rect.width;
            rect.width = ArrayControlWidth;
            if (GUI.Button(rect, "+"))
            {
                this.AddElement(field.DataType);
                return true;
            }
            rect.x += ArrayControlWidth;
            var startX = rect.x;
            rect.height = YorozuDBEditorDataTreeView.RowHeight;
            var size = this.GetSize(field.DataType);
            for (int i = 0; i < size; i++)
            {
                rect.x = startX;
                rect.width = width - ArrayControlWidth * 2;
                
                DrawField(rect, field, content, enumData, i);
                rect.x += rect.width;
                rect.width = ArrayControlWidth;
                if (GUI.Button(rect, "-"))
                {
                    this.RemoveAt(field.DataType, i);
                    GUI.FocusControl("");
                    return true;
                }
                rect.y += rect.height;
            }
            
            return false;
        }
        
        private void DrawField(Rect rect, DataField field, GUIContent content, YorozuDBEnumDataObject enumData, int index)
        {
            this.CheckSize(field.DataType, index);
            switch (field.DataType)
            {
                case DataType.String:
                    _strings[index] = EditorGUI.TextField(rect, content, _strings[index]);
                    break;
                case DataType.DBClass:
                    using (var check = new EditorGUI.ChangeCheckScope())
                    {
                        rect.width -= 26;
                        _strings[index] = EditorGUI.TextField(rect, content, _strings[index]);
                        rect.x += rect.width;
                        rect.width = 26;
                        if (GUI.Button(rect, "Go"))
                        {
                            var tuple = YorozuDBEditorInternalUtility.FindData(field.ReferenceDefine, _strings[index]);
                            if (tuple.Item1 != null)
                            {
                                YorozuDBEditorWindow.ShowData(tuple.Item1.GetInstanceID(), tuple.Item2);
                            }
                            
                        }
                        if (check.changed && !string.IsNullOrEmpty(_strings[index]) && field.ReferenceDefine != null)
                        {
                            YorozuDBEditorInternalUtility.FindData(field.ReferenceDefine, _strings[index]);
                        }
                    }
                    break;
                case DataType.Float:
                    _floats[index] = EditorGUI.FloatField(rect, content, _floats[index]);
                    break;
                case DataType.Int:
                    _ints[index] = EditorGUI.IntField(rect, content, _ints[index]);
                    break;
                case DataType.Bool:
                    _bools[index] = EditorGUI.Toggle(rect, content, _bools[index]);
                    break;
                case DataType.Sprite:
                    // 18以上あるとUI変わってしまうので大きかったら小さくする
                    if (rect.height >= 18)
                        rect.height = 17.9f;
                    _unityObjects[index] = EditorGUI.ObjectField(rect, content, _unityObjects[index], typeof(Sprite), false);
                    break;
                case DataType.GameObject:
                    _unityObjects[index] = EditorGUI.ObjectField(rect, content, _unityObjects[index], typeof(GameObject), false);
                    break;
                case DataType.AudioClip:
                    _unityObjects[index] = EditorGUI.ObjectField(rect, content, _unityObjects[index], typeof(AudioClip), false);
                    break;
                case DataType.UnityObject:
                    _unityObjects[index] = EditorGUI.ObjectField(rect, content, _unityObjects[index], typeof(UnityEngine.Object), false);
                    break;
                case DataType.Vector2:
                    var vector2 = GetFromString<Vector2>(_strings[index]);
                    using (var check = new EditorGUI.ChangeCheckScope())
                    {
                        vector2 = EditorGUI.Vector2Field(rect, content, vector2);
                        if (check.changed)
                        {
                            SetToString(vector2, index);
                        }
                    }
                    break;
                case DataType.Vector3:
                    var vector3 = GetFromString<Vector3>(_strings[index]);
                    using (var check = new EditorGUI.ChangeCheckScope())
                    {
                        vector3 = EditorGUI.Vector3Field(rect, content, vector3);
                        if (check.changed)
                            SetToString(vector3, index);
                    }
                    break;
                case DataType.ScriptableObject:
                    rect.width -= 22;
                    _unityObjects[index] = EditorGUI.ObjectField(rect, content, _unityObjects[index], typeof(UnityEngine.ScriptableObject), false);
                    rect.x += rect.width;
                    rect.width = 22;
                    using (new EditorGUI.DisabledScope(_unityObjects[index] == null))
                    {
                        if (GUI.Button(rect, Style.OpenButton))
                        {
                            Selection.objects = new UnityEngine.Object[] {_unityObjects[index]};
                            EditorApplication.ExecuteMenuItem("Assets/Properties...");
                        }
                    }
                    break;
                case DataType.Vector2Int:
                    if (string.IsNullOrEmpty(_strings[index]))
                    {
                        _strings[index] = JsonUtility.ToJson(new SerializableIntArray(2));
                    }
                        
                    var v2Array = GetFromString<SerializableIntArray>(_strings[index]);
                    using (var check = new EditorGUI.ChangeCheckScope())
                    {
                        EditorGUI.MultiIntField(rect, Style.Vector2IntContents, v2Array.IntArray);
                        if (check.changed)
                        {
                            SetToString(v2Array, index);
                        }
                    }
                    break;
                case DataType.Vector3Int:
                    if (string.IsNullOrEmpty(_strings[index]))
                    {
                        _strings[index] = JsonUtility.ToJson(new SerializableIntArray(3));
                    }
                        
                    var v3Array = GetFromString<SerializableIntArray>(_strings[index]);
                    using (var check = new EditorGUI.ChangeCheckScope())
                    {
                        EditorGUI.MultiIntField(rect, Style.Vector3IntContents, v3Array.IntArray);
                        if (check.changed)
                        {
                            SetToString(v3Array, index);
                        }
                    }
                    break;
                case DataType.Enum:
                {
                    if (enumData == null)
                    {
                        EditorGUI.LabelField(rect, "Enum Asset Not Found.");
                        return;
                    }

                    var enums = enumData.GetEnums(field.EnumDefineId);
                    var enumIndex = enumData.GetEnumIndex(field.EnumDefineId, _ints[index]);
                    using (var check = new EditorGUI.ChangeCheckScope())
                    {
                        enumIndex = EditorGUI.Popup(rect, content.text, enumIndex, enums);
                        if (check.changed)
                        {
                            var key = enumData.GetEnumKey(field.EnumDefineId, enums[enumIndex]);
                            if (key.HasValue)
                            {
                                _ints[index] = key.Value;
                            }
                        }
                    }
                }
                    break;
                case DataType.Color:
                    var color = string.IsNullOrEmpty(_strings[index]) ? Color.white : GetFromString<Color>(_strings[index]);
                    using (var check = new EditorGUI.ChangeCheckScope())
                    {
                        color = EditorGUI.ColorField(rect, content, color);
                        if (check.changed)
                        {
                            SetToString(color, index);
                        }
                    }
                    break;
                case DataType.Flags:
                {
                    var name = enumData.GetEnumFlagName(field.EnumDefineId, _ints[index]);
                    if (GUI.Button(rect, name))
                    {
                        var enums = enumData.GetEnums(field.EnumDefineId);
                        var popup = new EnumFlagsPopup(enums, _ints[index]);
                        popup.FlagsChange += v => _ints[index] = v;
                        PopupWindow.Show(rect, popup);
                    }
                }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}

#endif