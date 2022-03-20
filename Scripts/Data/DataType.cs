namespace Yorozu.DB
{
    /// <summary>
    /// データの種類
    /// </summary>
    internal enum DataType
    {
        String,
        Float,
        Int,
        Bool,
        Sprite,
        GameObject,
        ScriptableObject,
        /// <summary>
        /// その他の UnityEngine.Object
        /// </summary>
        UnityObject,
        Vector2,
        Vector3,
        Vector2Int,
        Vector3Int,
    }
}