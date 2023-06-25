using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Ulift2._0.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver.Core.Authentication;
using MongoDB.Bson.IO;
using Newtonsoft.Json;
using Ulift2._0.Helpers;
using MongoDB.Bson.Serialization;

namespace Ulift2._0.Repository
{
    public class LiftCollection : ILiftCollection
    {
        internal MongoDBRepository _repository = new MongoDBRepository();
        private IMongoCollection<Lift> Collection;
        public LiftCollection()
        {
            Collection = _repository.db.GetCollection<Lift>("Lifts");
        }
        public async Task InsertLift(Lift lift)
        {
            await Collection.InsertOneAsync(lift);
        }
        public async Task UpdateLift(Lift lift)
        {
            var filter = Builders<Lift>.Filter.Eq(s => s.Id, lift.Id);
            await Collection.ReplaceOneAsync(filter, lift);
        }
        public async Task DeleteLift(String id)
        {
            var filter = Builders<Lift>.Filter.Eq(s => s.Id, new ObjectId(id));
            await Collection.DeleteOneAsync(filter);
        }
        public async Task<IEnumerable<Lift>> GetAllLifts()
        {
            return await Collection.FindAsync(new BsonDocument()).Result.ToListAsync();
        }

        public async Task<IActionResult> CreateLift([FromBody] LiftCreation Lift)
        {
            try 
            { 
                var driver = await _repository.db.GetCollection<User>("Users").FindAsync(x => x.Email == Lift.DriverEmail).Result.FirstOrDefaultAsync();
                if (driver == null)
                {
                    throw new Exception("El conductor no existe");
                }
                var driverStatus = await _repository.db.GetCollection<Lift>("Lifts").FindAsync(x => x.DriverEmail == Lift.DriverEmail && x.Status == "A").Result.FirstOrDefaultAsync();
                if (driverStatus != null)
                {
                    throw new Exception("El conductor ya tiene un viaje asociado");
                }

                var vehicle = await _repository.db.GetCollection<Vehicle>("Vehicles").FindAsync(x => x.Plate == Lift.Plate && x.Email == Lift.DriverEmail).Result.FirstOrDefaultAsync();
                if (vehicle == null)
                {
                    throw new Exception("El vehículo no existe o no está registrado al usuario");
                }

                var newLift = new Lift
                {
                    DriverEmail = Lift.DriverEmail,
                    DriverRating = 0,
                    Status = "A",
                    Plate = Lift.Plate,
                    Route = Lift.Route,
                    Seats = Lift.Seats,
                    WaitingTime = Lift.WaitingTime,
                    Email1 = "",
                    Email2 = "",
                    Email3 = "",
                    Email4 = "",
                    Email5 = "",
                    Rating1 = 0,
                    Rating2 = 0,
                    Rating3 = 0,
                    Rating4 = 0,
                    Rating5 = 0
                };

                await InsertLift(newLift);
                return new OkObjectResult(newLift);
            }catch (Exception ex)
            {
                return new BadRequestObjectResult(ex.Message);
            }
        }

        // make a method that list all lift available
        public async Task<List<AvailableLift>> GetAvailableLifts()
        {
            var filter = Builders<Lift>.Filter.Eq(lift => lift.Status, "A");
            var lifts = await Collection.FindAsync(filter).Result.ToListAsync();

            var users = _repository.db.GetCollection<User>("Users");
            var routes = _repository.db.GetCollection<URoute>("URoutes");
            var vehicles = _repository.db.GetCollection<Vehicle>("Vehicles");

            var liftsList = new List<AvailableLift>();

            foreach (var lift in lifts)
            {
                var driver = await users.FindAsync(user => user.Email == lift.DriverEmail).Result.FirstOrDefaultAsync();
                var route = await routes.FindAsync(uroute => uroute.Path == lift.Route).Result.FirstOrDefaultAsync();
                var vehicle = await vehicles.FindAsync(v => v.Plate == lift.Plate).Result.FirstOrDefaultAsync();

                var availableLift = new AvailableLift
                {
                    Lift = lift,
                    Driver = driver,
                    Route = route,
                    Vehicle = vehicle
                };

                liftsList.Add(availableLift);
            }

            return liftsList;
        }

        public Task<List<AvailableLift>> GetAvailableLifts(bool wOnly)
        {
            if (wOnly)
            {
                return GetAvailableLifts().ContinueWith(task => task.Result.Where(lift => lift.Driver.Gender == "F").ToList());
            }
            else
            {
                return GetAvailableLifts();
            }
        }

        public async Task<List<AvailableLift>> GetMatch(double lat, double lng, bool wOnly, int maxD)
        {
            var destination = new { lat, lng };
            var activeRoutes = await GetAvailableLifts(wOnly);

            var optimizedRoutes = new List<AvailableLift>();
            var distances = new Dictionary<(Lift, Vehicle), double>();

            if (maxD != 0)
            {
                foreach (var activeRoute in activeRoutes)
                {
                    foreach (var node in activeRoute.Route.Path)
                    {
                        var distance = Distance.CalculateDistance(node, destination);
                        if (distance <= maxD)
                        {
                            distances[(activeRoute.Lift, activeRoute.Vehicle)] = Distance.CalculateDistance(activeRoute.Route.Path.Last(), destination);
                            optimizedRoutes.Add(activeRoute);
                            break;
                        }

                    }
                }
                optimizedRoutes = optimizedRoutes.OrderBy(route => distances[(route.Lift, route.Vehicle)]).ToList();
                return optimizedRoutes;
            }
            else
            {
                foreach (var activeRoute in activeRoutes)
                {
                    distances[(activeRoute.Lift, activeRoute.Vehicle)] = Distance.CalculateDistance(activeRoute.Route.Path.Last(), destination);
                }
                activeRoutes = activeRoutes.OrderBy(route => distances[(route.Lift, route.Vehicle)]).ToList();
                return activeRoutes;
            }
        }

        public void ValidateLiftAttributes(Lift lift, ModelStateDictionary ModelState)
        {
            //if (lift.LiftID == null)
            //{
            //    ModelState.AddModelError("LiftID", "El viaje debe tener un ID asociado");
            //}
            if (lift.DriverEmail == null)
            {
                ModelState.AddModelError("Driver", "El viaje debe tener un conductor asociado");
            }
            if (lift.Status == null)
            {
                ModelState.AddModelError("Status", "El viaje debe tener un estado asociado");
            }
            if (lift.Plate == null)
            {
                ModelState.AddModelError("Plate", "El viaje debe tener una patente asociada");
            }
            if (lift.Route == null)
            {
                ModelState.AddModelError("Route", "El viaje debe tener una ruta asociada");
            }
            if (lift.Seats == 0)
            {
                ModelState.AddModelError("Seats", "El viaje debe tener una cantidad de asientos asociada");
            }
        }
    }
}
