using Aglet_Backend.DataAccess;
using Aglet_Backend.Profiles;
using Aglet_Backend.Services;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using System.Net.WebSockets;
using Microsoft.AspNetCore.Builder;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddAutoMapper(typeof(MappingProfile));
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<ShoeDbContext>(
        options =>
        {
            options.UseSqlServer(builder.Configuration.GetConnectionString("AgletDb"));
            options.LogTo(Console.WriteLine, Microsoft.Extensions.Logging.LogLevel.Information);
            options.EnableSensitiveDataLogging();
            options.EnableDetailedErrors();
        }
    );

builder.Services.AddScoped<IShoeService, ShoeService>();
builder.Services.AddScoped<IPurchaseService, PurchaseService>();
builder.Services.AddScoped<IStockService, StockService>();

// WebSocket support is built-in for ASP.NET Core, no need to add services

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.UseWebSockets();

app.MapControllers();

// WebSocket endpoint for real-time communication
app.Map("/ws", async (HttpContext context) =>
{
    if (context.WebSockets.IsWebSocketRequest)
    {
        var webSocket = await context.WebSockets.AcceptWebSocketAsync();
        await HandleWebSocketConnection(webSocket, app.Services);
    }
    else
    {
        context.Response.StatusCode = 400;
    }
});

app.Run();

// WebSocket handler method
async Task HandleWebSocketConnection(WebSocket webSocket, IServiceProvider services)
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

            // Rate limiting: max 100 messages per minute per connection
            // In production, implement proper rate limiting

            Console.WriteLine($"Received message: {message}");

            // Process the message
            using (var scope = services.CreateScope())
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

async Task<string> ProcessMessage(string message, IStockService stockService, IShoeService shoeService)
{
    try
    {
        var jsonMessage = System.Text.Json.JsonDocument.Parse(message);
        var type = jsonMessage.RootElement.GetProperty("type").GetString();

        // Basic authentication check - in production, use proper JWT or API keys
        var expectedToken = builder.Configuration["WebSocket:AuthToken"] ?? "aglet_secure_token_2024";
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

                return $"{{\"type\":\"stock_updated\",\"shoeId\":{shoeId},\"newStock\":{await GetCurrentStock(shoeService, shoeId)}}}";

            case "stock_query":
                var queryShoeId = jsonMessage.RootElement.GetProperty("shoeId").GetInt32();

                // Validate input
                if (queryShoeId <= 0)
                    return "{\"type\":\"error\",\"message\":\"Invalid shoe ID\"}";

                var currentStock = await GetCurrentStock(shoeService, queryShoeId);

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

async Task<int> GetCurrentStock(IShoeService shoeService, int shoeId)
{
    return await shoeService.GetCurrentStockAsync(shoeId);
}
