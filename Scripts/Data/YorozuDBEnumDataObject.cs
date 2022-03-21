using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Yorozu.DB
{
    /// <summary>
    /// Enum っぽいデータを保持
    /// </summary>
    internal class YorozuDBEnumDataObject : ScriptableObject
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
            internal List<string> Values = new List<string>();

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
                Values.Add(value);
            }
            
            internal void RemoveAt(int index)
            {
                Values.RemoveAt(index);
            }
        }

        /// <summary>
        /// 定義一覧
        /// </summary>
        [SerializeField]
        internal List<EnumDefine> Defines = new List<EnumDefine>();

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
        internal void RemoveDefine(int id)
        {
            if (Defines.RemoveAll(v => v.ID == id) > 0)
            {
                this.Dirty();
            }
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