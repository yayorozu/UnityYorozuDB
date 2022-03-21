using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Yorozu.DB
{
    [Serializable]
    internal class EnumEditModule : YorozuDBEditorModule
    {
        private static class Style
        {
            internal static GUIContent AddContent;
            internal static GUIStyle AddStyle;
            internal static GUIContent SubContent;
            internal static GUIContent DeleteContent;
            internal const float ButtonWidth = 16f;
            
            static Style()
            {
                AddContent = EditorGUIUtility.TrIconContent("Toolbar Plus", "Add to list");
                AddStyle = "RL FooterButton";
                SubContent = EditorGUIUtility.TrIconContent("Toolbar Minus", "Remove selection from list");
                DeleteContent = EditorGUIUtility.TrIconContent("d_TreeEditor.Trash");
            }
        }
        
        private List<ReorderableList> _list;
        private const float Width = 200;
        private Vector2 _scrollPosition;
        private string _temp;
        private YorozuDBEnumDataObject _data;
        
        private void Initialize(bool force = false)
        {
            if (!force && _list != null)
                return;

            _data = YorozuDBEditorUtility.LoadEnumDataAsset();
            if (_data == null)
                return;

            _list = new List<ReorderableList>(_data.Defines.Count);
            foreach (var define in _data.Defines.OrderBy(v => v.Name))
            {
                var list = CreateInstance(define);
                _list.Add(list);
            }
        }
        
        internal override bool OnGUI()
        {
            Initialize();
            
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                EditorGUILayout.LabelField($"Enum Editor");
                GUILayout.FlexibleSpace();
                
                _temp = EditorGUILayout.TextField("Enum Name", _temp, EditorStyles.toolbarTextField);
                using (new EditorGUI.DisabledScope(string.IsNullOrEmpty(_temp)))
                {
                    if (GUILayout.Button("Add Enum", EditorStyles.toolbarButton))
                    {
                        _data.AddDefine(_temp);
                        Initialize(true);
                        _temp = "";
                        GUI.FocusControl("");
                        return true;
                    }
                }
            }
            
            // 領域確保
            var rect = GUILayoutUtility.GetRect(0, 100000, 0, 100000);
            // 領域を描画
            using (new GUI.ClipScope(rect))
            {
                rect.x = rect.y = 0;
                
                var width = _list.Count * (Width + EditorGUIUtility.standardVerticalSpacing);
                var height = rect.height - GUI.skin.horizontalScrollbar.fixedHeight; 
                using (var scroll = new GUI.ScrollViewScope(rect, _scrollPosition, new Rect(0, -3, width, height), true, false))
                {
                    _scrollPosition = scroll.scrollPosition;
                    rect.width = Width;
                    
                    foreach (var list in _list)
                    {
                        list.DoList(rect);
                        rect.x += rect.width + EditorGUIUtility.standardVerticalSpacing;
                    }
                }
            }

            return false;
        }
        
        private ReorderableList CreateInstance(YorozuDBEnumDataObject.EnumDefine define)
        {
            return new ReorderableList(define.Values, typeof(string), true, true, false, false)
            {
                drawHeaderCallback = rect =>
                {
                    EditorGUI.LabelField(rect, $"{define.Name}", EditorStyles.boldLabel);

                    rect.x += rect.width - Style.ButtonWidth * 2;
                    rect.width = Style.ButtonWidth;

                    if (GUI.Button(rect, Style.DeleteContent, Style.AddStyle))
                    {
                        if (EditorUtility.DisplayDialog("Warning", $"Delete {define.Name}?",
                                "YES",
                                "NO"))
                        {
                            _data.RemoveDefine(define.ID);
                            Initialize(true);
                        }
                    }

                    rect.x += rect.width;

                    if (GUI.Button(rect, Style.AddContent, Style.AddStyle))
                    {
                        define.AddValue("");
                        _data.Dirty();
                    }
                },
                drawElementCallback = (rect, index, isActive, isFocused) =>
                {
                    if (define.Values.Count <= index)
                        return;

                    rect.width -= Style.ButtonWidth;

                    using (var change = new EditorGUI.ChangeCheckScope())
                    {
                        define.Values[index] = EditorGUI.TextField(rect, define.Values[index]);
                        if (change.changed)
                        {
                            _data.Dirty();
                        }
                    }
     
                    rect.x += rect.width + 2;
                    rect.width = Style.ButtonWidth;
                    if (GUI.Button(rect, Style.SubContent, Style.AddStyle))
                    {
                        define.RemoveAt(index);
                        _data.Dirty();
                    }
                },
     
                drawFooterCallback = rect => { },
                footerHeight = 0f,
            };
        }
    }
}