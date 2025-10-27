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
        var handler = new WebSocketHandler(app.Services, app.Configuration);
        await handler.HandleWebSocketConnection(webSocket);
    }
    else
    {
        context.Response.StatusCode = 400;
    }
});

app.Run();
