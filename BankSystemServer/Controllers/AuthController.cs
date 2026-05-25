using BankSystem.Server.Services;
using BankSystem.Shared.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace BankSystem.Server.Controllers
{
    /// <summary>Теллерийн нэвтрэлт, гарах endpoint-ууд.</summary>
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;

        public AuthController(AuthService authService)
        {
            _authService = authService;
        }

        /// <summary>
        /// Теллер нэвтрэнэ. Амжилттай бол token буцаана.
        /// POST /api/auth/login
        /// </summary>
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest req)
        {
            var (success, token, message) = _authService.Login(req.Username, req.Password);

            if (!success)
                return Unauthorized(new LoginResponse { Success = false, Message = message });

            var user = _authService.GetUserByToken(token!);
            return Ok(new LoginResponse
            {
                Success = true,
                Token = token,
                TellerWindowId = user?.TellerWindowId
            });
        }

        /// <summary>
        /// Системээс гарна.
        /// POST /api/auth/logout
        /// Header: X-Auth-Token: {token}
        /// </summary>
        [HttpPost("logout")]
        public IActionResult Logout()
        {
            var token = Request.Headers["X-Auth-Token"].FirstOrDefault();
            if (token is not null)
                _authService.Logout(token);
            return Ok();
        }
    }
}
