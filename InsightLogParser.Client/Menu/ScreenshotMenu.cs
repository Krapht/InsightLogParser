using System.Linq;
using InsightLogParser.Client.Screenshots;
using InsightLogParser.Common.Screenshots;
using InsightLogParser.Common.World;

namespace InsightLogParser.Client.Menu
{
    internal class ScreenshotMenu : IMenu
    {
        private readonly Spider _spider;
        private readonly CapturedScreenshot _capturedScreenshot;
        private readonly Configuration _configuration;
        private readonly MessageWriter _writer;
        private readonly ScreenshotManager? _screenshotManager;
        private readonly (ScreenshotCategory Category, string Description, bool IsDefault, bool IsRequested) _quickCategory;

        public ScreenshotMenu(Spider spider, MessageWriter messageWriter, Configuration configuration, CapturedScreenshot capturedScreenshot)
        {
            _spider = spider;
            _writer = messageWriter;
            _configuration = configuration;
            _capturedScreenshot = capturedScreenshot;
            _screenshotManager = spider.GetScreenshotManager();

            var puzzleName = WorldInformation.GetPuzzleName(_capturedScreenshot.PuzzleType);
            var menuOptions = new List<(char? key, string text)>
            {
                (null, $"Screenshot options for puzzle of type {puzzleName} ({_capturedScreenshot.PuzzleId}). Press backspace to go back to previous menu"),
                ('u', "Upload screenshot"),
                ('U', "Upload and delete screenshot"),
                ('c', "Ignore screenshot"),
                ('D', "Delete screenshot"),
            };

            _quickCategory = ScreenshotManager.GetScreenshotCategories(_capturedScreenshot.PuzzleType, _capturedScreenshot.IsSolved).FirstOrDefault(x => x.IsDefault);
            if (_quickCategory != default)
            {
                var categoryName = ScreenshotManager.GetCategoryName(_quickCategory.Category);
                menuOptions.Add(('q', $"Quick upload {puzzleName} ({_capturedScreenshot.PuzzleId}) as {categoryName} - {_quickCategory.Description}"));
            }

            MenuOptions = menuOptions;
        }

        public IEnumerable<(char? key, string text)> MenuOptions { get; }
        public async Task<MenuResult> HandleOptionAsync(char keyChar)
        {
            switch (keyChar)
            {
                case 'q':
                    if (_quickCategory == default) return MenuResult.NotValidOption;
                    var quickSuccess = await _spider.UploadScreenshotAsync(_capturedScreenshot, _quickCategory.Category).ConfigureAwait(ConfigureAwaitOptions.None);
                    if (!quickSuccess)
                    {
                        _writer.WriteError("Upload failed");
                        return MenuResult.PrintMenu;
                    }
                    _writer.WriteInfo("Upload successful");
                    if (_configuration.DeleteOnQuickUpload)
                    {
                        ConfirmDelete();
                    }
                    return MenuResult.CloseMenu;

                case 'u':
                    var t1 = SelectScreenshotType();
                    if (t1 == null) return MenuResult.Ok;
                    var success1 = await _spider.UploadScreenshotAsync(_capturedScreenshot, t1.Value).ConfigureAwait(ConfigureAwaitOptions.None);
                    if (!success1)
                    {
                        _writer.WriteError("Upload failed");
                        return MenuResult.PrintMenu;
                    }
                    _writer.WriteInfo("Upload successful");
                    return MenuResult.CloseMenu;

                case 'U':
                    var t2 = SelectScreenshotType();
                    if (t2 == null) return MenuResult.Ok;
                    var success2 = await _spider.UploadScreenshotAsync(_capturedScreenshot, t2.Value).ConfigureAwait(ConfigureAwaitOptions.None);
                    if (!success2)
                    {
                        _writer.WriteError("Upload failed");
                        return MenuResult.PrintMenu;
                    }
                    _writer.WriteInfo("Upload successful");
                    ConfirmDelete();
                    return MenuResult.CloseMenu;

                case 'c':
                    _screenshotManager?.FlagScreenshotAsHandled();
                    _writer.WriteLine("Ignored");
                    return MenuResult.CloseMenu;

                case 'D':
                    ConfirmDelete();
                    return MenuResult.CloseMenu;

                default:
                    return MenuResult.NotValidOption;
            }
        }

        private ScreenshotCategory? SelectScreenshotType()
        {
            var options = ScreenshotManager.GetScreenshotCategories(_capturedScreenshot.PuzzleType, _capturedScreenshot.IsSolved)
                .Select((x, i) => (index: i, category: x.Category, menu: ((char?)('0'+i), x.Description)))
                .ToList();
            var baseOptions = new (char? option, string text)[]
            {
                (default, "Select category or any other key to cancel"),
            };
            _writer.WriteMenu(baseOptions.Concat(options.Select(x => x.menu)));
            var keyPressed = Console.ReadKey(true).KeyChar;
            var selected = options.FirstOrDefault(x => keyPressed - '0' == x.index);
            if (selected == default) return null;

            return selected.category;
        }

        private void ConfirmDelete()
        {
            if (_configuration.ConfirmScreenshotDelete)
            {
                _writer.ConfirmDelete(_capturedScreenshot.ScreenshotPath);
                var pressedKey = Console.ReadKey(true);
                if (pressedKey.Key != ConsoleKey.Y)
                {
                    _writer.WriteError("Cancelled");
                    return;
                }
            }
            _screenshotManager?.DeleteScreenshot(_capturedScreenshot.ScreenshotPath);
            _writer.WriteInfo("Source screenshot deleted");
        }

        public bool IsPersistent => false;
    }
}
