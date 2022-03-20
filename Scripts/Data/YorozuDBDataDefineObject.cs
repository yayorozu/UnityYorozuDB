using System.Collections.Generic;
using UnityEngine;

namespace Yorozu.DB
{
    /// <summary>
    /// データを保存
    /// </summary>
    internal partial class YorozuDBDataDefineObject : ScriptableObject
    {
        [SerializeField]
        private List<DBDataField> _fields = new List<DBDataField>();

        internal List<DBDataField> Fields => _fields;
        
        /// <summary>
        /// 検索時にKeyにする名前
        /// 重複は許可されない
        /// </summary>
        [SerializeField]
        private int _keyID;
        
        internal bool IsKeyField(DBDataField field) => field.ID == _keyID;
    }
}