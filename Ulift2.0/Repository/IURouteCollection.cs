using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ulift2._0.Models;

namespace Ulift2._0.Repository
{
    interface IURouteCollection
    {
        Task InsertRoute(URoute route);
        Task UpdateRoute(URoute route);
        Task DeleteRoute(String id);
        Task<IEnumerable<URoute>> GetAllRoutes();
        void ValidateRouteAttributes(URoute route, ModelStateDictionary ModelState);
    }
}
