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
using Microsoft.Extensions.Azure;

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

        public async Task<Lift> CreateLift([FromBody] LiftCreation Lift)
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

                Guid myuuid = Guid.NewGuid();
                string myuuidAsString = myuuid.ToString();

                var newLift = new Lift
                {
                    LiftId = myuuidAsString,
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
                    CreatedAt = DateTime.Now,
                    complete = false
                };

                driver.Status = "D";
                await _repository.db.GetCollection<User>("Users").ReplaceOneAsync(x => x.Email == Lift.DriverEmail, driver);

                await InsertLift(newLift);
                return newLift;
            }catch (Exception ex)
            {
                throw new Exception("Error al crear el viaje");
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

        public async Task AcceptRequest(string LiftId, string passengerEmail)
        {
            var filter = Builders<Lift>.Filter.Eq(lift => lift.LiftId, LiftId);
            var liftCursor = await Collection.FindAsync(filter);
            var lift = await liftCursor.FirstOrDefaultAsync();

            if (lift == null)
            {
                throw new Exception("El viaje no existe");
            }

            if (lift.Status != "A")
            {
                throw new Exception("El viaje no está disponible");
            }

            if (lift.Status == "P")
            {
                throw new Exception("El viaje ya está en proceso");
            }

            if (lift.Status == "F")
            {
                throw new Exception("El viaje ya finalizó");
            }

            if (lift.Email1 == passengerEmail || lift.Email2 == passengerEmail || lift.Email3 == passengerEmail || lift.Email4 == passengerEmail || lift.Email5 == passengerEmail)
            {
                throw new Exception("El pasajero ya está registrado en el viaje");
            }

            if (lift.Email1 == "")
            {
                lift.Email1 = passengerEmail;
            }
            else if (lift.Email2 == "")
            {
                lift.Email2 = passengerEmail;
            }
            else if (lift.Email3 == "")
            {
                lift.Email3 = passengerEmail;
            }
            else if (lift.Email4 == "")
            {
                lift.Email4 = passengerEmail;
            }
            else if (lift.Email5 == "")
            {
                lift.Email5 = passengerEmail;
            }
            else
            {
                throw new Exception("El viaje está lleno");
            }

            lift.Seats = lift.Seats - 1;

            using (HttpClient client = new HttpClient())
            {
                //xd
                string url = $"https://ulift.azurewebsites.net/api/WaitingList?Id={LiftId}&Email={passengerEmail}";
                HttpResponseMessage response = await client.DeleteAsync(url);
                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception("Error al eliminar la instancia de WaitingList");
                }
            }


            await Collection.ReplaceOneAsync(filter, lift);
        }

        public async Task StartLift(string LiftId)
        {
            var filter = Builders<Lift>.Filter.Eq(lift => lift.LiftId, LiftId);
            var liftCursor = await Collection.FindAsync(filter);
            var lift = await liftCursor.FirstOrDefaultAsync();

            if (lift == null)
            {
                throw new Exception("El viaje no existe");
            }

            if (lift.Status != "A")
            {
                throw new Exception("El viaje no está disponible");
            }

            if (lift.Status == "P")
            {
                throw new Exception("El viaje ya está en proceso");
            }

            if (lift.Status == "F")
            {
                throw new Exception("El viaje ya finalizó");
            }

            lift.Status = "P";
            await Collection.ReplaceOneAsync(filter, lift);
        }

        public async Task<string> PasajeroCheck(string passengerEmail)
        {
            var filter = Builders<Lift>.Filter.Or(
                               Builders<Lift>.Filter.Eq(lift => lift.Email1, passengerEmail),
                                              Builders<Lift>.Filter.Eq(lift => lift.Email2, passengerEmail),
                                                             Builders<Lift>.Filter.Eq(lift => lift.Email3, passengerEmail),
                                                                            Builders<Lift>.Filter.Eq(lift => lift.Email4, passengerEmail),
                                                                                           Builders<Lift>.Filter.Eq(lift => lift.Email5, passengerEmail)
                                                                                                      );
            var liftCursor = await Collection.FindAsync(filter);
            var lift = await liftCursor.FirstOrDefaultAsync();

            if (lift == null)
            {
                throw new Exception("El pasajero no está registrado en ningún viaje");
            }
            if (lift.Status == "A")
            {
                throw new Exception("El viaje no ha iniciado");
            }

            if (lift.Status != "P")
            {
                throw new Exception("El viaje no está en proceso");
            }

            var response = new
            {
                message = "Tu viaje ha finalizado. ¡Gracias por viajar con nosotros!"
            };

            return JsonConvert.SerializeObject(response);
        }

        public async Task LiftCompleteCheck(String liftId)
        {
            var filter = Builders<Lift>.Filter.Eq(x => x.LiftId, liftId);
            var liftCursor = await Collection.FindAsync(filter);
            var lift = await liftCursor.FirstOrDefaultAsync();

            if (lift == null)
            {
                throw new Exception("El viaje no existe");
            }

            if (lift.Status != "F")
            {
                throw new Exception("El viaje no ha finalizado");
            }

            var update = Builders<Lift>.Update.Set(x => x.complete, true);

            await Collection.UpdateOneAsync(filter, update);
        }


        // solo para pruebas del front
        public async Task DeleteLiftByDriver(string driverEmail)
        {
            var filterEmail = Builders<Lift>.Filter.Eq(lift => lift.DriverEmail, driverEmail);
            var lifts = Collection.FindAsync(filterEmail);
            await Collection.DeleteManyAsync(filterEmail);
        }

        // public async Task<IActionResult> LiftCompleteCheck()
        // {
        //     var filter = Builders<Lift>.Filter.Eq(lift => lift.Status, "P");
        //     var liftCursor = await Collection.FindAsync(filter);
        //     var lifts = await liftCursor.ToListAsync();

        //     foreach (var lift in lifts)
        //     {
        //         if (lift.Status == "P")
        //         {
        //             lift.Status = "F";
        //             await Collection.ReplaceOneAsync(filter, lift);
        //         }
        //     }

        //     return new OkResult();
        // }

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
