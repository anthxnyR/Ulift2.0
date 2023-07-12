using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ulift2._0.Hubs;
using Ulift2._0.Models;
using Ulift2._0.Repository;

namespace Ulift2._0.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MessageController : Controller
    {
        private IMessageCollection db = new MessageCollection();
        private readonly ILogger<MessageController> _logger;
        private readonly IHubContext<ChatHub> _chatHubContext;

        public MessageController(ILogger<MessageController> logger, IHubContext<ChatHub> chatHubContext)
        {
            _logger = logger;
            _chatHubContext = chatHubContext;
        }

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

        [HttpPost("{liftId}/{senderEmail}/{receiverEmail}/{message}")]
        public async Task<IActionResult> Send(string liftId, string senderEmail, string receiverEmail, string message)
        {
            await _chatHubContext.Clients.Client(receiverEmail).SendAsync("ReceiveMessage", senderEmail, message);
            
            var newMessage = new Message
            {
                SenderEmail = senderEmail,
                ReceiverEmail = receiverEmail,
                Content = message,
                DateTime = DateTime.Now,
                LiftID = liftId
            };

            await db.InsertMessage(newMessage);

            return Ok();
        }

        [HttpGet("{senderEmail}/{receiverEmail}")]
        public async Task<IActionResult> GetAllMessagesFromUser(String senderEmail, String receiverEmail)
        {
            return Ok(await db.GetAllMessagesFromUser(senderEmail, receiverEmail));
        }

    }
}
