using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace Yorozu.DB
{
    internal class YorozuDBEditorDataListTreeView : UnityEditor.IMGUI.Controls.TreeView
    {
        private readonly List<TreeViewItem> _rows = new List<TreeViewItem>(100);

        /// <summary>
        /// TreeViewItemのクリック
        /// </summary>
        internal event Action<int> SelectItemEvent;
        
        public YorozuDBEditorDataListTreeView(TreeViewState state) : base(state)
        {
            showAlternatingRowBackgrounds = true;
            showBorder = true;
            Reload();
        }

        protected override TreeViewItem BuildRoot()
        {
            var defineAssets = YorozuDBEditorUtility.LoadAllDefineAsset();
            var dataAssets = YorozuDBEditorUtility.LoadAllDataAsset();
            var root = new TreeViewItem(-1, -1, "root");
            
            foreach (var asset in defineAssets)
            {
                var define = new TreeViewItem(asset.GetInstanceID(), 0, asset.name);

                var currentDefineAssets = dataAssets.Where(d => d.Define == asset);
                foreach (var data in currentDefineAssets)
                {
                    var child = new TreeViewItem(data.GetInstanceID(), 1, data.name);
                    define.AddChild(child);
                }
             
                root.AddChild(define);
            }

            return root;
        }
        
        protected override IList<TreeViewItem> BuildRows(TreeViewItem root)
        {
            _rows.Clear();

            if (root.hasChildren)
            {
                foreach (var child in root.children)
                {
                    _rows.Add(child);
                }
            }
            
            SetupParentsAndChildrenFromDepths(root, _rows);

            return _rows;
        }

        protected override void ContextClickedItem(int id)
        {
            var obj = EditorUtility.InstanceIDToObject(id);
            
        }

        protected override void SingleClickedItem(int id)
        {
            SelectItemEvent?.Invoke(id);
        }

        protected override void DoubleClickedItem(int id)
        {
            SelectItemEvent?.Invoke(id);
        }
    }
}