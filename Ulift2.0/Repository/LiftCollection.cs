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

                //var vehicle = await _repository.db.GetCollection<Vehicle>("Vehicles").FindAsync(x => x.Plate == Lift.Plate && x.Email == Lift.DriverEmail).Result.FirstOrDefaultAsync();
                //if (vehicle == null)
                //{
                //    throw new Exception("El vehiculo no existe");
                //}

                var newLift = new Lift
                {
                    DriverEmail = Lift.DriverEmail,
                    DriverRating = 0,
                    Status = "A",
                    Plate = Lift.Plate,
                    Route = Lift.Route,
                    Seats = Lift.Seats,
                    WaitingTime = DateTime.Now,
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
