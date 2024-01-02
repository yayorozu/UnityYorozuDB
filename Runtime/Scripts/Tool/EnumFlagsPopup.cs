#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace Yorozu.DB
{
    /// <summary>
    /// Flags用のPopup
    /// </summary>
    internal class EnumFlagsPopup : PopupWindowContent
    {
        private readonly FlagsTreeView _treeView;
        internal event Action<int> FlagsChange;
        private float _height;

        public EnumFlagsPopup(string[] enums, int value)
        {
            _treeView = new FlagsTreeView(new TreeViewState(), this, enums, value);
            _height = (enums.Length + 2) * EditorGUIUtility.singleLineHeight;
            GUIUtility.keyboardControl = _treeView.treeViewControlID;
        }

        public override void OnGUI(Rect rect)
        {
            _treeView.OnGUI(rect);
        }

        public override Vector2 GetWindowSize()
        {
            var size = base.GetWindowSize();
            size.y = _height;
            return size;
        }

        private class FlagsTreeView : UnityEditor.IMGUI.Controls.TreeView
        {
            private Texture2D _icon;

            private readonly EnumFlagsPopup _enumFlagsPopup;
            private List<TreeViewItem> _child;
            private int _value;
            private int _all;
            private const int Nothing = -3;

            public FlagsTreeView(TreeViewState state, EnumFlagsPopup enumFlagsPopup, string[] enums, int value) :
                base(state)
            {
                showBorder = true;
                showAlternatingRowBackgrounds = true;
                _value = value;
                _icon = (Texture2D) EditorGUIUtility.Load("d_FilterSelectedOnly");
                _enumFlagsPopup = enumFlagsPopup;
                _all = (int) Mathf.Pow(2, enums.Length) - 1;
                _child = new List<TreeViewItem>(enums.Length + 2)
                {
                    new(Nothing, 0, "Nothing"),
                    new(_all, 0, "EveryThing")
                };
                for (var index = 0; index < enums.Length; index++)
                {
                    _child.Add(new TreeViewItem((int) Mathf.Pow(2, index), 0, enums[index]));
                }

                Reload();
            }

            protected override TreeViewItem BuildRoot()
            {
                var root = new TreeViewItem(-1, -1, "root");
                foreach (var item in _child)
                    root.AddChild(item);

                return root;
            }

            protected override void SingleClickedItem(int id)
            {
                if (id == Nothing)
                {
                    _value = 0;
                }
                else if (id == _all)
                {
                    _value = _all;
                }
                else
                {
                    // 定義済み
                    if ((_value & id) == id)
                        _value &= ~id;
                    else
                        _value |= id;
                }

                _enumFlagsPopup?.FlagsChange?.Invoke(_value);
                SetSelection(new List<int>());
            }

            protected override bool CanMultiSelect(TreeViewItem item) => false;

            protected override void RowGUI(RowGUIArgs args)
            {
                var rect = args.rowRect;
                var width = rect.width;
                rect.width = rect.height - 2;
                rect.xMin += 2;
                if (Valid(args.item.id))
                    GUI.DrawTexture(rect, _icon, ScaleMode.ScaleToFit);

                rect.xMin += rect.width;
                rect.width = width - rect.width;
                base.RowGUI(args);
            }

            private bool Valid(int id)
            {
                if (_value == 0)
                {
                    return id == Nothing;
                }

                return (_value & id) == id;
            }
        }
    }
}

#endif