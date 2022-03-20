using System;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace Yorozu.DB
{
    internal static class YorozuDBEditorUtility
    {
        /// <summary>
        /// データからファイルを出力
        /// </summary>
        internal static void GenerateScript(string savePath)
        {
            var assets = LoadAllDataAsset();
            foreach (var data in assets)
            {
                var builder = new StringBuilder();
                builder.AppendLine("using UnityEngine;");
                builder.AppendLine("");
                
                builder.AppendLine("namespace Yorozu.DB");
                builder.AppendLine("{");
                builder.AppendLine($"    public class {data.name} : {nameof(YorozuDBData)}");
                builder.AppendLine("    {");
                foreach (var pair in data.Groups.Select((v, i) => new {v, i}))
                {
                    builder.AppendLine($"       public {pair.v.DataType.ConvertString()} {pair.v.Name} => {pair.v.DataType.ToString()}({pair.i});");
                    builder.AppendLine("");
                }
                builder.AppendLine("    }");
                builder.AppendLine("}");

                var exportPath = Path.Combine(savePath, $"{data.name}.cs");
                // 出力
                using (StreamWriter writer = new StreamWriter(exportPath, false))
                {
                    writer.WriteLine(builder.ToString());
                }
            }   
            
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
        
        internal static YorozuDBDataObject[] LoadAllDataAsset()
        {
            var findAssets = AssetDatabase.FindAssets($"t:{nameof(YorozuDBDataObject)}");
            return findAssets.Select(AssetDatabase.GUIDToAssetPath)
                    .Select(AssetDatabase.LoadAssetAtPath<YorozuDBDataObject>)
                    .ToArray()
                ;
        }

        /// <summary>
        /// 文字列に変換
        /// </summary>
        private static string ConvertString(this DataType type)
        {
            return type switch
            {
                DataType.String => "string",
                DataType.Float => "float",
                DataType.Int => "int",
                DataType.Sprite => "Sprite",
                DataType.GameObject => "GameObject",
                DataType.ScriptableObject => "ScriptableObject",
                DataType.UnityObject => "UnityEngine.Object",
                DataType.Vector2 => "Vector2",
                DataType.Vector3 => "Vector3",
                DataType.Vector2Int => "Vector3Int",
                DataType.Vector3Int => "Vector3Int",
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            };
        }
        
        internal static void DrawDataField(Rect rect, DataType type, DBData data)
        {
            switch (type)
            {
                case DataType.String:
                    data.String = EditorGUI.TextField(rect, GUIContent.none, data.String);
                    break;
                case DataType.Float:
                    data.Float = EditorGUI.FloatField(rect, GUIContent.none, data.Float);
                    break;
                case DataType.Int:
                    data.Int = EditorGUI.IntField(rect, GUIContent.none, data.Int);
                    break;
                case DataType.Sprite:
                    data.UnityObject = EditorGUI.ObjectField(rect, GUIContent.none, data.UnityObject, typeof(Sprite), false);
                    break;
                case DataType.GameObject:
                    data.UnityObject = EditorGUI.ObjectField(rect, GUIContent.none, data.UnityObject, typeof(GameObject), false);
                    break;
                case DataType.UnityObject:
                    data.UnityObject = EditorGUI.ObjectField(rect, GUIContent.none, data.UnityObject, typeof(UnityEngine.Object), false);
                    break;
                case DataType.Vector2:
                    var vector2 = data.GetFromString<Vector2>();
                    using (var check = new EditorGUI.ChangeCheckScope())
                    {
                        vector2 = EditorGUI.Vector2Field(rect, GUIContent.none, vector2);
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
                        vector3 = EditorGUI.Vector3Field(rect, GUIContent.none, vector3);
                        if (check.changed)
                            data.SetToString(vector3);
                    }
                    break;
                case DataType.ScriptableObject:
                    data.UnityObject = EditorGUI.ObjectField(rect, GUIContent.none, data.UnityObject, typeof(UnityEngine.ScriptableObject), false);
                    break;
                case DataType.Vector2Int:
                    var vector2Int = data.GetFromString<Vector2Int>();
                    using (var check = new EditorGUI.ChangeCheckScope())
                    {
                        vector2Int = EditorGUI.Vector2IntField(rect, GUIContent.none, vector2Int);
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
                        vector3Int = EditorGUI.Vector3IntField(rect, GUIContent.none, vector3Int);
                        if (check.changed)
                        {
                            data.SetToString(vector3Int);
                        }
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        internal static MultiColumnHeaderState CreateMultiColumnHeaderState(YorozuDBDataObject data)
        {
            var columns = new MultiColumnHeaderState.Column[data.Groups.Count() + 1];
            // 制御用に1フィールド用意する
            columns[0] = new MultiColumnHeaderState.Column()
            {
                headerContent = new GUIContent(""),
                width = 28,
                minWidth = 28,
                maxWidth = 28,
            };
            
            foreach (var pair in data.Groups.Select((v, i) => new {v, i}))
            {
                columns[pair.i + 1] = new MultiColumnHeaderState.Column()
                {
                    headerContent = new GUIContent($"    {pair.v.Name}"),
                    headerTextAlignment = TextAlignment.Left,
                    contextMenuText = pair.v.DataType.ToString(),
                    sortedAscending = true,
                    sortingArrowAlignment = TextAlignment.Right,
                    width = 150,
                    minWidth = 150,
                    maxWidth = 200,
                    autoResize = false,
                    allowToggleVisibility = false,
                    userData = data.IsPrimaryGroup(pair.v) ? 1 : 0,
                };
            }
            
            return new MultiColumnHeaderState(columns);
        }
    }
}