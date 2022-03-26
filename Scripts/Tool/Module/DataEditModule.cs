#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using Yorozu.DB.TreeView;

namespace Yorozu.DB
{
    [Serializable]
    internal class DataEditModule : YorozuDBEditorModule
    {
        [SerializeField]
        private YorozuDBDataObject _data;

        [SerializeField]
        private MultiColumnHeaderState _columnHeaderState;

        [SerializeField]
        private TreeViewState _state;

        [NonSerialized]
        private bool _Initialized;

        private YorozuDBEditorDataTreeView _treeView;
        private int _fieldCount;
        
        internal void SetData(object param)
        {
            _data = param as YorozuDBDataObject;
            Reload();
            InitIfNeeded();
        }

        internal override bool OnGUI()
        {
            if (_data == null)
                return false;

            InitIfNeeded();

            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                EditorGUILayout.LabelField($"Data Editor: 【{_data.name}】", EditorStyles.boldLabel);
                
                GUILayout.FlexibleSpace();

                using (new EditorGUI.DisabledScope(_fieldCount <= 0))
                {
                    if (GUILayout.Button("Add Row", EditorStyles.toolbarButton))
                    {
                        _data.AddRow();
                        Reload();
                        return true;
                    }
                }
            }

            var rect = GUILayoutUtility.GetRect(0, 100000, 0, 100000);
            _treeView.OnGUI(rect);
            
            return false;
        }

        private void Reload() => _Initialized = false;

        void InitIfNeeded()
        {
            if (_Initialized)
                return;

            if (_state == null)
            {
                _state = new TreeViewState();
            }

            var headerState = YorozuDBEditorDataTreeView.CreateMultiColumnHeaderState(_data);
            if (MultiColumnHeaderState.CanOverwriteSerializedFields(_columnHeaderState, headerState))
                MultiColumnHeaderState.OverwriteSerializedFields(_columnHeaderState, headerState);
            _columnHeaderState = headerState;

            var multiColumnHeader = new YorozuDBEditorMultiColumnHeader(_columnHeaderState);
            multiColumnHeader.DeleteEvent += DeleteColumn;
            multiColumnHeader.ChangeWidthEvent += ColumnChangeWidth;
            
            multiColumnHeader.ResizeToFit();

            _treeView = new YorozuDBEditorDataTreeView(_state, multiColumnHeader, _data);
            _treeView.DeleteRowEvent += DeleteRow;
            _treeView.ResetRowEvent += ResetRow;
            _treeView.SortEvent += InsertItems;
            _treeView.AutoIdEvent += SetAutoId;
            _treeView.SameIdEvent += SetSameId;
            
            var fields =YorozuDBExtendUtility.FindFields(_data.Define.ExtendFieldsObject);
            _fieldCount = _data.Define.Fields.Count + fields.Count;
            
            // 足りない分の補充
            YorozuDBExtendUtility.FitFieldsSize(_data.Define.ExtendFieldsObject, _data.DataCount);

            _Initialized = true;
        }

        private void ColumnChangeWidth(int index, float width)
        {
            // Extend
            if (index >= _data.Define.Fields.Count)
            {
                index -= _data.Define.Fields.Count;
                var fields = YorozuDBExtendUtility.FindFields(_data.Define.ExtendFieldsObject);
                var widthIndex = _data.Define.ExtendFieldWidths.FindIndex(v => v.Name == fields[index].Name);
                if (widthIndex >= 0)
                {
                    if (Math.Abs(_data.Define.ExtendFieldWidths[widthIndex].Width - width) > 10f)
                    {
                        _data.Define.ExtendFieldWidths[widthIndex].Width = width;
                    }
                }
                return;
            }

            if (Math.Abs(_data.Define.Fields[index].GUIWidth - width) > 10f)
            {
                _data.Define.Fields[index].GUIWidth = width;
            }
        }

        /// <summary>
        /// 選択したやつに最初のIDから順番に振り分けていく
        /// </summary>
        private void SetAutoId(IList<int> indexes)
        {
            var keyField = _data.Define.KeyField;
            if (keyField == null)
                return;
            
            var minIndex = indexes.Min();
            var incId = Mathf.Max(_data.GetData(keyField.ID, minIndex).Int, 1);
            foreach (var index in indexes)
            {
                _data.GetData(keyField.ID, index).UpdateInt(incId++); 
            }
            _data.Dirty();
            Reload();
        }

        /// <summary>
        /// 同じID振り分け
        /// </summary>
        private void SetSameId(IList<int> indexes)
        {
            var keyField = _data.Define.KeyField;
            if (keyField == null)
                return;
            
            var minIndex = indexes.Min();
            var replaceId = _data.GetData(keyField.ID, minIndex).Int;
            foreach (var index in indexes)
            {
                _data.GetData(keyField.ID, index).UpdateInt(replaceId); 
            }
            _data.Dirty();
            Reload();
        }

        /// <summary>
        /// 並べ替え
        /// </summary>
        private void InsertItems(int insertIndex, IList<int> targetIndexes)
        {
            _data.Insert(insertIndex, targetIndexes.OrderByDescending(v => v));
            Reload();
        }

        /// <summary>
        /// Row の値を全部リセット
        /// </summary>
        private void ResetRow(IList<int> indexes)
        {
            var text = string.Join(',', indexes);
            if (EditorUtility.DisplayDialog("Warning", $"Can Reset row value [{text}] ?", "YES", "NO"))
            {
                foreach (var index in indexes)
                {
                    _data.ResetAt(index);
                }

                Reload();
            }
        }

        /// <summary>
        /// 列削除
        /// </summary>
        private void DeleteColumn(MultiColumnHeaderState.Column column)
        {
            if (EditorUtility.DisplayDialog("Warning", $"Delete {column.headerContent.text} Field?\nFields of related data will be Deleted.",
                    "YES",
                    "NO"))
            {
                var fieldId = column.userData;
                _data.Define.RemoveField(fieldId);
                Reload();
            }
        }

        /// <summary>
        /// 行削除
        /// </summary>
        private void DeleteRow(IList<int> indexes)
        {
            var text = string.Join(',', indexes);
            if (EditorUtility.DisplayDialog("Warning", $"Can Delete row [{text}]?", "YES", "NO"))
            {
                _data.RemoveRows(indexes.OrderByDescending(v => v));
                Reload();
            }
        }
    }
}

#endif