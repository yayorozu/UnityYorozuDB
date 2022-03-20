using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Yorozu.DB
{
    internal class YorozuDBEditorWindow : EditorWindow
    {
        [MenuItem("Tools/YorozuDB")]
        private static void ShowWindow()
        {
            var window = GetWindow<YorozuDBEditorWindow>("YorozuDB");
            window.Show();
        }

        private static readonly Color SPLITTER_COLOR = new Color(0.2f, 0.2f, 0.2f);
        
        [SerializeField]
        private ListModule _list;
        [SerializeField]
        private DefineEditModule _editDefine;
        [SerializeField]
        private DataEditModule _editData;

        private enum Mode
        {
            None,
            Define,
            Data
        }

        [SerializeField]
        private Mode _mode;

        private void OnEnable()
        {
            if (_list == null)
            {
                _list = new ListModule();
            }
            // コンパイルでイベントは消える
            _list.SelectEvent += SelectDataEvent;
            
            if (_editDefine == null)
            {
                _editDefine = new DefineEditModule();
            }
            if (_editData == null)
            {
                _editData = new DataEditModule();
            }
        }

        private void SelectDataEvent(int instanceId)
        {
            var obj = EditorUtility.InstanceIDToObject(instanceId);
            if (obj == null)
                return;

            if (obj.GetType() == typeof(YorozuDBDataDefineObject))
            {
                _editDefine?.SetData(obj as YorozuDBDataDefineObject);
                _mode = Mode.Define;
            }
            if (obj.GetType() == typeof(YorozuDBDataObject))
            {
                _editData?.SetData(obj as YorozuDBDataObject);
                _mode = Mode.Data;
            }
            
            Repaint();
        }

        private void OnGUI()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                using (new EditorGUILayout.VerticalScope(GUILayout.Width(200)))
                {
                    if (_list.OnGUI()) 
                        Repaint();
                }
                
                var rect = GUILayoutUtility.GetRect(2, 2, 0, 100000);
                EditorGUI.DrawRect(rect, SPLITTER_COLOR);
                
                EditorGUILayout.Space(3);

                using (new EditorGUILayout.VerticalScope())
                {
                    var module = GetRightModule();
                    if (module != null)
                        if (module.OnGUI())
                            Repaint();
                }
            }
        }

        private YorozuDBEditorModule GetRightModule()
        {
            return _mode switch
            {
                Mode.Define => _editDefine,
                Mode.Data => _editData,
                _ => null
            };
        }
    }
}