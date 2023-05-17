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
    public class URouteCollection : IURouteCollection
    {
        internal MongoDBRepository _repository = new MongoDBRepository();
        private IMongoCollection<URoute> Collection;
        public URouteCollection()
        {
            Collection = _repository.db.GetCollection<URoute>("Routes");
        }
        public async Task<IEnumerable<URoute>> GetAllRoutes()
        {
            return await Collection.FindAsync(new BsonDocument()).Result.ToListAsync();
        }
    }
}
