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
using JsonConvert = Newtonsoft.Json.JsonConvert;
using System.Text;
using System.Net.Http;


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
                    Rating5 = 0,
                    CreatedAt = DateTime.Now
                };

                await InsertLift(newLift);
                return new OkObjectResult(newLift);
            }catch (Exception ex)
            {
                return new BadRequestObjectResult(ex.Message);
            }
        }

        public async Task<List<AvailableLift>> GetAvailableLifts()
        {
            var filter = Builders<Lift>.Filter.Eq(lift => lift.Status, "A");
            var liftsCursor = await Collection.FindAsync(filter);
            var lifts = await liftsCursor.ToListAsync();

            if (lifts == null || !lifts.Any())
            {
                return new List<AvailableLift>();
            }

            var liftDriverEmail = lifts.Select(lift => lift.DriverEmail).ToList();

            var users = _repository.db.GetCollection<User>("Users");
            var driversCursor = await users.Find(user => liftDriverEmail.Contains(user.Email)).ToListAsync();
            var drivers = driversCursor.ToList();

            var routes = _repository.db.GetCollection<URoute>("Routes");
            var routeNamesCursor = await routes.Find(uroute => liftDriverEmail.Contains(uroute.Email)).ToListAsync();
            var routeNames = routeNamesCursor.ToList();

            var vehicles = _repository.db.GetCollection<Vehicle>("Vehicles");
            var vehiclePlatesCursor = await vehicles.Find(vehicle => liftDriverEmail.Contains(vehicle.Email)).ToListAsync();
            var vehiclePlates = vehiclePlatesCursor.ToList();

            var liftsList = new List<AvailableLift>();

            foreach (var lift in lifts)
            {
                var driver = drivers.FirstOrDefault(d => d.Email == lift.DriverEmail);
                var route = routeNames.FirstOrDefault(rp => rp.Name == lift.Route);
                
                var vehicle = vehiclePlates.FirstOrDefault(vp => vp.Plate == lift.Plate && vp.Email == lift.DriverEmail);
                    
                if (driver != null && route != null && vehicle != null)
                {
                    var availableLift = new AvailableLift
                    {
                        Lift = lift,
                        Driver = driver,
                        Route = route,
                        Vehicle = vehicle
                    };

                    liftsList.Add(availableLift);
                }
            }
            return liftsList;
        }

        public async Task<List<AvailableLift>> GetAvailableLiftsByDriverGender(bool wOnly)
        {
            var availableLifts = await GetAvailableLifts();

            if (!wOnly)
            {
                return availableLifts;
            }
            
            var filteredLifts = availableLifts.Where(lift => lift.Driver.Gender == "F").ToList();
            return filteredLifts;
        }

        public async Task<List<AvailableLift>> GetMatch(double lat, double lng, bool wOnly, int maxD)
        {
            var destination = new { lat, lng };
            var activeRoutes = await GetAvailableLiftsByDriverGender(wOnly);

            var optimizedRoutes = new List<AvailableLift>();
            var distances = new Dictionary<(Lift, Vehicle), double>();

            if (maxD != 0)
            {
                foreach (var activeRoute in activeRoutes)
                {
                    var data_string = activeRoute.Route.Path;
                    List<Coordinates> coordinatesList = new List<Coordinates>();
                    coordinatesList = JsonConvert.DeserializeObject<List<Coordinates>>(data_string);
                    foreach (var node in coordinatesList)
                    {
                        var distance = Distance.CalculateDistance(node, destination);
                        if (distance <= maxD)
                        {
                            distances[(activeRoute.Lift, activeRoute.Vehicle)] = Distance.CalculateDistance(coordinatesList.Last(), destination);
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

        public async Task<List<User>> GetLiftRequest(string email)
        {
            try
            {
                var waiting = await _repository.db.GetCollection<WaitingList>("WaitingList").FindAsync(x => x.DriverEmail == email).Result.ToListAsync();

                if (waiting.Count == 0)
                {
                    return new List<User>();
                    throw new Exception("No hay solicitudes de viaje");
                }

                var userIDs = waiting.Select(w => w.Id).ToList();
                var usersRequests = await _repository.db.GetCollection<User>("Users").FindAsync(u => userIDs.Contains(u.Id)).Result.ToListAsync();
                return usersRequests;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
