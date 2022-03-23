using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace Yorozu.DB.TreeView
{
    internal class YorozuDBEditorDataTreeView : UnityEditor.IMGUI.Controls.TreeView
    {
	    internal event Action<IList<int>> DeleteRowEvent;
	    internal event Action<IList<int>> ResetRowEvent;
	    internal event Action<IList<int>> AutoIdEvent;
	    internal event Action<IList<int>> SameIdEvent;
	    internal event Action<int, IList<int>> SortEvent;
	    
	    private YorozuDBDataObject _data;
	    
        private readonly List<TreeViewItem> _rows = new List<TreeViewItem>(100);
        
        internal YorozuDBEditorDataTreeView(TreeViewState state, MultiColumnHeader multiColumnHeader, YorozuDBDataObject data) : base(state, multiColumnHeader)
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
	        var enumData = YorozuDBEditorUtility.LoadEnumDataAsset();
	        var root = _data.CreateTree(enumData);
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
	        // KeyがInt だったらAutoInc有効にする
	        if (_data.Define.KeyField != null && _data.Define.KeyField.DataType == DataType.Int)
	        {
		        menu.AddItem(new GUIContent("Apply Sequential Number"), false, () => AutoIdEvent?.Invoke(GetSelection()));
		        menu.AddItem(new GUIContent("Apply Same Number"), false, () => SameIdEvent?.Invoke(GetSelection()));
	        }
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
	    
	            
	    /// <summary>
	    /// TreeView の Header情報を取得
	    /// </summary>
	    /// <param name="data"></param>
	    /// <returns></returns>
	    internal static MultiColumnHeaderState CreateMultiColumnHeaderState(YorozuDBDataObject data)
	    {
		    var fields = data.Define.Fields;
		    var columns = new MultiColumnHeaderState.Column[fields.Count() + 1];
		    // 制御用に1フィールド用意する
		    columns[0] = new MultiColumnHeaderState.Column()
		    {
			    headerContent = new GUIContent(""),
			    width = 28,
			    minWidth = 28,
			    maxWidth = 28,
		    };
            
		    foreach (var (field, i) in fields.Select((v, i) => (v, i)))
		    {
			    columns[i + 1] = new MultiColumnHeaderState.Column()
			    {
				    headerContent = data.Define.IsKeyField(field) ? new GUIContent($"★ {field.Name}") : new GUIContent($"    {field.Name}"),
				    headerTextAlignment = TextAlignment.Left,
				    contextMenuText = field.DataType.ToString(),
				    sortedAscending = true,
				    sortingArrowAlignment = TextAlignment.Right,
				    width = field.GetRectWidth(),
				    minWidth = field.GetRectWidth(),
				    maxWidth = field.GetRectWidth() + 50,
				    autoResize = false,
				    allowToggleVisibility = false,
				    userData = field.ID,
			    };
		    }
            
		    return new MultiColumnHeaderState(columns);
	    }
    }

    internal class YorozuDBEditorMultiColumnHeader : MultiColumnHeader
    {
	    internal event Action<MultiColumnHeaderState.Column> DeleteEvent;
	    
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

    /// <summary>
    /// 1行分のデータをもたせる
    /// </summary>
    internal class YorozuDBEditorTreeViewItem : TreeViewItem
    {
	    private List<DataField> _fields = new List<DataField>();
	    private List<DataContainer> _data = new List<DataContainer>();

	    private YorozuDBEnumDataObject _enumData;
	    
	    internal YorozuDBEditorTreeViewItem(int id, YorozuDBEnumDataObject enumData) : base(id, 0, (id).ToString())
	    {
		    _enumData = enumData;
	    }

	    internal void AddData(DataField field, DataContainer data)
	    {
		    _fields.Add(field);
		    _data.Add(data);
	    }

	    internal void Draw(Rect cellRect, int index)
	    {
		    _data[index].DrawField(cellRect, _fields[index], GUIContent.none, _enumData);
	    }
    }
}