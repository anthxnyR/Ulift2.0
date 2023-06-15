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
    public class VehicleController : Controller
    {
        private IVehicleCollection db = new VehicleCollection();
        private readonly ILogger<VehicleController> _logger;

        public VehicleController(ILogger<VehicleController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllVehicles()
        {
            return Ok(await db.GetAllVehicles());
        }
        [HttpPost]
        public async Task<IActionResult> InsertVehicle([FromBody] Models.Vehicle vehicle)
        {
            if (vehicle == null)
            {
                _logger.LogInformation("Vehicle null");
                return BadRequest();
            }
            db.ValidateVehicleAttributes(vehicle, ModelState);
            await db.InsertVehicle(vehicle);
            _logger.LogInformation("Vehicle inserted");
            return Created("Created", true);
        }
        [HttpPut]
        public async Task<IActionResult> UpdateVehicle([FromBody] Models.Vehicle vehicle)
        {
            if (vehicle == null)
            {
                return BadRequest();
            }
            db.ValidateVehicleAttributes(vehicle, ModelState);
            await db.UpdateVehicle(vehicle);
            return Created("Created", true);
        }
        [HttpDelete]
        public async Task<IActionResult> DeleteVehicle(String id)
        {
            await db.DeleteVehicle(id);
            return NoContent();
        }
    }
}
