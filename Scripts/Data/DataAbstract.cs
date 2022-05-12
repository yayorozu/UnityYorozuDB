using System;
using System.Linq;
using UnityEngine;

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
        protected bool fixKey => _data.IsFxKey;
        protected int GetFixKeyInt => _data.FixKeyData.Int;
        protected string GetFixKeyString => _data.FixKeyData.String;
        protected int GetFixKeyEnum(int enumDefineId) => GetEnumValue(enumDefineId, _data.FixKeyData.Int);

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
    }
}