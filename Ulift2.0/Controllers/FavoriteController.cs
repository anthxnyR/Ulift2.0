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
        
        [HttpGet]
        public async Task<IActionResult> GetAllFavorites()
        {
            return Ok(await db.GetAllFavorites());
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

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFavorite(String id)
        {
            await db.DeleteFavorite(id);
            return NoContent();
        }
    }
}