using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Ulift2._0.Models;

namespace Ulift2._0.Repository
{
    public class VehicleCollection : IVehicleCollection
    {
        internal MongoDBRepository _repository = new MongoDBRepository();
        private IMongoCollection<Vehicle> Collection;
        public VehicleCollection()
        {
            Collection = _repository.db.GetCollection<Vehicle>("Vehicles");
        }
        public async Task<IEnumerable<Vehicle>> GetAllVehicles()
        {
            return await Collection.FindAsync(new BsonDocument()).Result.ToListAsync();
        }
    }
}
