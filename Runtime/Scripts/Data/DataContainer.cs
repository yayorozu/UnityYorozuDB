using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using Yorozu.DB.TreeView;
#endif

namespace Yorozu.DB
{
    [Serializable]
    internal class DataContainer
    {
        internal int Size => Mathf.Max(_strings.Length, _ints.Length, _floats.Length, _bools.Length, _unityObjects.Length);

        [SerializeField]
        internal string[] _strings;
        internal string String
        {
            get { return _strings.Length <= 0 ? default : _strings[0]; }
#if UNITY_EDITOR
            set
            {
                if (_strings.Length <= 0)
                    throw new Exception("string is not set");

                _strings[0] = value;
            }
#endif
        }

        [SerializeField]
        internal int[] _ints;
        internal int Int
        {
            get { return _ints.Length <= 0 ? default : _ints[0]; }
#if UNITY_EDITOR
            set
            {
                if (_strings.Length <= 0)
                    throw new Exception("string is not set");

                _ints[0] = value;
            }
#endif
        }

        [SerializeField]
        internal float[] _floats;
        internal float Float
        {
            get { return _floats.Length <= 0 ? default : _floats[0]; }
#if UNITY_EDITOR
            set
            {
                if (_floats.Length <= 0)
                    throw new Exception("string is not set");

                _floats[0] = value;
            }
#endif
        }

        [SerializeField]
        internal bool[] _bools;
        internal bool Bool
        {
            get { return _bools.Length <= 0 ? default : _bools[0]; }
#if UNITY_EDITOR
            set
            {
                if (_bools.Length <= 0)
                    throw new Exception("string is not set");

                _bools[0] = value;
            }
#endif
        }

        [SerializeField]
        internal UnityEngine.Object[] _unityObjects;
        internal UnityEngine.Object UnityObject
        {
            get { return _unityObjects.Length <= 0 ? default : _unityObjects[0]; }
#if UNITY_EDITOR
            set
            {
                if (_unityObjects.Length <= 0)
                    throw new Exception("string is not set");

                _unityObjects[0] = value;
            }
#endif
        }
        
        internal Vector2 Vector2 => GetFromString<Vector2>(_strings[0]);
        internal Vector3 Vector3 => GetFromString<Vector3>(_strings[0]);
        internal Vector2Int Vector2Int
        {
            get
            {
                var array = GetFromString<SerializableIntArray>(_strings[0]); 
                return new Vector2Int(array.IntArray[0], array.IntArray[1]);
            }
        }
        internal Vector3Int Vector3Int
        {
            get
            {
                var array = GetFromString<SerializableIntArray>(_strings[0]); 
                return new Vector3Int(array.IntArray[0], array.IntArray[1], array.IntArray[2]);
            }
        }
        internal Color Color => GetFromString<Color>(_strings[0]);
        
        internal IEnumerable<Vector2> Vector2s => _strings.Select(GetFromString<Vector2>);
        internal IEnumerable<Vector3> Vector3s => _strings.Select(GetFromString<Vector3>);
        internal IEnumerable<Vector2Int> Vector2Ints => _strings.Select(GetFromString<Vector2Int>);
        internal IEnumerable<Vector3Int> Vector3Ints => _strings.Select(GetFromString<Vector3Int>);
        internal IEnumerable<Color> Colors => _strings.Select(GetFromString<Color>);
        
        internal DataContainer Copy()
        {
            var copy = new DataContainer();
            copy._strings = new string[_strings.Length];
            for (var i = 0; i < _strings.Length; i++)
                copy._strings[i] = _strings[i];
            
            copy._ints = new int[_ints.Length];
            for (var i = 0; i < _ints.Length; i++)
                copy._ints[i] = _ints[i];
            
            copy._floats = new float[_floats.Length];
            for (var i = 0; i < _floats.Length; i++)
                copy._floats[i] = _floats[i];
            
            copy._bools = new bool[_bools.Length];
            for (var i = 0; i < _bools.Length; i++)
                copy._bools[i] = _bools[i];
            
            copy._unityObjects = new UnityEngine.Object[_unityObjects.Length];
            for (var i = 0; i < _unityObjects.Length; i++)
                copy._unityObjects[i] = _unityObjects[i];
            
            return copy;
        }

        private T GetFromString<T>(string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                return default;
            }
            return JsonUtility.FromJson<T>(s);
        }
        
        internal void SetToString(object obj, int index)
        {
            _strings[index] = JsonUtility.ToJson(obj);
        }
        
#if UNITY_EDITOR

        private static class Style
        {
            internal static GUIContent OpenButton;

            static Style()
            {
                OpenButton = EditorGUIUtility.TrIconContent("d_UnityEditor.InspectorWindow", "Open Properties");
            }
        }
        
        private static GUIContent[] Vector2IntContents = new GUIContent[]
        {
            new GUIContent("x"),
            new GUIContent("y"),
        };
        
        private static GUIContent[] Vector3IntContents = new GUIContent[]
        {
            new GUIContent("x"),
            new GUIContent("y"),
            new GUIContent("z"),
        };
        
        public DataContainer(){}

        public DataContainer(DataType dataType)
        {
            this.Initialize(dataType);
        }
        
        internal void DrawField(Rect rect, DataField field, GUIContent content, YorozuDBEnumDataObject enumData)
        {
            rect.height = YorozuDBEditorDataTreeView.RowHeight;
            if (!field.IsArray)
            {
                rect.y += rect.height / 2f - YorozuDBEditorDataTreeView.RowHeight / 2f;
                DrawField(rect, field, content, enumData, 0);
                return;
            }

            var size = this.GetSize(field.DataType);
            for (int i = 0; i < size; i++)
            {
                DrawField(rect, field, content, enumData, i);        
                rect.y += rect.height;
            }
        }
        
        private void DrawField(Rect rect, DataField field, GUIContent content, YorozuDBEnumDataObject enumData, int index)
        {
            switch (field.DataType)
            {
                case DataType.String:
                    _strings[index] = EditorGUI.TextField(rect, content, _strings[index]);
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
                        EditorGUI.MultiIntField(rect, Vector2IntContents, v2Array.IntArray);
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
                        EditorGUI.MultiIntField(rect, Vector3IntContents, v3Array.IntArray);
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
#endif
    }
}