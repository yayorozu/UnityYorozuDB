using UnityEngine;

namespace Yorozu.DB
{
    public abstract class YorozuDBDataAbstract
    {
        /// <summary>
        /// もととなるデータ生成されたやつはIndexでデータを見る
        /// </summary>
        private YorozuDBDataObject _data;
        /// <summary>
        /// 何行目のデータか
        /// </summary>
        private int _row;
        
        internal void SetUp(YorozuDBDataObject data, int row)
        {
            _data = data;
            _row = row;
        }

        private DBDataContainer Data(int fieldId) => _data.GetData(fieldId, _row);
        
        /// <summary>
        /// primitive
        /// </summary>
        /// <returns></returns>
        protected string String(int fieldId) => Data(fieldId).String;
        protected float Float(int fieldId) => Data(fieldId).Float;
        protected int Int(int fieldId) => Data(fieldId).Int;
        
        /// <summary>
        /// UnityEngin.Object
        /// </summary>
        protected Sprite Sprite(int fieldId) => Data(fieldId).UnityObject as Sprite;
        protected GameObject GameObject(int fieldId) => Data(fieldId).UnityObject as GameObject;
        protected ScriptableObject ScriptableObject(int fieldId) => Data(fieldId).UnityObject as ScriptableObject;
        protected UnityEngine.Object UnityObject(int fieldId) => Data(fieldId).UnityObject;
        
        /// <summary>
        /// TODO キャストしてるため、アクセス頻度が高いとGCが無駄にでるのでキャッシュする
        /// </summary>
        protected Vector2 Vector2(int fieldId) => Data(fieldId).GetFromString<Vector2>();
        protected Vector3 Vector3(int fieldId) => Data(fieldId).GetFromString<Vector3>();
        protected Vector2Int Vector2Int(int fieldId) => Data(fieldId).GetFromString<Vector2Int>();
        protected Vector3Int Vector3Int(int fieldId) => Data(fieldId).GetFromString<Vector3Int>();
    }
}