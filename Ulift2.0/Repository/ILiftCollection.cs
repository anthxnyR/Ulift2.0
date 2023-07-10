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
        Task<List<AvailableLift>> GetAvailableLifts(bool inUcab);
        Task<List<AvailableLift>> GetAvailableLiftsByDriverGender(bool wOnly, bool inUcab);
        void ValidateLiftAttributes(Lift lift, ModelStateDictionary ModelState);
        Task<Lift> CreateLift(LiftCreation lift);
        Task<List<AvailableLift>> GetMatch(double lat, double lng, bool wOnly, int maxD, bool inUcab);
        Task AcceptRequest (string LiftId, string passengerEmail);
        Task StartLift (string LiftId);
        Task<string> PasajeroCheck (string passengerEmail);
        Task DeleteLiftByDriver(string liftId);
        Task LiftCompleteCheck2(String liftId);
        Task LiftCompleteCheck([FromBody] PassengerRatings ratingList);
        Task CreateRatingPassenger(String liftId, String passengerEmail, int rating);
        Task<bool> CheckAllArriving(String liftId);
        Task<bool> CheckAcceptCola (String liftId, String email);
        Task<IEnumerable<User>> UsersInLift(string liftId);
        Task<User> DriverInLift(string LiftId);

    }
}
