using System;
using System.Linq;
using UnityEngine;

namespace Yorozu.DB
{
    /// <summary>
    /// 各フィールドの定義
    /// </summary>
    [Serializable]
    internal class DBDataField : IDBName
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
        private int _dataTypeId;

        /// <summary>
        /// 並べ替えや削除したときにIDでデータと一致させる
        /// </summary>
        [SerializeField]
        internal int ID;
        
        /// <summary>
        /// Keyとして有効かどうか
        /// </summary>
        internal bool ValidKey() => DataType == DataType.Int || DataType == DataType.String;

        internal DBDataField(int typeId)
        {
            _dataTypeId = typeId;
        }

#if UNITY_EDITOR
        [NonSerialized]
        private bool _searched;

        [NonSerialized]
        private string[] _enumValues;
        
        internal string[] GetEnums()
        {
            if (!_searched)
            {
                _searched = true;
                var enumData = YorozuDBEditorUtility.LoadEnumDataAsset();
                if (enumData != null)
                {
                    var enumDefine = enumData.Find(_dataTypeId);
                    if (enumDefine != null)
                    {
                        _enumValues = enumDefine.Values
                            .Where(v => !string.IsNullOrEmpty(v))
                            .ToArray();
                    }
                }
            }

            if (_enumValues == null)
                _enumValues = Array.Empty<string>();

            return _enumValues;
        }
        
#endif
    }
}