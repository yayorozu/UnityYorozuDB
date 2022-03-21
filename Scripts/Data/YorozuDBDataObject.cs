using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using Yorozu.DB.TreeView;

namespace Yorozu.DB
{ 
    /// <summary>
    /// データをまとめる
    /// </summary>
    internal class YorozuDBDataObject : ScriptableObject
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
            internal List<DBDataContainer> Data = new List<DBDataContainer>();

            internal Field(int fieldId)
            {
                ID = fieldId;
            }
            
            /// <summary>
            /// 入れ替え
            /// </summary>
            internal void Insert(int insertIndex, IOrderedEnumerable<int> targetIndexes)
            {
                var cache = new List<DBDataContainer>();
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
                    return 0;

                return _fields[0].Data.Count;
            }
        }
        
        [NonSerialized]
        private Dictionary<int, int> _fieldIdToIndex;

        /// <summary>
        /// ID からデータを取得
        /// </summary>
        internal DBDataContainer GetData(int fieldId, int row)
        {
            if (_fieldIdToIndex == null)
            {
                _fieldIdToIndex = new Dictionary<int, int>();
                foreach (var (v, i) in _fields.Select((v, i) => (v, i)))
                {
                    _fieldIdToIndex.Add(v.ID, i);
                }
            }
            
            if (_fieldIdToIndex.TryGetValue(fieldId, out var index))
            {
                if (row >= _fields[index].Data.Count)
                {
                    throw new Exception($"Data Count is {_fields[index].Data.Count}. Require Index is {row}.");        
                }
                
                return _fields[index].Data[row];
            }
            
            throw new Exception($"Field Id {fieldId}. Is Not Contains");
        }

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
                    addField.Data.Add(new DBDataContainer());
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
            _fieldIdToIndex = null;
            _fields.RemoveAll(g => g.ID == fieldId);
            this.Dirty();
        }


        /// <summary>
        /// データの追加
        /// </summary>
        internal void Add()
        {
            foreach (var g in _fields)
            {
                g.Data.Add(new DBDataContainer());
            }
            this.Dirty();
        }

        /// <summary>
        /// データの削除
        /// </summary>
        internal void RemoveRow(int index)
        {
            foreach (var g in _fields)
            {
                g.Data.RemoveAt(index);
            }
            this.Dirty();
        }

        /// <summary>
        /// データの初期化
        /// </summary>
        internal void ResetAt(int index)
        {
            foreach (var g in _fields)
            {
                g.Data[index] = new DBDataContainer();
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
            if (Define.Fields.Any())
            {
                for (var i = 0; i < DataCount; i++)
                {
                    var item = new YorozuDBEditorTreeViewItem(i, enumData);
                    foreach (var f in Define.Fields)
                    {
                        item.AddData(f, GetData(f.ID, i));
                    }
                    
                    root.AddChild(item);
                }
            }

            return root;
        }
    }

    [CustomEditor(typeof(YorozuDBDataObject))]
    internal class YorozuDBDataObjectEditor : Editor
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