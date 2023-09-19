using System;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR

namespace Yorozu.DB
{
    public static class YorozuDBEditorUtility
    {
        /// <summary>
        /// データを追加するインスタンスは
        /// </summary>
        /// <param name="data"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T AddData<T>(this YorozuDBDataObject data) where T : DataAbstract
        {
            var dataCount = data.DataCount;
            data.AddRow();
            var instance = Activator.CreateInstance(typeof(T)) as DataAbstract;            
            instance.SetUp(data, dataCount);
            
            return instance as T;
        }

        public static void Clear(this YorozuDBDataObject data)
        {
            data.Clear();
        }
        
        public static YorozuDBDataObject LoadDataObject(string path)
        {
            return AssetDatabase.LoadAssetAtPath<YorozuDBDataObject>(path);
        }
        
        public static void Dirty(this ScriptableObject asset)
        {
            EditorUtility.SetDirty(asset);
            AssetDatabase.SaveAssets();
        }
    }
}

#endif