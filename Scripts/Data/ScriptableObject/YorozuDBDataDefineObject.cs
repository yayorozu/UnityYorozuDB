using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

namespace Yorozu.DB
{
    /// <summary>
    /// データを保存
    /// </summary>
    internal class YorozuDBDataDefineObject : ScriptableObject
    {
        [FormerlySerializedAs("_fields")]
        [SerializeField]
        internal List<DataField> Fields = new List<DataField>();

        /// <summary>
        /// 追加でここに定義されている IList のフィールドをそのまま利用できるようにする
        /// その際には複数のデータを作成することはできない
        /// </summary>
        [SerializeField]
        internal ScriptableObject ExtendFieldsObject;

        internal string ClassName => name.Replace("Define", "");
        
        /// <summary>
        /// 検索時にKeyにする名前
        /// 重複は許可されない
        /// </summary>
        [SerializeField]
        private int _keyID;
        
        internal bool IsKeyField(DataField field) => field.ID == _keyID;

        internal DataField KeyField => Fields.FirstOrDefault(IsKeyField);
        
#if UNITY_EDITOR

        [Serializable]
        internal class ExtendFieldWidth
        {
            [SerializeField]
            internal string Name;

            [SerializeField]
            internal float Width = 150;
        }

        /// <summary>
        /// 拡張オブジェクトの幅をキャッシュ
        /// </summary>
        [SerializeField]
        internal List<ExtendFieldWidth> ExtendFieldWidths = new List<ExtendFieldWidth>();

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
        internal void AddField(string name, DataType dataType, string enumName)
        {
            if (!YorozuDBEditorUtility.NameValidator(Fields, name, out name))
                return;

            var fieldId = 1;
            if (Fields.Any())
            {
                fieldId = Fields.Max(f => f.ID) + 1;
            }

            var typeId = 0;
            if (dataType == DataType.Enum)
            {
                var enumData = YorozuDBEditorUtility.LoadEnumDataAsset();
                var index = enumData.Defines.FindIndex(d => d.Name == enumName);
                if (index >= 0)
                {
                    typeId = enumData.Defines[index].ID;
                }
            }

            var field = new DataField(typeId)
            {
                Name = name,
                ID = fieldId,
                DataType = dataType,
            };

            Fields.Add(field);

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
            if (!YorozuDBEditorUtility.NameValidator(Fields, newName, out newName))
                return;
            
            var index = Fields.FindIndex(f => f.ID == fieldId);
            if (index >= 0)
            {
                Fields[index].Name = newName;
            }
            
            this.Dirty();
        }
        
        /// <summary>
        /// 特定のフィールドを削除
        /// </summary>
        internal void RemoveField(int fieldId)
        {
            // 削除
            Fields.RemoveAll(g => g.ID == fieldId);
            var assets = YorozuDBEditorUtility.LoadAllDataAsset(this);
            foreach (var asset in assets)
            {
                asset.RemoveField(fieldId);
            }
            this.Dirty();
        }
    }
    
    [UnityEditor.CustomEditor(typeof(YorozuDBDataDefineObject))]
    internal class YorozuDBDataDefineObjectEditor : UnityEditor.Editor
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