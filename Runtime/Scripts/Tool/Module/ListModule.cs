#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace Yorozu.DB
{
    [Serializable]
    internal class ListModule : YorozuDBEditorModule
    {
        private static class Styles
        {
            internal static Texture2D TextureRefresh;
            
            static Styles()
            {
                TextureRefresh = EditorResources.Load<Texture2D>("d_Refresh");
            }
        }
        
        internal event Action<int> SelectEvent;

        [SerializeField]
        private TreeViewState _state;

        private YorozuDBEditorDataListTreeView _treeView;

        private bool _hasEnum; 

        internal void Initialize()
        {
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

                _hasEnum = YorozuDBEditorInternalUtility.LoadEnumDataAsset() != null;
            }
        }

        internal override bool OnGUI()
        {
            Initialize();
            
            // データ定義を作成
            using (new GUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Create Data Define Asset"))
                {
                    if (YorozuDBEditorInternalUtility.CreateDefineAsset())
                    {
                        _treeView.Reload();
                    }
                }

                if (GUILayout.Button(Styles.TextureRefresh))
                {
                    _treeView.Reload();
                }                
            }

            if (!_hasEnum)
            {
                if (GUILayout.Button("Create Enum Data Asset"))
                {
                    if (YorozuDBEditorInternalUtility.CreateEnumAsset())
                    {
                        _treeView.Reload();
                        _hasEnum = true;
                    }
                }
            }
            
            EditorGUILayout.Space(1);
            
            EditorGUILayout.LabelField("Define, Data, Enum", EditorStyles.boldLabel);

            var rect = GUILayoutUtility.GetRect(0, 100000, 0, 100000);
            _treeView.OnGUI(rect);

            if (GUILayout.Button("Generate Script From Define"))
            {
                YorozuDBScriptGenerator.GenerateScript();
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
            if (YorozuDBEditorInternalUtility.CreateDataAsset(define as YorozuDBDataDefineObject, parentPath))
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
                
                var dependencyAssets = YorozuDBEditorInternalUtility.LoadAllDataAsset(obj as YorozuDBDataDefineObject);
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

#endif