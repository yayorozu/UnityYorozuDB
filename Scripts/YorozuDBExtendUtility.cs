using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Yorozu.DB
{
    /// <summary>
    /// 拡張ScriptableObject周りの処理を行う
    /// </summary>
    internal static class YorozuDBExtendUtility
    {
        /// <summary>
        /// 拡張データの個数を取得
        /// </summary>
        internal static int DataCount(ScriptableObject scriptableObject)
        {
            var fields = FindFields(scriptableObject);
            if (fields.Count <= 0)
                return 0;
            
            // 上に要素追加されると困るので全部見つけて最大値を要素数とする
            return fields.Max(f =>
            {
                var elementType = f.FieldType.GetArrayType();
                if (elementType.IsArray)
                {
                    var array = f.GetValue(scriptableObject) as Array;
                    return array.Length;
                }

                var list = f.GetValue(scriptableObject) as IList;
                return list.Count;
            });
        }
        
        private static Dictionary<int, List<FieldInfo>> _fieldCache = new Dictionary<int, List<FieldInfo>>();
        
#if UNITY_EDITOR
        
        /// <summary>
        /// 対象となるフィールドを探す
        /// </summary>
        internal static List<FieldInfo> FindFields(ScriptableObject scriptableObject)
        {
            if (scriptableObject == null)
                return new List<FieldInfo>();
            
            var id = scriptableObject.GetInstanceID();
            
            if (!_fieldCache.ContainsKey(id))
            {
                var targetFields = new List<FieldInfo>();
                var type = scriptableObject.GetType();
                var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                foreach (var field in fields)
                {
                    if (!field.IsSerializableIList())
                        continue;
                    
                    targetFields.Add(field);
                }
                
                _fieldCache.Add(id, targetFields);
            }
            
            return _fieldCache[id];
        }

        
        /// <summary>
        /// 対象となるフィールドか判定
        /// </summary>
        private static bool IsSerializableIList(this FieldInfo fieldInfo)
        {
            var attributes = fieldInfo.GetCustomAttributes(true);
            if (
                attributes.Any(attr => attr is NonSerializedAttribute) ||
                fieldInfo.IsPrivate ||
                !fieldInfo.FieldType.IsSerializable
            )
                return false;
 
            if (fieldInfo.FieldType.IsArray)
                return true;

            if (fieldInfo.FieldType.IsGenericType && fieldInfo.FieldType.GetGenericTypeDefinition() == typeof(List<>))
                return true;

            return false;
        }
        
        internal static Type GetArrayType(this Type type)
        {
            if (type.IsArray)
                return type.GetElementType();
 
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
                return type.GetGenericArguments()[0];
            return null;
        }

        /// <summary>
        /// 追加定義のフィールドの中身を増やす
        /// </summary>
        internal static void AddFields(ScriptableObject scriptableObject)
        {
            if (scriptableObject == null)
                return;
            
            var editor = UnityEditor.Editor.CreateEditor(scriptableObject);
            var fields = FindFields(scriptableObject);

            editor.serializedObject.UpdateIfRequiredOrScript();
            foreach (var field in fields)
            {
                var prop = editor.serializedObject.FindProperty(field.Name);
                prop.InsertArrayElementAtIndex(prop.arraySize);
            }
            editor.serializedObject.ApplyModifiedProperties();
        }
        
        /// <summary>
        /// 特定Indexを削除
        /// </summary>
        internal static void RemoveFields(ScriptableObject scriptableObject, IOrderedEnumerable<int> indexes)
        {
            if (scriptableObject == null)
                return;

            var editor = UnityEditor.Editor.CreateEditor(scriptableObject);
            var fields = FindFields(scriptableObject);
            
            editor.serializedObject.UpdateIfRequiredOrScript();
            foreach (var field in fields)
            {
                var prop = editor.serializedObject.FindProperty(field.Name);
                foreach (var index in indexes)
                {
                    prop.DeleteArrayElementAtIndex(index);
                }
            }
            editor.serializedObject.ApplyModifiedProperties();
        }

        /// <summary>
        /// 新規で追加されたフィールドのサイズをあわせる
        /// </summary>
        internal static void FitFieldsSize(ScriptableObject scriptableObject, int requireCount)
        {
            if (scriptableObject == null)
                return;
            
            var editor = UnityEditor.Editor.CreateEditor(scriptableObject);
            var fields = FindFields(scriptableObject);

            editor.serializedObject.UpdateIfRequiredOrScript();
            foreach (var field in fields)
            {
                var prop = editor.serializedObject.FindProperty(field.Name);
                for (var i = prop.arraySize; i < requireCount; i++)
                {
                    prop.InsertArrayElementAtIndex(i);
                }
            }
            editor.serializedObject.ApplyModifiedProperties();
        }

        /// <summary>
        /// 各フィールドの最大サイズを計算
        /// </summary>
        internal static float ItemHeight(ScriptableObject scriptableObject, int row)
        {
            if (scriptableObject == null)
                return 0;
            
            var editor = UnityEditor.Editor.CreateEditor(scriptableObject);
            var fields = FindFields(scriptableObject);

            var max = 0f;
            editor.serializedObject.UpdateIfRequiredOrScript();
            foreach (var field in fields)
            {
                var prop = editor.serializedObject.FindProperty(field.Name);
                if (prop.arraySize <= 0)
                    continue;
                if (row >= prop.arraySize)
                    continue;
                
                var elementProp = prop.GetArrayElementAtIndex(row);
                elementProp.isExpanded = true;
                max = Mathf.Max(EditorGUI.GetPropertyHeight(elementProp), max);
            }

            return max;
        }

        /// <summary>
        /// 並べ替え
        /// </summary>
        internal static void Insert(ScriptableObject scriptableObject, int insertIndex, IOrderedEnumerable<int> targetIndexes)
        {
            if (scriptableObject == null)
                return;
            
            var editor = UnityEditor.Editor.CreateEditor(scriptableObject);
            var fields = FindFields(scriptableObject);
            
            editor.serializedObject.UpdateIfRequiredOrScript();
            foreach (var field in fields)
            {
                var backInsertedCount = 0;
                var frontInsertedCount = 0;
                var prop = editor.serializedObject.FindProperty(field.Name);
                foreach (var index in targetIndexes)
                {
                    if (index > insertIndex)
                    {
                        prop.MoveArrayElement(index + backInsertedCount++, insertIndex);
                    }
                    else
                    {
                        prop.MoveArrayElement(index, insertIndex - ++frontInsertedCount);
                    }
                }
            }
            editor.serializedObject.ApplyModifiedProperties();
        }
        
#endif

    }
}