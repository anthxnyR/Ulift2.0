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

        public async Task<IEnumerable<Favorite>> GetAllFavoritesOfAnUser(string UserEmail) 
        {
            var filter = Builders<Favorite>.Filter.Eq("UserEmail", UserEmail);
            return await Collection.Find(filter).ToListAsync();
        }

        public void ValidateFavoriteAttributes(Favorite favorite, ModelStateDictionary ModelState)
        {
            string domainPattern = @"@(est.ucab.edu.ve|ucab.edu.ve)$";
            if (!Regex.IsMatch(favorite.UserEmail, domainPattern, RegexOptions.IgnoreCase))
            {
                ModelState.AddModelError("Email", "El correo electrónico no pertenece al dominio UCAB");
                throw new Exception("El correo electrónico no pertenece al dominio UCAB");
            }
            if (!Regex.IsMatch(favorite.FavoriteEmail, domainPattern, RegexOptions.IgnoreCase))
            {
                ModelState.AddModelError("Email", "El correo electrónico no pertenece al dominio UCAB");
                throw new Exception("El correo electrónico no pertenece al dominio UCAB");
            }
            if (favorite.UserEmail == null)
            {
                ModelState.AddModelError("UserEmail", "El favorito debe tener un usuario asociado");
            }
            if (favorite.FavoriteEmail == null)
            {
                ModelState.AddModelError("FavoriteEmail", "El favorito debe tener un ID asociado");
            }
        }
    }
}
