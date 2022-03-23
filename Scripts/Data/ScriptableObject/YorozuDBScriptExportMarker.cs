using UnityEngine;

namespace Yorozu.DB
{
    /// <summary>
    /// Script をExportする場所においておくアセット
    /// 設定ファイルを作成してもいいが、用途がなさすぎるので簡易にこちらで
    /// </summary>
    internal class YorozuDBScriptExportMarker : ScriptableObject
    {
    }
    
#if UNITY_EDITOR
#endif
}