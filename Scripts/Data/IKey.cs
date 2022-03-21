namespace Yorozu.DB
{
    /// <summary>
    /// 検索時のKey となる要素につける
    /// </summary>
    internal interface IIntKey
    {
        int Key { get; }
    }
    
    internal interface IStringKey
    {
        string Key { get; }
    }
}