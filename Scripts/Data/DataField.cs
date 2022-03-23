using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Yorozu.DB
{
    /// <summary>
    /// 各フィールドの定義
    /// </summary>
    [Serializable]
    internal class DataField : IDBName
    {
        [SerializeField]
        internal string Name;
        
        string IDBName.Name => Name;
        
        [SerializeField]
        internal DataType DataType;

        /// <summary>
        /// Enum のときにどれと紐付いているのか判定するよう
        /// </summary>
        [FormerlySerializedAs("DataTypeId")]
        [SerializeField]
        internal int EnumDefineId;

        /// <summary>
        /// 並べ替えや削除したときにIDでデータと一致させる
        /// </summary>
        [SerializeField]
        internal int ID;
        
        /// <summary>
        /// Keyとして有効かどうか
        /// </summary>
        internal bool ValidKey() => DataType == DataType.Int || DataType == DataType.String || DataType == DataType.Enum;

        [SerializeField]
        internal DataContainer DefaultValue;

#if UNITY_EDITOR
        
        internal DataField(int typeId)
        {
            EnumDefineId = typeId;
        }

        internal float GetRectWidth()
        {
            switch (DataType)
            {
                case DataType.Int:
                case DataType.Float:
                case DataType.Bool:
                    return 100;
                case DataType.String:
                case DataType.Sprite:
                case DataType.GameObject:
                case DataType.ScriptableObject:
                case DataType.UnityObject:
                case DataType.Enum:
                    return 150;
                case DataType.Vector2:
                case DataType.Vector3:
                case DataType.Vector2Int:
                case DataType.Vector3Int:
                    return 200;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
#endif
    }
}