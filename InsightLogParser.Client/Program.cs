using InsightLogParser.Client;
using InsightLogParser.Client.Websockets;

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

    var webSocketServer = Server.StartWebSocketServer(forcedExitSource.Token);

    var mainThing = new MainThing(forcedExitSource.Token);
    await mainThing.RunAsync();

    forcedExitSource.Cancel();

    await webSocketServer;

    Console.WriteLine("Successfully shut down, press any key to exit");
    Console.ReadKey(true);
}
catch (Exception e)
{
    Console.WriteLine("### !!! Unhandled exception, see crash.log for details !!! ###");
    File.WriteAllText("crash.log", e.ToString());
    throw;
}
