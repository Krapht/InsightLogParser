
namespace InsightLogParser.Client.Menu
{
    public interface IMenu
    {
        IEnumerable<(char? key, string text)> MenuOptions { get; }
        Task<bool> HandleOptionAsync(char keyChar);
    }
}
