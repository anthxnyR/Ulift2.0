using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ulift2._0.Models;
using Ulift2._0.Repository;

namespace Ulift2._0.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MessageController : Controller
    {
        private IMessageCollection db = new MessageCollection();
        [HttpGet]
        public async Task<IActionResult> GetAllMessages()
        {
            return Ok(await db.GetAllMessages());
        }
        [HttpPost]
        public async Task<IActionResult> InsertMessage([FromBody] Models.Message message)
        {
            if (message == null)
            {
                return BadRequest();
            }
            db.ValidateMessageAttributes(message,ModelState);
            await db.InsertMessage(message);
            return Created("Created", true);
        }
        [HttpPut]
        public async Task<IActionResult> UpdateMessage([FromBody] Models.Message message)
        {
            if (message == null)
            {
                return BadRequest();
            }
            db.ValidateMessageAttributes(message, ModelState);
            await db.UpdateMessage(message);
            return Created("Created", true);
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMessage(String id)
        {
            await db.DeleteMessage(id);
            return NoContent();
        }
    }
}
