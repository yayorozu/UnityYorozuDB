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
            
            static Style()
            {
                Rename = EditorGUIUtility.TrIconContent("d_Grid.PaintTool");
                Delete = EditorGUIUtility.TrIconContent("d_TreeEditor.Trash");
            }
        }
        [SerializeField]
        private YorozuDBDataDefineObject _data;

        [SerializeField]
        private string _name;

        [SerializeField]
        private DataType _dataType;

        [SerializeField]
        private GUIContent _content = new GUIContent();

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
            
            
            //  Primaryを更新
            // if (GUILayout.Button("Reset Key"))
            // {
            //     _data.SetKey(string.Empty);
            // }

            GUILayout.Space(10);

            using (new EditorGUILayout.HorizontalScope())
            {
                _name = EditorGUILayout.TextField("Name", _name);
                _dataType = (DataType) EditorGUILayout.EnumPopup("DataType", _dataType);
                
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
            
            GUILayout.Space(10);

            EditorGUILayout.LabelField("Fields", EditorStyles.boldLabel);

            var data = new DBDataContainer();
            var rect = GUILayoutUtility.GetRect(0, 100000, 0, 100000);
            
            rect.height = EditorGUIUtility.singleLineHeight;
            
            using (new EditorGUI.IndentLevelScope())
            {
                foreach (var field in _data.Fields)
                {
                    rect.width = 300;
                    _content.text = $"{field.Name} ( {field.DataType.ToString()} )";
                    var boxyRect = EditorGUI.PrefixLabel(rect, _content);
                    YorozuDBEditorUtility.DrawDataField(boxyRect, field.DataType, data, GUIContent.none);
                    
                    // Key として有効なのであればボタンを表示
                    if (field.ValidKey())
                    {
                        rect.x -= 3f;
                        var prevWidth = rect.width;
                        rect.width = 24;

                        var content = _data.IsKeyField(field)
                            ? EditorGUIUtility.TrIconContent("d_Favorite")
                            : EditorGUIUtility.TrIconContent("TestNormal");
                        
                        if (GUI.Button(rect, content, EditorStyles.label))
                        {
                            _data.SetKey(field.ID);
                        }

                        rect.width = prevWidth;
                        rect.x += 3f;
                    }

                    // リネーム状態だったら TextField を表示
                    if (_renameID == field.ID)
                    {
                        rect.x += EditorGUI.indentLevel * 15f;
                        rect.width = EditorGUIUtility.labelWidth;

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

                    boxyRect.x += boxyRect.width + EditorGUIUtility.standardVerticalSpacing;
                    boxyRect.width = 16;

                    // リネームボタン
                    if (GUI.Button(boxyRect, Style.Rename, EditorStyles.label))
                    {
                        if (_renameID == field.ID)
                            _renameID = -1;
                        else
                            _renameID = field.ID;
                        
                        _temp = field.Name;
                        GUI.FocusControl(EditorField);
                        return true;
                    }
                    
                    boxyRect.x += boxyRect.width + EditorGUIUtility.standardVerticalSpacing;
                    
                    // 削除ボタン
                    if (GUI.Button(boxyRect, Style.Delete, EditorStyles.label))
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