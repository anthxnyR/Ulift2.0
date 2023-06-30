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
    public class WaitingListController : Controller
    {
        private IWaitingListCollection db = new WaitingListCollection();
        private readonly ILogger<WaitingListController> _logger;

        public WaitingListController(ILogger<WaitingListController> logger)
        {
            _logger = logger;
        }

        [HttpPost("Request")]
        public async Task<IActionResult> InsertRequest([FromBody] Models.WaitingList waitingList)
        {
            if (waitingList == null)
            {
                _logger.LogInformation("Request null");
                return BadRequest(ModelState);
            }
            try
            {
                await db.InsertRequest(waitingList);
            }catch(Exception e)
            {
                _logger.LogInformation(e.Message);
                return BadRequest(ModelState);
            }
            return Created("Created", true);
        }

        [HttpPut]
        public async Task<IActionResult> UpdateVehicle([FromBody] Models.WaitingList waitingList)
        {
            if ( waitingList == null)
            {
                return BadRequest();
            }
            await db.UpdateRequest(waitingList);
            return Ok();
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteVehicle(String id)
        {
            await db.DeleteRequest(id);
            return Ok();
        }
        [HttpGet]
        public async Task<IActionResult> GetAllRequests()
        {
            return Ok(await db.GetAllRequests());
        }
        [HttpGet("Requests/{driverEmail}")]
        public async Task<IActionResult> GetAllRequestsByDriver(String driverEmail)
        {
            return Ok(await db.GetAllRequestsByDriver(driverEmail));
        }
    }
}
