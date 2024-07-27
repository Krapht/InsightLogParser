using System.Collections.Concurrent;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace InsightLogParser.Client.Websockets {
    internal class Server {
        private static readonly ConcurrentBag<WebSocket> _clients = [];

        internal static async Task StartWebSocketServer(CancellationToken cancellationToken) {
            // Create HTTP listener.
            var httpListener = new HttpListener();
            httpListener.Prefixes.Add("http://localhost:38254/ws/");
            httpListener.Start();

            try {
                while (!cancellationToken.IsCancellationRequested) {
                    // Wait for incoming WebSocket connection and handle it.
                    var contextTask = httpListener.GetContextAsync();
                    var completedTask = await Task.WhenAny(contextTask, Task.Delay(Timeout.Infinite, cancellationToken));

                    if (completedTask != contextTask) {
                        continue;
                    }

                    var context = await contextTask;

                    if (context.Request.IsWebSocketRequest) {
                        var webSocketContext = await context.AcceptWebSocketAsync(null);
                        _ = HandleWebSocketConnection(webSocketContext.WebSocket, cancellationToken);
                    } else {
                        context.Response.StatusCode = 400;
                        context.Response.Close();
                    }
                }
            } finally {
                httpListener.Stop();
            }
        }

        internal static async Task StopWebSocketServer() {
            await Server.SendAsync(new { type = "shutdown" });

            foreach (var client in _clients) {
                await client.CloseAsync(WebSocketCloseStatus.NormalClosure, "Server shutting down", CancellationToken.None);
            }
        }

        private static async Task HandleWebSocketConnection(WebSocket webSocket, CancellationToken cancellationToken) {
            // Add the connection.
            _clients.Add(webSocket);

            while (webSocket.State == WebSocketState.Open && !cancellationToken.IsCancellationRequested) {
                var buffer = new byte[1024];

                // Wait for incoming message and handle it.
                var resultTask = webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken);
                var completedTask = await Task.WhenAny(resultTask, Task.Delay(Timeout.Infinite, cancellationToken));

                if (completedTask != resultTask) {
                    continue;
                }

                var result = await resultTask;

                if (result.MessageType == WebSocketMessageType.Close) {
                    // Client is closing the connection.
                    await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", cancellationToken);
                    continue;
                } else {
                    // Message received.  Parse it as JSON.
                    var message = Encoding.UTF8.GetString(buffer, 0, result.Count);

                    JsonDocument data;
                    try {
                        data = JsonDocument.Parse(message);

                        // Handle the message.
                        var type = data.RootElement.GetProperty("type").GetString();

                        switch (type) {
                            case "ping":
                                // Respond with a pong.
                                var id = data.RootElement.GetProperty("id").GetInt32();

                                var response = new {
                                    type = "pong",
                                    id
                                };

                                await webSocket.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(response))), WebSocketMessageType.Text, true, cancellationToken);

                                break;
                        }
                    } catch (Exception) {
                        // Close the connection if the message is not valid JSON.
                        await webSocket.CloseAsync(WebSocketCloseStatus.InvalidPayloadData, "Invalid JSON", cancellationToken);
                        continue;
                    }
                }
            }

            // Remove the connection.
            _clients.TryTake(out _);
        }

        internal static async Task SendAsync(object message) {
            // Encode the message.
            var messageBuffer = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));
            var messageSegment = new ArraySegment<byte>(messageBuffer);

            // Send the message to all clients.
            foreach (var client in _clients) {
                if (client.State == WebSocketState.Open) {
                    await client.SendAsync(messageSegment, WebSocketMessageType.Text, true, CancellationToken.None);
                }
            }
        }
    }
}
