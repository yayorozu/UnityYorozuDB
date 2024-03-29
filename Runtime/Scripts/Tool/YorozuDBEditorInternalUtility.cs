#if UNITY_EDITOR

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace Yorozu.DB
{
    internal static class YorozuDBEditorInternalUtility
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

        /// <summary>
        /// DefineAssetを作成
        /// </summary>
        internal static bool CreateDefineAsset()
        {
            var defines = LoadAllDefineAsset();
            var loadFrom = defines is {Length: > 0} ?
                Path.GetDirectoryName(AssetDatabase.GetAssetPath(defines[0])) :
                "Assets/";

            var path = EditorUtility.SaveFilePanelInProject("Select", "Define", "asset", "Select Create Path",
                loadFrom);
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
            var path = EditorUtility.SaveFilePanelInProject("Select", $"{define.ClassName}Data", "asset",
                "Select Create Path", defaultPath);
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

        internal static (YorozuDBDataObject, int) FindData(YorozuDBDataDefineObject define, string key)
        {
            var data = LoadAllDataAsset(define);
            var keyField = define.KeyField;
            int intValue = 0;
            if (keyField.DataType == DataType.Int &&
                !int.TryParse(key, out intValue))
            {
                Debug.LogError("Target Class Key is int");
                return (null, -1);
            }

            foreach (var dataObject in data)
            {
                for (int i = 0; i < dataObject.DataCount; i++)
                {
                    var keyData = dataObject.GetData(keyField.ID, i);
                    if (
                        (keyField.DataType == DataType.String && keyData.String == key) ||
                        (keyField.DataType == DataType.Int && keyData.Int == intValue)
                    )
                    {
                        return (dataObject, i);
                    }
                }
            }

            Debug.LogError($"Key Not found. {key}");
            return (null, -1);
        }
    }
}

#endif