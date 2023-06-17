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
    public class URouteCollection : IURouteCollection
    {
        internal MongoDBRepository _repository = new MongoDBRepository();
        private IMongoCollection<URoute> Collection;
        public URouteCollection()
        {
            Collection = _repository.db.GetCollection<URoute>("Routes");
        }
        public async Task InsertRoute(URoute route)
        {
            await Collection.InsertOneAsync(route);
        }
        public async Task UpdateRoute(URoute route)
        {
            var filter = Builders<URoute>.Filter.Eq(s => s.Id, route.Id);
            await Collection.ReplaceOneAsync(filter, route);
        }
        public async Task DeleteRoute(String id)
        {
            var filter = Builders<URoute>.Filter.Eq(s => s.Id, new ObjectId(id));
            await Collection.DeleteOneAsync(filter);
        }
        public async Task<IEnumerable<URoute>> GetAllRoutes()
        {
            return await Collection.FindAsync(new BsonDocument()).Result.ToListAsync();
        }
        public async Task<IEnumerable<URoute>> GetUserRoutes(String email)
        {
            var filter = Builders<URoute>.Filter.Eq(s => s.Email, email);
            return await Collection.FindAsync(filter).Result.ToListAsync();
        }
        public void ValidateRouteAttributes(URoute route, ModelStateDictionary ModelState)
        {
            if (route.Path == null)
            {
                ModelState.AddModelError("Path", "La ruta debe tener un path asociado");
            }
            if (route.Name == null)
            {
                ModelState.AddModelError("Name", "La ruta debe tener un nombre asociado");
            }
        }
    }
}
