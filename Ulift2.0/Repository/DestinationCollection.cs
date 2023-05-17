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
    public class DestinationCollection : IDestinationCollection
    {
        internal MongoDBRepository _repository = new MongoDBRepository();
        private IMongoCollection<Destination> Collection;
        public DestinationCollection()
        {
            Collection = _repository.db.GetCollection<Destination>("Destinations");
        }
        public async Task<IEnumerable<Destination>> GetAllDestinations()
        {
            return await Collection.FindAsync(new BsonDocument()).Result.ToListAsync();
        }
    }
}
