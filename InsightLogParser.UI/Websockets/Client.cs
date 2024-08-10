using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace InsightLogParser.UI.Websockets {
    internal class Client {
        private readonly ClientWebSocket _clientWebSocket = new();
        private readonly CancellationTokenSource _cancellationTokenSource = new();

        public event EventHandler<MessageReceivedEventArgs> MessageReceived;

        public async Task ConnectAsync(int port) {
            try {
                await _clientWebSocket.ConnectAsync(new Uri($"ws://localhost:{port}/ws/"), _cancellationTokenSource.Token);
            } catch (Exception) {
                _cancellationTokenSource.Cancel();
            }
            _ = ReceiveMessagesAsync();

            // Every 30 seconds, send a ping message to the server.
            while (_clientWebSocket.State == WebSocketState.Open) {
                await SendAsync(new {
                    type = "ping",
                    id = new Random().Next()
                });

                await Task.Delay(30000);
            }
        }

        private async Task ReceiveMessagesAsync() {
            while (_clientWebSocket.State == WebSocketState.Open) {
                var buffer = new byte[1024];
                var result = await _clientWebSocket.ReceiveAsync(new ArraySegment<byte>(buffer), _cancellationTokenSource.Token);
                var message = Encoding.UTF8.GetString(buffer, 0, result.Count);

                JsonDocument data;
                try {
                    data = JsonDocument.Parse(message);

                    // Handle the message.
                    var type = data.RootElement.GetProperty("type").GetString();

                    switch (type) {
                        case "pong":
                            // Handle the pong message.
                            var id = data.RootElement.GetProperty("id").GetInt32();

                            break;
                        default:
                            // Raise an event with the JSON data.
                            OnMessageReceived(new MessageReceivedEventArgs(message));
                            break;
                    }
                } catch (Exception) {
                    // Close the application if the message is not valid JSON.
                    await DisconnectAsync();
                }
            }
        }

        public async Task SendAsync(object message) {
            var bytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));
            await _clientWebSocket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, _cancellationTokenSource.Token);
        }

        public async Task DisconnectAsync() {
            await _clientWebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", _cancellationTokenSource.Token);
            Application.Exit();
        }

        protected virtual void OnMessageReceived(MessageReceivedEventArgs e) {
            MessageReceived?.Invoke(this, e);
        }
    }
}
