#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace Yorozu.DB
{
    internal interface IDBName
    {
        string Name { get; }
    }
    
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

        internal static YorozuDBEnumDataObject LoadEnumDataAsset()
        {
            var findEnumAssetsGuids = AssetDatabase.FindAssets($"t:{nameof(YorozuDBEnumDataObject)}");
            if (!findEnumAssetsGuids.Any())
                return null;

            var path = AssetDatabase.GUIDToAssetPath(findEnumAssetsGuids.First());
            var enumData = AssetDatabase.LoadAssetAtPath<YorozuDBEnumDataObject>(path);
            return enumData;
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
        
        internal static void Dirty(this YorozuDBEnumDataObject asset)
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
                // TODO Enumの場合は型がいる
                DataType.Enum => "enum",
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            };
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
        internal static bool CreateDefineAsset()
        {
            var defines = LoadAllDefineAsset();
            var loadFrom = defines is {Length: > 0} ? 
                Path.GetDirectoryName(AssetDatabase.GetAssetPath(defines[0])) :
                "Assets/";
            
            var path = EditorUtility.SaveFilePanelInProject("Select", "Define", "asset", "Select Create Path", loadFrom);
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

        internal static bool CreateEnumAsset()
        {
            var defines = LoadAllDefineAsset();
            var loadFrom = defines is {Length: > 0} ? 
                Path.GetDirectoryName(AssetDatabase.GetAssetPath(defines[0])) :
                "Assets/";
            
            var path = EditorUtility.SaveFilePanelInProject("Select", "Enum", "asset", "Select Create Path", loadFrom);
            if (string.IsNullOrEmpty(path)) 
                return false;
            
            var instance = ScriptableObject.CreateInstance<YorozuDBEnumDataObject>();
            AssetDatabase.CreateAsset(instance, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            return true;
        }

        /// <summary>
        /// 文字のチェック
        /// </summary>
        internal static bool NameValidator(IEnumerable<IDBName> names, string name, out string editName)
        {
            editName = name.Trim();
            if (names.Any(d => d.Name == name))
                return false;
            
            if (string.IsNullOrEmpty(name))
                return false;

            if (Regex.IsMatch(name, @"^[0-9]"))
                return false;
            
            // 小文字なら大文字にする
            if (Regex.IsMatch(name, @"^[a-z]"))
            {
                var array = editName.ToCharArray();
                array[0] = char.ToUpper(array[0]);
                editName = new string(array);
            }
            
            // 英数以外は許可しない
            return Regex.IsMatch(name, @"^[0-9a-zA-Z]+$");
        }
    }
}

#endif