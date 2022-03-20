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

        [SerializeReference]
        private List<YorozuDBEditorModule> _modules;
        private YorozuDBEditorModule current => _modules[_moduleIndex];

        [SerializeField]
        private int _moduleIndex;

        [SerializeField]
        private ListModule _left;
        [SerializeField]
        private TreeViewModule _right;

        private void OnEnable()
        {
            if (_left == null)
            {
                _left = new ListModule();
                _left.SelectEvent += SelectDataEvent;
            }

            if (_right == null)
            {
                _right = new TreeViewModule();
            }
        }

        private void SelectDataEvent(YorozuDBDataObject data)
        {
            _right?.SetData(data);
            Repaint();
        }

        internal void ChangeModule(Type nextType, object param)
        {
            var index = _modules.FindIndex(m => m.GetType() == nextType);
            if (index < 0)
                return;

            current.OnExit();
            _moduleIndex = index;
            current.OnEnter(param);
            Repaint();
        }

        private void OnGUI()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                using (new EditorGUILayout.VerticalScope(GUILayout.Width(200)))
                {
                    _left.OnGUI();
                }

                using (new EditorGUILayout.VerticalScope("helpbox", GUILayout.Width(1), GUILayout.ExpandHeight(true)))
                {
                }
                
                using (new EditorGUILayout.VerticalScope())
                {
                    _right.OnGUI();
                }
            }
        }
    }
}