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
    public class DestinationController : Controller
    {
        private IDestinationCollection db = new DestinationCollection();
        [HttpGet]
        public async Task<IActionResult> GetAllDestinations()
        {
            return Ok(await db.GetAllDestinations());
        }
    }
}
