using System;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Yorozu.DB
{
    [Serializable]
    internal class DataContainer
    {
        [SerializeField]
        private string _string;
        internal string String => _string;
        [SerializeField]
        private int _int;
        internal int Int => _int;
        [SerializeField]
        private float _float;
        internal float Float => _float;
        [SerializeField]
        private bool _bool;
        internal bool Bool => _bool;
        [SerializeField]
        private UnityEngine.Object _unityObject;
        internal UnityEngine.Object UnityObject => _unityObject;

        internal DataContainer Copy()
        {
            var copy = new DataContainer();
            copy._string = _string;
            copy._int = _int;
            copy._float = _float;
            copy._bool = _bool;
            copy._unityObject = _unityObject;
            return copy;
        }

        internal void UpdateInt(int value)
        {
            _int = value;
        }
        
        internal T GetFromString<T>()
        {
            if (string.IsNullOrEmpty(_string))
            {
                return default;
            }
            return JsonUtility.FromJson<T>(_string);
        }
        
        internal void SetToString(object obj)
        {
            _string = JsonUtility.ToJson(obj);
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
        
        internal void DrawField(Rect rect, DataField field, GUIContent content, YorozuDBEnumDataObject enumData)
        {
            switch (field.DataType)
            {
                case DataType.String:
                    _string = EditorGUI.TextField(rect, content, _string);
                    break;
                case DataType.Float:
                    _float = EditorGUI.FloatField(rect, content, _float);
                    break;
                case DataType.Int:
                    _int = EditorGUI.IntField(rect, content, _int);
                    break;
                case DataType.Bool:
                    _bool = EditorGUI.Toggle(rect, content, _bool);
                    break;
                case DataType.Sprite:
                    // 18???????????????UI????????????????????????????????????????????????????????????
                    if (rect.height >= 18)
                        rect.height = 17.9f;
                    _unityObject = EditorGUI.ObjectField(rect, content, _unityObject, typeof(Sprite), false);
                    break;
                case DataType.GameObject:
                    _unityObject = EditorGUI.ObjectField(rect, content, _unityObject, typeof(GameObject), false);
                    break;
                case DataType.AudioClip:
                    _unityObject = EditorGUI.ObjectField(rect, content, _unityObject, typeof(AudioClip), false);
                    break;
                case DataType.UnityObject:
                    _unityObject = EditorGUI.ObjectField(rect, content, _unityObject, typeof(UnityEngine.Object), false);
                    break;
                case DataType.Vector2:
                    var vector2 = GetFromString<Vector2>();
                    using (var check = new EditorGUI.ChangeCheckScope())
                    {
                        vector2 = EditorGUI.Vector2Field(rect, content, vector2);
                        if (check.changed)
                        {
                            SetToString(vector2);
                        }
                    }
                    break;
                case DataType.Vector3:
                    var vector3 = GetFromString<Vector3>();
                    using (var check = new EditorGUI.ChangeCheckScope())
                    {
                        vector3 = EditorGUI.Vector3Field(rect, content, vector3);
                        if (check.changed)
                            SetToString(vector3);
                    }
                    break;
                case DataType.ScriptableObject:
                    rect.width -= 22;
                    _unityObject = EditorGUI.ObjectField(rect, content, _unityObject, typeof(UnityEngine.ScriptableObject), false);
                    rect.x += rect.width;
                    rect.width = 22;
                    using (new EditorGUI.DisabledScope(_unityObject == null))
                    {
                        if (GUI.Button(rect, Style.OpenButton))
                        {
                            Selection.objects = new UnityEngine.Object[] {_unityObject};
                            EditorApplication.ExecuteMenuItem("Assets/Properties...");
                        }
                    }
                    break;
                case DataType.Vector2Int:
                    if (string.IsNullOrEmpty(_string))
                    {
                        _string = JsonUtility.ToJson(new SerializableIntArray(2));
                    }
                        
                    var v2Array = GetFromString<SerializableIntArray>();
                    using (var check = new EditorGUI.ChangeCheckScope())
                    {
                        EditorGUI.MultiIntField(rect, Vector2IntContents, v2Array.IntArray);
                        if (check.changed)
                        {
                            SetToString(v2Array);
                        }
                    }
                    break;
                case DataType.Vector3Int:
                    if (string.IsNullOrEmpty(_string))
                    {
                        _string = JsonUtility.ToJson(new SerializableIntArray(3));
                    }
                        
                    var v3Array = GetFromString<SerializableIntArray>();
                    using (var check = new EditorGUI.ChangeCheckScope())
                    {
                        EditorGUI.MultiIntField(rect, Vector3IntContents, v3Array.IntArray);
                        if (check.changed)
                        {
                            SetToString(v3Array);
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
                    var index = enumData.GetEnumIndex(field.EnumDefineId, _int);
                    using (var check = new EditorGUI.ChangeCheckScope())
                    {
                        index = EditorGUI.Popup(rect, content.text, index, enums);
                        if (check.changed)
                        {
                            var key = enumData.GetEnumKey(field.EnumDefineId, enums[index]);
                            if (key.HasValue)
                            {
                                _int = key.Value;
                            }
                        }
                    }
                }
                    break;
                case DataType.Color:
                    var color = string.IsNullOrEmpty(_string) ? Color.white : GetFromString<Color>();
                    using (var check = new EditorGUI.ChangeCheckScope())
                    {
                        color = EditorGUI.ColorField(rect, content, color);
                        if (check.changed)
                        {
                            SetToString(color);
                        }
                    }
                    break;
                case DataType.Flags:
                {
                    var name = enumData.GetEnumFlagName(field.EnumDefineId, _int);
                    if (GUI.Button(rect, name))
                    {
                        var enums = enumData.GetEnums(field.EnumDefineId);
                        var popup = new EnumFlagsPopup(enums, _int);
                        popup.FlagsChange += v => _int = v;
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