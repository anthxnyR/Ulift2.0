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
    [Route("api/[controller]")]
    [ApiController]
    public class DestinationController : Controller
    {
        private IDestinationCollection db = new DestinationCollection();
        private readonly ILogger<DestinationController> _logger;

        public DestinationController(ILogger<DestinationController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllDestinations()
        {
            return Ok(await db.GetAllDestinations());
        }
        [HttpPost]
        public async Task<IActionResult> InsertDestination([FromBody] Models.Destination destination)
        {
            if (destination == null)
            {
                return BadRequest();
            }
            db.ValidateDestinationAttributes(destination, ModelState);
            await db.InsertDestination(destination);
            return Created("Created", true);
        }
        [HttpPut]
        public async Task<IActionResult> UpdateDestination([FromBody] Models.Destination destination)
        {
            if (destination == null)
            {
                return BadRequest();
            }
            db.ValidateDestinationAttributes(destination, ModelState);
            await db.UpdateDestination(destination);
            return Created("Created", true);
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDestination(String id)
        {
            await db.DeleteDestination(id);
            return NoContent();
        }
    }
}
