using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Ulift2._0.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Ulift2._0.Repository
{
    public class FavoriteCollection : IFavoriteCollection
    {
        internal MongoDBRepository _repository = new MongoDBRepository();
        private IMongoCollection<Favorite> Collection;
        public FavoriteCollection()
        {
            Collection = _repository.db.GetCollection<Favorite>("Favorites");
        }
        public async Task InsertFavorite(Favorite favorite)
        {
            await Collection.InsertOneAsync(favorite);
        }
        public async Task UpdateFavorite(Favorite favorite)
        {
            var filter = Builders<Favorite>.Filter.Eq(s => s.UserId, favorite.UserId);
            await Collection.ReplaceOneAsync(filter, favorite);
        }
        public async Task DeleteFavorite(String id)
        {
            var filter = Builders<Favorite>.Filter.Eq(s => s.UserId, new ObjectId(id));
            await Collection.DeleteOneAsync(filter);
        }
        public async Task<IEnumerable<Favorite>> GetAllFavorites()
        {
            return await Collection.FindAsync(new BsonDocument()).Result.ToListAsync();
        }
        public void ValidateFavoriteAttributes(Favorite favorite, ModelStateDictionary ModelState)
        {
            if (favorite.UserId == null)
            {
                ModelState.AddModelError("UserId", "El favorito debe tener un usuario asociado");
            }
            if (favorite.FavoriteId == null)
            {
                ModelState.AddModelError("FavoriteId", "El favorito debe tener un ID asociado");
            }
        }
    }
}