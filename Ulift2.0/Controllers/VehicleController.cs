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
        [HttpGet]
        public async Task<IActionResult> GetAllVehicles()
        {
            return Ok(await db.GetAllVehicles());
        }
    }
}
