using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

            /// <summary>
            /// Apply FlagsAttribute
            /// </summary>
            [SerializeField]
            internal bool Flags;

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
        }

        /// <summary>
        /// 名前と Key を連動させる
        /// これにより名前を変えても Key は変わらない
        /// </summary>
        [Serializable]
        internal class KeyValue
        {
            [SerializeField]
            internal int Key;

            [SerializeField]
            internal string Value;
        }

        /// <summary>
        /// 定義一覧
        /// </summary>
        [SerializeField]
        internal List<EnumDefine> Defines = new List<EnumDefine>();

#if UNITY_EDITOR

        /// <summary>
        /// 追加 
        /// </summary>
        internal void AddDefine(string name)
        {
            if (!YorozuDBEditorInternalUtility.NameValidator(Defines, name, out name))
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
            if (!YorozuDBEditorInternalUtility.NameValidator(Defines, newName, out newName))
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
            var defines = YorozuDBEditorInternalUtility.LoadAllDefineAsset();
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

        internal void AddValue(int defineId, string value)
        {
            var index = Defines.FindIndex(d => d.ID == defineId);
            if (index < 0)
                return;

            var key = 1;
            if (Defines[index].KeyValues.Any())
            {
                key = Defines[index].KeyValues.Max(v => v.Key) + 1;
            }

            Defines[index].KeyValues.Add(new KeyValue()
            {
                Key = key,
                Value = value,
            });
            this.Dirty();
        }

        internal void RemoveValue(int defineId, int removeIndex)
        {
            var index = Defines.FindIndex(d => d.ID == defineId);
            if (index < 0)
                return;

            Defines[index].KeyValues.RemoveAt(removeIndex);
            this.Dirty();
        }

        /// <summary>
        /// DataEditor側で見る
        /// </summary>
        [NonSerialized]
        private Dictionary<int, string[]> _valuesDictionary = new Dictionary<int, string[]>();

        [NonSerialized]
        private Dictionary<int, int[]> _keysDictionary = new Dictionary<int, int[]>();

        internal void ResetEnumCache()
        {
            _valuesDictionary.Clear();
            _keysDictionary.Clear();
        }

        private void CacheDictionary(int id)
        {
            if (_valuesDictionary.ContainsKey(id))
                return;
            
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
        
        internal string[] GetEnums(int id)
        {
            CacheDictionary(id);
            return _valuesDictionary[id];
        }

        private static StringBuilder _builder = new StringBuilder();

        internal string GetEnumFlagName(int id, int value)
        {
            if (value == 0)
                return "None";

            var enums = GetEnums(id);
            var all = (int) Mathf.Pow(2, enums.Length) - 1;
            if (value == all)
                return "All";

            _builder.Clear();
            for (var i = 0; i < enums.Length; i++)
            {
                var v = (int) Mathf.Pow(2, i);
                if ((value & v) == v)
                {
                    _builder.Append(_builder.Length > 1 ? $", {enums[i]}" : enums[i]);
                }
            }

            return _builder.ToString();
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
            CacheDictionary(id);
            if (!_valuesDictionary.ContainsKey(id))
                return null;

            var index = Array.IndexOf(_valuesDictionary[id], value);
            return _keysDictionary[id][index];
        }
#endif
    }

#if UNITY_EDITOR
    [UnityEditor.CustomEditor(typeof(YorozuDBEnumDataObject))]
    internal class YorozuDBEnumDataObjectEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            if (GUILayout.Button("Open Editor"))
            {
                UnityEditor.EditorApplication.ExecuteMenuItem(YorozuDBEditorWindow.MenuPath);
            }
        }
    }
#endif
}