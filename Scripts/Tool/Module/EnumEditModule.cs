using System;
using UnityEditor;
using UnityEngine;

namespace Yorozu.DB
{
    [Serializable]
    internal class EnumEditModule : YorozuDBEditorModule
    {
        internal override bool OnGUI()
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                EditorGUILayout.LabelField($"Enum Editor");
            }
            
            var rect = GUILayoutUtility.GetRect(0, 100000, 0, 100000);

            return true;
        }
    }
}