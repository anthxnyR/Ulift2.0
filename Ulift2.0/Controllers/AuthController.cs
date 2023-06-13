using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using MongoDB.Bson;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ulift2._0.Models;
using Ulift2._0.Repository;
using Ulift2._0.Helpers;
namespace Ulift2._0.Controllers
{

    [EnableCors("_myAllowSpecificOrigins")]
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : Controller
    {
        private IAuthCollection db = new AuthCollection();
        private readonly ILogger<AuthController> _logger;

        public AuthController(ILogger<AuthController> logger)
        {
            _logger = logger;
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] Login user)
        {
            string Email = user.Email;
            string Password = user.Password;

            if (Email == null)
            {
                return BadRequest();
            }
            bool LoginSuccess = await db.Login(Email, Password);
            if (!LoginSuccess)
            {
                return BadRequest();
            }
            else
            {
                var payload = new { Email = Email, Password = Password };
                string tkn = JwtService.GetToken(payload);
                return Ok(new { Token = tkn, Message = "Logged in successfully" });
            }
        }

        [EnableCors("MyCorsPolicy")]
        [HttpPost("SignUp")]
        public async Task<IActionResult> Register([FromBody] Models.User user)
        {
            if (user == null)
            {
                return BadRequest();
            }
            await db.Register(user);
            return Created("Created", true);
        }
        [HttpGet("Verify")]
        public async Task<IActionResult> Verify(string token)
        {
            if (token == null)
            {
                return BadRequest();
            }
            else
            {
                await db.Verify(token);
                return Ok("Tu cuenta fue verificada");
            }
            
        }
    }
}
