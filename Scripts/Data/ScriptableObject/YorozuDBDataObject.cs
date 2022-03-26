using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Yorozu.DB.TreeView;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.IMGUI.Controls;
#endif

namespace Yorozu.DB
{ 
    /// <summary>
    /// データをまとめる
    /// </summary>
    public class YorozuDBDataObject : ScriptableObject
    {
        [Serializable]
        private class Field
        {
            /// <summary>
            /// Field の ID と紐づくID
            /// </summary>
            [SerializeField]
            internal int ID;
            
            [SerializeField]
            internal List<DataContainer> Data = new List<DataContainer>();

            internal Field(int fieldId)
            {
                ID = fieldId;
            }
            
#if UNITY_EDITOR
            /// <summary>
            /// 入れ替え
            /// </summary>
            internal void Insert(int insertIndex, IOrderedEnumerable<int> targetIndexes)
            {
                var cache = new List<DataContainer>();
                foreach (var index in targetIndexes)
                {
                    cache.Add(Data[index]);
                    Data.RemoveAt(index);
                }

                cache.Reverse();

                // 消した分indexをへらす
                var frontRemoveCount = targetIndexes.Count(v => v < insertIndex);
                insertIndex -= frontRemoveCount;
                Data.InsertRange(insertIndex, cache);
            }
#endif
            
        }
        
        /// <summary>
        /// このデータの型情報
        /// </summary>
        [SerializeField]
        internal YorozuDBDataDefineObject Define;

        [SerializeField]
        private List<Field> _fields = new List<Field>();

        /// <summary>
        /// データ数
        /// </summary>
        internal int DataCount
        {
            get
            {
                if (_fields == null || !_fields.Any())
                {
                    // こっちの定義がない場合は拡張側を見る
                    var extendCount = YorozuDBExtendUtility.DataCount(Define.ExtendFieldsObject);
                    return extendCount;
                }

                return _fields[0].Data.Count;
            }
        }
        
        /// <summary>
        /// ID からデータを取得
        /// </summary>
        internal DataContainer GetData(int fieldId, int row)
        {
            return _fields
                .Where(f => f.ID == fieldId)
                .Select(f => f.Data[row])
                .First();
        }

#if UNITY_EDITOR
        /// <summary>
        /// フィールドの追加
        /// </summary>
        internal void AddField(int fieldId)
        {
            var find = _fields.Any(g => g.ID == fieldId);
            if (find)
            {
                return;
            }

            var addField = new Field(fieldId);
            // 既存のフィールドのデータ分だけ追加する必要がある
            if (DataCount > 0)
            {
                for (var i = 0; i < DataCount; i++)
                {
                    addField.Data.Add(new DataContainer());
                }
            }

            _fields.Add(addField);
            this.Dirty();
        }
        
        /// <summary>
        /// 特定のフィールドを削除
        /// </summary>
        internal void RemoveField(int fieldId)
        {
            _fields.RemoveAll(g => g.ID == fieldId);
            this.Dirty();
        }


        /// <summary>
        /// データの追加
        /// </summary>
        internal void AddRow()
        {
            foreach (var field in _fields)
            {
                var targetField = Define.Fields.First(f => f.ID == field.ID);
                field.Data.Add(targetField.DefaultValue.Copy());
            }

            // 対象のフィールド追加
            if (Define.ExtendFieldsObject != null)
            {
                YorozuDBExtendUtility.AddFields(Define.ExtendFieldsObject);
            }
            this.Dirty();
        }

        /// <summary>
        /// データの削除
        /// </summary>
        internal void RemoveRows(IOrderedEnumerable<int> descIndexes)
        {
            foreach (var g in _fields)
            {
                foreach (var index in descIndexes)
                {
                    g.Data.RemoveAt(index);
                }
            }
            
            YorozuDBExtendUtility.RemoveFields(Define.ExtendFieldsObject, descIndexes);
            this.Dirty();
        }

        /// <summary>
        /// データの初期化
        /// </summary>
        internal void ResetAt(int index)
        {
            foreach (var g in _fields)
            {
                g.Data[index] = new DataContainer();
            }
            this.Dirty();
        }

        /// <summary>
        /// 入れ替え
        /// </summary>
        internal void Insert(int insertIndex, IOrderedEnumerable<int> targetIndexes)
        {
            foreach (var g in _fields)
            {
                g.Insert(insertIndex, targetIndexes);   
            }
            this.Dirty();
        }
        
        /// <summary>
        /// TreeView用の木構造を作成
        /// </summary>
        internal TreeViewItem CreateTree(YorozuDBEnumDataObject enumData)
        {
            var root = new TreeViewItem(-1, -1, "root");
            
            for (var i = 0; i < DataCount; i++)
            {
                var item = new YorozuDBEditorTreeViewItem(i, this, enumData);
                root.AddChild(item);
            }
            
            return root;
        }
#endif
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(YorozuDBDataObject))]
    internal class YorozuDBDataObjectEditor : Editor
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