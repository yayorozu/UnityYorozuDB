using UnityEditor;

#if UNITY_EDITOR

namespace Yorozu.DB
{
    public class YorozuDBLoadDataViewer : EditorWindow
    {
        [MenuItem("Tools/YorozuDB/DataViewer")]
        private static void Open()
        {
            var window = GetWindow<YorozuDBLoadDataViewer>();
            window.titleContent = new UnityEngine.GUIContent("YorozuDBDataViewer");
            window.Show();
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("EnumData", YorozuDB.EnumData != null ? "Loaded" : "None");
            if (YorozuDB.LoadedData == null || YorozuDB.LoadedData.Count <= 0)
            {
                EditorGUILayout.LabelField("LoadData is None");
                return;
            }
            
            
            foreach (var data in YorozuDB.LoadedData)
            {
                EditorGUILayout.LabelField(data.Key.Name);
                using (new EditorGUI.IndentLevelScope())
                {
                    foreach (var d in data.Value)
                    {
                        var dataString = string.Join(',', d.ToString().Split("\n"));
                        EditorGUILayout.LabelField(dataString);
                    }
                }
            }
        }
    }
}

#endif