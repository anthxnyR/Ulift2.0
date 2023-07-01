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
        Task<List<AvailableLift>> GetAvailableLifts();
        Task<List<AvailableLift>> GetAvailableLiftsByDriverGender(bool wOnly);
        void ValidateLiftAttributes(Lift lift, ModelStateDictionary ModelState);
        Task<IActionResult> CreateLift(LiftCreation lift);
        Task<List<AvailableLift>> GetMatch(double lat, double lng, bool wOnly, int maxD);
        Task AcceptRequest (string LiftId, string passengerEmail);
        Task StartLift (string LiftId);
        Task<string> PasajeroCheck (string passengerEmail);
    }
}
