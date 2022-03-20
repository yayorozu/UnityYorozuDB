using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Yorozu.DB
{
    /// <summary>
    /// データを保存
    /// </summary>
    internal partial class YorozuDBDataObject : ScriptableObject
    {
        [SerializeField]
        private List<DBDataGroup> _groups = new List<DBDataGroup>();

        internal List<DBDataGroup> Groups => _groups;
        
        /// <summary>
        /// 検索時にKeyにする名前
        /// 重複は許可されない
        /// </summary>
        [SerializeField]
        private string _keyName;

        internal bool IsPrimaryGroup(DBDataGroup g) => g.Name == _keyName;
    }

    /// <summary>
    /// データの種類
    /// </summary>
    internal enum DataType
    {
        String,
        Float,
        Int,
        Sprite,
        GameObject,
        ScriptableObject,
        /// <summary>
        /// その他の UnityEngine.Object
        /// </summary>
        UnityObject,
        Vector2,
        Vector3,
        Vector2Int,
        Vector3Int,
    }

    [Serializable]
    internal class DBDataGroup
    {
        [SerializeField]
        internal string Name;
        
        [SerializeField]
        internal DataType DataType;

        [SerializeField]
        private List<DBData> _data = new List<DBData>();

        internal List<DBData> Data => _data;
        
        /// <summary>
        /// Keyとして有効かどうか
        /// </summary>
        internal bool ValidPrimary() => DataType == DataType.Int || DataType == DataType.String; 
        
        internal void AddData()
        {
            _data.Add(new DBData());
        }

        internal void RemoveAt(int index)
        {
            _data.RemoveAt(index);
        }
        
        internal void ResetAt(int index)
        {
            _data[index] = new DBData();
        }

        internal void Insert(int insertIndex, IOrderedEnumerable<int> targetIndexes)
        {
            var cache = new List<DBData>(targetIndexes.Count());
            foreach (var index in targetIndexes)
            {
                cache.Add(_data[index]);
                _data.RemoveAt(index);
            }

            cache.Reverse();

            // 消した分indexをへらす
            var frontRemoveCount = targetIndexes.Count(v => v < insertIndex);
            insertIndex -= frontRemoveCount;
            _data.InsertRange(insertIndex, cache);
        }
    }
    
    [Serializable]
    internal class DBData
    {
        [SerializeField]
        internal string String;
        [SerializeField]
        internal int Int;
        [SerializeField]
        internal float Float;
        [SerializeField]
        internal UnityEngine.Object UnityObject;

        internal T GetFromString<T>()
        {
            if (string.IsNullOrEmpty(String))
            {
                return default;
            }
            return JsonUtility.FromJson<T>(String);
        }
        
        internal void SetToString(object obj)
        {
            String = JsonUtility.ToJson(obj);
        }
    }
}