using System.Linq;
using UnityEngine;

namespace Yorozu.DB
{
    public abstract class YorozuDBData
    {
        /// <summary>
        /// もととなるデータ生成されたやつはIndexでデータを見る
        /// </summary>
        private YorozuDBDataObject _data;
        /// <summary>
        /// 何行目のデータか
        /// </summary>
        private int _rowIndex;
        
        internal void SetUp(YorozuDBDataObject data, int row)
        {
            _data = data;
            _rowIndex = row;
        }

        private DBData Data(int index) => _data.Groups[_rowIndex].Data[index];
        
        protected string String(int index) => Data(index).String;
        protected float Float(int index) => Data(index).Float;
        protected int Int(int index) => Data(index).Int;
        protected Sprite Sprite(int index) => Data(index).UnityObject as Sprite;
        protected GameObject GameObject(int index) => Data(index).UnityObject as GameObject;
        protected ScriptableObject ScriptableObject(int index) => Data(index).UnityObject as ScriptableObject;
        protected UnityEngine.Object UnityObject(int index) => Data(index).UnityObject;
        /// <summary>
        /// TODO キャストしてるため、アクセス頻度が高いとGCが無駄にでるのでキャッシュする
        /// </summary>
        protected Vector2 Vector2(int index) => Data(index).GetFromString<Vector2>();
        protected Vector3 Vector3(int index) => Data(index).GetFromString<Vector3>();
        protected Vector2Int Vector2Int(int index) => Data(index).GetFromString<Vector2Int>();
        protected Vector3Int Vector3Int(int index) => Data(index).GetFromString<Vector3Int>();
    }
}