using System;
using UnityEngine;

namespace Yorozu.DB
{
    /// <summary>
    /// 各フィールドの定義
    /// </summary>
    [Serializable]
    internal class DBDataField
    {
        [SerializeField]
        internal string Name;
        
        [SerializeField]
        internal DataType DataType;

        /// <summary>
        /// 並べ替えや削除したときにIDでデータと一致させる
        /// </summary>
        [SerializeField]
        internal int ID;
        
        /// <summary>
        /// Keyとして有効かどうか
        /// </summary>
        internal bool ValidKey() => DataType == DataType.Int || DataType == DataType.String;
    }
}