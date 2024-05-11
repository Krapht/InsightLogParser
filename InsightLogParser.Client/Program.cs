using InsightLogParser.Client;

try
{
    var forcedExitSource = new CancellationTokenSource();

    AppDomain.CurrentDomain.ProcessExit += async (sender, eventArgs) => { await forcedExitSource.CancelAsync(); };

    async void ConsoleOnCancelKeyPress(object? sender, ConsoleCancelEventArgs eventArgs)
    {
        eventArgs.Cancel = true;
        Console.WriteLine("CTRL+C pressed");
        Console.CancelKeyPress -= ConsoleOnCancelKeyPress;
        await forcedExitSource.CancelAsync();
    }

    Console.CancelKeyPress += ConsoleOnCancelKeyPress;

    var mainThing = new MainThing(forcedExitSource.Token);
    await mainThing.RunAsync();
    Console.WriteLine("Successfully shut down, press any key to exit");
    Console.ReadKey(true);
}
catch (Exception e)
{
    Console.WriteLine("### !!! Unhandled exception, see crash.log for details !!! ###");
    File.WriteAllText("crash.log", e.ToString());
    throw;
}
