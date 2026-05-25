using BankSystem.Server.BackgroundServices;
using BankSystem.Server.Hubs;
using BankSystem.Server.Services;
using BankSystem.Shared.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace BankSystem.Server.Controllers
{
    /// <summary>Дугаар олгох, дуудах, дарааллын байдал харах endpoint-ууд.</summary>
    [ApiController]
    [Route("api/[controller]")]
    public class TicketController : ControllerBase
    {
        private readonly TicketQueueService _queueService;
        private readonly IHubContext<BankHub> _hub;
        private readonly TcpSocketServer _socketServer;

        public TicketController(
            TicketQueueService queueService,
            IHubContext<BankHub> hub,
            TcpSocketServer socketServer)
        {
            _queueService = queueService;
            _hub = hub;
            _socketServer = socketServer;
        }

        /// <summary>
        /// Шинэ дугаар олгоно — NumberTerminal дуудна.
        /// POST /api/ticket/issue
        /// </summary>
        [HttpPost("issue")]
        public async Task<ActionResult<IssueTicketResponse>> Issue()
        {
            var ticket = await _queueService.IssueTicketAsync();
            return Ok(new IssueTicketResponse
            {
                TicketNumber = ticket.TicketNumber,
                IssuedAt = ticket.IssuedAt,
                QueueCount = _queueService.GetQueueCount()
            });
        }

        /// <summary>
        /// Дараагийн харилцагчийг дуудна.
        /// TCP-ээр дугаарын дэлгэцүүдэд, SignalR-ээр Blazor-д мэдэгдэнэ.
        /// POST /api/ticket/call-next
        /// Body: tellerWindowId (int)
        /// </summary>
        [HttpPost("call-next")]
        public async Task<ActionResult<CallNextResponse>> CallNext([FromBody] int tellerWindowId)
        {
            var ticket = await _queueService.CallNextAsync(tellerWindowId);
            if (ticket is null)
                return BadRequest("Дараалал хоосон байна");

            var response = new CallNextResponse
            {
                TicketNumber = ticket.TicketNumber,
                TellerWindowId = tellerWindowId,
                RemainingCount = _queueService.GetQueueCount()
            };

            // TCP: дугаарын дэлгэцүүдэд (NumberTerminal) push
            await _socketServer.BroadcastAsync(new SocketMessage
            {
                Type = "SHOW_NUMBER",
                Payload = System.Text.Json.JsonSerializer.Serialize(response)
            });

            // SignalR: Blazor болон бусад WebSocket клиентэд broadcast
            await _hub.Clients.All.SendAsync("ReceiveNumberUpdate", response);

            return Ok(response);
        }

        /// <summary>
        /// Одоогийн дугаар болон хүлээлтийн тоо.
        /// GET /api/ticket/status
        /// </summary>
        [HttpGet("status")]
        public IActionResult GetStatus() => Ok(new
        {
            currentNumber = _queueService.GetCurrentNumber(),
            queueCount = _queueService.GetQueueCount()
        });

        /// <summary>
        /// Хүлээж буй бүх дугааруудын жагсаалт.
        /// GET /api/ticket/queue → [1, 2, 3, ...]
        /// Теллерийн апп дэлгэц дээр жагсаалт харуулахад ашиглана.
        /// </summary>
        [HttpGet("queue")]
        public IActionResult GetQueue() => Ok(_queueService.GetWaitingNumbers());
    }
}
