using UnityEditor;
using UnityEngine;

namespace Yorozu.DB
{
    /// <summary>
    /// 設定データ
    /// </summary>
    public class YorozuDBSetting : ScriptableObject
    {
#if UNITY_EDITOR
        /// <summary>
        /// 保存先
        /// </summary>
        [SerializeField]
        internal DefaultAsset ScriptExportFolder;

        internal bool SetFolder(string path)
        {
            if (!AssetDatabase.IsValidFolder(path))
                return false;
            
            var loadAsset = AssetDatabase.LoadAssetAtPath<DefaultAsset>(path);
            if (loadAsset == null)
                return false;
            
            ScriptExportFolder = loadAsset;
            EditorUtility.SetDirty(this);
            return true;
        }
#endif

        internal static YorozuDBSetting Load()
        {
            var path = "YorozuDBSetting";
            var setting = Resources.Load<YorozuDBSetting>(path);
            if (setting == null)
            {
                setting = CreateInstance<YorozuDBSetting>();
            }

            return setting;
        }
    }
}