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
        [HttpGet]
        public async Task<IActionResult> GetAllRoutes()
        {
            return Ok(await db.GetAllRoutes());
        }
    }
}
