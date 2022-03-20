#if UNITY_EDITOR

using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using Yorozu.DB.TreeView;

namespace Yorozu.DB
{
    internal partial class YorozuDBDataObject
    {
        internal void Dirty()
        {
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
        }
        
        /// <summary>
        /// Primary を変更
        /// </summary>
        internal void SetKey(string name)
        {
            _keyName = name;
            Dirty();
        }

        /// <summary>
        /// データの追加
        /// </summary>
        internal void AddGroup(string name, DataType dataType)
        {
            name = name.Trim();
            // 重複禁止
            if (_groups.Any(g => g.Name == name))
                return;

            var group = new DBDataGroup()
            {
                Name = name,
                DataType = dataType,
            };

            if (_groups.Any())
            {
                var count = _groups.First().Data.Count();
                for (var i = 0; i < count; i++)
                {
                    group.AddData();
                }
            }
            _groups.Add(group);
            Dirty();
        }

        /// <summary>
        /// 名前を変更する
        /// </summary>
        internal void RenameGroup(int index, string newName)
        {
            newName = newName.Trim();
            // すでに存在する
            if (_groups.Any(g => g.Name == newName))
                return;

            _groups[index].Name = newName;
            Dirty();
        }
        
        
        /// <summary>
        /// 特定のグループを削除
        /// </summary>
        internal void RemoveGroup(string name)
        {
            // 削除
            _groups.RemoveAll(g => g.Name == name);
            Dirty();
        }

        /// <summary>
        /// 1行削除
        /// </summary>
        internal void RemoveRow(int index)
        {
            foreach (var group in Groups)
            {
                group.RemoveAt(index);
            }
            Dirty();
        }
        
        /// <summary>
        /// 値を初期化
        /// </summary>
        internal void ResetRowValue(int index)
        {
            foreach (var group in Groups)
            {
                group.ResetAt(index);
            }
            Dirty();
        }

        internal void AddColumn()
        {
            foreach (var group in Groups)
            {
                group.AddData();
            }
            Dirty();
        }

        internal TreeViewItem CreateTree()
        {
            var root = new TreeViewItem(-1, -1, "root");
            if (_groups.Any())
            {
                var count = _groups.First().Data.Count();
                for (var i = 0; i < count; i++)
                {
                    var item = new YorozuDBEditorTreeViewItem(i);
                    foreach (var g in _groups)
                    {
                        item.AddData(g.DataType, g.Data.ElementAt(i));
                    }
                    
                    root.AddChild(item);
                }
            }

            return root;
        }
        
        /// <summary>
        /// 並べ替え
        /// </summary>
        /// <param name="insertIndex"></param>
        /// <param name="targetIndexes"></param>
        internal void Insert(int insertIndex, IList<int> targetIndexes)
        {
            var descIndexes = targetIndexes.OrderByDescending(v => v);
            foreach (var g in _groups)
            {
                g.Insert(insertIndex, descIndexes);
            }
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

#endif
