using BankSystem.Shared.DTOs;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace BankSystem.Server.BackgroundServices
{
    /// <summary>
    /// TCP Socket сервер. Port 9001 дээр ажиллана.
    /// NumberTerminal дэлгэцүүд нэг удаа холбогдоод байнга сонсож байна.
    /// Теллер дугаар дуудахад сервер SHOW_NUMBER мессежийг бүх дэлгэцэд push хийнэ.
    ///
    /// HTTP-с ялгаа: HTTP = request → response → disconnect
    ///               TCP  = connect нэг удаа → сервер хүссэн үедээ push
    /// </summary>
    public class TcpSocketServer : BackgroundService
    {
        /// <summary>
        /// Холбогдсон клиентүүдийн жагсаалт.
        /// List thread-safe биш тул _clientLock-оор хамгаална.
        /// </summary>
        private readonly List<TcpClient> _clients = new();
        private readonly object _clientLock = new();
        private TcpListener? _listener;

        /// <summary>
        /// Бүх холбогдсон дэлгэцэнд мессеж илгээнэ.
        /// Салсан клиентийг жагсаалтаас автоматаар хасна.
        /// </summary>
        public async Task BroadcastAsync(SocketMessage message)
        {
            var json = JsonSerializer.Serialize(message);

            // \n: клиент мессеж хэзээ дуусахыг мэднэ (line delimiter)
            var data = Encoding.UTF8.GetBytes(json + "\n");

            List<TcpClient> deadClients = new();

            lock (_clientLock)
            {
                foreach (var client in _clients)
                {
                    try
                    {
                        if (client.Connected)
                            client.GetStream().Write(data, 0, data.Length);
                        else
                            deadClients.Add(client);
                    }
                    catch
                    {
                        // Бичиж чадахгүй бол клиент салсан гэж үзнэ
                        deadClients.Add(client);
                    }
                }

                foreach (var dead in deadClients)
                    _clients.Remove(dead);
            }

            await Task.CompletedTask;
        }

        /// <summary>
        /// BackgroundService-ийн үндсэн loop. Програм эхлэхэд автоматаар ажиллана.
        /// Клиентийн холболт хүлээж, тус бүрийг тусдаа async task-д боловсруулна.
        /// </summary>
        protected override async Task ExecuteAsync(CancellationToken ct)
        {
            _listener = new TcpListener(IPAddress.Any, 9001);
            _listener.Start();
            Console.WriteLine("[TCP] Socket сервер port 9001 дээр эхэллээ");

            while (!ct.IsCancellationRequested)
            {
                try
                {
                    var client = await _listener.AcceptTcpClientAsync(ct);
                    Console.WriteLine($"[TCP] Холбогдлоо: {client.Client.RemoteEndPoint}");

                    lock (_clientLock)
                        _clients.Add(client);

                    // Fire-and-forget: клиент тус бүрийг хүлээхгүйгээр зэрэг боловсруулна
                    _ = HandleClientAsync(client, ct);
                }
                catch (OperationCanceledException) { break; }
                catch (Exception ex)
                {
                    Console.WriteLine($"[TCP] Алдаа: {ex.Message}");
                }
            }

            _listener.Stop();
            Console.WriteLine("[TCP] Socket сервер зогслоо");
        }

        /// <summary>
        /// Нэг клиенттэй харилцах loop.
        /// Клиент салахад жагсаалтаас хасаж resource чөлөөлнө.
        /// </summary>
        private async Task HandleClientAsync(TcpClient client, CancellationToken ct)
        {
            var buffer = new byte[1024];
            try
            {
                var stream = client.GetStream();
                while (!ct.IsCancellationRequested && client.Connected)
                {
                    var bytesRead = await stream.ReadAsync(buffer, ct);
                    if (bytesRead == 0) break; // Клиент холболтоо хааж салсан

                    // PING мессеж ирвэл ACK хариулна
                    var msg = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim();
                    if (msg == "PING")
                    {
                        var ack = Encoding.UTF8.GetBytes("ACK\n");
                        await stream.WriteAsync(ack, ct);
                    }
                }
            }
            catch { /* Клиент гэнэт салсан */ }
            finally
            {
                lock (_clientLock)
                    _clients.Remove(client);
                client.Dispose();
                Console.WriteLine("[TCP] Клиент салалаа");
            }
        }
    }
}
