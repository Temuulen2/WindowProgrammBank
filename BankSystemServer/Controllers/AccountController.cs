using BankSystem.Server.Services;
using BankSystem.Shared.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace BankSystem.Server.Controllers
{
    /// <summary>Дансны мэдээлэл харах, мөнгө шилжүүлэх endpoint-ууд.</summary>
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly AccountService _accountService;

        public AccountController(AccountService accountService)
        {
            _accountService = accountService;
        }

        /// <summary>Бүх дансны жагсаалт. GET /api/account</summary>
        [HttpGet]
        public IActionResult GetAll() => Ok(_accountService.GetAllAccounts());

        /// <summary>Нэг дансны мэдээлэл. GET /api/account/ACC001</summary>
        [HttpGet("{accountNumber}")]
        public IActionResult Get(string accountNumber)
        {
            var account = _accountService.GetAccount(accountNumber);
            return account is null ? NotFound("Данс олдсонгүй") : Ok(account);
        }

        /// <summary>
        /// Мөнгө шилжүүлнэ.
        /// POST /api/account/transfer
        /// </summary>
        [HttpPost("transfer")]
        public async Task<ActionResult<TransferResponse>> Transfer([FromBody] TransferRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.FromAccountNumber) ||
                string.IsNullOrWhiteSpace(req.ToAccountNumber) ||
                req.Amount <= 0)
                return BadRequest(new TransferResponse
                {
                    Success = false,
                    Message = "Дансны дугаар болон дүн заавал байна"
                });

            var (success, message) = await _accountService.TransferAsync(
                req.FromAccountNumber, req.ToAccountNumber, req.Amount);

            return success
                ? Ok(new TransferResponse { Success = true })
                : BadRequest(new TransferResponse { Success = false, Message = message });
        }
    }
}
