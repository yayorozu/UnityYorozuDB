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
            var assets = LoadAllDefineAsset();
            foreach (var data in assets)
            {
                var builder = new StringBuilder();
                builder.AppendLine("using UnityEngine;");
                builder.AppendLine("");
                
                builder.AppendLine("namespace Yorozu.DB");
                builder.AppendLine("{");
                builder.AppendLine($"    public class {data.name} : {nameof(YorozuDBDataAbstract)}");
                builder.AppendLine("    {");
                foreach (var field in data.Fields)
                {
                    builder.AppendLine($"       public {field.DataType.ConvertString()} {field.Name} => {field.DataType.ToString()}({field.ID});");
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
        
        internal static YorozuDBDataDefineObject[] LoadAllDefineAsset()
        {
            var findAssets = AssetDatabase.FindAssets($"t:{nameof(YorozuDBDataDefineObject)}");
            return findAssets.Select(AssetDatabase.GUIDToAssetPath)
                    .Select(AssetDatabase.LoadAssetAtPath<YorozuDBDataDefineObject>)
                    .ToArray()
                ;
        }
        
        internal static YorozuDBDataObject[] LoadAllDataAsset(YorozuDBDataDefineObject searchDefine = null)
        {
            var findAssetsGuids = AssetDatabase.FindAssets($"t:{nameof(YorozuDBDataObject)}");
            return findAssetsGuids.Select(AssetDatabase.GUIDToAssetPath)
                    .Select(AssetDatabase.LoadAssetAtPath<YorozuDBDataObject>)
                    // 対象指定されていた場合はそれを全部探す
                    .Where(v => searchDefine == null || (searchDefine != null && v.Define == searchDefine))
                    .ToArray()
                ;
        }

        internal static void Dirty(this YorozuDBDataDefineObject asset)
        {
            EditorUtility.SetDirty(asset);
            AssetDatabase.SaveAssets();
        }
        
        internal static void Dirty(this YorozuDBDataObject asset)
        {
            EditorUtility.SetDirty(asset);
            AssetDatabase.SaveAssets();
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
                DataType.Bool => "bool",
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
        
        internal static void DrawDataField(Rect rect, DataType type, DBDataContainer data, GUIContent content)
        {
            switch (type)
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
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
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
            
            foreach (var (v, i) in fields.Select((v, i) => (v, i)))
            {
                columns[i + 1] = new MultiColumnHeaderState.Column()
                {
                    headerContent = new GUIContent($"    {v.Name}"),
                    headerTextAlignment = TextAlignment.Left,
                    contextMenuText = v.DataType.ToString(),
                    sortedAscending = true,
                    sortingArrowAlignment = TextAlignment.Right,
                    width = 150,
                    minWidth = 150,
                    maxWidth = 200,
                    autoResize = false,
                    allowToggleVisibility = false,
                    userData = v.ID,
                };
            }
            
            return new MultiColumnHeaderState(columns);
        }

        /// <summary>
        /// DefineAssetを作成
        /// </summary>
        internal static bool CreateDefineAsset(string defaultPath)
        {
            var path = EditorUtility.SaveFilePanelInProject("Select", "Define", "asset", "Select Create Path", defaultPath);
            if (string.IsNullOrEmpty(path)) 
                return false;
            
            var instance = ScriptableObject.CreateInstance<YorozuDBDataDefineObject>();
            AssetDatabase.CreateAsset(instance, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            return true;
        }
        
        /// <summary>
        /// データアセットを作成
        /// </summary>
        internal static bool CreateDataAsset(YorozuDBDataDefineObject define, string defaultPath)
        {
            var path = EditorUtility.SaveFilePanelInProject("Select", "Data", "asset", "Select Create Path", defaultPath);
            if (string.IsNullOrEmpty(path)) 
                return false;
            
            var instance = ScriptableObject.CreateInstance<YorozuDBDataObject>();
            instance.Define = define;
            foreach (var field in define.Fields)
            {
                instance.AddField(field.ID);
            }
            AssetDatabase.CreateAsset(instance, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            return true;
        }
    }
}