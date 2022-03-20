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
    internal class TreeViewModule
    {
        private static class Styles
        {
            internal static GUILayoutOption TextLabelWidth;
            internal static GUILayoutOption TextFieldWidth;
            internal static GUILayoutOption DataLabelWidth;
            internal static GUILayoutOption DataFieldWidth;

            static Styles()
            {
                TextLabelWidth = GUILayout.Width(35f);
                TextFieldWidth = GUILayout.Width(100f);
                DataLabelWidth = GUILayout.Width(60f);
                DataFieldWidth = GUILayout.Width(60f);
            }
        }
        
        [SerializeField]
        private YorozuDBDataObject _data;

        [SerializeField]
        private MultiColumnHeaderState _columnHeaderState;
        [SerializeField]
        private TreeViewState _state;
        
        [NonSerialized]
        private bool _Initialized;
        
        private YorozuDBEditorTreeView _treeView;
        private string _name;
        private DataType _dataType;
        
        internal void OnGUI()
        {
            if (_data == null)
                return;
            
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                EditorGUILayout.LabelField($"[ {_data.name} ]");
                
                GUILayout.FlexibleSpace();

                //  Primaryを更新
                if (GUILayout.Button("Reset Key", EditorStyles.toolbarButton))
                {
                    _data.SetKey(string.Empty);
                    Reload();
                }
                
                GUILayout.Space(10);

                EditorGUILayout.LabelField("Name", Styles.TextLabelWidth);
                _name = EditorGUILayout.TextField(GUIContent.none, _name, EditorStyles.toolbarTextField,
                    Styles.TextFieldWidth);
                EditorGUILayout.LabelField("DataType", Styles.DataLabelWidth);
                _dataType = (DataType) EditorGUILayout.EnumPopup(GUIContent.none, _dataType, EditorStyles.toolbarPopup,
                    Styles.DataFieldWidth);
                using (new EditorGUI.DisabledScope(string.IsNullOrEmpty(_name)))
                {
                    if (GUILayout.Button("Add Row", EditorStyles.toolbarButton))
                    {
                        _data.AddGroup(_name, _dataType);
                        _name = "";
                        _dataType = default;
                        Reload();
                    }
                }

                GUILayout.Space(10);
                if (GUILayout.Button("Add Column", EditorStyles.toolbarButton))
                {
                    _data.AddColumn();
                    Reload();
                }
            }
            InitIfNeeded();
            
            var rect = GUILayoutUtility.GetRect(0, 100000, 0, 100000);
            _treeView.OnGUI(rect);
        }

        private void Reload() => _Initialized = false;

        internal void SetData(object param)
        {
            _data = param as YorozuDBDataObject;
            Reload();
            InitIfNeeded();
        }

        void InitIfNeeded()
        {
            if (_Initialized)
                return;
            
            if (_state == null)
            {
                _state = new TreeViewState();
            }

            var headerState = YorozuDBEditorUtility.CreateMultiColumnHeaderState(_data);
            if (MultiColumnHeaderState.CanOverwriteSerializedFields(_columnHeaderState, headerState))
                MultiColumnHeaderState.OverwriteSerializedFields(_columnHeaderState, headerState);
            _columnHeaderState = headerState;
            
            var multiColumnHeader = new YorozuDBEditorMultiColumnHeader(_columnHeaderState);
            multiColumnHeader.DeleteEvent += DeleteColumn;
            multiColumnHeader.RenameEvent += RenameColumn;
            multiColumnHeader.SetKeyEvent += HeaderSetKey;
            multiColumnHeader.ResizeToFit();
            
            _treeView = new YorozuDBEditorTreeView(_state, multiColumnHeader, _data);
            _treeView.DeleteRowEvent += DeleteRow;
            _treeView.ResetRowEvent += ResetRow;
            _treeView.SortEvent += InsertItems;
            
            _Initialized = true;
        }

        private void HeaderSetKey(int index)
        {
            var g = _data.Groups.ElementAt(index);
            _data.SetKey(_data.IsPrimaryGroup(g) ? string.Empty : g.Name);
            Reload();
        }

        /// <summary>
        /// 並べ替え
        /// </summary>
        private void InsertItems(int insertIndex, IList<int> targetIndexes)
        {
            _data.Insert(insertIndex, targetIndexes);
            Reload();
        }

        /// <summary>
        /// 名前を変更
        /// </summary>
        private void RenameColumn(int index, string rename)
        {
            if (string.IsNullOrEmpty(rename))
                return;
            
            _data.RenameGroup(index, rename);
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
                    _data.ResetRowValue(index);
                }
                Reload();
            }
        }

        /// <summary>
        /// 行削除
        /// </summary>
        private void DeleteColumn(int index)
        {
            var target = _data.Groups.ElementAt(index);
            _data.RemoveGroup(target.Name);
            Reload();
        }

        /// <summary>
        /// 列削除
        /// </summary>
        private void DeleteRow(IList<int> indexes)
        {
            var text = string.Join(',', indexes);
            if (EditorUtility.DisplayDialog("Warning", $"Can Delete row [{text}]?", "YES", "NO"))
            {
                // 降順にして消す
                foreach (var index in indexes.OrderByDescending(v => v))
                {
                    _data.RemoveRow(index);
                }
                Reload();
            }
        }
    }
}