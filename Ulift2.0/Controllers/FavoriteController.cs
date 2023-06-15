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
    public class FavoriteController : Controller
    {
        private IFavoriteCollection db = new FavoriteCollection();
        private readonly ILogger<FavoriteController> _logger;

        public FavoriteController(ILogger<FavoriteController> logger)
        {
            _logger = logger;
        }
        
        [HttpGet]
        public async Task<IActionResult> GetAllFavoritesOfAnUser(string UserEmail)
        {
            return Ok(await db.GetAllFavoritesOfAnUser(UserEmail));
        }

        [HttpPost]
        public async Task<IActionResult> InsertFavorite([FromBody] Models.Favorite favorite)
        {
            if (favorite == null)
            {
                return BadRequest();
            }
            db.ValidateFavoriteAttributes(favorite, ModelState);
            await db.InsertFavorite(favorite);
            return Created("Created", true);
        }
    }
}