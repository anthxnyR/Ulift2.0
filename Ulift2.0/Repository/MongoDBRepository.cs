using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ulift2._0.Repository
{
    public class MongoDBRepository
    {
        public MongoClient client;
        public IMongoDatabase db;
        public MongoDBRepository()
        {
            client = new MongoClient("mongodb://mongo:79oqX31rEX2kLUCTjJvd@containers-us-west-146.railway.app:7770");
            db = client.GetDatabase("Ulift");
        }
    }
}
