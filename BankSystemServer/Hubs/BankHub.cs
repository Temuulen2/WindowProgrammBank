using Microsoft.AspNetCore.SignalR;

namespace BankSystem.Server.Hubs
{
    /// <summary>
    /// SignalR WebSocket hub. Клиентүүд /bankhub хаягаар холбогдоно.
    /// Controller-ууд IHubContext&lt;BankHub&gt; дамжуулж бүх клиентэд broadcast хийнэ:
    ///   - "ReceiveNumberUpdate" → дугаар дуудагдсан үед
    ///   - "ReceiveRateUpdate"   → ханш өөрчлөгдсөн үед
    /// </summary>
    public class BankHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            Console.WriteLine($"[SignalR] Холбогдлоо: {Context.ConnectionId}");
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            Console.WriteLine($"[SignalR] Салалаа: {Context.ConnectionId}");
            await base.OnDisconnectedAsync(exception);
        }
    }
}
