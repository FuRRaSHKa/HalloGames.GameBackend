using GameBackend.HalloGames.Auth.Api.Models.Requests;
using GameBackend.HalloGames.Auth.Api.Models.Credentials;
using GameBackend.HalloGames.Auth.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace GameBackend.HalloGames.Auth.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register/email")]
        public async Task<IActionResult> RegisterByEmail(EmailRegisterRequest emailRegisterRequest)
        {
            var credentials = new PasswordCredentials(emailRegisterRequest.Email, emailRegisterRequest.Password);
            var user = await _authService.Register(credentials);

            if (user == null)
                return Conflict();

            return Ok(user);
        }

        [HttpPost("login/email")]
        public async Task<IActionResult> LoginByEmail(EmailLoginRequest emailLoginRequest)
        {
            var credentials = new PasswordCredentials(emailLoginRequest.Email, emailLoginRequest.Password);
            var tokens = await _authService.Login(credentials);

            if (tokens == null)
                return Unauthorized();

            return Ok(tokens);
        }
    }
}
