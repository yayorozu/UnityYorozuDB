using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace Yorozu.DB
{
    [Serializable]
    internal class ListModule : YorozuDBEditorModule
    {
        [SerializeField]
        private YorozuDBSetting _setting;

        internal event Action<int> SelectEvent;

        [SerializeField]
        private TreeViewState _state;

        private YorozuDBEditorDataListTreeView _treeView;

        private bool _hasEnum; 

        internal void Initialize()
        {
            if (_setting == null)
            {
                _setting = YorozuDBSetting.Load();
            }

            if (_state == null)
                _state = new TreeViewState();

            if (_treeView == null)
            {
                _treeView = new YorozuDBEditorDataListTreeView(_state);
                _treeView.SelectItemEvent += id =>
                {
                    SelectEvent?.Invoke(id);
                };
                // 選択したやつを全部削除
                _treeView.DeleteItemsEvent += DeleteAssets;
                _treeView.CreateDataEvent += CreateDataAsset;

                _hasEnum = YorozuDBEditorUtility.LoadEnumDataAsset() != null;
            }
        }

        internal override bool OnGUI()
        {
            Initialize();

            // データ定義を作成
            if (GUILayout.Button("Create Data Define Asset"))
            {
                if (YorozuDBEditorUtility.CreateDefineAsset())
                {
                    _treeView.Reload();
                }
            }

            if (!_hasEnum)
            {
                if (GUILayout.Button("Create Data Enum Asset"))
                {
                    if (YorozuDBEditorUtility.CreateEnumAsset())
                    {
                        _treeView.Reload();
                        _hasEnum = true;
                    }
                }
            }

            EditorGUILayout.Space(1);
            
            EditorGUILayout.LabelField("Define & Data", EditorStyles.boldLabel);

            var rect = GUILayoutUtility.GetRect(0, 100000, 0, 100000);
            _treeView.OnGUI(rect);
            
            using (var check = new EditorGUI.ChangeCheckScope())
            {
                var newAsset = (DefaultAsset) EditorGUILayout.ObjectField("Script Export Folder", _setting.ScriptExportFolder, typeof(DefaultAsset), false);
                if (check.changed && newAsset != null)
                {
                    var path = AssetDatabase.GetAssetPath(newAsset);
                    if (AssetDatabase.IsValidFolder(path))
                    {
                        _setting.SetFolder(path);
                    }
                }
            }
            
            using (new EditorGUI.DisabledScope(_setting.ScriptExportFolder == null))
            {
                if (GUILayout.Button("Generate Script From Define"))
                {
                    var exportPath = AssetDatabase.GetAssetPath(_setting.ScriptExportFolder);
                    YorozuDBEditorUtility.GenerateScript(exportPath);
                }
            }
            
            EditorGUILayout.Space(1);
            
            return false;
        }

        /// <summary>
        /// Data を作成
        /// </summary>
        private void CreateDataAsset(int parentId)
        {
            var define= EditorUtility.InstanceIDToObject(parentId);
            var path = AssetDatabase.GetAssetPath(define);
            var parentPath = System.IO.Path.GetDirectoryName(path);
            if (YorozuDBEditorUtility.CreateDataAsset(define as YorozuDBDataDefineObject, parentPath))
            {
                _treeView?.Reload();
            }   
        }

        /// <summary>
        /// 全削除
        /// </summary>
        private void DeleteAssets(IList<int> ids)
        {
            if (!EditorUtility.DisplayDialog("Warning", $"Delete Select Assets?",
                    "YES",
                    "NO"))
            {
                return;
            }
            
            var deletes = ids.Select(EditorUtility.InstanceIDToObject)
                .Where(o => o.GetType() != typeof(YorozuDBEnumDataObject));
            
            foreach (var id in ids)
            {
                var obj = EditorUtility.InstanceIDToObject(id);
                // defineだったら依存しているやつを全部削除
                if (obj.GetType() != typeof(YorozuDBDataDefineObject)) 
                    continue;
                
                var dependencyAssets = YorozuDBEditorUtility.LoadAllDataAsset(obj as YorozuDBDataDefineObject);
                deletes = deletes.Concat(dependencyAssets);
            }

            var deletePaths = deletes.Distinct()
                .Select(AssetDatabase.GetAssetPath)
                .ToArray();
            var fails = new List<string>();
            AssetDatabase.DeleteAssets(deletePaths, fails);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            _treeView?.Reload();
        }
    }
}