namespace Yorozu.DB
{
    /// <summary>
    /// 検索時のKey となる要素につける
    /// </summary>
    public interface IIntKey
    {
        int Key { get; }
    }
    
    public interface IStringKey
    {
        string Key { get; }
    }
}