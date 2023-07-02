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
    public class URouteController : Controller
    {
        private IURouteCollection db = new URouteCollection();
        private readonly ILogger<URouteController> _logger;

        public URouteController(ILogger<URouteController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllRoutes()
        {
            return Ok(await db.GetAllRoutes());
        }
        [HttpPost]
        public async Task<IActionResult> InsertRoute([FromBody] Models.URoute route)
        {
            if (route == null)
            {
                return BadRequest();
            }
            db.ValidateRouteAttributes(route, ModelState);
            await db.InsertRoute(route);
            return Created("Created", true);
        }
        [HttpPut]
        public async Task<IActionResult> UpdateRoute([FromBody] Models.URoute route)
        {
            if (route == null)
            {
                return BadRequest();
            }
            db.ValidateRouteAttributes(route, ModelState);
            await db.UpdateRoute(route);
            return Created("Created", true);
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRoute(String id)
        {
            await db.DeleteRoute(id);
            return NoContent();
        }
        [HttpGet("{email}/{inUCAB}")]
        public async Task<IActionResult> GetUserRoutes(String email, bool inUCAB)
        {
            return Ok(await db.GetUserRoutes(email, inUCAB));
        }
    }
}
