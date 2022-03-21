using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace Yorozu.DB.TreeView
{
    internal class YorozuDBEditorTreeView : UnityEditor.IMGUI.Controls.TreeView
    {
	    internal event Action<IList<int>> DeleteRowEvent;
	    internal event Action<IList<int>> ResetRowEvent;
	    internal event Action<int, IList<int>> SortEvent;
	    
	    private YorozuDBDataObject _data;
	    
        private readonly List<TreeViewItem> _rows = new List<TreeViewItem>(100);
        
        internal YorozuDBEditorTreeView(TreeViewState state, MultiColumnHeader multiColumnHeader, YorozuDBDataObject data) : base(state, multiColumnHeader)
        {
            _data = data;
            
            rowHeight = EditorGUIUtility.singleLineHeight;
            columnIndexForTreeFoldouts = 0;
            showAlternatingRowBackgrounds = true;
            showBorder = true;
            
            Reload();
        }

        protected override TreeViewItem BuildRoot()
        {
	        var root = _data.CreateTree();
	        return root;
        }

        protected override IList<TreeViewItem> BuildRows(TreeViewItem root)
        {
            _rows.Clear();

            if (!root.hasChildren)
	            return _rows;

            return base.BuildRows(root);
        }
        
        protected override void ContextClickedItem(int id)
        {
	        var ev = Event.current;
	        ev.Use();
    
	        var menu = new GenericMenu();
	        menu.AddItem(new GUIContent("Delete"), false, () => DeleteRowEvent?.Invoke(GetSelection()));
	        menu.AddItem(new GUIContent("Reset Row Value"), false, () => ResetRowEvent?.Invoke(GetSelection()));
	        menu.ShowAsContext();
        }

        protected override bool CanStartDrag(CanStartDragArgs args) => true;

        protected override bool CanMultiSelect(TreeViewItem item) => true;
        
        protected override void SetupDragAndDrop(SetupDragAndDropArgs args)
        {
	        if (hasSearch)
		        return;

	        DragAndDrop.PrepareStartDrag();
	        var draggedRows = GetRows().Where(item => args.draggedItemIDs.Contains(item.id)).ToList();
	        DragAndDrop.SetGenericData("YorozuDBDragging", draggedRows);
	        DragAndDrop.objectReferences = new UnityEngine.Object[] { };
	        var title = draggedRows.Count == 1 ? draggedRows[0].displayName : "< Multiple >";
	        DragAndDrop.StartDrag(title);
        }
        
        protected override DragAndDropVisualMode HandleDragAndDrop(DragAndDropArgs args)
        {
	        var draggedRows = DragAndDrop.GetGenericData("YorozuDBDragging") as List<TreeViewItem>;
	        if (draggedRows == null)
		        return DragAndDropVisualMode.None;
	        
	        switch (args.dragAndDropPosition)
	        {
		        case DragAndDropPosition.UponItem :
		        case DragAndDropPosition.BetweenItems:
		        {
			        if (args.performDrop)
			        {
				        var insertId = args.insertAtIndex;
				        if (insertId == -1 && args.parentItem.id != -1)
					        insertId = args.parentItem.id + 1;

				        if (insertId == -1)
					        return DragAndDropVisualMode.None;
				        
				        SortEvent?.Invoke(insertId, draggedRows.Select(i => i.id).ToArray());
			        }
			        return DragAndDropVisualMode.Move;
		        }
	        }
			return DragAndDropVisualMode.None;
        }
        
        protected override void RowGUI(RowGUIArgs args)
        {
            var item = (YorozuDBEditorTreeViewItem) args.item;

            for (var i = 0; i < args.GetNumVisibleColumns(); ++i)
            {
                CellGUI(args.GetCellRect(i), item, args.GetColumn(i));
            }
        }
        
	    private void CellGUI(Rect cellRect, YorozuDBEditorTreeViewItem item, int columnIndex)
		{
			CenterRectUsingSingleLineHeight(ref cellRect);
			if (columnIndex == 0)
			{
				EditorGUI.LabelField(cellRect, item.displayName, DefaultStyles.labelRightAligned);
				return;
			}

			using (var check = new EditorGUI.ChangeCheckScope())
			{
				item.Draw(cellRect, --columnIndex);
				if (check.changed)
				{
					_data.Dirty();
				}
			}
        }
    }

    internal class YorozuDBEditorMultiColumnHeader : MultiColumnHeader
    {
	    internal event Action<int> DeleteEvent;
	    
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
	        
	        var width = 16;
	        headerRect.y = headerRect.height - EditorGUIUtility.singleLineHeight;  
			headerRect.x += headerRect.width - width;
	        headerRect.width = width;
	        headerRect.height = EditorGUIUtility.singleLineHeight;
	        
	        // 削除ボタン
	        if (GUI.Button(headerRect, EditorGUIUtility.TrIconContent("Toolbar minus"), EditorStyles.label))
	        {
		        if (EditorUtility.DisplayDialog("Warning", $"Delete {column.headerContent.text}?",
			            "YES",
			            "NO"))
		        {
			        DeleteEvent?.Invoke(column.userData);
			        GUIUtility.ExitGUI();
		        }
	        }
        }

        protected override void AddColumnHeaderContextMenuItems(GenericMenu menu)
        {
        }
    }

    /// <summary>
    /// 1行分のデータをもたせる
    /// </summary>
    internal class YorozuDBEditorTreeViewItem : TreeViewItem
    {
	    private List<DataType> _types = new List<DataType>();
	    private List<DBDataContainer> _data = new List<DBDataContainer>();
	    
	    internal YorozuDBEditorTreeViewItem(int id) : base(id, 0, (id).ToString())
	    {
	    }

	    internal void AddData(DataType type, DBDataContainer data)
	    {
		    _types.Add(type);
		    _data.Add(data);
	    }

	    internal void Draw(Rect cellRect, int index)
	    {
            YorozuDBEditorUtility.DrawDataField(cellRect, _types[index], _data[index], GUIContent.none);
	    }
    }
}