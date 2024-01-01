using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using Yorozu.DB.TreeView;
#endif

namespace Yorozu.DB
{
    [Serializable]
    internal partial class DataContainer
    {
        internal int Size => Mathf.Max(_strings.Length, _ints.Length, _floats.Length, _bools.Length, _unityObjects.Length);

        [SerializeField]
        internal string[] _strings = Array.Empty<string>();
        internal string String
        {
            get { return _strings.Length <= 0 ? default : _strings[0]; }
#if UNITY_EDITOR
            set
            {
                if (_strings.Length <= 0)
                    throw new Exception("string is not set");

                _strings[0] = value;
            }
#endif
        }

        [SerializeField]
        internal int[] _ints = Array.Empty<int>();
        internal int Int
        {
            get { return _ints.Length <= 0 ? default : _ints[0]; }
#if UNITY_EDITOR
            set
            {
                if (_ints.Length <= 0)
                    throw new Exception("int is not set");

                _ints[0] = value;
            }
#endif
        }

        [SerializeField]
        internal float[] _floats = Array.Empty<float>();
        internal float Float
        {
            get { return _floats.Length <= 0 ? default : _floats[0]; }
#if UNITY_EDITOR
            set
            {
                if (_floats.Length <= 0)
                    throw new Exception("string is not set");

                _floats[0] = value;
            }
#endif
        }

        [SerializeField]
        internal bool[] _bools = Array.Empty<bool>();
        internal bool Bool
        {
            get { return _bools.Length <= 0 ? default : _bools[0]; }
#if UNITY_EDITOR
            set
            {
                if (_bools.Length <= 0)
                    throw new Exception("string is not set");

                _bools[0] = value;
            }
#endif
        }

        [SerializeField]
        internal UnityEngine.Object[] _unityObjects = Array.Empty<UnityEngine.Object>();
        internal UnityEngine.Object UnityObject
        {
            get { return _unityObjects.Length <= 0 ? default : _unityObjects[0]; }
#if UNITY_EDITOR
            set
            {
                if (_unityObjects.Length <= 0)
                    throw new Exception("string is not set");

                _unityObjects[0] = value;
            }
#endif
        }
        
        internal Vector2 Vector2 => GetFromString<Vector2>(_strings[0]);
        internal Vector3 Vector3 => GetFromString<Vector3>(_strings[0]);
        internal Vector2Int Vector2Int
        {
            get
            {
                var array = GetFromString<SerializableIntArray>(_strings[0]); 
                return new Vector2Int(array.IntArray[0], array.IntArray[1]);
            }
        }
        internal Vector3Int Vector3Int
        {
            get
            {
                var array = GetFromString<SerializableIntArray>(_strings[0]); 
                return new Vector3Int(array.IntArray[0], array.IntArray[1], array.IntArray[2]);
            }
        }
        internal Color Color => GetFromString<Color>(_strings[0]);
        
        internal IEnumerable<Vector2> Vector2s => _strings.Select(GetFromString<Vector2>);
        internal IEnumerable<Vector3> Vector3s => _strings.Select(GetFromString<Vector3>);
        internal IEnumerable<Vector2Int> Vector2Ints => _strings.Select(GetFromString<Vector2Int>);
        internal IEnumerable<Vector3Int> Vector3Ints => _strings.Select(GetFromString<Vector3Int>);
        internal IEnumerable<Color> Colors => _strings.Select(GetFromString<Color>);
        
        internal DataContainer Copy()
        {
            var copy = new DataContainer();
            if (_strings != null && _strings.Length > 0)
            {
                copy._strings = new string[_strings.Length];
                for (var i = 0; i < _strings.Length; i++)
                    copy._strings[i] = _strings[i];
            }

            if (_ints != null && _ints.Length > 0)
            {
                copy._ints = new int[_ints.Length];
                for (var i = 0; i < _ints.Length; i++)
                    copy._ints[i] = _ints[i];
            }

            if (_floats != null && _floats.Length > 0)
            {
                copy._floats = new float[_floats.Length];
                for (var i = 0; i < _floats.Length; i++)
                    copy._floats[i] = _floats[i];
            }

            if (_bools != null && _bools.Length > 0)
            {
                copy._bools = new bool[_bools.Length];
                for (var i = 0; i < _bools.Length; i++)
                    copy._bools[i] = _bools[i];
            }

            if (_unityObjects != null && _unityObjects.Length > 0)
            {
                copy._unityObjects = new UnityEngine.Object[_unityObjects.Length];
                for (var i = 0; i < _unityObjects.Length; i++)
                    copy._unityObjects[i] = _unityObjects[i];
            }

            return copy;
        }

        private T GetFromString<T>(string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                return default;
            }
            return JsonUtility.FromJson<T>(s);
        }
        
        internal void SetToString(object obj, int index)
        {
            _strings[index] = JsonUtility.ToJson(obj);
        }
    }
}