using GameBackend.Controllers.HalloGames.Auth.Api.Models.Requests;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GameBackend.Controllers.HalloGames.Auth.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        [HttpGet("ping")]
        public IActionResult Ping()
        {
            return Ok();
        }

        [HttpPost("register/email")]
        public async Task<IActionResult> RegisterByEmail(EmailRegisterRequest emailRegisterRequest)
        {
            return Ok(emailRegisterRequest);
        }

        public async Task<IActionResult> LoginByEmail(EmailLoginRequest emailLoginRequest)
        {
            return Ok(emailLoginRequest);
        }
    }
}
