using Microsoft.AspNetCore.Mvc;
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
        Task<List<(Lift, User, URoute, Vehicle)>> GetAvailableLifts();
        void ValidateLiftAttributes(Lift lift, ModelStateDictionary ModelState);
        Task<IActionResult> CreateLift(LiftCreation lift);
        Task<List<(Lift, User, URoute, Vehicle)>> GetMatch(double lat, double lng, bool wOnly, int maxD);
    }
}
