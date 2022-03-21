using System;
using System.Collections.Generic;
using UnityEngine;

namespace Yorozu.DB
{
    /// <summary>
    /// Enum っぽいデータを保持
    /// </summary>
    internal class YorozuDBEnumDataObject : ScriptableObject
    {
        [Serializable]
        internal class EnumDefine
        {
            [SerializeField]
            internal List<string> Values;
        }

        /// <summary>
        /// 定義一覧
        /// </summary>
        [SerializeField]
        internal List<EnumDefine> Defines = new List<EnumDefine>();
    }
    
#if UNITY_EDITOR
    [UnityEditor.CustomEditor(typeof(YorozuDBEnumDataObject))]
    internal class YorozuDBEnumDataObjectEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            if (GUILayout.Button("Open Editor"))
            {
                // TODO 
            }
        }
    }
#endif
}