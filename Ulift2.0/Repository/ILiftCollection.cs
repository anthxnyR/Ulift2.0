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
        Task<IEnumerable<Lift>> GetAvailableLifts();
        void ValidateLiftAttributes(Lift lift, ModelStateDictionary ModelState);
        Task<IActionResult> CreateLift(LiftCreation lift);
        // Task<IEnumerable<Lift>> GetMatch(double lon, double lat, bool wOnly, int maxD);
    }
}
