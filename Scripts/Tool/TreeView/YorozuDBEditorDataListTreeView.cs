#if UNITY_EDITOR

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
            ExpandAll();
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
            
            // Enum追加
            var enumData = YorozuDBEditorUtility.LoadEnumDataAsset();
            if (enumData != null)
            {
                root.AddChild(new TreeViewItem(enumData.GetInstanceID(), 0, "Enum"));
            }
            
            return root;
        }
        
        protected override IList<TreeViewItem> BuildRows(TreeViewItem root)
        {
            _rows.Clear();
            
            if (!root.hasChildren)
                return _rows;

            return base.BuildRows(root);
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

            if (obj.GetType() != typeof(YorozuDBEnumDataObject))
            {
                menu.AddItem(new GUIContent("Delete"), false, () =>
                {
                    DeleteItemsEvent?.Invoke(GetSelection());
                });
            }
    
            menu.ShowAsContext();
        }

        protected override bool CanRename(TreeViewItem item)
        {
            // EnumじゃなかったらRename可
            var asset = EditorUtility.InstanceIDToObject(item.id);
            return asset.GetType() != typeof(YorozuDBEnumDataObject);
        }

        protected override void RenameEnded(RenameEndedArgs args)
        {
            if (!args.acceptedRename)
                return;

            if (string.IsNullOrEmpty(args.newName))
                return;
                    
            if (args.newName == args.originalName)
                return;
            
            // 同じ名前のアセット内科
            var asset = EditorUtility.InstanceIDToObject(args.itemID);
            var assetPath = AssetDatabase.GetAssetPath(asset);
            var parentPath = System.IO.Path.GetDirectoryName(assetPath);
            var guids = AssetDatabase.FindAssets("*", new[] {parentPath});
            foreach (var path in guids.Select(AssetDatabase.GUIDToAssetPath))
            {
                if (!path.EndsWith(".asset"))
                    continue;

                var fileName = System.IO.Path.GetFileNameWithoutExtension(path);
                // 前のやつ
                if (fileName == args.originalName)
                    continue;

                // 新しい名前に一致するものがあったため無理
                if (fileName == args.newName)
                {
                    Debug.LogError("file with the same name already exists.");
                    return;
                }
            }
            
            // ここまで来たら許可
            AssetDatabase.RenameAsset(assetPath, args.newName);

            var item = FindItem(args.itemID, rootItem);
            item.displayName = args.newName;
            
            EditorUtility.SetDirty(asset);
            AssetDatabase.SaveAssets();
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
             Selection.activeInstanceID = id;
        }
    }
}

#endif