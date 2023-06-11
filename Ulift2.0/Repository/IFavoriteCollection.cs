using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ulift2._0.Models;

namespace Ulift2._0.Repository
{
    interface IFavoriteCollection
    {
        Task InsertFavorite(Favorite favorite);
        Task DeleteFavorite(String id);
        Task<IEnumerable<Lift>> GetAllFavorites();
        void ValidateFavoriteAttributes(Favorite favorite, ModelStateDictionary ModelState);
    }
}
