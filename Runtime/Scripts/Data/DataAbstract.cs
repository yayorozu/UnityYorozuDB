using System;
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
        
        /// <summary>
        /// UnityEngin.Object
        /// </summary>
        protected Sprite Sprite(int fieldId) => Data(fieldId).UnityObject as Sprite;
        protected GameObject GameObject(int fieldId) => Data(fieldId).UnityObject as GameObject;
        protected AudioClip AudioClip(int fieldId) => Data(fieldId).UnityObject as AudioClip;
        protected ScriptableObject ScriptableObject(int fieldId) => Data(fieldId).UnityObject as ScriptableObject;
        protected UnityEngine.Object UnityObject(int fieldId) => Data(fieldId).UnityObject;
        /// <summary>
        /// TODO キャストしてるため、アクセス頻度が高いとGCが無駄にでるのでキャッシュする
        /// </summary>
        protected Vector2 Vector2(int fieldId) => Data(fieldId).GetFromString<Vector2>();
        protected Vector3 Vector3(int fieldId) => Data(fieldId).GetFromString<Vector3>();
        protected Vector2Int Vector2Int(int fieldId)
        {
            var array = Data(fieldId).GetFromString<SerializableIntArray>(); 
            return new Vector2Int(array.IntArray[0], array.IntArray[1]);
        }
        protected Vector3Int Vector3Int(int fieldId)
        {
            var array = Data(fieldId).GetFromString<SerializableIntArray>(); 
            return new Vector3Int(array.IntArray[0], array.IntArray[1], array.IntArray[2]);
        }
        protected Color Color(int fieldId) => Data(fieldId).GetFromString<Color>();
        
#if UNITY_EDITOR
        protected void Set(int fieldId, string value) => Data(fieldId).String = value;
        protected void Set(int fieldId, float value) => Data(fieldId).Float = value;
        protected void Set(int fieldId, bool value) => Data(fieldId).Bool = value;
        protected void Set(int fieldId, int value) => Data(fieldId).Int = value;
        protected void Set(int fieldId, int enumDefineId, Enum value)
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
                Set(fieldId, key.Value);
            }
        }
        
        protected void Set(int fieldId, UnityEngine.Object value) => Data(fieldId).UnityObject = value;
        protected void Set(int fieldId, Vector2 value) => Data(fieldId).SetToString(value);
        protected void Set(int fieldId, Vector3 value) => Data(fieldId).SetToString(value);
        protected void Set(int fieldId, Vector2Int value) => Data(fieldId).SetToString(value);
        protected void Set(int fieldId, Vector3Int value) => Data(fieldId).SetToString(value);
        protected void Set(int fieldId, Color value) => Data(fieldId).SetToString(value);
        
#endif
    }
}