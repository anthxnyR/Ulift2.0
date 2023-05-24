using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Ulift2._0.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;

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
        public void ValidateLiftAttributes(Lift lift, ModelStateDictionary ModelState)
        {
            if (lift.LiftID == null)
            {
                ModelState.AddModelError("LiftID", "El viaje debe tener un ID asociado");
            }
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
