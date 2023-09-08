#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
	    internal event Action<int> DuplicateEvent;
	    
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
	        var enumData = YorozuDBEditorInternalUtility.LoadEnumDataAsset();
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
	        if (GetSelection().Count == 1)
	        {
		        // 複製
		        menu.AddItem(new GUIContent("Duplicate"), false, () => DuplicateEvent?.Invoke(GetSelection().First()));
	        }
	        // KeyがInt だったらAutoInc有効にする
	        if (GetSelection().Count > 1 && _data.Define.KeyField != null && _data.Define.KeyField.DataType == DataType.Int)
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

        protected override float GetCustomRowHeight(int row, TreeViewItem item)
        {
	        var i = item as YorozuDBEditorTreeViewItem;
	        if (i.Height > 0)
		        return i.Height;
	        
	        return base.GetCustomRowHeight(row, item);
        }

        private void CellGUI(Rect cellRect, YorozuDBEditorTreeViewItem item, int columnIndex)
		{
			// Rect を真ん中にする処理
			//CenterRectUsingSingleLineHeight(ref cellRect);
			if (columnIndex == 0)
			{
				EditorGUI.LabelField(cellRect, item.displayName, DefaultStyles.labelRightAligned);
				return;
			}

			using (var check = new EditorGUI.ChangeCheckScope())
			{
				--columnIndex;
				// Extend
				if (columnIndex >= _data.Define.Fields.Count)
				{
					item.DrawExtend(cellRect, columnIndex - _data.Define.Fields.Count);
				}
				else
				{
					item.Draw(cellRect, columnIndex);
				}
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
		    var columns = new List<MultiColumnHeaderState.Column>();
		    // 制御用に1フィールド用意する
		    columns.Add(new MultiColumnHeaderState.Column()
		    {
			    headerContent = new GUIContent(""),
			    width = 28,
			    minWidth = 28,
			    maxWidth = 28,
		    });
		    
		    foreach (var (field, i) in fields.Select((v, i) => (v, i)))
		    {
			    var width = MathF.Max(field.GUIWidth, 50);
			    columns.Add(new MultiColumnHeaderState.Column()
			    {
				    headerContent = data.Define.IsKeyField(field) ? new GUIContent($"★ {field.Name}", field.Memo) : new GUIContent($"    {field.Name}", field.Memo),
				    headerTextAlignment = TextAlignment.Left,
				    sortedAscending = true,
				    sortingArrowAlignment = TextAlignment.Right,
				    width = width,
				    minWidth = 50,
				    maxWidth = 500,
				    autoResize = false,
				    allowToggleVisibility = false,
				    userData = field.ID,
			    });
		    }
		    
		    if (data.Define.ExtendFieldsType != null &&
		        data.ExtendFieldsObject != null &&
		        data.ExtendFieldsObject.GetType() == data.Define.ExtendFieldsType)
		    {
			    var addFields = YorozuDBExtendUtility.FindFields(data.ExtendFieldsObject);
			    // 今定義されてないやつは全部消す
			    data.Define.ExtendFieldWidths.RemoveAll(v => !addFields.Select(f => f.Name).Contains(v.Name));
			    
			    foreach (var (value, i) in addFields.Select((v, i) => (v, i)))
			    {
				    var widthIndex = data.Define.ExtendFieldWidths.FindIndex(v => v.Name == value.Name);
				    var width = 150f;
				    if (widthIndex < 0)
				    {
					    data.Define.ExtendFieldWidths.Add(new YorozuDBDataDefineObject.ExtendFieldWidth()
					    {
						    Name = value.Name,
						    Width = width,
					    });
				    }
				    else
				    {
					    width = data.Define.ExtendFieldWidths[widthIndex].Width;
				    }

				    columns.Add(new MultiColumnHeaderState.Column()
				    {
					    headerContent = new GUIContent($"    {value.Name}"),
					    headerTextAlignment = TextAlignment.Left,
					    sortedAscending = true,
					    sortingArrowAlignment = TextAlignment.Right,
					    width = width,
					    minWidth = 50,
					    maxWidth = 500,
					    autoResize = false,
					    allowToggleVisibility = false,
					    userData = -1,
				    });
			    }
		    }

		    return new MultiColumnHeaderState(columns.ToArray());
	    }
    }
}

#endif