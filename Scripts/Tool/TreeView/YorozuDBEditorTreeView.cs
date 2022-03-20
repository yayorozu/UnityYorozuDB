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

            if (root.hasChildren)
            {
                foreach (var child in root.children)
                {
                    _rows.Add(child);
                }
            }
            
            SetupParentsAndChildrenFromDepths(root, _rows);

            return _rows;
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
	    private static readonly string EditorField = "EditorField";
	    private int _renameIndex = -1;
	    private string _temp;
	    
	    internal event Action<int> DeleteEvent;
	    internal event Action<int, string> RenameEvent;
	    internal event Action<int> SetKeyEvent;
	    
        internal YorozuDBEditorMultiColumnHeader(MultiColumnHeaderState state) : base(state)
        {
            canSort = false;
            height = 24f;
        }

        protected override void ColumnHeaderGUI(MultiColumnHeaderState.Column column, Rect headerRect, int columnIndex)
        {
	        if (_renameIndex == columnIndex)
	        {
		        headerRect.height = EditorGUIUtility.singleLineHeight;
		        GUI.SetNextControlName(EditorField);
		        _temp = GUI.TextField(headerRect, _temp);
		        var e = Event.current;
		        if (e.keyCode == KeyCode.Return && _renameIndex != -1)
		        {
			        RenameEvent?.Invoke(columnIndex - 1, _temp);
			        _renameIndex = -1;
			        Repaint();
		        }

		        if (e.keyCode == KeyCode.Escape)
		        {
			        Repaint();
			        _renameIndex = -1;
		        }
	        }
	        else
	        {
				base.ColumnHeaderGUI(column, headerRect, columnIndex);
	        }
	        
	        if (columnIndex <= 0 || _renameIndex == columnIndex)
		        return;

	        headerRect.y = headerRect.height - EditorGUIUtility.singleLineHeight;  
	        headerRect.height = EditorGUIUtility.singleLineHeight - 4;

	        
	        // Key として有効ならば表示
	        if (column.contextMenuText == DataType.Int.ToString() ||
	            column.contextMenuText == DataType.String.ToString())
	        {
		        var prevWidth = headerRect.width;
		        headerRect.width = 24;
		        
		        var content = column.userData == 1
			        ? EditorGUIUtility.TrIconContent("d_Favorite")
			        : EditorGUIUtility.TrIconContent("TestNormal");
		        if (GUI.Button(headerRect, content, EditorStyles.label))
		        {
			        SetKeyEvent?.Invoke(columnIndex - 1);
		        }

		        headerRect.width = prevWidth;
	        }
	        
	        var width = 16;
			headerRect.x += headerRect.width - width * 2;
	        headerRect.width = width;
	        
	        // リネームボタン
	        if (GUI.Button(headerRect, EditorGUIUtility.TrIconContent("d_Grid.PaintTool"), EditorStyles.label))
	        {
				_renameIndex = columnIndex;
				_temp = column.headerContent.text;
				GUI.FocusControl(EditorField);
	        }

	        headerRect.x += headerRect.width;
	        headerRect.height = EditorGUIUtility.singleLineHeight;
	        
	        // 削除ボタン
	        if (GUI.Button(headerRect, EditorGUIUtility.TrIconContent("Toolbar minus"), EditorStyles.label))
	        {
		        if (EditorUtility.DisplayDialog("Warning", $"Can Delete {column.headerContent.text} row?",
			            "YES",
			            "NO"))
		        {
			        DeleteEvent?.Invoke(columnIndex - 1);
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
	    private List<DBData> _data = new List<DBData>();
	    
	    internal YorozuDBEditorTreeViewItem(int id) : base(id, 0, (id).ToString())
	    {
	    }

	    internal void AddData(DataType type, DBData data)
	    {
		    _types.Add(type);
		    _data.Add(data);
	    }

	    internal void Draw(Rect cellRect, int index)
	    {
            YorozuDBEditorUtility.DrawDataField(cellRect, _types[index], _data[index]);
	    }
    }
}