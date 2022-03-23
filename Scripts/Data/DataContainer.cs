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
                    _unityObject = EditorGUI.ObjectField(rect, content, _unityObject, typeof(Sprite), false);
                    break;
                case DataType.GameObject:
                    _unityObject = EditorGUI.ObjectField(rect, content, _unityObject, typeof(GameObject), false);
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
                    _unityObject = EditorGUI.ObjectField(rect, content, _unityObject, typeof(UnityEngine.ScriptableObject), false);
                    break;
                case DataType.Vector2Int:
                    var vector2Int = GetFromString<Vector2Int>();
                    using (var check = new EditorGUI.ChangeCheckScope())
                    {
                        vector2Int = EditorGUI.Vector2IntField(rect, content, vector2Int);
                        if (check.changed)
                        {
                            SetToString(vector2Int);
                        }
                    }
                    break;
                case DataType.Vector3Int:
                    var vector3Int = GetFromString<Vector3Int>();
                    using (var check = new EditorGUI.ChangeCheckScope())
                    {
                        vector3Int = EditorGUI.Vector3IntField(rect, content, vector3Int);
                        if (check.changed)
                        {
                            SetToString(vector3Int);
                        }
                    }
                    break;
                case DataType.Enum:
                    var enums = enumData.GetEnums(field.EnumDefineId);
                    var index = enumData.GetEnumIndex(field.EnumDefineId, _int);
                    using (var check = new EditorGUI.ChangeCheckScope())
                    {
                        index = EditorGUI.Popup(rect, index, enums);
                        if (check.changed)
                        {
                            var key = enumData.GetEnumKey(field.EnumDefineId, enums[index]);
                            if (key.HasValue)
                            {
                                _int = key.Value;
                            }
                        }
                    }
                    break;
            }
        }
#endif
    }
}