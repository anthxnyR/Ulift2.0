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
    public class UserController : Controller
    {
        private IUserCollection db = new UserCollection();
        private readonly ILogger<UserController> _logger;
        public UserController(ILogger<UserController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            return Ok(await db.GetAllUsers());
        }
        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] Models.User user)
        {
            if (user == null)
            {
                return BadRequest();
            }
            Console.WriteLine("Validating user attributes");
            db.ValidateUserAttributes(user, ModelState);
            if (ModelState.IsValid)
            {
                await db.InsertUser(user);
                return Created("Created", true);
            }
            else
            {
                return BadRequest(ModelState);
            }
            
        }
        [HttpPut]
        public async Task<IActionResult> UpdateUser([FromBody] Models.User user)
        {
            if (user == null)
            {
                return BadRequest();
            }
            db.ValidateUserAttributes(user, ModelState);
            await db.UpdateUser(user);
            return Created("Created", true);
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(String id)
        {
            await db.DeleteUser(id);
            return NoContent();
        }
    }
}
