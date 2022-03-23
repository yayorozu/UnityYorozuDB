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
	        if (enumData != null)
	        {
				enumData.ResetEnumCache();
	        }
	        
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
		        menu.AddItem(new GUIContent("Assign sequential numbers from the first Key Number"), false, () => AutoIdEvent?.Invoke(GetSelection()));
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
	        headerRect.y = headerRect.height - EditorGUIUtility.singleLineHeight;  
			headerRect.x += headerRect.width - width;
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
            DrawDataField(cellRect, _fields[index], _data[index], GUIContent.none);
	    }
	    
	    private void DrawDataField(Rect rect, DataField field, DataContainer data, GUIContent content)
        {
            switch (field.DataType)
            {
                case DataType.String:
                    data.String = EditorGUI.TextField(rect, content, data.String);
                    break;
                case DataType.Float:
                    data.Float = EditorGUI.FloatField(rect, content, data.Float);
                    break;
                case DataType.Int:
                    data.Int = EditorGUI.IntField(rect, content, data.Int);
                    break;
                case DataType.Bool:
                    data.Bool = EditorGUI.Toggle(rect, content, data.Bool);
                    break;
                case DataType.Sprite:
                    data.UnityObject = EditorGUI.ObjectField(rect, content, data.UnityObject, typeof(Sprite), false);
                    break;
                case DataType.GameObject:
                    data.UnityObject = EditorGUI.ObjectField(rect, content, data.UnityObject, typeof(GameObject), false);
                    break;
                case DataType.UnityObject:
                    data.UnityObject = EditorGUI.ObjectField(rect, content, data.UnityObject, typeof(UnityEngine.Object), false);
                    break;
                case DataType.Vector2:
                    var vector2 = data.GetFromString<Vector2>();
                    using (var check = new EditorGUI.ChangeCheckScope())
                    {
                        vector2 = EditorGUI.Vector2Field(rect, content, vector2);
                        if (check.changed)
                        {
                            data.SetToString(vector2);
                        }
                    }
                    break;
                case DataType.Vector3:
                    var vector3 = data.GetFromString<Vector3>();
                    using (var check = new EditorGUI.ChangeCheckScope())
                    {
                        vector3 = EditorGUI.Vector3Field(rect, content, vector3);
                        if (check.changed)
                            data.SetToString(vector3);
                    }
                    break;
                case DataType.ScriptableObject:
                    data.UnityObject = EditorGUI.ObjectField(rect, content, data.UnityObject, typeof(UnityEngine.ScriptableObject), false);
                    break;
                case DataType.Vector2Int:
                    var vector2Int = data.GetFromString<Vector2Int>();
                    using (var check = new EditorGUI.ChangeCheckScope())
                    {
                        vector2Int = EditorGUI.Vector2IntField(rect, content, vector2Int);
                        if (check.changed)
                        {
                            data.SetToString(vector2Int);
                        }
                    }
                    break;
                case DataType.Vector3Int:
                    var vector3Int = data.GetFromString<Vector3Int>();
                    using (var check = new EditorGUI.ChangeCheckScope())
                    {
                        vector3Int = EditorGUI.Vector3IntField(rect, content, vector3Int);
                        if (check.changed)
                        {
                            data.SetToString(vector3Int);
                        }
                    }
                    break;
                case DataType.Enum:
                    var enums = _enumData.GetEnums(field.EnumDefineId);
                    var index = _enumData.GetEnumIndex(field.EnumDefineId, data.Int);
                    using (var check = new EditorGUI.ChangeCheckScope())
                    {
                        index = EditorGUI.Popup(rect, index, enums);
                        if (check.changed)
                        {
                            var key = _enumData.GetEnumKey(field.EnumDefineId, enums[index]);
                            if (key.HasValue)
                            {
                                data.Int = key.Value;
                            }
                        }
                    }
                    
                    break;
            }
        }
    }
}