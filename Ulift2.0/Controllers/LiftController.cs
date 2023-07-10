using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ulift2._0.Models;
using Ulift2._0.Repository;
using Ulift2._0.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace Ulift2._0.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LiftController : Controller
    {
        private ILiftCollection db;
        private readonly ILogger<LiftController> _logger;

        public LiftController(ILogger<LiftController> logger, IHubContext<ChatHub> chatHubContext)
        {
            _logger = logger;
            db = new LiftCollection(chatHubContext);
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
            if (response == null)
            {
                return BadRequest("El viaje no ha podido ser creado");
            }
            Created("Lift Created", true);
            return Ok(response);
        }
        [HttpGet("Available")]
        public async Task<IActionResult> GetAvailableLiftsByDriverGender(bool wOnly, bool inUcab)
        {
            var availableLifts = await db.GetAvailableLiftsByDriverGender(wOnly, inUcab);
            
            foreach (var lift in availableLifts)
            {
                _logger.LogInformation($"Driver Name: {lift.Driver.Gender}");
                _logger.LogInformation("");
            }
            return Ok(availableLifts);
        }

       [HttpGet("{lat}/{lng}/{wOnly}/{maxD}/{inUcab}")]
        public async Task<IActionResult> GetMatch(double lat, double lng, bool wOnly, int maxD, bool inUcab)
        {
            try
            {
                // var lat = double.Parse(Request.Query["lat"]);
                // var lng = double.Parse(Request.Query["lng"]);
                // var wOnly = bool.Parse(Request.Query["wOnly"]);
                // var maxD = int.Parse(Request.Query["maxD"]);
                if (lat == 0 && lng == 0)
                {
                    var availableLifts = await db.GetAvailableLiftsByDriverGender(wOnly, inUcab);
                    return Ok(availableLifts);
                }

                var lifts = await db.GetMatch(lat, lng, wOnly, maxD, inUcab);

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
                        lifts
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
            catch (Exception)
            {
                throw new Exception("Error");
            }
        }
        [HttpPost("AcceptRequest/{liftId}/{passengerEmail}")]
        public async Task<IActionResult> AcceptRequest(string LiftId, string passengerEmail)
        {
            try
            {
                await db.AcceptRequest(LiftId, passengerEmail);
            }catch(Exception e)
            {
                return BadRequest(e.Message);
            }
            return Ok();
        }
        [HttpPost("StartLift/{liftId}")]
        public async Task<IActionResult> StartLift(string LiftId)
        {
            try
            {
                await db.StartLift(LiftId);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
            return Ok();
        }
        [HttpPost("PasajeroCheck/{passengerEmail}")]
        public async Task<IActionResult> PasajeroCheck(string passengerEmail)
        {
            try
            {
                var response = await db.PasajeroCheck(passengerEmail);
                return Ok(response);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpDelete("cancelarLift")]
        public async Task<IActionResult> DeleteLiftByDriver(string driverEmail)
        {
            try
            {
                await db.DeleteLiftByDriver(driverEmail);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
            return Ok();
        }

        [HttpPut("complete/{liftId}")]
        public async Task<IActionResult> LiftCompleteCheck2(string liftId)
        {
            try
            {
                await db.LiftCompleteCheck2(liftId);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
            return Ok();
        }

        //Este es el nuevo metodo donde agrega los ratings de los pasajeros
        [HttpPut("complete")]
        public async Task<IActionResult> LiftCompleteCheck([FromBody] PassengerRatings ratingList)
        {
            try
            {
                await db.LiftCompleteCheck(ratingList);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
            return Ok();
        }

        //Esto recibe el rating del pasajero al conductor y le hace el check de que llegó
        [HttpPost("createRatingPassenger")]
        public async Task<IActionResult> CreateRatingPassenger(String liftId, String passengerEmail, int rating )
        {
            try
            {
                await db.CreateRatingPassenger(liftId,passengerEmail,rating);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
            return Ok();
        }
        [HttpGet("checkPassengerArriving")]
        public async Task<IActionResult> CheckPassengerArriving(String liftId)
        {
            try
            {
                var response = await db.CheckAllArriving(liftId);
                return Ok(response);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("checkAcceptCola")]
        public async Task<IActionResult> CheckAcceptCola(String liftId, String email)
        {
            try
            {
                var response = await db.CheckAcceptCola(liftId, email);
                return Ok(response);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("usersInLift/{liftId}")]
        public async Task<IActionResult> UsersInLift(String liftId)
        {
            try
            {
                var response = await db.UsersInLift(liftId);
                return Ok(response);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("driverInLift/{liftId}")]
        public async Task<IActionResult> DriverInLift(String liftId)
        {
            try
            {
                var response = await db.DriverInLift(liftId);
                return Ok(response);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}
