using System;
using UnityEngine;

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
        [SerializeField]
        internal int EnumDefineId;

        /// <summary>
        /// 並べ替えや削除したときにIDでデータと一致させる
        /// </summary>
        [SerializeField]
        internal int ID;

        /// <summary>
        /// GUIで利用するカラムの幅
        /// </summary>
        [SerializeField]
        internal float GUIWidth = 150;

        /// <summary>
        /// コメント
        /// </summary>
        [SerializeField]
        internal string Memo;
        
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
        
#endif
    }
}