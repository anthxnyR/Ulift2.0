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
    public class LiftController : Controller
    {
        private ILiftCollection db = new LiftCollection();
        private readonly ILogger<LiftController> _logger;

        public LiftController(ILogger<LiftController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllLifts()
        {
            return Ok(await db.GetAllLifts());
        }
        [HttpPost]
        public async Task<IActionResult> InsertLift([FromBody] Models.Lift lift)
        {
            if (lift == null)
            {
                return BadRequest();
            }
            db.ValidateLiftAttributes(lift,ModelState);
            await db.InsertLift(lift);
            return Created("Created", true);
        }
        [HttpPut]
        public async Task<IActionResult> UpdateLift([FromBody] Models.Lift lift)
        {
            if (lift == null)
            {
                return BadRequest();
            }
            db.ValidateLiftAttributes(lift, ModelState);
            await db.UpdateLift(lift);
            return Created("Created", true);
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLift(String id)
        {
            await db.DeleteLift(id);
            return NoContent();
        }
        [HttpPost("Create")]
        public async Task<IActionResult> CreateLift([FromBody] Models.LiftCreation lift)
        {
            var response = await db.CreateLift(lift);
            if (response is BadRequestObjectResult badRequestResult)
            {
                return BadRequest("El viaje no ha podido ser creado");
            }
            return Created("Lift Created", true);
        }
        [HttpGet("Available")]
        public async Task<IActionResult> GetAvailableLifts()
        {
            return Ok(await db.GetAvailableLifts());
        }

        [HttpGet("{lat}/{lng}/{wOnly}/{maxD}")]
        public async Task<IActionResult> GetMatch(double lat, double lng, bool wOnly, int maxD)
        {
            try
            {
                if (lat == 0 && lng == 0)
                {
                    return Ok(await db.GetAvailableLifts(wOnly));
                }

                var lifts = await db.GetMatch(lat, lng, wOnly, maxD);

                if (lifts == null)
                {
                    throw new Exception("No se han encontrado viajes");
                }

                if (lifts.Count > 0)
                {
                    return Ok(new
                    {
                        success = true,
                        message = "optimal routes",
                        lifts = lifts
                    });
                }
                else
                {
                    return Ok(new
                    {
                        success = true,
                        message = "no active lifts",
                    });
                }
            }
            catch (System.Exception)
            {
                throw new Exception("Error");
            }
        }
    }
}
