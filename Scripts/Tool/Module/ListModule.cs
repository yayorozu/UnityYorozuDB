using System;
using UnityEditor;
using UnityEngine;

namespace Yorozu.DB
{
    [Serializable]
    internal class ListModule
    {
        [SerializeField]
        private YorozuDBDataObject[] _data;

        [SerializeField]
        private YorozuDBSetting _setting;

        private string _saveScriptFolder;
        private Vector2 _scrollPosition;

        internal event Action<YorozuDBDataObject> SelectEvent;

        internal void Initialize()
        {
            _setting = YorozuDBSetting.Load();
            _saveScriptFolder = AssetDatabase.GetAssetPath(_setting.ScriptExportFolder);
            _data = YorozuDBEditorUtility.LoadAllDataAsset();
        }

        internal void OnGUI()
        {
            if (_setting == null)
            {
                Initialize();
            }
            
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

                    _saveScriptFolder = path;
                }
            }

            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.TextField($"Script Generate Folder", _saveScriptFolder);
            }

            if (GUILayout.Button("Create Data Asset"))
            {
                var loadFrom = _data is {Length: > 0} ? AssetDatabase.GetAssetPath(_data[0]) : "Assets/"; 
                var path = EditorUtility.SaveFilePanelInProject("Select", "Data", "asset", "Select Create Path", loadFrom);
                if (!string.IsNullOrEmpty(path))
                {
                    var instance = ScriptableObject.CreateInstance<YorozuDBDataObject>();
                    AssetDatabase.CreateAsset(instance, path);
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                    _data = YorozuDBEditorUtility.LoadAllDataAsset();
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
            
            if (_data == null || _data.Length <= 0)
            {
                EditorGUILayout.HelpBox("Not Found", MessageType.Error);
                return;
            }
            
            using (var scroll = new EditorGUILayout.ScrollViewScope(_scrollPosition))
            {
                _scrollPosition = scroll.scrollPosition;
                foreach (var data in _data)
                {
                    if (GUILayout.Button(data.name))
                    {
                        SelectEvent?.Invoke(data);
                    }
                }
            }
        }
    }
}