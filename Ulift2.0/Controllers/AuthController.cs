using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ulift2._0.Repository;
namespace Ulift2._0.Controllers
{
    [EnableCors("AllowOrigin")]
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : Controller
    {
        private IAuthCollection db = new AuthCollection();

        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] Models.User user)
        {
            if (user == null)
            {
                return BadRequest();
            }
            await db.Login(user.Email, user.Password);
            return Ok("Logueado");
        }

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
