namespace Yorozu.DB
{
    /// <summary>
    /// データの種類
    /// </summary>
    internal enum DataType
    {
        Int,
        String,
        Float,
        Bool,
        Sprite,
        GameObject,
        ScriptableObject,
        /// <summary>
        /// その他の UnityEngine.Object
        /// </summary>
        UnityObject,
        /// <summary>
        /// YorozuDBEnumDataObject で定義した Enum を利用する
        /// </summary>
        Enum,
        Vector2,
        Vector3,
        Vector2Int,
        Vector3Int,
        Color,
        AudioClip,
    }
}
