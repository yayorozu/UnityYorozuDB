using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Yorozu.DB
{
    /// <summary>
    /// Enum っぽいデータを保持
    /// </summary>
    public class YorozuDBEnumDataObject : ScriptableObject
    {
        [Serializable]
        internal class EnumDefine : IDBName
        {
            /// <summary>
            /// 定義名
            /// </summary>
            [SerializeField]
            internal string Name;

            string IDBName.Name => Name;

            /// <summary>
            /// リネーム困るのでIDで紐付け
            /// </summary>
            [SerializeField]
            internal int ID;
            
            [SerializeField]
            internal List<KeyValue> KeyValues = new List<KeyValue>();
            
            /// <summary>
            /// 説明
            /// </summary>
            [SerializeField]
            internal string Description;

            internal EnumDefine(string name, int id)
            {
                Name = name;
                ID = id;
            }

            internal void AddValue(string value)
            {
                var key = 1;
                if (KeyValues.Any())
                {
                    key = KeyValues.Max(v => v.Key) + 1;
                }
                
                KeyValues.Add(new KeyValue()
                {
                    Key = key,
                    Value = value,
                });
            }
            
            internal void RemoveAt(int index)
            {
                KeyValues.RemoveAt(index);
            }
        }

        /// <summary>
        /// 名前と Key を連動させる
        /// これにより名前を変えても Key は変わらない
        /// </summary>
        [Serializable]
        internal class KeyValue
        {
            internal int Key;
            internal string Value;
        }

        /// <summary>
        /// 定義一覧
        /// </summary>
        [SerializeField]
        internal List<EnumDefine> Defines = new List<EnumDefine>();

        /// <summary>
        /// 探す
        /// </summary>
        internal EnumDefine Find(int defineId)
        {
            var index = Defines.FindIndex(d => d.ID == defineId);
            if (index < 0)
                return null;

            return Defines[index];
        }

        /// <summary>
        /// 追加 
        /// </summary>
        internal void AddDefine(string name)
        {
            if (!YorozuDBEditorUtility.NameValidator(Defines, name, out name))
                return;
            
            var id = !Defines.Any() ? 1 : Defines.Max(v => v.ID) + 1;
            
            Defines.Add(new EnumDefine(name, id));
            this.Dirty();
        }

        /// <summary>
        /// 名前変更
        /// </summary>
        internal void Rename(int defineId, string newName)
        {
            if (!YorozuDBEditorUtility.NameValidator(Defines, newName, out newName))
                return;
            
            var index = Defines.FindIndex(d => d.ID == defineId);
            if (index >= 0)
            {
                Defines[index].Name = newName;
                this.Dirty();
            }
        }

        /// <summary>
        /// 削除
        /// </summary>
        internal void RemoveDefine(int enumDefineId)
        {
            // 関連するデータのフィールドも削除する
            var defines = YorozuDBEditorUtility.LoadAllDefineAsset();
            foreach (var d in defines)
            {
                var targetFieldIds = d.Fields.Where(f => f.DataType == DataType.Enum && f.EnumDefineId == enumDefineId)
                    .Select(f => f.ID)
                    .ToArray();
                // 定義したデータがある場合は削除する
                foreach (var fieldId in targetFieldIds)
                {
                    d.RemoveField(fieldId);
                }
            }
            
            if (Defines.RemoveAll(v => v.ID == enumDefineId) > 0)
            {
                this.Dirty();
            }
        }

        /// <summary>
        /// DataEditor側で見る
        /// </summary>
        [NonSerialized]
        private static Dictionary<int, string[]> _valuesDictionary = new Dictionary<int, string[]>();
        [NonSerialized]
        private static Dictionary<int, int[]> _keysDictionary = new Dictionary<int, int[]>();

        internal void ResetEnumCache()
        {
            _valuesDictionary.Clear();
            _keysDictionary.Clear();
        }
        
        internal string[] GetEnums(int id)
        {
            if (!_valuesDictionary.ContainsKey(id))
            {
                var values = Array.Empty<string>();
                var keys = Array.Empty<int>();
                var index = Defines.FindIndex(d => d.ID == id);
                if (index >= 0)
                {
                    // 同じタイミングで両方作る
                    values = Defines[index].KeyValues
                        .Where(v => !string.IsNullOrEmpty(v.Value))
                        .Select(v => v.Value)
                        .ToArray();
                    
                    keys = Defines[index].KeyValues
                        .Where(v => !string.IsNullOrEmpty(v.Value))
                        .Select(v => v.Key)
                        .ToArray();
                }
                
                _valuesDictionary.Add(id, values);
                _keysDictionary.Add(id, keys);
            }
            
            return _valuesDictionary[id];
        }

        internal int GetEnumIndex(int id, int key)
        {
            if (!_keysDictionary.ContainsKey(id))
            {
                return 0;
            }

            return Mathf.Max(Array.IndexOf(_keysDictionary[id], key), 0);
        }

        internal int? GetEnumKey(int id, string value)
        {
            if (!_valuesDictionary.ContainsKey(id))
                return null;

            var index = Array.IndexOf(_valuesDictionary[id], value);
            return _keysDictionary[id][index];
        }
        
    }
    
#if UNITY_EDITOR
    [UnityEditor.CustomEditor(typeof(YorozuDBEnumDataObject))]
    internal class YorozuDBEnumDataObjectEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            if (GUILayout.Button("Open Editor"))
            {
                // TODO 
            }
        }
    }
#endif
}