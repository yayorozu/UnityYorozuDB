using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Yorozu.DB
{
    public abstract class DataAbstract
    {
        /// <summary>
        /// もととなるデータ生成されたやつはIndexでデータを見る
        /// </summary>
        private YorozuDBDataObject _data;

        /// <summary>
        /// 拡張を参照する際にはこれから
        /// </summary>
        protected T Extend<T>() where T : ScriptableObject
        {
            if (_data.ExtendFieldsObject == null)
                return default;
            
            return _data.ExtendFieldsObject as T;
        }

        private YorozuDBEnumDataObject _enumData => YorozuDB.EnumData;
        /// <summary>
        /// 何行目のデータか
        /// </summary>
        private int _row;

        protected int row => _row;
        
        internal void SetUp(YorozuDBDataObject data, int row)
        {
            _data = data;
            _row = row;
        }

        private DataContainer Data(int fieldId) => _data.GetData(fieldId, _row);
        
        private int GetEnumValue(int enumDefineId, int key)
        {
            if (_enumData == null)
            {
                throw new Exception($"{nameof(YorozuDBEnumDataObject)} is not attach.");
            }
            var findDefine = _enumData.Defines.FirstOrDefault(d => d.ID == enumDefineId);
            if (findDefine == null)
            {
                Debug.LogError("Enum Data is not Define");
                return 0;
            }
            var index = Mathf.Max(findDefine.KeyValues.FindIndex(kv => kv.Key == key), 0);
            return index;
        }
        
        /// <summary>
        /// primitive
        /// </summary>
        /// <returns></returns>
        protected string String(int fieldId) => Data(fieldId).String;
        protected float Float(int fieldId) => Data(fieldId).Float;
        protected bool Bool(int fieldId) => Data(fieldId).Bool;
        protected int Int(int fieldId) => Data(fieldId).Int;
        protected int Enum(int fieldId, int enumDefineId) => GetEnumValue(enumDefineId, Int(fieldId));
        
        protected IEnumerable<string> Strings(int fieldId) => Data(fieldId)._strings;
        protected IEnumerable<float> Floats(int fieldId) => Data(fieldId)._floats;
        protected IEnumerable<bool> Bools(int fieldId) => Data(fieldId)._bools;
        protected IEnumerable<int> Ints(int fieldId) => Data(fieldId)._ints;
        protected IEnumerable<int> Enums(int fieldId, int enumDefineId) => Data(fieldId)._ints.Select(v => GetEnumValue(enumDefineId, v));
        
        /// <summary>
        /// UnityEngin.Object
        /// </summary>
        protected Sprite Sprite(int fieldId) => Data(fieldId).UnityObject as Sprite;
        protected GameObject GameObject(int fieldId) => Data(fieldId).UnityObject as GameObject;
        protected AudioClip AudioClip(int fieldId) => Data(fieldId).UnityObject as AudioClip;
        protected ScriptableObject ScriptableObject(int fieldId) => Data(fieldId).UnityObject as ScriptableObject;
        protected UnityEngine.Object UnityObject(int fieldId) => Data(fieldId).UnityObject;
        
        protected IEnumerable<Sprite> Sprites(int fieldId) => Data(fieldId)._unityObjects.Select(v => v as Sprite);
        protected IEnumerable<GameObject> GameObjects(int fieldId) => Data(fieldId)._unityObjects.Select(v => v as GameObject);
        protected IEnumerable<AudioClip> AudioClips(int fieldId) => Data(fieldId)._unityObjects.Select(v => v as AudioClip);
        protected IEnumerable<ScriptableObject> ScriptableObjects(int fieldId) => Data(fieldId)._unityObjects.Select(v => v as ScriptableObject);
        protected IEnumerable<UnityEngine.Object> UnityObjects(int fieldId) => Data(fieldId)._unityObjects;

        /// <summary>
        /// TODO キャストしてるため、アクセス頻度が高いとGCが無駄にでるのでキャッシュする
        /// </summary>
        protected Vector2 Vector2(int fieldId) => Data(fieldId).Vector2;
        protected Vector3 Vector3(int fieldId) => Data(fieldId).Vector3;
        protected Vector2Int Vector2Int(int fieldId) => Data(fieldId).Vector2Int;
        protected Vector3Int Vector3Int(int fieldId) => Data(fieldId).Vector3Int;
        protected Color Color(int fieldId) => Data(fieldId).Color;

        protected IEnumerable<Vector2> Vector2s(int fieldId) => Data(fieldId).Vector2s;
        protected IEnumerable<Vector3> Vector3s(int fieldId) => Data(fieldId).Vector3s;
        protected IEnumerable<Vector2Int> Vector2Ints(int fieldId) => Data(fieldId).Vector2Ints;
        protected IEnumerable<Vector3Int> Vector3Ints(int fieldId) => Data(fieldId).Vector3Ints;
        protected IEnumerable<Color> Colors(int fieldId) => Data(fieldId).Colors;

#if UNITY_EDITOR
        protected void Set(int fieldId, string value, int index = 0) => Data(fieldId)._strings[index] = value;
        protected void Set(int fieldId, float value, int index = 0) => Data(fieldId)._floats[index] = value;
        protected void Set(int fieldId, bool value, int index = 0) => Data(fieldId)._bools[index] = value;
        protected void Set(int fieldId, int value, int index = 0) => Data(fieldId)._ints[index] = value;
        protected void Set(int fieldId, int enumDefineId, Enum value, int index = 0)
        {
            var guids = AssetDatabase.FindAssets($"t:{nameof(YorozuDBEnumDataObject)}");
            if (guids.Length <= 0)
                throw new Exception($"{nameof(YorozuDBEnumDataObject)} is not attach.");

            var path = AssetDatabase.GUIDToAssetPath(guids[0]);
            var enumData = AssetDatabase.LoadAssetAtPath<YorozuDBEnumDataObject>(path);
            var findDefine = enumData.Defines.FirstOrDefault(d => d.ID == enumDefineId);
            if (findDefine == null)
            {
                Debug.LogError("Enum Data is not Define");
                return;
            }

            var key = enumData.GetEnumKey(enumDefineId, value.ToString());
            if (key.HasValue)
            {
                Set(fieldId, key.Value, index);
            }
        }
        
        protected void Set(int fieldId, UnityEngine.Object value, int index = 0) => Data(fieldId)._unityObjects[index] = value;
        protected void Set(int fieldId, Vector2 value, int index = 0) => Data(fieldId).SetToString(value, index);
        protected void Set(int fieldId, Vector3 value, int index = 0) => Data(fieldId).SetToString(value, index);
        protected void Set(int fieldId, Vector2Int value, int index = 0) => Data(fieldId).SetToString(value, index);
        protected void Set(int fieldId, Vector3Int value, int index = 0) => Data(fieldId).SetToString(value, index);
        protected void Set(int fieldId, Color value, int index = 0) => Data(fieldId).SetToString(value, index);
#endif
    }
}