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
        internal event Action<IList<int>> DeleteItemsEvent;
        internal event Action<int> CreateDataEvent;
        
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
            
            if (!root.hasChildren)
                return _rows;
            
            foreach (var child in root.children)
            {
                _rows.Add(child);
                if (!child.hasChildren) 
                    continue;
                
                foreach (var child2 in child.children)
                {
                    _rows.Add(child2);
                }
            }
            
            return _rows;
        }

        protected override void ContextClickedItem(int id)
        {
            var ev = Event.current;
            ev.Use();
            var menu = new GenericMenu();
            
            var obj = EditorUtility.InstanceIDToObject(id);
            if (obj.GetType() == typeof(YorozuDBDataDefineObject))
            {
                menu.AddItem(new GUIContent("Create Data"), false, () => 
                {
                    CreateDataEvent?.Invoke(id);
                });
            }
            
            menu.AddItem(new GUIContent("Delete"), false, () =>
            {
                DeleteItemsEvent?.Invoke(GetSelection());
            });
    
            menu.ShowAsContext();
        }

        protected override void SelectionChanged(IList<int> selectedIds)
        {
            SelectItemEvent?.Invoke(selectedIds.First());
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