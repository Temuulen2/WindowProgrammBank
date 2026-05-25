using BankSystem.Server.BackgroundServices;
using BankSystem.Server.Hubs;
using BankSystem.Server.Services;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// HTTP endpoint-ууд (Controllers/ хавтасны классуудыг идэвхжүүлнэ)
builder.Services.AddControllers();

// SignalR: Blazor ханшийн дэлгэцтэй realtime харилцана (.NET 10-д framework дотор байна)
builder.Services.AddSignalR();

// API баримт бичиг
builder.Services.AddOpenApi();

// Өөр port-оос ирсэн хүсэлтийг зөвшөөрнө (TellerApp, NumberTerminal, Blazor)
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader());
});

// ── Өөрсдийн сервисүүд ───────────────────────────────────────────────
// Singleton: програм дуустал нэг л instance амьдарна, state хадгалагдана

builder.Services.AddSingleton<AuthService>();
builder.Services.AddSingleton<TicketQueueService>();
builder.Services.AddSingleton<AccountService>();
builder.Services.AddSingleton<ExchangeRateService>();

// TcpSocketServer-г Singleton + HostedService хоёуланд бүртгэнэ:
// - Singleton: TicketController inject хийж BroadcastAsync дуудна
// - HostedService: програм эхлэхэд автоматаар TCP port 9001 нээнэ
builder.Services.AddSingleton<TcpSocketServer>();
builder.Services.AddHostedService(sp => sp.GetRequiredService<TcpSocketServer>());

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    // http://localhost:5272/scalar/v1 — browser дээр API тест хийх UI
    app.MapScalarApiReference();
}

// Хүсэлт controller-д очихоос өмнө CORS шалгагдана
app.UseCors();
app.UseAuthorization();

// Controller-уудын endpoint-ууд
app.MapControllers();

// SignalR WebSocket endpoint — клиентүүд /bankhub хаягаар холбогдоно
app.MapHub<BankHub>("/bankhub");

app.Run();
