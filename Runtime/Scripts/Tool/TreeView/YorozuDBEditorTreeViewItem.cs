#if UNITY_EDITOR

using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace Yorozu.DB.TreeView
{
    /// <summary>
    /// 1行分のデータをもたせる
    /// </summary>
    internal class YorozuDBEditorTreeViewItem : TreeViewItem
    {
        private YorozuDBDataObject _data;
        private YorozuDBEnumDataObject _enumData;
        private List<FieldInfo> _extendFieldInfos;
        private Editor _editor;
        internal float Height;
        private float ArrayWidth = 18f;

        internal YorozuDBEditorTreeViewItem(int id, YorozuDBDataObject data, YorozuDBEnumDataObject enumData) : base(id, 0, (id).ToString())
        {
            _data = data;
            _enumData = enumData;
            if (_data.ExtendFieldsObject != null)
            {
                _editor = Editor.CreateEditor(_data.ExtendFieldsObject);
                _extendFieldInfos = YorozuDBExtendUtility.FindFields(_data.ExtendFieldsObject);
                Height = YorozuDBExtendUtility.ItemHeight(_data.ExtendFieldsObject, id);
            }
        }

        internal bool Draw(Rect rect, int index)
        {
            var field = _data.Define.Fields[index];
            var isFix = _data.IsFixField(field.ID);
            using (new EditorGUI.DisabledScope(isFix))
            {
                var container = _data.GetData(field.ID, id);
                if (field.IsArray)
                {
                    var width = rect.width;
                    rect.width = ArrayWidth;
                    if (GUI.Button(rect, "+"))
                    {
                        container.Add(field.DataType);
                        return true;
                    }
                    rect.x += ArrayWidth;
                    rect.width = width - ArrayWidth * 2;
                }
                else
                {
                    rect.y += rect.height / 2f - YorozuDBEditorDataTreeView.RowHeight / 2f;
                    rect.height = YorozuDBEditorDataTreeView.RowHeight;
                }
                container.DrawField(rect, field, GUIContent.none, _enumData);
                if (field.IsArray)
                {
                    rect.height = YorozuDBEditorDataTreeView.RowHeight;
                    rect.x += rect.width;
                    rect.width = ArrayWidth; 
                    var size = container.GetSize(field.DataType);
                    for (int i = 0; i < size; i++)
                    {
                        if (GUI.Button(rect, "-"))
                        {
                            container.RemoveAt(field.DataType, i);
                            return true;
                        }
                        rect.y += rect.height;
                    }
                }
            }

            return false;
        }

        internal void DrawExtend(Rect rect, int index)
        {
            _editor.serializedObject.UpdateIfRequiredOrScript();
            var prop = _editor.serializedObject.FindProperty(_extendFieldInfos[index].Name);
            var elementProp = prop.GetArrayElementAtIndex(id);
            if (!elementProp.isExpanded)
                elementProp.isExpanded = true;
			
            EditorGUI.PropertyField(rect, elementProp, GUIContent.none, true);
            _editor.serializedObject.ApplyModifiedProperties();
        }
    }
}

#endif