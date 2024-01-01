using System;
using UnityEditor;

namespace Yorozu.DB
{
    internal static class DataContainerUtility
    {
        
        /// <summary>
        /// 配列のサイズを変更する
        /// </summary>
        internal static void AddElement(this DataContainer self, DataType dataType)
        {
            switch (dataType)
            {
                case DataType.Int:
                case DataType.Enum:
                case DataType.Flags:
                    ArrayUtility.Add(ref self._ints, 0);
                    break;
                case DataType.String:
                case DataType.Vector2:
                case DataType.Vector3:
                case DataType.Vector2Int:
                case DataType.Vector3Int:
                case DataType.Color:
                case DataType.DBClass:
                    ArrayUtility.Add(ref self._strings, "");
                    break;
                case DataType.Float:
                    ArrayUtility.Add(ref self._floats, 0f);
                    break;
                case DataType.Bool:
                    ArrayUtility.Add(ref self._bools, false);
                    break;
                case DataType.Sprite:
                case DataType.GameObject:
                case DataType.ScriptableObject:
                case DataType.UnityObject:
                case DataType.AudioClip:
                    ArrayUtility.Add(ref self._unityObjects, null);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(dataType), dataType, null);
            }
        }
        
        internal static void RemoveAt(this DataContainer self, DataType dataType, int index)
        {
            switch (dataType)
            {
                case DataType.Int:
                case DataType.Enum:
                case DataType.Flags:
                    ArrayUtility.RemoveAt(ref self._ints, index);
                    break;
                case DataType.String:
                case DataType.Vector2:
                case DataType.Vector3:
                case DataType.Vector2Int:
                case DataType.Vector3Int:
                case DataType.Color:
                case DataType.DBClass:
                    ArrayUtility.RemoveAt(ref self._strings, index);
                    break;
                case DataType.Float:
                    ArrayUtility.RemoveAt(ref self._floats, index);
                    break;
                case DataType.Bool:
                    ArrayUtility.RemoveAt(ref self._bools, index);
                    break;
                case DataType.Sprite:
                case DataType.GameObject:
                case DataType.ScriptableObject:
                case DataType.UnityObject:
                case DataType.AudioClip:
                    ArrayUtility.RemoveAt(ref self._unityObjects, index);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(dataType), dataType, null);
            }
        }
        
        internal static int GetSize(this DataContainer self, DataType dataType)
        {
            switch (dataType)
            {
                case DataType.Int:
                case DataType.Enum:
                case DataType.Flags:
                    return self._ints?.Length ?? 0;
                case DataType.String:
                case DataType.Vector2:
                case DataType.Vector3:
                case DataType.Vector2Int:
                case DataType.Vector3Int:
                case DataType.Color:
                case DataType.DBClass:
                    return self._strings?.Length ?? 0;
                case DataType.Float:
                    return self._floats?.Length ?? 0;
                case DataType.Bool:
                    return self._bools?.Length ?? 0;
                case DataType.Sprite:
                case DataType.GameObject:
                case DataType.ScriptableObject:
                case DataType.UnityObject:
                case DataType.AudioClip:
                    return self._unityObjects?.Length ?? 0;
                default:
                    throw new ArgumentOutOfRangeException(nameof(dataType), dataType, null);
            }
        }
        
        internal static void Initialize(this DataContainer self, DataType dataType)
        {
            // 初期データを構築
            switch (dataType)
            {
                case DataType.Int:
                case DataType.Enum:
                case DataType.Flags:
                    self._ints = new []{0};
                    break;
                case DataType.String:
                case DataType.Vector2:
                case DataType.Vector3:
                case DataType.Vector2Int:
                case DataType.Vector3Int:
                case DataType.Color:
                case DataType.DBClass:
                    self._strings = new []{""};
                    break;
                case DataType.Float:
                    self._floats = new []{0f};
                    break;
                case DataType.Bool:
                    self._bools = new []{false};
                    break;
                case DataType.Sprite:
                case DataType.GameObject:
                case DataType.ScriptableObject:
                case DataType.UnityObject:
                case DataType.AudioClip:
                    self._unityObjects = new []{default(UnityEngine.Object)};
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        internal static void CheckSize(this DataContainer self, DataType dataType, int index)
        {
            // 初期データを構築
            switch (dataType)
            {
                case DataType.Int:
                case DataType.Enum:
                case DataType.Flags:
                    for (var i = self._ints.Length; i <= index; i++)
                    {
                        ArrayUtility.Add(ref self._ints, 0);                        
                    }
                    break;
                case DataType.String:
                case DataType.Vector2:
                case DataType.Vector3:
                case DataType.Vector2Int:
                case DataType.Vector3Int:
                case DataType.Color:
                case DataType.DBClass:
                    for (var i = self._strings.Length; i <= index; i++)
                    {
                        ArrayUtility.Add(ref self._strings, "");
                    }
                    break;
                case DataType.Float:
                    for (var i = self._floats.Length; i <= index; i++)
                    {
                        ArrayUtility.Add(ref self._floats, 0);                        
                    }
                    break;
                case DataType.Bool:
                    for (var i = self._bools.Length; i <= index; i++)
                    {
                        ArrayUtility.Add(ref self._bools, false);                        
                    }
                    break;
                case DataType.Sprite:
                case DataType.GameObject:
                case DataType.ScriptableObject:
                case DataType.UnityObject:
                case DataType.AudioClip:
                    for (var i = self._unityObjects.Length; i <= index; i++)
                    {
                        ArrayUtility.Add(ref self._unityObjects, null);                        
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
    }
}