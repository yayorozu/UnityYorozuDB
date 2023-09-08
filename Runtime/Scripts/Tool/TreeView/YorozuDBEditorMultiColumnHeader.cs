using System;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

#if UNITY_EDITOR

namespace Yorozu.DB.TreeView
{
    internal class YorozuDBEditorMultiColumnHeader : MultiColumnHeader
    {
        internal event Action<MultiColumnHeaderState.Column> DeleteEvent;
        internal event Action<int, float> ChangeWidthEvent;
	    
        internal YorozuDBEditorMultiColumnHeader(MultiColumnHeaderState state) : base(state)
        {
            canSort = false;
            height = 24f;
        }

        protected override void ColumnHeaderGUI(MultiColumnHeaderState.Column column, Rect headerRect, int columnIndex)
        {
            base.ColumnHeaderGUI(column, headerRect, columnIndex);
	        
            if (columnIndex <= 0)
                return;
	        
            ChangeWidthEvent?.Invoke(columnIndex - 1, column.width);
	        
            var width = 16;
            headerRect.y += 1;
            headerRect.x += headerRect.width - width - EditorGUIUtility.standardVerticalSpacing * 2;
            headerRect.width = width;
            headerRect.height = EditorGUIUtility.singleLineHeight;
	        
            // 削除ボタン
            if (GUI.Button(headerRect, EditorGUIUtility.TrIconContent("Toolbar minus"), EditorStyles.label))
            {
                DeleteEvent?.Invoke(column);
                GUIUtility.ExitGUI();
            }
        }

        protected override void AddColumnHeaderContextMenuItems(GenericMenu menu)
        {
        }
    }
}
#endif