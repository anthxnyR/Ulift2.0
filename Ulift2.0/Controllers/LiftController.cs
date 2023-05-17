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
        [HttpGet]
        public async Task<IActionResult> GetAllLifts()
        {
            return Ok(await db.GetAllLifts());
        }
    }
}
