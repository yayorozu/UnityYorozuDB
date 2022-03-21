#if UNITY_EDITOR

using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Yorozu.DB
{
    internal partial class YorozuDBDataDefineObject
    {
        /// <summary>
        /// Primary を変更
        /// </summary>
        internal void SetKey(int fieldID)
        {
            if (_keyID == fieldID)
                _keyID = -1;
            else
                _keyID = fieldID;
            
            this.Dirty();
        }

        /// <summary>
        /// データの追加
        /// </summary>
        internal void AddField(string name, DataType dataType)
        {
            if (!YorozuDBEditorUtility.NameValidator(_fields, name, out name))
                return;

            var fieldId = 1;
            if (_fields.Any())
            {
                fieldId = _fields.Max(f => f.ID) + 1;
            }

            var field = new DBDataField()
            {
                Name = name,
                ID = fieldId,
                DataType = dataType,
            };

            _fields.Add(field);

            // 依存するデータにフィールド追加
            var assets = YorozuDBEditorUtility.LoadAllDataAsset(this);
            foreach (var asset in assets)
            {
                asset.AddField(field.ID);
            }
            
            this.Dirty();
        }

        /// <summary>
        /// 名前を変更する
        /// </summary>
        internal void RenameField(int fieldId, string newName)
        {
            if (!YorozuDBEditorUtility.NameValidator(_fields, newName, out newName))
                return;
            
            var index = _fields.FindIndex(f => f.ID == fieldId);
            if (index >= 0)
            {
                _fields[index].Name = newName;
            }
            
            this.Dirty();
        }
        
        /// <summary>
        /// 特定のフィールドを削除
        /// </summary>
        internal void RemoveField(int fieldId)
        {
            // 削除
            _fields.RemoveAll(g => g.ID == fieldId);
            var assets = YorozuDBEditorUtility.LoadAllDataAsset(this);
            foreach (var asset in assets)
            {
                asset.RemoveField(fieldId);
            }
            this.Dirty();
        }
    }
    
    [CustomEditor(typeof(YorozuDBDataDefineObject))]
    internal class YorozuDBDataDefineObjectEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            if (GUILayout.Button("Open Editor"))
            {
                // TODO 
            }
        }
    }
}

#endif
