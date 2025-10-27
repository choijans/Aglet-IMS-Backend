using Aglet_Backend.Services;
using System.Net.WebSockets;
using System.Text;

namespace Aglet_Backend.Services
{
    public class WebSocketHandler
    {
        private readonly IServiceProvider _services;
        private readonly IConfiguration _configuration;

        public WebSocketHandler(IServiceProvider services, IConfiguration configuration)
        {
            _services = services;
            _configuration = configuration;
        }

        public async Task HandleWebSocketConnection(WebSocket webSocket)
        {
            var buffer = new byte[1024 * 4];
            var receiveResult = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

            // Connection timeout after 30 minutes of inactivity
            var timeoutCancellation = new CancellationTokenSource(TimeSpan.FromMinutes(30));

            try
            {
                while (!receiveResult.CloseStatus.HasValue && !timeoutCancellation.Token.IsCancellationRequested)
                {
                    var message = System.Text.Encoding.UTF8.GetString(buffer, 0, receiveResult.Count);
                    Console.WriteLine($"Received message: {message}");

                    // Process the message
                    using (var scope = _services.CreateScope())
                    {
                        var stockService = scope.ServiceProvider.GetRequiredService<IStockService>();
                        var shoeService = scope.ServiceProvider.GetRequiredService<IShoeService>();

                        try
                        {
                            var response = await ProcessMessage(message, stockService, shoeService);
                            var responseBytes = System.Text.Encoding.UTF8.GetBytes(response);
                            await webSocket.SendAsync(new ArraySegment<byte>(responseBytes), WebSocketMessageType.Text, true, CancellationToken.None);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error processing WebSocket message: {ex.Message}");
                            var errorResponse = $"{{\"type\":\"error\",\"message\":\"Internal server error\"}}";
                            var errorBytes = System.Text.Encoding.UTF8.GetBytes(errorResponse);
                            await webSocket.SendAsync(new ArraySegment<byte>(errorBytes), WebSocketMessageType.Text, true, CancellationToken.None);
                        }
                    }

                    receiveResult = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                }
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("WebSocket connection timed out");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"WebSocket connection error: {ex.Message}");
            }
            finally
            {
                if (webSocket.State == WebSocketState.Open)
                {
                    await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Connection closed", CancellationToken.None);
                }
            }
        }

        private async Task<string> ProcessMessage(string message, IStockService stockService, IShoeService shoeService)
        {
            try
            {
                var jsonMessage = System.Text.Json.JsonDocument.Parse(message);
                var type = jsonMessage.RootElement.GetProperty("type").GetString();

                // Basic authentication check - in production, use proper JWT or API keys
                var expectedToken = _configuration["WebSocket:AuthToken"] ?? "aglet_secure_token_2024";
                if (!jsonMessage.RootElement.TryGetProperty("authToken", out var authTokenElement) ||
                    authTokenElement.GetString() != expectedToken)
                {
                    return "{\"type\":\"error\",\"message\":\"Authentication failed\"}";
                }

                switch (type)
                {
                    case "stock_update":
                        var shoeId = jsonMessage.RootElement.GetProperty("shoeId").GetInt32();
                        var quantityChange = jsonMessage.RootElement.GetProperty("quantityChange").GetInt32();

                        // Validate input
                        if (shoeId <= 0)
                            return "{\"type\":\"error\",\"message\":\"Invalid shoe ID\"}";

                        await shoeService.UpdateStockAsync(shoeId, quantityChange);

                        return $"{{\"type\":\"stock_updated\",\"shoeId\":{shoeId},\"newStock\":{await shoeService.GetCurrentStockAsync(shoeId)}}}";

                    case "stock_query":
                        var queryShoeId = jsonMessage.RootElement.GetProperty("shoeId").GetInt32();

                        // Validate input
                        if (queryShoeId <= 0)
                            return "{\"type\":\"error\",\"message\":\"Invalid shoe ID\"}";

                        var currentStock = await shoeService.GetCurrentStockAsync(queryShoeId);

                        return $"{{\"type\":\"stock_info\",\"shoeId\":{queryShoeId},\"currentStock\":{currentStock}}}";

                    default:
                        return $"{{\"type\":\"error\",\"message\":\"Unknown message type: {type}\"}}";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing message: {ex.Message}");
                return $"{{\"type\":\"error\",\"message\":\"Internal server error\"}}";
            }
        }
    }
}