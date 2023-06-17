using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Ulift2._0.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Ulift2._0.Repository
{
    public class DestinationCollection : IDestinationCollection
    {
        internal MongoDBRepository _repository = new MongoDBRepository();
        private IMongoCollection<Destination> Collection;
        public DestinationCollection()
        {
            Collection = _repository.db.GetCollection<Destination>("Destinations");
        }
        public async Task InsertDestination(Destination destination)
        {
            await Collection.InsertOneAsync(destination);
        }
        public async Task UpdateDestination(Destination destination)
        {
            var filter = Builders<Destination>.Filter.Eq(s => s.Id, destination.Id);
            await Collection.ReplaceOneAsync(filter, destination);
        }
        public async Task DeleteDestination(String id)
        {
            var filter = Builders<Destination>.Filter.Eq(s => s.Id, new ObjectId(id));
            await Collection.DeleteOneAsync(filter);
        }
        public async Task<IEnumerable<Destination>> GetAllDestinations()
        {
            return await Collection.FindAsync(new BsonDocument()).Result.ToListAsync();
        }
        public async Task<IEnumerable<Destination>> GetUserDestinations(String email)
        {
            var filter = Builders<Destination>.Filter.Eq(s => s.Email, email);
            return await Collection.FindAsync(filter).Result.ToListAsync();
        }
        public void ValidateDestinationAttributes(Destination destination, ModelStateDictionary ModelState)
        {
            if (destination.Lat == 0)
            {
                ModelState.AddModelError("Lat", "El destino debe tener una latitud asociada");
            }
            if (destination.Lng == 0)
            {
                ModelState.AddModelError("Lng", "El destino debe tener una longitud asociada");
            }
        }
    }
}
