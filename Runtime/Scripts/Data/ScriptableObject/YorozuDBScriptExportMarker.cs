using UnityEditor;
using UnityEngine;

namespace Yorozu.DB
{
    /// <summary>
    /// Script をExportする場所においておくアセット
    /// 設定ファイルを作成してもいいが、用途がなさすぎるので簡易にこちらで
    /// </summary>
    internal class YorozuDBScriptExportMarker : ScriptableObject
    {
    }

#if UNITY_EDITOR
    [UnityEditor.CustomEditor(typeof(YorozuDBScriptExportMarker))]
    internal class YorozuDBScriptExportMarkerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            var message = "This is a marker used in YorozuDB\n" +
                          "Export the script to the location where this file is located";

            EditorGUILayout.HelpBox(message, MessageType.Info);

            if (GUILayout.Button("Open Editor"))
            {
                UnityEditor.EditorApplication.ExecuteMenuItem(YorozuDBEditorWindow.MenuPath);
            }
        }
    }
#endif
}