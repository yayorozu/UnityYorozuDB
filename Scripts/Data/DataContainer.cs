using System;
using UnityEngine;

namespace Yorozu.DB
{
    [Serializable]
    internal class DataContainer
    {
        [SerializeField]
        internal string String;
        [SerializeField]
        internal int Int;
        [SerializeField]
        internal float Float;
        [SerializeField]
        internal bool Bool;
        [SerializeField]
        internal UnityEngine.Object UnityObject;

        internal T GetFromString<T>()
        {
            if (string.IsNullOrEmpty(String))
            {
                return default;
            }
            return JsonUtility.FromJson<T>(String);
        }
        
        internal void SetToString(object obj)
        {
            String = JsonUtility.ToJson(obj);
        }
    }
}