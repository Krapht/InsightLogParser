
namespace InsightLogParser.Client.Menu
{
    public enum MenuResult
    {
        NotValidOption, //The keypress was not valid
        Ok, //The keypress was valid and has been handled
        PrintMenu, //The menu should be displayed again
        CloseMenu, //The keypress was valid and menu can be closed
    }

    public interface IMenu
    {
        bool IsPersistent => true;
        IEnumerable<(char? key, string text)> MenuOptions { get; }
        Task<MenuResult> HandleOptionAsync(char keyChar);
    }
}
