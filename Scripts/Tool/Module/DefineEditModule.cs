using System;
using UnityEditor;
using UnityEngine;

namespace Yorozu.DB
{
    /// <summary>
    /// 定義情報をいじる用
    /// </summary>
    [Serializable]
    internal class DefineEditModule : YorozuDBEditorModule
    {
        private static class Style
        {
            internal static GUIContent Rename;
            internal static GUIContent Delete;
            internal static GUIContent Key;
            internal static GUIContent UnKey;
            
            static Style()
            {
                Rename = EditorGUIUtility.TrIconContent("d_Grid.PaintTool");
                Delete = EditorGUIUtility.TrIconContent("d_TreeEditor.Trash");
                Key = EditorGUIUtility.TrIconContent("Favorite On Icon");
                UnKey = EditorGUIUtility.TrIconContent("TestIgnored");
            }
        }
        [SerializeField]
        private YorozuDBDataDefineObject _data;

        [SerializeField]
        private string _name;

        [SerializeField]
        private DataType _dataType;

        private int _renameID = -1;
        private string _temp;
        
        private static readonly string EditorField = "EditorField";
        
        internal void SetData(YorozuDBDataDefineObject data)
        {
            _data = data;
        }

        internal override bool OnGUI()
        {
            if (_data == null)
                return false;

            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                EditorGUILayout.LabelField($"[ {_data.name} ]");
            }

            GUILayout.Space(10);

            EditorGUILayout.LabelField("Add Field", EditorStyles.boldLabel);
            using (new EditorGUILayout.VerticalScope("box"))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    _name = EditorGUILayout.TextField("Name", _name);
                    _dataType = (DataType) EditorGUILayout.EnumPopup("DataType", _dataType);
                    
                }

                using (new EditorGUILayout.HorizontalScope())
                {
                    GUILayout.FlexibleSpace();
                    
                    using (new EditorGUI.DisabledScope(string.IsNullOrEmpty(_name)))
                    {
                        if (GUILayout.Button("Add Field"))
                        {
                            _data.AddField(_name, _dataType);
                            _name = "";
                            _dataType = default;
                        }
                    }
                }
            }

            GUILayout.Space(10);

            EditorGUILayout.LabelField("Fields", EditorStyles.boldLabel);

            var rect = GUILayoutUtility.GetRect(0, 100000, 0, 100000);
            
            rect.height = EditorGUIUtility.singleLineHeight;
            var initialX = rect.x;
            
            using (new EditorGUI.IndentLevelScope())
            {
                foreach (var field in _data.Fields)
                {
                    // Key として有効なのであればボタンを表示
                    if (field.ValidKey())
                    {
                        rect.x = initialX - 4f;
                        rect.width = 20;

                        var content = _data.IsKeyField(field) ? Style.Key : Style.UnKey;
                        if (GUI.Button(rect, content, EditorStyles.label))
                        {
                            _data.SetKey(field.ID);
                        }
                    }
                    
                    rect.x = initialX;
                    rect.width = 100;
                    // リネーム状態だったら TextField を表示
                    if (_renameID == field.ID)
                    {
                        rect.x += EditorGUI.indentLevel * 15f;
                        
                        GUI.SetNextControlName(EditorField);
                        _temp = GUI.TextField(rect, _temp);
                        var e = Event.current;
                        if (e.keyCode == KeyCode.Return && _renameID != -1)
                        {
                            _data.RenameField(_renameID, _temp);
                            _renameID = -1;
                            return true;
                        }

                        if (e.keyCode == KeyCode.Escape)
                        {
                            _renameID = -1;
                            return true;
                        }

                        rect.x -= EditorGUI.indentLevel * 15f;
                    }
                    else
                    {
                        EditorGUI.LabelField(rect, field.Name);   
                    }
                    
                    rect.x += rect.width;

                    rect.width = 140;
                    using (new EditorGUI.DisabledScope(true))
                    {
                        field.DataType = (DataType) EditorGUI.EnumPopup(rect, GUIContent.none, field.DataType);
                    }

                    rect.x += rect.width + EditorGUIUtility.standardVerticalSpacing;
                    rect.width = 16;

                    // リネームボタン
                    if (GUI.Button(rect, Style.Rename, EditorStyles.label))
                    {
                        if (_renameID == field.ID)
                            _renameID = -1;
                        else
                            _renameID = field.ID;
                        
                        _temp = field.Name;
                        GUI.FocusControl(EditorField);
                        return true;
                    }
                    
                    rect.x += rect.width + EditorGUIUtility.standardVerticalSpacing;
                    
                    // 削除ボタン
                    if (GUI.Button(rect, Style.Delete, EditorStyles.label))
                    {
                        if (EditorUtility.DisplayDialog("Warning", $"Delete {field.Name}?",
                                "YES",
                                "NO"))
                        {
                            _data.RemoveField(field.ID);
                            GUIUtility.ExitGUI();
                        }
                    }

                    rect.y += rect.height + EditorGUIUtility.standardVerticalSpacing;
                }
            }

            return false;
        }
    }
}