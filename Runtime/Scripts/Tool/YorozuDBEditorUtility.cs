#if UNITY_EDITOR

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace Yorozu.DB
{
    internal static class YorozuDBEditorUtility
    {
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
            enumData.ResetEnumCache();
            return enumData;
        }

        internal static void Dirty(this ScriptableObject asset)
        {
            EditorUtility.SetDirty(asset);
            AssetDatabase.SaveAssets();
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