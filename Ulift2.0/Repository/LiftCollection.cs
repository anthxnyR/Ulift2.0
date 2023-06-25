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

        public async Task<List<(Lift, User, URoute, Vehicle)>> GetAvailableLifts()
        {
            var filter = Builders<Lift>.Filter.Eq(lift => lift.Status, "A");

            var pipeline = new BsonDocument[]
            {
                BsonDocument.Parse("{$match: {Status: 'A'}}"),
                BsonDocument.Parse("{$lookup: {from: 'users', localField: 'DriverEmail', foreignField: 'Email', as: 'Driver'}}"),
                BsonDocument.Parse("{$lookup: {from: 'uroutes', localField: 'Route', foreignField: 'Path', as: 'Route'}}"),
                BsonDocument.Parse("{$lookup: {from: 'vehicles', localField: 'Plate', foreignField: 'Plate', as: 'Vehicle'}}"),
                BsonDocument.Parse("{$unwind: '$Driver'}"),
                BsonDocument.Parse("{$unwind: '$Route'}"),
                BsonDocument.Parse("{$unwind: '$Vehicle'}"),
                BsonDocument.Parse("{$project: {Lift: '$$ROOT', Driver: '$Driver', Route: '$Route', Vehicle: '$Vehicle'}}")
            };

            var aggregateOptions = new AggregateOptions { AllowDiskUse = true };

            var cursor = await Collection.AggregateAsync<BsonDocument>(pipeline, aggregateOptions);

            var lifts = new List<(Lift, User, URoute, Vehicle)>();

            await cursor.ForEachAsync(document =>
            {
                var lift = BsonSerializer.Deserialize<Lift>(document["Lift"].AsBsonDocument);
                var driver = BsonSerializer.Deserialize<User>(document["Driver"].AsBsonDocument);
                var route = BsonSerializer.Deserialize<URoute>(document["Route"].AsBsonDocument);
                var vehicle = BsonSerializer.Deserialize<Vehicle>(document["Vehicle"].AsBsonDocument);

                lifts.Add((lift, driver, route, vehicle));
            });

            return lifts;
        }

        public async Task<List<(Lift, User, URoute, Vehicle)>> GetMatch(double lat, double lng, bool wOnly, int maxD)
        {
            var destination = new { lat, lng };
            var activeRoutes = await GetAvailableLifts();

            if (wOnly)
            {
                activeRoutes = activeRoutes.Where(route => route.Item2.Gender == "F").ToList();
            }

            var optimizedRoutes = new List<(Lift, User, URoute, Vehicle)>();
            var distances = new Dictionary<(Lift, Vehicle), double>();

            if (maxD != 0)
            {
                foreach (var activeRoute in activeRoutes)
                {
                    foreach (var node in activeRoute.Item3.Path)
                    {
                        var distance = Distance.CalculateDistance(node, destination);
                        if (distance <= maxD)
                        {
                            distances[(activeRoute.Item1, activeRoute.Item4)] = Distance.CalculateDistance(activeRoute.Item3.Path.Last(), destination);
                            optimizedRoutes.Add(activeRoute);
                            break;
                        }
                    }
                }
                optimizedRoutes = optimizedRoutes.OrderBy(route => distances[(route.Item1, route.Item4)]).ToList();
                return optimizedRoutes;
            }
            else
            {
                foreach (var activeRoute in activeRoutes)
                {
                    distances[(activeRoute.Item1, activeRoute.Item4)] = Distance.CalculateDistance(activeRoute.Item3.Path.Last(), destination);
                }
                activeRoutes = activeRoutes.OrderBy(route => distances[(route.Item1, route.Item4)]).ToList();
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
