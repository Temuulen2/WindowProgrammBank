using BankSystem.Server.Hubs;
using BankSystem.Server.Services;
using BankSystem.Shared.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace BankSystem.Server.Controllers
{
    /// <summary>Ханш харах, теллер шинэчлэх endpoint-ууд.</summary>
    [ApiController]
    [Route("api/[controller]")]
    public class ExchangeRateController : ControllerBase
    {
        private readonly ExchangeRateService _rateService;
        private readonly IHubContext<BankHub> _hub;

        public ExchangeRateController(ExchangeRateService rateService, IHubContext<BankHub> hub)
        {
            _rateService = rateService;
            _hub = hub;
        }

        /// <summary>Бүх валютын ханш. GET /api/exchangerate</summary>
        [HttpGet]
        public IActionResult GetAll() => Ok(_rateService.GetAll());

        /// <summary>Нэг валютын ханш. GET /api/exchangerate/USD</summary>
        [HttpGet("{currencyCode}")]
        public IActionResult Get(string currencyCode)
        {
            var rate = _rateService.Get(currencyCode);
            return rate is null ? NotFound($"'{currencyCode}' валют олдсонгүй") : Ok(rate);
        }

        /// <summary>
        /// Ханш шинэчилнэ. SignalR-ээр Blazor дэлгэцэнд тэр даруй мэдэгдэнэ.
        /// PUT /api/exchangerate/USD
        /// </summary>
        [HttpPut("{currencyCode}")]
        public async Task<IActionResult> Update(string currencyCode, [FromBody] UpdateRateRequest req)
        {
            if (req.BuyRate <= 0 || req.SellRate <= 0 || req.BuyRate > req.SellRate)
                return BadRequest("Ханш буруу байна: авах ханш ≤ зарах ханш байх ёстой");

            var updated = _rateService.Update(currencyCode, req.BuyRate, req.SellRate, req.TellerWindowId);
            if (updated is null)
                return NotFound($"'{currencyCode}' валют олдсонгүй");

            // SignalR broadcast: Blazor дэлгэц "ReceiveRateUpdate" event сонсоод шинэчлэгдэнэ
            await _hub.Clients.All.SendAsync("ReceiveRateUpdate", updated);

            return Ok(updated);
        }
    }
}
