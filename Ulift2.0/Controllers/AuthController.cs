using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ulift2._0.Repository;

namespace Ulift2._0.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : Controller
    {
        private IUserCollection db = new UserCollection();
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] Models.User user)
        {
            if (user == null)
            {
                return BadRequest();
            }
            var userFromDb = await db.Filter.Eq(s => s.Email, user.Email);
            if (userFromDb == null)
            {
                return NotFound();
            }
            if (!userFromDb.Password.Equals(user.Password))
            {
                return Unauthorized();
            }
            return Ok(userFromDb);
        }
    }
}
