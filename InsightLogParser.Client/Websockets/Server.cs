using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using InsightLogParser.Common.PuzzleParser;
using InsightLogParser.Common.World;

namespace InsightLogParser.Client.Websockets;

internal class Server : ISocketUiCommands
{
    private readonly ConcurrentBag<WebSocket> _clients = [];
    private readonly CancellationTokenSource _stopTokenSource;

    private ISocketParserCommands _parserCommands;
    private Task _listenerTask;

    public Server(CancellationToken forcedCancellationToken)
    {
        _stopTokenSource = CancellationTokenSource.CreateLinkedTokenSource(forcedCancellationToken);
    }

    internal void SetParserCommands(ISocketParserCommands parserCommands)
    {
        _parserCommands = parserCommands;
    }

    internal async Task<int> StartWebSocketServer()
    {
        // Find a random open port.
        int port;
        try
        {
            using (var tcpListener = new TcpListener(IPAddress.Loopback, 0))
            {
                tcpListener.Start();
                port = ((IPEndPoint)tcpListener.LocalEndpoint).Port;
                tcpListener.Stop();
            }
        }
        catch (Exception)
        {
            throw new Exception("Failed to find an open port.");
        }

        // Create HTTP listener.
        var httpListener = new HttpListener();
        httpListener.Prefixes.Add($"http://localhost:{port}/ws/");
        httpListener.Start();

        _listenerTask = StartListening(httpListener);
        return port;
    }

    private async Task StartListening(HttpListener httpListener)
    {
        try
        {
            while (!_stopTokenSource.IsCancellationRequested)
            {
                // Wait for incoming WebSocket connection and handle it.
                var contextTask = httpListener.GetContextAsync();
                var completedTask = await Task.WhenAny(contextTask, Task.Delay(Timeout.Infinite, _stopTokenSource.Token));

                if (completedTask != contextTask) continue;

                var context = await contextTask;

                if (context.Request.IsWebSocketRequest)
                {
                    var webSocketContext = await context.AcceptWebSocketAsync(null);
                    _ = HandleWebSocketConnection(webSocketContext.WebSocket, _stopTokenSource.Token);
                }
                else
                {
                    context.Response.StatusCode = 400;
                    context.Response.Close();
                }
            }
        }
        finally
        {
            httpListener.Stop();
        }
    }

    internal async Task StopWebSocketServer()
    {
        await SendAsync(new { type = "shutdown" });

        foreach (var client in _clients)
        {
            await client.CloseAsync(WebSocketCloseStatus.NormalClosure, "Server shutting down", CancellationToken.None);
        }

        await _stopTokenSource.CancelAsync().ConfigureAwait(ConfigureAwaitOptions.None);

        //Wait for the listener to stop
        await _listenerTask.ConfigureAwait(ConfigureAwaitOptions.None);
    }

    private async Task HandleWebSocketConnection(WebSocket webSocket, CancellationToken cancellationToken)
    {
        // Add the connection.
        _clients.Add(webSocket);

        while (webSocket.State == WebSocketState.Open && !cancellationToken.IsCancellationRequested)
        {
            var buffer = new byte[1024];

            // Wait for incoming message and handle it.
            var resultTask = webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken);
            var completedTask = await Task.WhenAny(resultTask, Task.Delay(Timeout.Infinite, cancellationToken));

            if (completedTask != resultTask) continue;

            var result = await resultTask;

            if (result.MessageType == WebSocketMessageType.Close)
            {
                // Client is closing the connection.
                await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", cancellationToken);
                continue;
            }

            // Message received.  Parse it as JSON.
            var message = Encoding.UTF8.GetString(buffer, 0, result.Count);

            JsonDocument data;
            try
            {
                data = JsonDocument.Parse(message);

                // Handle the message.
                var type = data.RootElement.GetProperty("type").GetString();

                switch (type)
                {
                    case "ping":
                    {
                        // Respond with a pong.
                        var id = data.RootElement.GetProperty("id").GetInt32();

                        var response = new
                        {
                            type = "pong",
                            id
                        };

                        await webSocket.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(response))), WebSocketMessageType.Text, true, cancellationToken);

                        break;
                    }
                    case "screenshot":
                    {
                        // Get the screenshot URL.
                        var puzzleId = data.RootElement.GetProperty("puzzleId").GetInt32();
                        if (puzzleId == 0) continue;

                        var screenshots = await _parserCommands.GetPuzzleScreenshotsAsync(puzzleId);
                        if (screenshots == null) continue;

                        var first = screenshots.FirstOrDefault(s => s.IsPrimaryCategory);
                        if (first == null) continue;

                        var response = new
                        {
                            type = "screenshot",
                            puzzleId,
                            url = first.ImageUrl,
                            uploader = first.Uploader,
                        };

                        await webSocket.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(response))), WebSocketMessageType.Text, true, cancellationToken);

                        break;
                    }
                }
            }
            catch (Exception)
            {
                // Close the connection if the message is not valid JSON.
                await webSocket.CloseAsync(WebSocketCloseStatus.InvalidPayloadData, "Invalid JSON", cancellationToken);
            }
        }

        // Remove the connection.
        _clients.TryTake(out _);
    }

    private async Task SendAsync(object message)
    {
        // Encode the message.
        var messageBuffer = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));
        var messageSegment = new ArraySegment<byte>(messageBuffer);

        // Send the message to all clients.
        foreach (var client in _clients)
        {
            if (client.State != WebSocketState.Open) continue;
            await client.SendAsync(messageSegment, WebSocketMessageType.Text, true, CancellationToken.None);
        }
    }

    #region ISocketUiCommands

    public void SetTarget(Coordinate coordinate, InsightPuzzle puzzle, int routeNumber, int routeLength)
    {
        _ = SendAsync(new {
            type = "setTarget",
            target = new {
                coordinate.X,
                coordinate.Y,
                coordinate.Z
            },
            puzzleType = (int)puzzle.Type,
            puzzleId = puzzle.KrakenId,
            routeNumber = routeNumber,
            routeLength = routeLength,
        });
    }

    public void MovePlayer(Coordinate coordinate)
    {
        _ = SendAsync(new {
            type = "movePlayer",
            destination = new {
                coordinate.X,
                coordinate.Y,
                coordinate.Z
            }
        });
    }


    #endregion
}