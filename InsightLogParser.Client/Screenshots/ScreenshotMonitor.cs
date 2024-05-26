namespace InsightLogParser.Client.Screenshots;

internal class ScreenshotMonitor : IDisposable
{
    private readonly ScreenshotManager _screenshotManager;
    private readonly FileSystemWatcher _fsw;

    public ScreenshotMonitor(string screenshotFolder, ScreenshotManager screenshotManager)
    {
        _screenshotManager = screenshotManager;
        _fsw = new FileSystemWatcher(screenshotFolder, "*.jpg");
        _fsw.IncludeSubdirectories = false;
        _fsw.Created += Fsw_Created;
        _fsw.EnableRaisingEvents = true;
    }

    private void Fsw_Created(object sender, FileSystemEventArgs e)
    {
        if (e.ChangeType != WatcherChangeTypes.Created)
        {
            return;
        }
        _screenshotManager.NewScreenshot(e.FullPath);
    }

    public void Dispose()
    {
        _fsw.Dispose();
    }
}