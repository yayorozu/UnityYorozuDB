using System;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace Yorozu.DB
{
    [Serializable]
    internal class ListModule : YorozuDBEditorModule
    {
        [SerializeField]
        private YorozuDBDataDefineObject[] _defines;

        [SerializeField]
        private YorozuDBSetting _setting;

        private string _saveScriptFolder;

        internal event Action<int> SelectEvent;

        [SerializeField]
        private TreeViewState _state;

        private YorozuDBEditorDataListTreeView _treeView;

        internal void Initialize()
        {
            if (_setting == null)
            {
                _setting = YorozuDBSetting.Load();
                _saveScriptFolder = AssetDatabase.GetAssetPath(_setting.ScriptExportFolder);
                _defines = YorozuDBEditorUtility.LoadAllDefineAsset();
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
            }
        }

        internal override bool OnGUI()
        {
            Initialize();

            using (var check = new EditorGUI.ChangeCheckScope())
            {
                var newAsset = (DefaultAsset) EditorGUILayout.ObjectField("Script Export Folder",
                    _setting.ScriptExportFolder, typeof(DefaultAsset), false);
                if (check.changed && newAsset != null)
                {
                    var path = AssetDatabase.GetAssetPath(newAsset);
                    if (AssetDatabase.IsValidFolder(path))
                    {
                        _setting.SetFolder(path);
                    }

                    _saveScriptFolder = path;
                }
            }

            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.TextField($"Script Generate Folder", _saveScriptFolder);
            }

            // データ定義を作成
            if (GUILayout.Button("Create Data Define Asset"))
            {
                var loadFrom = _defines is {Length: > 0} ? AssetDatabase.GetAssetPath(_defines[0]) : "Assets/";
                var path = EditorUtility.SaveFilePanelInProject("Select", "Data", "asset", "Select Create Path", loadFrom);
                if (!string.IsNullOrEmpty(path))
                {
                    var instance = ScriptableObject.CreateInstance<YorozuDBDataDefineObject>();
                    AssetDatabase.CreateAsset(instance, path);
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                    _defines = YorozuDBEditorUtility.LoadAllDefineAsset();
                    _treeView.Reload();
                }
            }

            using (new EditorGUI.DisabledScope(_setting.ScriptExportFolder == null))
            {
                if (GUILayout.Button("Generate Script From Data"))
                {
                    var exportPath = AssetDatabase.GetAssetPath(_setting.ScriptExportFolder);
                    YorozuDBEditorUtility.GenerateScript(exportPath);
                }
            }
            
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Data", EditorStyles.boldLabel);

            var rect = GUILayoutUtility.GetRect(0, 100000, 0, 100000);
            _treeView.OnGUI(rect);
            return false;
        }
    }
}