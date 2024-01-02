using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using Yorozu.DB.TreeView;
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
            internal bool IsFix;

            [SerializeField]
            internal DataContainer FixData;

            [SerializeField]
            internal List<DataContainer> Data = new List<DataContainer>();

            internal Field(int fieldId, DataType dataType)
            {
                ID = fieldId;
                FixData = new DataContainer(dataType);
            }

#if UNITY_EDITOR
            /// <summary>
            /// 入れ替え
            /// </summary>
            internal void Insert(int insertIndex, IEnumerable<int> targetIndexes)
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

        /// <summary>
        /// 拡張データ
        /// </summary>
        [SerializeField]
        internal ScriptableObject ExtendFieldsObject;

        [SerializeField]
        private List<Field> _fields = new List<Field>();

        /// <summary>
        /// Key が Int 際にデータを追加時に前の値+1するか
        /// </summary>
        [SerializeField]
        internal bool AutoIncrementKey;

        /// <summary>
        /// データ数
        /// </summary>
        internal int DataCount
        {
            get
            {
                if (_fields == null ||
                    !_fields.Any())
                {
#if UNITY_EDITOR
                    // こっちの定義がない場合は拡張側を見る
                    var extendCount = YorozuDBExtendUtility.DataCount(ExtendFieldsObject);
                    return extendCount;
#else
                    return 0;
#endif
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
                .Select(f => f.IsFix ? f.FixData : f.Data[row])
                .First();
        }

#if UNITY_EDITOR

        /// <summary>
        /// 1行あたりに含まれる最大の数
        /// </summary>
        internal int MaxRowLength
        {
            get
            {
                if (_fields == null ||
                    !_fields.Any())
                    return 1;
                return _fields.Max(f =>
                {
                    if (f.Data == null ||
                        f.Data.Count <= 0)
                        return 1;

                    var find = Define.Fields.First(f2 => f2.ID == f.ID);
                    if (f.IsFix)
                    {
                        return f.FixData.GetSize(find.DataType);
                    }

                    return f.Data.Max(d => d.GetSize(find.DataType));
                });
            }
        }

        internal bool IsFixField(int fieldId) => _fields.First(f => f.ID == fieldId).IsFix;

        /// <summary>
        /// 固定の設定を行えるように
        /// </summary>
        internal void DrawFixFields(YorozuDBEnumDataObject enumData)
        {
            var fields = Define.Fields;
            EditorGUILayout.LabelField("Fix Fields", EditorStyles.boldLabel);
            foreach (var field in fields)
            {
                using (new EditorGUI.IndentLevelScope())
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        var f = _fields.First(f => f.ID == field.ID);
                        f.IsFix = EditorGUILayout.ToggleLeft(field.Name, f.IsFix);
                        if (f.IsFix)
                        {
                            var rect = GUILayoutUtility.GetRect(0, 100000, EditorGUIUtility.singleLineHeight,
                                EditorGUIUtility.singleLineHeight);
                            using (var check = new EditorGUI.ChangeCheckScope())
                            {
                                f.FixData.DrawField(rect, field, GUIContent.none, enumData);
                                if (check.changed)
                                {
                                    this.Dirty();
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// フィールドの追加
        /// </summary>
        internal void AddField(int fieldId)
        {
            var findIndex = _fields.FindIndex(g => g.ID == fieldId);
            if (findIndex >= 0)
                return;

            var targetField = Define.Fields.First(f => f.ID == fieldId);
            var addField = new Field(fieldId, targetField.DataType);
            // 既存のフィールドのデータ分だけ追加する必要がある
            if (DataCount > 0)
            {
                for (var i = 0; i < DataCount; i++)
                {
                    addField.Data.Add(new DataContainer(targetField.DataType));
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

        internal int MaxKey()
        {
            var key = Define.KeyField;
            if (key == null)
                return 0;

            var keyField = _fields.FirstOrDefault(f => f.ID == key.ID);
            if (keyField == null)
                return 0;

            return keyField.Data.Max(d => d.Int);
        }

        /// <summary>
        /// データの追加
        /// </summary>
        internal void AddRow(int? copyIndex = null)
        {
            var keyField = Define.KeyField;
            foreach (var field in _fields)
            {
                var targetField = Define.Fields.First(f => f.ID == field.ID);
                var addData = copyIndex.HasValue ? field.Data[copyIndex.Value].Copy() : targetField.DefaultValue.Copy();

                if (keyField != null &&
                    keyField.ID == field.ID &&
                    !copyIndex.HasValue &&
                    AutoIncrementKey &&
                    keyField.DataType == DataType.Int)
                {
                    var maxId = 1;
                    if (field.Data != null &&
                        field.Data.Count > 0)
                        maxId = field.Data.Max(d => d.Int) + 1;

                    addData.Int = maxId;
                }

                field.Data.Add(addData);
            }

            // 対象のフィールド追加
            if (ExtendFieldsObject != null)
            {
                YorozuDBExtendUtility.AddFields(ExtendFieldsObject, copyIndex);
            }

            this.Dirty();
        }

        /// <summary>
        /// 初期化
        /// </summary>
        internal void Clear()
        {
            var indexes = new List<int>();
            for (var i = 0; i < DataCount; i++)
            {
                indexes.Add(i);
            }

            RemoveRows(indexes.OrderByDescending(v => v));
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

            YorozuDBExtendUtility.RemoveFields(ExtendFieldsObject, descIndexes);
            this.Dirty();
        }

        /// <summary>
        /// データの初期化
        /// </summary>
        internal void ResetAt(int index)
        {
            foreach (var g in _fields)
            {
                var targetField = Define.Fields.First(f => f.ID == g.ID);
                g.Data[index] = new DataContainer(targetField.DataType);
            }

            this.Dirty();
        }

        /// <summary>
        /// 入れ替え
        /// </summary>
        internal void Insert(int insertIndex, IEnumerable<int> targetIndexes)
        {
            foreach (var g in _fields)
            {
                g.Insert(insertIndex, targetIndexes);
            }

            // 拡張分も入れ替え
            YorozuDBExtendUtility.Insert(ExtendFieldsObject, insertIndex, targetIndexes);
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

        /// <summary>
        /// フィールドの値更新
        /// </summary>
        internal void UpdateDefaultValue(int fieldId, DataContainer src)
        {
            var index = _fields.FindIndex(f => f.ID == fieldId);
            if (index < 0)
                return;

            for (var i = 0; i < _fields[index].Data.Count; i++)
            {
                _fields[index].Data[i] = src.Copy();
            }

            this.Dirty();
        }

        internal void Duplicate(int index)
        {
            AddRow(index);
            // 入れ替え
            Insert(index + 1, new int[] {DataCount - 1});
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