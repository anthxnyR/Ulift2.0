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
using Ulift2._0.Repository;
using Ulift2._0.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace Ulift2._0.Repository
{
    public class LiftCollection : ILiftCollection
    {
        internal MongoDBRepository _repository = new MongoDBRepository();
        private IMongoCollection<Lift> Collection;
        private IMongoCollection<User> UserCollection;
        private readonly IHubContext<ChatHub> _chatHubContext;

        public LiftCollection(IHubContext<ChatHub> chatHubContext)
        {
            Collection = _repository.db.GetCollection<Lift>("Lifts");
            _chatHubContext = chatHubContext;
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
                var driverStatus = await _repository.db.GetCollection<Lift>("Lifts").FindAsync(x => x.DriverEmail == Lift.DriverEmail && (x.Status == "A" || x.Status == "P")).Result.FirstOrDefaultAsync();
                if (driverStatus != null)
                {
                    throw new Exception("El conductor ya tiene un viaje asociado");
                }

                var vehicle = await _repository.db.GetCollection<Vehicle>("Vehicles").FindAsync(x => x.Plate == Lift.Plate && x.Email == Lift.DriverEmail).Result.FirstOrDefaultAsync();
                if (vehicle == null)
                {
                    throw new Exception("El vehículo no existe o no está registrado al usuario");
                }

                var route = await _repository.db.GetCollection<URoute>("Routes").FindAsync(x => x.Name == Lift.Route).Result.FirstOrDefaultAsync();

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
                    Check1 = false,
                    Check2 = false,
                    Check3 = false,
                    Check4 = false,
                    Check5 = false,
                    CreatedAt = DateTime.Now,
                    inUcab = route.inUcab
                };

                driver.Status = "D";
                await _repository.db.GetCollection<User>("Users").ReplaceOneAsync(x => x.Email == Lift.DriverEmail, driver);

                await InsertLift(newLift);
                return newLift;
            } catch (Exception ex)
            {
                throw new Exception("Error al crear el viaje");
            }
        }

        public async Task<List<AvailableLift>> GetAvailableLifts(bool inUcab)
        {
            var filter = Builders<Lift>.Filter.Eq(lift => lift.Status, "A");
            var filter2 = inUcab ? Builders<Lift>.Filter.Eq(lift => lift.inUcab, true) : Builders<Lift>.Filter.Eq(lift => lift.inUcab, false);

            var liftsCursor = await Collection.FindAsync(filter & filter2);

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

        public async Task<List<AvailableLift>> GetAvailableLiftsByDriverGender(bool wOnly, bool inUcab)
        {
            var availableLifts = await GetAvailableLifts(inUcab);

            if (!wOnly)
            {
                return availableLifts;
            }

            var filteredLifts = availableLifts.Where(lift => lift.Driver.Gender == "F").ToList();
            return filteredLifts;
        }

        public async Task<List<AvailableLift>> GetMatch(double lat, double lng, bool wOnly, int maxD, bool inUcab)
        {
            var destination = new { lat, lng };
            var activeRoutes = await GetAvailableLiftsByDriverGender(wOnly, inUcab);

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
            
            await _chatHubContext.Clients.User(passengerEmail).SendAsync("ReceiveMessage", "¡Has sido aceptado como pasajero!");
        }

        public async Task<IEnumerable<User>> UsersInLift(string LiftId)
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

            if (lift.Status != "P")
            {
                throw new Exception("El viaje no está en proceso");
            }

            var users = _repository.db.GetCollection<User>("Users");
            var usersCursor = await users.FindAsync(user => (user.Email != null && (user.Email == lift.Email1 || user.Email == lift.Email2 || user.Email == lift.Email3 || user.Email == lift.Email4 || user.Email == lift.Email5)));
            var usersList = await usersCursor.ToListAsync();
        
            return usersList;
        }

        public async Task<User> DriverInLift(string LiftId)
        {
            var filter = Builders<Lift>.Filter.Eq(lift => lift.LiftId , LiftId);
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

            if (lift.Status != "P")
            {
                throw new Exception("El viaje no está en proceso");
            }

            var d = _repository.db.GetCollection<User>("Users");
            var driverCursor = await d.FindAsync(driver => driver.Email == lift.DriverEmail);
            var driver = await driverCursor.FirstOrDefaultAsync();
            return driver;
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

        public async Task LiftCompleteCheck2(String liftId)
        {
            try
            {
                var filter = Builders<Lift>.Filter.Eq(x => x.LiftId, liftId);
                var liftCursor = await Collection.FindAsync(filter);
                var lift = await liftCursor.FirstOrDefaultAsync();

                if (lift == null)
                {
                    throw new Exception("El viaje no existe");
                }

                var update = Builders<Lift>.Update.Set(x => x.Status, "F");

                await Collection.UpdateOneAsync(filter, update);
            }
            catch
            {
                throw new Exception("Error al actualizar el viaje");
            }
        }

        public async Task LiftCompleteCheck([FromBody] PassengerRatings ratingList)
        {
            var filter = Builders<Lift>.Filter.Eq(x => x.LiftId, ratingList.LiftId);
            var liftCursor = await Collection.FindAsync(filter);
            var lift = await liftCursor.FirstOrDefaultAsync();
            if (lift == null)
            {
                throw new Exception("El viaje no existe");
            }

            if(lift.Status == "A")
            {
                throw new Exception("El viaje no ha iniciado");
            }

            if (lift.Status == "F")
            {
                throw new Exception("El viaje ya está marcado como finalizado");
            }

            for (int i = 1; i <= 5; i++)
            {
                var emailProperty = ratingList.GetType().GetProperty("Email" + i);
                var emailValue = (string)emailProperty.GetValue(ratingList);
                if(emailValue != "")
                {
                    var ratingProperty = ratingList.GetType().GetProperty("Rating" + i);
                    var ratingValue = (int)ratingProperty.GetValue(ratingList);

                    for (int j = 1; j <= 5; j++)
                    {
                        var emailProperty2 = lift.GetType().GetProperty("Email" + j);
                        var emailValue2 = (string)emailProperty2.GetValue(lift);
                        if (emailValue2 == emailValue)
                        {

                            //Actualizar rating del conductor
                            var filterUser = Builders<User>.Filter.Eq(x => x.Email, emailValue);
                            var userCursor = await _repository.db.GetCollection<User>("Users").FindAsync(filterUser);
                            var user = await userCursor.FirstOrDefaultAsync();
                            user.PassengerRating = (user.PassengerRating*user.LiftCountAsPassenger + ratingValue) / (user.LiftCountAsPassenger + 1);
                            user.LiftCountAsPassenger = user.LiftCountAsPassenger + 1;
                            await _repository.db.GetCollection<User>("Users").ReplaceOneAsync(x => x.Email == emailValue, user);

                            var ratingProperty2 = lift.GetType().GetProperty("Rating" + j);
                            ratingProperty2.SetValue(lift, ratingValue);
                            break;
                        }
                    }
                }

            }

            lift.Status = "F";
            var driver = await _repository.db.GetCollection<User>("Users").FindAsync(x => x.Email == lift.DriverEmail).Result.FirstOrDefaultAsync();
            driver.Status = "P";
            driver.DriverRating = (driver.DriverRating * driver.LiftCountAsDriver + lift.DriverRating) / (driver.LiftCountAsDriver + 1);
            driver.LiftCountAsDriver = driver.LiftCountAsDriver + 1;
            await _repository.db.GetCollection<User>("Users").ReplaceOneAsync(x => x.Email == lift.DriverEmail, driver);
            await Collection.ReplaceOneAsync(filter, lift);
        }

        // solo para pruebas del front
        public async Task DeleteLiftByDriver(string driverEmail)
        {
            var filterEmail = Builders<Lift>.Filter.Eq(lift => lift.DriverEmail, driverEmail);
            var lifts = Collection.FindAsync(filterEmail);
            await Collection.DeleteManyAsync(filterEmail);
        }

        public async Task CreateRatingPassenger(String liftId, String passengerEmail, int rating)
        {
            var filter = Builders<Lift>.Filter.Eq(lift => lift.LiftId, liftId);
            var liftCursor = await Collection.FindAsync(filter);
            var lift = await liftCursor.FirstOrDefaultAsync();

            if (lift == null)
            {
                throw new Exception("El viaje no existe");
            }

            if (lift.Status == "A")
            {
                throw new Exception("El viaje no ha comenzado.");
            }

            if (lift.DriverRating == 0)
            {
                lift.DriverRating = rating;
            }
            else
            {
                float checkCounter = 0;
                for (int i = 1; i < 5; i++)
                {
                    var checkProperty = lift.GetType().GetProperty("Check" + i);
                    bool checkValue = (bool)checkProperty.GetValue(lift);
                    if (checkValue)
                    {
                        checkCounter++;
                    }
                }
                
                float driverPoints = lift.DriverRating * checkCounter;
                driverPoints += rating;
                lift.DriverRating = driverPoints / (checkCounter + 1);
            }

            for (int i = 1; i <= 5; i++)
            {
                var emailProperty = lift.GetType().GetProperty("Email" + i);
                String emailValue = (String)emailProperty.GetValue(lift);
                if(emailValue == passengerEmail)
                {
                    var checkProperty = lift.GetType().GetProperty("Check" + i);
                    checkProperty.SetValue(lift, true);
                    break;
                }
            }
            await Collection.ReplaceOneAsync(filter, lift);
        }

        public async Task<bool> CheckAllArriving(String liftId)
        {
            int emailCounter = 0;
            int checkCounter = 0;
            var filter = Builders<Lift>.Filter.Eq(lift => lift.LiftId, liftId);
            var liftCursor = await Collection.FindAsync(filter);
            var lift = await liftCursor.FirstOrDefaultAsync();

            if (lift == null)
            {
                throw new Exception("El viaje no existe");
            }

            for (int i = 1; i <= 5; i++)
            {
                var emailProperty = lift.GetType().GetProperty("Email" + i);
                String emailValue = (String)emailProperty.GetValue(lift);
                if (emailValue != "")
                {
                    emailCounter++;
                    var checkProperty = lift.GetType().GetProperty("Check" + i);
                    bool checkValue = (bool)checkProperty.GetValue(lift);
                    if (checkValue)
                    {
                        checkCounter++;
                    }
                }
            }

            if (emailCounter == checkCounter)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public async Task<bool> CheckAcceptCola (String liftId, String email)
        {
            var filter = Builders<Lift>.Filter.Eq(lift => lift.LiftId, liftId);
            var liftCursor = await Collection.FindAsync(filter);
            var lift = await liftCursor.FirstOrDefaultAsync();

            if (lift == null)
            {
                  throw new Exception("El viaje no existe");
            }

            for (int i = 1; i <= 5; i++)
            {
                var emailProperty = lift.GetType().GetProperty("Email" + i);
                String emailValue = (String)emailProperty.GetValue(lift);
                if (emailValue == email)
                {
                    return true;
                }
            }
            return false;
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
