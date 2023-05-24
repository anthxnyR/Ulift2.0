using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ulift2._0.Models;

namespace Ulift2._0.Repository
{
    interface ILiftCollection
    {
        Task InsertLift(Lift lift);
        Task UpdateLift(Lift lift);
        Task DeleteLift(String id);
        Task<IEnumerable<Lift>> GetAllLifts();
        void ValidateLiftAttributes(Lift lift, ModelStateDictionary ModelState);
    }
}
