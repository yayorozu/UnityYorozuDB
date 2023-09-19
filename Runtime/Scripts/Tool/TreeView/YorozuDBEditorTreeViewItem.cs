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

        internal void Draw(Rect rect, int index)
        {
            var field = _data.Define.Fields[index];
            var isFix = _data.IsFixField(field.ID);
            using (new EditorGUI.DisabledScope(isFix))
            {
                var container = _data.GetData(field.ID, id);
                container.DrawField(rect, field, GUIContent.none, _enumData);
            }
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